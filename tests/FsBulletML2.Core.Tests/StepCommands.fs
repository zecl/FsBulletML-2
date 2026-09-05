namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// 命令を 1 つずつ、落とした `BulletRunner` と同じ形で動くかを見る。
///
/// 走らせるのに弾も木も要らない。Env はレコード 1 行、状態はレコード 1 個で足りる。
[<TestFixture>]
type StepCommands() =

  let env = { Rand = (fun () -> 0.5f); Rank = 0.5f; AimDir = 0.f; EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }

  let state =
    { Pos = { X = 0.f; Y = 0.f }
      Speed = 1.f
      Dir = 0.f
      Accel = { X = 0.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = [] }

  /// 参照を解けない Resolvers。bulletRef / actionRef が絡まないテストで使う
  let noResolvers : Step.Resolvers =
    { Bullet = fun _ _ -> None
      Action = fun _ _ -> None }

  [<Test>]
  member _.``wait 3 は、3 フレーム 止めてから終わる``() =
    // 旧の waitCommand: term >= 0 なら 1 減らし、その後まだ term >= 0 なら Stop。
    // Init が入れた term は getValue initTerm なので 3。
    //   1 回め  3 -> 2   Stop
    //   2 回め  2 -> 1   Stop
    //   3 回め  1 -> 0   Stop
    //   4 回め  0 -> -1  Ended
    let script = Action.Wait (numExpr "3")
    let mutable p = Progress.initial script
    let results =
      [ for _ in 1 .. 4 ->
          let (r, p'), _, _ = Sim.run env state (stepWait script p)
          p <- p'
          r ]
    results |> should equal [ Step.Stopped; Step.Stopped; Step.Stopped; Step.Ended ]

  [<Test>]
  member _.``wait 0 は、止まらずに 1 回で終わる``() =
    //   1 回め  0 -> -1 ... ではない。term >= 0 なので 1 減らして -1、Stop にならず Ended
    let script = Action.Wait (numExpr "0")
    let p = Progress.initial script
    let (r, _), _, _ = Sim.run env state (stepWait script p)
    r |> should equal Step.Ended

  [<Test>]
  member _.``vanish は、効果を 1 つ出して終わる``() =
    let p = Progress.initial Action.Vanish
    let (r, _), _, w = Sim.run env state (Step.vanish p)
    r |> should equal Step.Ended
    w |> should equal [ Vanished ]

  [<Test>]
  member _.``終わった wait をもう一度 呼んでも、止めない``() =
    // 旧は getFinish で飛ばすので、この呼び方は起きない。
    // それでも Ended を返すことで、走査の側の作りに依存しない形にしておく
    let script = Action.Wait (numExpr "1")
    let mutable p = Progress.initial script
    for _ in 1 .. 3 do
      let (_, p'), _, _ = Sim.run env state (stepWait script p)
      p <- p'
    let (r, _), _, _ = Sim.run env state (stepWait script p)
    r |> should equal Step.Ended

  [<Test>]
  member _.``accel absolute は、term フレームかけて目標へ寄せる``() =
    // horizontal absolute 4, term 4。1 フレームあたり (4 - 0) / 4 = 1
    //   1 回め  term 4 -> 3   Accel.X = 1   Continue
    //   2 回め  term 3 -> 2   Accel.X = 2   Continue
    //   3 回め  term 2 -> 1   Accel.X = 3   Continue
    //   4 回め  term 1 -> 0   Accel.X = 4   Continue
    //   5 回め  term 0 -> -1  加算せず       Ended
    let script =
      Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "4")),
                         None,
                         Term (numExpr "4"))
    let mutable p = Progress.initial script
    let mutable st = state
    let mutable rs = []
    for _ in 1 .. 5 do
      let (r, p'), st', _ = Sim.run env st (stepAccel script p)
      p <- p'
      st <- st'
      rs <- rs @ [ r ]
    rs |> should equal [ Step.Continue; Step.Continue; Step.Continue; Step.Continue; Step.Ended ]
    st.Accel.X |> should (equalWithin 0.0001) 4.0f

  [<Test>]
  member _.``accel は、最後の 1 回で加算しない``() =
    // 上の 5 回めで Accel.X が 5 になっていたら写し間違い
    let script =
      Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "4")),
                         None,
                         Term (numExpr "4"))
    let mutable p = Progress.initial script
    let mutable st = state
    for _ in 1 .. 5 do
      let (_, p'), st', _ = Sim.run env st (stepAccel script p)
      p <- p'
      st <- st'
    st.Accel.X |> should (equalWithin 0.0001) 4.0f

  [<Test>]
  member _.``accel relative は、値を term で割って毎フレーム足す``() =
    let script =
      Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Relative }, numExpr "4")),
                         None,
                         Term (numExpr "4"))
    let mutable p = Progress.initial script
    let mutable st = { state with Accel = { X = 10.0f; Y = 0.0f } }
    for _ in 1 .. 4 do
      let (_, p'), st', _ = Sim.run env st (stepAccel script p)
      p <- p'
      st <- st'
    // relative は現在値を見ないので 10 + 4 = 14
    st.Accel.X |> should (equalWithin 0.0001) 14.0f

  [<Test>]
  member _.``accel は horizontal と vertical が別々に効く``() =
    let script =
      Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "2")),
                         Some (Vertical (Some { verticalType = VerticalType.Absolute }, numExpr "-3")),
                         Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    for _ in 1 .. 2 do
      let (_, p'), st', _ = Sim.run env st (stepAccel script p)
      p <- p'
      st <- st'
    st.Accel.X |> should (equalWithin 0.0001) 2.0f
    st.Accel.Y |> should (equalWithin 0.0001) -3.0f

  [<Test>]
  member _.``accel 省略軸は、既存の加速度を term で減衰させる``() =
    // 省略された軸は None のまま届き、Step.accel が "0" として扱うので、
    // catch-all の計算が働く：(0 - currentAccel) / term。
    // これは現状維持ではなく減衰である。
    // 初回フレームで dx = (0 - 10) / 5 = -2 を計算。以降キャッシュを再利用する。
    // 初期 Accel.X = 10.0f、accel absolute なし (省略)、term 5
    // 1 回め: dx = -2、Accel.X = 10 - 2 = 8
    // 2 回め: dx = -2、Accel.X = 8 - 2 = 6
    // 3 回め: dx = -2、Accel.X = 6 - 2 = 4
    // 4 回め: dx = -2、Accel.X = 4 - 2 = 2
    // 5 回め: dx = -2、Accel.X = 2 - 2 = 0
    let script =
      Action.Accel (None,
                         None,
                         Term (numExpr "5"))
    let mutable p = Progress.initial script
    let mutable st = { state with Accel = { X = 10.0f; Y = 0.0f } }
    for _ in 1 .. 5 do
      let (_, p'), st', _ = Sim.run env st (stepAccel script p)
      p <- p'
      st <- st'
    // 減衰で 10 が 0 に下がっているはず。現状維持なら 10.0 のまま
    st.Accel.X |> should (equalWithin 0.0001) 0.0f

  [<Test>]
  member _.``accel 省略軸は getValue を通す（乱数ストリーム）``() =
    // getValue は式の中身に関わらず env.Rand () を呼ぶ。
    // 省略軸でも旧と同じ数だけ乱数を消費しないと、その後の値がずれる。
    // 初回フレームで term + horizontal + vertical = 3 回 getValue が呼ばれる。
    // Rand を呼ぶたびカウンタを増やす env を渡し、期待値と一致するか見る。
    let mutable randCount = 0
    let countingEnv =
      { Rand = (fun () -> randCount <- randCount + 1; 0.5f)
        Rank = 0.5f
        AimDir = 0.f
        EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }

    let script =
      Action.Accel (None,
                         None,
                         Term (numExpr "2"))
    let p = Progress.initial script
    let _ = Sim.run countingEnv state (stepAccel script p)
    // 初回フレーム：term ("2") + horizontal (None -> getValue "0") + vertical (None -> getValue "0")
    // = getValue env "2" + getValue env "0" + getValue env "0" = 3 回
    randCount |> should equal 3

  [<Test>]
  member _.``changeDirection は、最後の 1 回も加算してから終わる``() =
    // absolute 90 度、term 2。現在 0 なので差は π/2。2 で割って π/4 ずつ。
    //   1 回め  term 2 -> 1   Dir = π/4   Continue
    //   2 回め  term 1 -> 0   Dir = π/2   Ended（加算してから）
    let script =
      Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"),
                                   Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    let mutable rs = []
    for _ in 1 .. 2 do
      let (r, p'), st', _ = Sim.run env st (stepChangeDirection script p)
      p <- p'
      st <- st'
      rs <- rs @ [ r ]
    rs |> should equal [ Step.Continue; Step.Ended ]
    st.Dir |> should (equalWithin 0.0001) (float32 (System.Math.PI / 2.0))

  [<Test>]
  member _.``changeSpeed は、最後の 1 回も加算してから終わる``() =
    // absolute 3、term 2、現在 1。差は 2。2 で割って 1 ずつ。
    let script =
      Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, numExpr "3"), Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    let mutable rs = []
    for _ in 1 .. 2 do
      let (r, p'), st', _ = Sim.run env st (stepChangeSpeed script p)
      p <- p'
      st <- st'
      rs <- rs @ [ r ]
    rs |> should equal [ Step.Continue; Step.Ended ]
    st.Speed |> should (equalWithin 0.0001) 3.0f

  [<Test>]
  member _.``changeSpeed sequence は、毎フレーム同じ量を足す``() =
    // sequence は term で割らない
    let script =
      Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Sequence }, numExpr "0.5"), Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    for _ in 1 .. 2 do
      let (_, p'), st', _ = Sim.run env st (stepChangeSpeed script p)
      p <- p'
      st <- st'
    st.Speed |> should (equalWithin 0.0001) 2.0f

  [<Test>]
  member _.``accel と changeDirection の終わり方は非対称``() =
    // 同じ term 1 でも、accel は 2 回めで加算せず終わり、
    // changeDirection は 1 回めで加算して終わる
    let a =
      Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "1")),
                         None, Term (numExpr "1"))
    let d =
      Action.ChangeDirection (Direction (Some { directionType = DirectionType.Relative }, numExpr "10"),
                                   Term (numExpr "1"))
    let (ra, _), sa, _ = Sim.run env state (stepAccel a (Progress.initial a))
    let (rd, _), sd, _ = Sim.run env state (stepChangeDirection d (Progress.initial d))
    ra |> should equal Step.Continue
    sa.Accel.X |> should (equalWithin 0.0001) 1.0f
    rd |> should equal Step.Ended
    sd.Dir |> should not' (equal 0.0f)

  [<Test>]
  member _.``changeDirection の getValue 回数は旧と同じ（初回 2 回、終わりでさらに 1 回）``() =
    // 旧 changeDirection: first の枝に入ると term (initTerm) + directionValue で
    // 2 回。value は type で分岐する前に計算しているので、sequence でも absolute でも
    // 同じ 2 回になる（型で分岐が変わるのは fold の有無だけ）。
    // term <= 0 で終わるフレームは、term を戻すためにさらに 1 回 getValue を呼ぶ。
    // term 2 の script なら
    //   1 回め  first の 2 回                     累計 2   Continue
    //   2 回め  first は素通り、終わりの戻しで 1 回   累計 3   Ended
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script =
      Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"),
                                   Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    let (r1, p1), st1, _ = Sim.run counting st (stepChangeDirection script p)
    p <- p1
    st <- st1
    r1 |> should equal Step.Continue
    draws |> should equal 2
    let (r2, _), _, _ = Sim.run counting st (stepChangeDirection script p)
    r2 |> should equal Step.Ended
    draws |> should equal 3

  [<Test>]
  member _.``changeSpeed の getValue 回数は旧と同じ（初回 2 回、終わりでさらに 1 回）``() =
    // changeSpeed も changeDirection と同じ形。speedValue の getValue は
    // type ごとの分岐の中に 1 回ずつあるだけなので、枝によらず 1 回。
    // term 2 の script なら
    //   1 回め  first の 2 回（initTerm + speedValue）    累計 2   Continue
    //   2 回め  first は素通り、終わりの戻しで 1 回        累計 3   Ended
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script =
      Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, numExpr "3"), Term (numExpr "2"))
    let mutable p = Progress.initial script
    let mutable st = state
    let (r1, p1), st1, _ = Sim.run counting st (stepChangeSpeed script p)
    p <- p1
    st <- st1
    r1 |> should equal Step.Continue
    draws |> should equal 2
    let (r2, _), _, _ = Sim.run counting st (stepChangeSpeed script p)
    r2 |> should equal Step.Ended
    draws |> should equal 3

  // ------------------------------------------------------------------
  // action の走査。旧の actionCommand を写す
  // ------------------------------------------------------------------

  [<Test>]
  member _.``action は、Stopped で走査を止める``() =
    // wait 2 の後ろに vanish。1 回めは wait で止まるので vanish は出ない
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Wait (numExpr "2"); Action.Vanish ])
    let p = Progress.initial script
    let (r, _, _), _, w = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    r |> should equal Step.Stopped
    w |> should be Empty

  [<Test>]
  member _.``action は、Continue では走査を止めない``() =
    // accel（Continue を返す）の後ろに vanish。同じフレームで vanish まで届く
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "1")),
                                               None, Term (numExpr "5"))
                            Action.Vanish ])
    let p = Progress.initial script
    let (r, _, _), _, w = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    w |> should equal [ Vanished ]
    r |> should equal Step.Continue

  [<Test>]
  member _.``終わった命令は次のフレームで飛ばす``() =
    // vanish の後ろに wait 1。1 回めで vanish が出て wait で止まる。
    // 2 回めは vanish を飛ばして wait だけ進むので、効果は増えない
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Vanish; Action.Wait (numExpr "1") ])
    let mutable p = Progress.initial script
    let (_, p1, _), _, w1 = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    let (_, _, _), _, w2 = Sim.run env state (stepAction noResolvers script p1 FireContext.zero)
    w1 |> should equal [ Vanished ]
    w2 |> should be Empty

  [<Test>]
  member _.``全部 終わったら Ended``() =
    let script = Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ Action.Vanish ])
    let p = Progress.initial script
    let (r, _, _), _, _ = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    r |> should equal Step.Ended

  [<Test>]
  member _.``action は FireContext をそのまま素通しする``() =
    // action の走査自体は sequence を積まない（fire だけが FireContext を書き換える）。
    // 中身が wait / vanish だけなら、渡した fc がそのまま返ってくるはず
    let script = Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ Action.Vanish ])
    let p = Progress.initial script
    let fc = { FireContext.zero with SrcDir = 1.5f; SrcSpeed = 2.5f; SpeedInit = true }
    let (_, _, fc'), _, _ = Sim.run env state (stepAction noResolvers script p fc)
    fc' |> should equal fc

  [<Test>]
  member _.``action の走査そのものは getValue を呼ばない``() =
    // 走査（isDone / setDone / 振り分け）は式を評価しない。乱数を消費するのは
    // 中の命令（wait の term や accel の値）を評価するときだけ
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script = Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ Action.Vanish ])
    let p = Progress.initial script
    Sim.run counting state (stepAction noResolvers script p FireContext.zero) |> ignore
    draws |> should equal 0

  // ------------------------------------------------------------------
  // actionRef（輪を解いた並び）。展開していない actionRef は Progress.initial で
  // PNoop になる（自分は状態を持たないため）。輪を解いた並びは、actionRef
  // 自身ではなく親の action の PAction.loop が持つ
  // ------------------------------------------------------------------

  [<Test>]
  member _.``actionRef は 1 段だけ解いて、残りの兄弟を繋いで loop にする``() =
    // wait 0（即終わる） の後ろに actionRef、そのまた後ろに vanish。
    // 1 回めのフレームで wait が終わり、actionRef が解けて loop に積まれ、
    // 走査はそこで止まる（vanish はまだ出ない）。済んだ手前（wait と actionRef 自身）は
    // 捨てるので、loop は「解いた中身 + vanish」の 2 要素になる
    let referenced = ActionElm.Action ({ actionLabel = Some (ActionLabel "sub") }, [ Action.Wait (numExpr "5") ])
    let resolvers : Step.Resolvers =
      { Bullet = fun _ _ -> None
        Action = fun label _ -> if label = ActionLabel "sub" then Some referenced else None }
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Wait (numExpr "0")
                            Action.ActionRef ({ actionRefLabel = ActionLabel "sub" }, [])
                            Action.Vanish ])
    let p = Progress.initial script
    let (r, p', _), _, w = Sim.run env state (stepAction resolvers script p FireContext.zero)
    r |> should equal Step.Stopped
    w |> should be Empty
    match p' with
    | PAction (false, Some loop, progs) ->
        loop |> should equal [ Action.Wait (numExpr "5"); Action.Vanish ]
        List.length progs |> should equal 2
    | other -> Assert.Fail (sprintf "PAction (false, Some _, _) のはずが %A" other)

  /// final review 5: 5 つめの draw site（設計文書 5.3 参照）。
  ///
  /// 旧 expandActionRefOnce は、輪を
  /// 1 段 解いた瞬間に展開した中身の wait をまとめて引いていた。actionRef は
  /// このあと Init 相当を挟まないので、ここで引いた値がそのまま最終値になる。
  /// Progress.initial のまま組むと、この 1 回ぶんの乱数消費が丸ごと欠け、
  /// term の評価も「まだ評価前（started = false）」の状態に取り違わる
  [<Test>]
  member _.``actionRef を解いた瞬間に、展開した中身の wait をまとめて引く``() =
    let referenced = ActionElm.Action ({ actionLabel = Some (ActionLabel "sub") }, [ Action.Wait (numExpr "5") ])
    let resolvers : Step.Resolvers =
      { Bullet = fun _ _ -> None
        Action = fun label _ -> if label = ActionLabel "sub" then Some referenced else None }
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.ActionRef ({ actionRefLabel = ActionLabel "sub" }, []) ])
    let p = Progress.initial script
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let (_, p', _), _, _ = Sim.run counting state (stepAction resolvers script p FireContext.zero)
    draws |> should equal 1
    match p' with
    | PAction (false, Some _, [ PWait (started, left) ]) ->
        // started = true（もう評価済み）でなければ、次のフレームでもう一度
        // 評価してしまい、term を消費した回数がずれる
        started |> should equal true
        left |> should (equalWithin 0.0001) 5.0f
    | other -> Assert.Fail (sprintf "PAction (false, Some _, [ PWait _ ]) のはずが %A" other)

  [<Test>]
  member _.``自己参照の actionRef は毎フレーム 1 段ずつ解け続け、並びは伸びない``() =
    // 自己参照 actionRef はパーサが展開せず残す（輪を作るための意図的な仕様）。
    // 輪を解くたびに同じ形の並びへ差し替わるだけなので、並びの長さは伸びない
    let body =
      ActionElm.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Vanish; Action.ActionRef ({ actionRefLabel = ActionLabel "top" }, []) ])
    let resolvers : Step.Resolvers =
      { Bullet = fun _ _ -> None
        Action = fun label _ -> if label = ActionLabel "top" then Some body else None }
    let mutable p = Progress.initialActionElm body
    for i in 1 .. 3 do
      let (r, p', _), _, w = Sim.run env state (Step.actionElm resolvers body p FireContext.zero)
      p <- p'
      r |> should equal Step.Stopped
      w |> should equal [ Vanished ]
      match p' with
      | PAction (false, Some loop, progs) ->
          List.length loop |> should equal 2
          List.length progs |> should equal 2
      | other -> Assert.Fail (sprintf "%d 回め: PAction (false, Some _, _) のはずが %A" i other)

  [<Test>]
  member _.``解決できない actionRef は、走査を止めずに後ろの命令へ進む``() =
    // ラベルが見つからない actionRef はその場では何も起きない（PNoop のまま）。
    // Stop も Continue も立てないので、後ろの vanish は同じフレームで出る
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.ActionRef ({ actionRefLabel = ActionLabel "missing" }, [])
                            Action.Vanish ])
    let p = Progress.initial script
    let (r, p', _), _, w = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    w |> should equal [ Vanished ]
    r |> should equal Step.Ended
    match p' with
    | PAction (true, None, [ PNoop; PVanish true ]) -> ()
    | other -> Assert.Fail (sprintf "予期しない Progress: %A" other)

  // ------------------------------------------------------------------
  // command の振り分け
  // ------------------------------------------------------------------

  [<Test>]
  member _.``command は、Action を action へ振り分ける``() =
    let script = Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ Action.Vanish ])
    let p = Progress.initial script
    let viaCommand, _, wc = Sim.run env state (Step.command noResolvers script p FireContext.zero)
    let viaAction, _, wa = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    viaCommand |> should equal viaAction
    wc |> should equal wa

  [<Test>]
  member _.``command は、Repeat を repeat へ振り分ける``() =
    let body = ActionElm.Action ({ actionLabel = None }, [ Action.Vanish ])
    let script = Action.Repeat (Times (numExpr "1"), body)
    let p = Progress.initial script
    let viaCommand, _, wc = Sim.run env state (Step.command noResolvers script p FireContext.zero)
    let viaRepeat, _, wr = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    viaCommand |> should equal viaRepeat
    wc |> should equal wr

  [<Test>]
  member _.``command は、まだ振り分け先の無い命令は Ended を返し fc を素通しする``() =
    // 振り分け先の無い命令は触らずに Ended で返す。
    // 以前はここに NotCommand を置いていたが、型から消えた。
    // いま「振り分け先が無い」のは展開していない参照（fireRef / actionRef）だけ
    let script = Action.FireRef ({ fireRefLabel = FireLabel "none" }, [])
    let p = PNoop
    let fc = { FireContext.zero with SrcDir = 3.0f }
    let (r, p', fc'), _, w = Sim.run env state (Step.command noResolvers script p fc)
    r |> should equal Step.Ended
    p' |> should equal PNoop
    fc' |> should equal fc
    w |> should be Empty

  [<Test>]
  member _.``command は、5 つの既存の命令には fc をそのまま返す``() =
    let script = Action.Vanish
    let p = Progress.initial script
    let fc = { FireContext.zero with SrcSpeed = 4.0f; SpeedInit = true }
    let (_, _, fc'), _, _ = Sim.run env state (Step.command noResolvers script p fc)
    fc' |> should equal fc

  // ------------------------------------------------------------------
  // 走査の中で changeDirection / changeSpeed を使う穴。isDone が countdown の
  // リセット（repeat の次周のための term の戻し）に惑わされないことを見る
  // ------------------------------------------------------------------

  [<Test>]
  member _.``action の中の changeDirection は、term が尽きたら再適用しない``() =
    // absolute 90 度、term 2。後ろに wait 5 を置いて、changeDirection が終わった
    // 後も走査が長く続くようにする（wait 5 は 6 フレーム目まで Stopped で
    // 引っ張り続ける）。changeDirection は 2 フレームで終わるはずで、
    // 3 フレーム目以降は再実行されず delta も足され直さないはず。
    //
    // changeDirection は終わるフレームで term を getValue initTerm へ戻す
    // （repeat の次周のため）。この戻した正の値を isDone が「まだ途中」と
    // 読み違えると、3 フレーム目以降も delta を足し続けてしまう
    // （壊れていれば 6 回ぶん = 3π/2、正しければ 2 回ぶん = π/2）
    let changeDir =
      Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"), Term (numExpr "2"))
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ changeDir; Action.Wait (numExpr "5") ])
    let mutable p = Progress.initial script
    let mutable st = state
    for _ in 1 .. 6 do
      let (_, p', _), st', _ = Sim.run env st (stepAction noResolvers script p FireContext.zero)
      p <- p'
      st <- st'
    st.Dir |> should (equalWithin 0.0001) (float32 (System.Math.PI / 2.0))

  [<Test>]
  member _.``action の中の changeSpeed も、term が尽きたら再適用しない``() =
    // changeDirection と同じ形の穴が changeSpeed 側にも無いことを見る。
    // absolute 3、term 2、後ろに wait 5。壊れていれば速さが足され続ける
    let changeSpd =
      Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, numExpr "3"), Term (numExpr "2"))
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") }, [ changeSpd; Action.Wait (numExpr "5") ])
    let mutable p = Progress.initial script
    let mutable st = state
    for _ in 1 .. 6 do
      let (_, p', _), st', _ = Sim.run env st (stepAction noResolvers script p FireContext.zero)
      p <- p'
      st <- st'
    st.Speed |> should (equalWithin 0.0001) 3.0f

  [<Test>]
  member _.``Continue の後ろで Stopped が出ても、全体としては Stopped が勝つ``() =
    // 1 つめが Continue（accel）、2 つめが Stopped（wait）。優先順位は
    // Stop > Continue > End なので、全体の結果は Stopped でなければならない
    let script =
      Action.Action ({ actionLabel = Some (ActionLabel "top") },
                          [ Action.Accel (Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "1")),
                                               None, Term (numExpr "5"))
                            Action.Wait (numExpr "2") ])
    let p = Progress.initial script
    let (r, _, _), _, _ = Sim.run env state (stepAction noResolvers script p FireContext.zero)
    r |> should equal Step.Stopped

  // ------------------------------------------------------------------
  // repeat。旧の repeatCommand を写す
  // ------------------------------------------------------------------

  [<Test>]
  member _.``repeat 3 は、子を 3 回 走らせる``() =
    let body = ActionElm.Action ({ actionLabel = None }, [ Action.Vanish ])
    let script = Action.Repeat (Times (numExpr "3"), body)
    let p = Progress.initial script
    let (r, _, _), _, w = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    List.length w |> should equal 3
    r |> should equal Step.Ended

  // fix round 1, item 2: effects <- effects @ w を ResizeArray へ書き換えたとき、
  // 並びを保つ門が無かった。この 2 本（3 回 という数、Vanish Vanish という
  // 同じ効果 2 つ）は逆順に積んでも構造的等価性ではすり抜ける。
  // sequence な speed は周を追うごとに積み上がるので、周ごとに違う値の弾が
  // 生まれる —— 並びが逆転すれば速さの並びも逆転するので、識別できる
  [<Test>]
  member _.``repeat の中の fire は、周の順のまま効果に積まれる（同じ効果 2 つでは見えない並び）``() =
    let bullet = BulletElm.Bullet ({ bulletLabel = None }, None, None, [])
    let fire =
      Action.Fire ({ fireLabel = None }, None,
                        Some (Speed (Some { speedType = SpeedType.Sequence }, numExpr "1")),
                        bullet)
    let body = ActionElm.Action ({ actionLabel = None }, [ fire ])
    let script = Action.Repeat (Times (numExpr "3"), body)
    let p = Progress.initial script
    let (_, _, _), _, w = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    let speeds = w |> List.map (function Spawn b -> b.Speed | Vanished -> -1.0f)
    speeds |> should equal [ 1.0f; 2.0f; 3.0f ]

  [<Test>]
  member _.``repeat の times は式が書ける。7 割る 2 は 3``() =
    let body = ActionElm.Action ({ actionLabel = None }, [ Action.Vanish ])
    let script = Action.Repeat (Times (numExpr "7/2"), body)
    let p = Progress.initial script
    let (_, _, _), _, w = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    List.length w |> should equal 3

  [<Test>]
  member _.``repeat の中の wait は、周をまたいで止める``() =
    // wait 1 を 2 回。1 フレームめは 1 周めの wait で止まる
    let body = ActionElm.Action ({ actionLabel = None }, [ Action.Wait (numExpr "1") ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let p = Progress.initial script
    let (r, _, _), _, _ = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    r |> should equal Step.Stopped

  [<Test>]
  member _.``times が 0 でも止まらない``() =
    // 旧の癖。while に入らないだけで Ended は返る
    let body = ActionElm.Action ({ actionLabel = None }, [ Action.Vanish ])
    let script = Action.Repeat (Times (numExpr "0"), body)
    let p = Progress.initial script
    let (r, _, _), _, w = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    r |> should equal Step.Ended
    w |> should be Empty

  [<Test>]
  member _.``repeat の 2 周目でも changeSpeed が同じだけ効く``() =
    // 「終わりに term を初期値へ戻すのは repeat の 2 周目で効く」を見る門。
    // 戻しを消すと 2 周目の term が 0 のままになり、速さの増え方が変わる
    let body =
      ActionElm.Action ({ actionLabel = None },
                          [ Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Relative }, numExpr "2"), Term (numExpr "2"))
                            Action.Wait (numExpr "1") ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let mutable p = Progress.initial script
    let mutable st = state
    // 1 周が changeSpeed 2 フレーム ＋ wait 1 フレームなので、2 周ぶん回す
    for _ in 1 .. 8 do
      let (_, p', _), st', _ = Sim.run env st (stepRepeat noResolvers script p FireContext.zero)
      p <- p'
      st <- st'
    // relative 2 を term 2 で割って 1 ずつ、それが 2 周ぶん。1 + 2 + 2 = 5
    st.Speed |> should (equalWithin 0.0001) 5.0f

  [<Test>]
  member _.``repeat の周ざかいは、旧の running の Init 二重引きと同じ回数だけ乱数を引く``() =
    // changeDirection だけの body、times 2、途中に止める wait は無い。
    // 旧は times を呼ぶたびに引き直し、周ざかりでは changeDirection 自身の
    // 終わり分岐（term を戻す）に加えて running |> Seq.iter Init がもう一度
    // term を引き直す（その値は次の周の first で上書きされて捨てられる）。
    // Progress.initial は乱数を引かないので、ここを肩代わりしないと
    // 乱数列が旧より 1 回ぶん前へずれる。
    //   1 回め  times(1) + changeDirection 開始(2)                     = 3   Continue
    //   2 回め  times(1) + 終わり(1) + 周ざかりの捨て引き(1)
    //           + 次の周の開始(2)                                      = 5   Continue（累計 8）
    //   3 回め  times(1) + 終わり(1)                                   = 2   Ended（累計 10）
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let body =
      ActionElm.Action ({ actionLabel = None },
                          [ Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"), Term (numExpr "2")) ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let mutable p = Progress.initial script
    let mutable st = state
    let (r1, p1, _), st1, _ = Sim.run counting st (stepRepeat noResolvers script p FireContext.zero)
    p <- p1
    st <- st1
    r1 |> should equal Step.Continue
    draws |> should equal 3
    let (r2, p2, _), st2, _ = Sim.run counting st (stepRepeat noResolvers script p FireContext.zero)
    p <- p2
    st <- st2
    r2 |> should equal Step.Continue
    draws |> should equal 8
    let (r3, _, _), _, _ = Sim.run counting st (stepRepeat noResolvers script p FireContext.zero)
    r3 |> should equal Step.Ended
    draws |> should equal 10

  // ------------------------------------------------------------------
  // fix round 1: repeat の周ざかいの reset に見つかった 3 つの不具合
  // ------------------------------------------------------------------

  [<Test>]
  member _.``周ざかいの reset は wait を含めて 1 つの並びを順に引き、その値がそのまま次の周の wait に入る``() =
    // changeDirection（term 1 で固定、単発で終わる）の後ろに wait "$rand"。
    // 旧の running |> Seq.iter Init は、この並びを順番に 1 回で辿って
    // 引く（changeDirection は捨てる引き、wait は使う引き）。
    // wait の引きだけ次の実際の開始まで遅延させると、同じ乱数列でも
    // wait が引く物理位置がずれ、2 周目の wait の初期値が変わる。
    // 定数式では見えないので、毎回ちがう値を返す Rand で可視化する。
    //
    // Ended か Stopped かという終わり方だけを見る門は、たまたま終わり方が
    // 一致してしまう入れ替わり（並びの順を守らずに wait を先に引く、等）を
    // 通してしまう。ここでは周ざかいで wait に入った値そのものを
    // Progress から読み戻して確かめる（下の壊し方の項を参照）。
    //
    // vals の各要素が何に対応するか（1 始まりの呼び出し回数）。
    // "1" や "90" には $rand が無いので、その回に何を返しても結果は
    // 変わらない（フィラー）
    //   1 回め  times（"2"。フィラー）
    //   2 回め  1 周めの changeDirection の term（"1"。フィラー）
    //   3 回め  1 周めの changeDirection の向き（"90"。フィラー）
    //   4 回め  1 周めの changeDirection の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    //   5 回め  1 周めの wait の開始値（"$rand"）。1 未満にして、1 回の
    //           decrement でその周のうちに Ended まで進める
    //   6 回め  周ざかいの reset で、並びの先頭（changeDirection）の
    //           term を引き直すぶん（"1"。フィラー。値は使われず捨てる）
    //   7 回め  周ざかいの reset で、並びの 2 番め（wait）に実際に入る値
    //           （"$rand"）。ここが本題。1 未満にして、2 周めの wait も
    //           その周のうちに Ended まで進める。壊れた版（wait の引きを
    //           遅延させる版）はこの回を引かず、この値は他の回へずれる
    //   8 回め  2 周めの changeDirection の term（"1"。フィラー）
    //   9 回め  2 周めの changeDirection の向き（"90"。フィラー）
    //   10 回め 2 周めの changeDirection の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    // フィラーは 0.5 で揃え、本題の 2 か所（5 回め・7 回め）だけ変えて
    // 目立たせてある
    let vals = [| 0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.3f; 0.5f; 0.5f; 1.5f |]
    let mutable n = 0
    let counting = { env with Rand = fun () -> let v = vals.[n] in n <- n + 1; v }
    let body =
      ActionElm.Action ({ actionLabel = None },
                          [ Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"), Term (numExpr "1"))
                            Action.Wait (numExpr "$rand") ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let p = Progress.initial script
    let (r, p', _), _, _ = Sim.run counting state (stepRepeat noResolvers script p FireContext.zero)
    // 終わり方も見ておく（見るのに何のコストも要らないし、値がそのまま
    // 動きに出ることの裏付けになる）
    r |> should equal Step.Ended
    // 本題: 2 周めの wait に実際入った値を読み戻す。7 回めで引いた 0.3 を
    // 1 回 decrement した -0.7 のはず。7 回めでなく 6 回め（0.5）が
    // 入っていたら -0.5 になる — 終わり方は Ended のまま変わらないので、
    // 値を読まない門ではこの入れ替わりに気づけない
    match p' with
    | PRepeat (2, true, PAction (true, None, [ PChangeDir (true, true, _, _); PWait (true, waitLeft) ])) ->
        waitLeft |> should (equalWithin 0.0001) -0.7f
    | other -> Assert.Fail (sprintf "予期しない Progress: %A" other)

  // ------------------------------------------------------------------
  // fix round 3: 値を読み戻す門が wait しか見ていなかった。changeSpeed
  // （と changeDirection）の捨て引きも同じ並びの中にあることを確かめる
  // ------------------------------------------------------------------

  [<Test>]
  member _.``周ざかいの reset は changeSpeed の捨て引きも並びの位置どおりに消費する``() =
    // changeDirection、changeSpeed、wait を 1 つの body に並べる。
    // changeDirection・changeSpeed はどちらも「引くだけ引いて値は捨てる」
    // 枝で、その値は自分の Progress には残らない（次の周の開始でまた
    // 引き直されて上書きされるため）。なので、どちらかの捨て引きが
    // 丸ごと無くなっても、changeDirection / changeSpeed 自身の Progress
    // の形（started = false, done_ = false）は変わらない。
    // 気づける先は、並びの後ろに置いた wait が実際に引く物理位置だけ
    // ——2 つぶんの捨て引きのどちらかが無くなれば、wait の引きが
    // 1 つぶん手前へずれる。ここではその wait の値を Progress から
    // 読み戻して確かめる（changeDirection / changeSpeed 側は形だけ見る。
    // 数値までは見分けが付かない理由は下の壊し方の項に書いた）。
    //
    // vals の各要素が何に対応するか（1 始まりの呼び出し回数）。
    // "1" / "90" / "5" には $rand が無いので、その回に何を返しても
    // 結果は変わらない（フィラー）
    //   1 回め  times（"2"。フィラー）
    //   2 回め  1 周めの changeDirection の term（"1"。フィラー）
    //   3 回め  1 周めの changeDirection の向き（"90"。フィラー）
    //   4 回め  1 周めの changeDirection の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    //   5 回め  1 周めの changeSpeed の term（"1"。フィラー）
    //   6 回め  1 周めの changeSpeed の速さ（"5"。フィラー）
    //   7 回め  1 周めの changeSpeed の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    //   8 回め  1 周めの wait の開始値（"$rand"）。1 未満にして、1 回の
    //           decrement でその周のうちに Ended まで進める
    //   9 回め  周ざかいの reset で、並びの 1 番め（changeDirection）の
    //           term を引き直すぶん（"1"。フィラー。値は捨てる）
    //   10 回め 周ざかいの reset で、並びの 2 番め（changeSpeed）の
    //           term を引き直すぶん（"1"。フィラー。値は捨てる）。
    //           changeSpeed の捨て引きが丸ごと消えると、次の 11 回めの
    //           はずの wait の値がここへ繰り上がる —— この回だけ、
    //           フィラーでも壊れ方が見える値（1.3）にしてある
    //   11 回め 周ざかいの reset で、並びの 3 番め（wait）に実際に入る値
    //           （"$rand"）。ここが本題（1.7）
    //   12 回め 2 周めの changeDirection の term（"1"。フィラー）
    //   13 回め 2 周めの changeDirection の向き（"90"。フィラー）
    //   14 回め 2 周めの changeDirection の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    //   15 回め 2 周めの changeSpeed の term（"1"。フィラー）
    //   16 回め 2 周めの changeSpeed の速さ（"5"。フィラー）
    //   17 回め 2 周めの changeSpeed の終わり分岐の term 引き直し
    //           （"1"。フィラー）
    // 2 周めの wait は 11 回めの値をそのまま使う（started = true なので
    // 引き直さない）。11 回め (1.7) を 1 回 decrement して 0.7、
    // 0.7 >= 0 なのでこの周のうちには終わらず Stopped で止まる
    let vals =
      [| 0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.4f
         0.5f; 1.3f; 1.7f
         0.5f; 0.5f; 0.5f; 0.5f; 0.5f; 0.5f |]
    let mutable n = 0
    let counting = { env with Rand = fun () -> let v = vals.[n] in n <- n + 1; v }
    let body =
      ActionElm.Action ({ actionLabel = None },
                          [ Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"), Term (numExpr "1"))
                            Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, numExpr "5"), Term (numExpr "1"))
                            Action.Wait (numExpr "$rand") ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let p = Progress.initial script
    let (r, p', _), _, _ = Sim.run counting state (stepRepeat noResolvers script p FireContext.zero)
    r |> should equal Step.Stopped
    // 本題: 2 周めの wait に実際入った値を読み戻す。11 回めで引いた 1.7 を
    // 1 回 decrement した 0.7 のはず。changeSpeed の捨て引き（10 回め）が
    // 無いと、この値は 10 回め相当（1.3）が繰り上がって入り、
    // decrement 後は 0.3 になる —— 終わり方はどちらも Stopped のままなので、
    // 値を読まない門ではこの入れ替わりに気づけない（壊し方の項を参照）。
    // changeDirection / changeSpeed 側は started / done_ の形だけ見る。
    // term / delta は自分の Progress に残らない（次の周の開始で上書きされる）
    // ので、そこに捨て引きの有無の証拠は残らない
    match p' with
    | PRepeat (1, false,
               PAction (false, None,
                        [ PChangeDir (true, true, _, _)
                          PChangeSpeed (true, true, _, _)
                          PWait (true, waitLeft) ])) ->
        waitLeft |> should (equalWithin 0.0001) 0.7f
    | other -> Assert.Fail (sprintf "予期しない Progress: %A" other)

  [<Test>]
  member _.``周ざかいの reset は fire の中の bullet の action にも潜って引く``() =
    // fire の中の bullet が持つ action に wait を仕込む。旧の Init は
    // Fire(pf,children) -> children.Init(env) で bullet を、
    // Bullet(...,actions) -> actions |> Seq.iter Init でその action を
    // 辿るので、撃たれるかどうかに関わらず repeat の周ざかりのたびに
    // この wait の term を引く。
    //
    // stepFire が command に繋がった今は、実際に撃つときの引き
    // （stepFire 自身が resetChild で撃たれた弾の Tops を組むぶん）も乗る。
    // times "2"、body は fire の後ろに vanish の 1 本だけなので、
    // 1 回の呼び出しの中で 2 周とも終わりまで進む：
    //   times                                          1 回
    //   1 周め: fire が実際に撃つ（wait の term）          1 回
    //   周ざかりの reset（撃ったかどうかに関わらず引く）    1 回
    //   2 周め: fire が実際に撃つ（wait の term）          1 回
    // 計 4 回。撃つたびの引きと周ざかりの引きは別の場所（前者は stepFire、
    // 後者は resetChild）が別の理由で行っており、どちらも旧の Init 呼び出しに
    // 対応するので、両方が乗って良い（二重に引いているわけではない）
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let bullet =
      BulletElm.Bullet ({ bulletLabel = None }, None, None,
                          [ ActionElm.Action ({ actionLabel = None }, [ Action.Wait (numExpr "3") ]) ])
    let fire = Action.Fire ({ fireLabel = None }, None, None, bullet)
    let body = ActionElm.Action ({ actionLabel = None }, [ fire; Action.Vanish ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let p = Progress.initial script
    let _ = Sim.run counting state (stepRepeat noResolvers script p FireContext.zero)
    draws |> should equal 4

  [<Test>]
  member _.``周ざかいの reset は、解けた actionRef の並びを次の周へも持ち越す``() =
    // 旧は running |> Seq.iter Init が running（pa.loop があればそちら）の
    // 要素ごとに Init を呼ぶだけで、actionElm 自身の pa.loop には触らない。
    // なので一度解けた並びはその後の周でも持ち越り、actionRef を解き直さない。
    // Progress.initial で毎周 loop を None に戻すと、静的な並び（まだ
    // actionRef のまま）に戻ってしまい、2 周目にもう一度解いて Stopped の
    // ぶんだけ余計に足踏みする
    let resolvers : Step.Resolvers =
      { Bullet = fun _ _ -> None
        Action = fun label _ ->
          if label = ActionLabel "ref" then Some (ActionElm.Action ({ actionLabel = None }, [ Action.Vanish ]))
          else None }
    let body =
      ActionElm.Action ({ actionLabel = Some (ActionLabel "top") }, [ Action.ActionRef ({ actionRefLabel = ActionLabel "ref" }, []) ])
    let script = Action.Repeat (Times (numExpr "2"), body)
    let mutable p = Progress.initial script
    let mutable st = state
    // 1 回め: actionRef を解くだけで Stopped。まだ Vanish は出ない
    let (r1, p1, _), st1, w1 = Sim.run env st (stepRepeat resolvers script p FireContext.zero)
    p <- p1
    st <- st1
    r1 |> should equal Step.Stopped
    w1 |> should be Empty
    // 2 回め: 解いた並び（Vanish だけ）を持ち越していれば、actionRef を
    // 再び解かずに 2 周とも終わり、この 1 回で Ended になる
    let (r2, _, _), _, w2 = Sim.run env st (stepRepeat resolvers script p FireContext.zero)
    r2 |> should equal Step.Ended
    w2 |> should equal [ Vanished; Vanished ]

  // ------------------------------------------------------------------
  // fix round 2, residual 2: repeat の子が Action でない形。
  //
  // DTD は repeat (times, (action | actionRef)) で actionRef も許す。
  // パーサ（IntermediateParser.convertRefBulletmlIn）は actionRef を
  // 1 段展開して実体の Action へ差し替えるが、自己参照（輪）だけは
  // 展開せずに残す。repeat の直下が展開されずに actionRef のまま残るのは、
  // その actionRef が自分を直接包む action への自己参照であるとき
  // （例: action "top" の中に、times が届く repeat が直接 actionRef "top"
  // を子に持つ）。旧の repeatCommand はこの形に実際に到達し、
  // while の中の `match actionElm with Action(pa,tasks) -> ... | _ ->
  // failwith "repeatCommand: repeat の子が action ではない"` で落ちる。
  // 黙らせて何もしないと、equivalence の橋が「片方だけ例外」を割れとして
  // 拾ったときに、原因が見えている場所（ここ）でなく橋の側から
  // 逆側を辿ることになる
  // ------------------------------------------------------------------

  [<Test>]
  member _.``repeat の子が Action でないと、旧と同じ例外で落ちる``() =
    // 自己参照で展開されずに残った actionRef を、repeat の直下にそのまま置く
    let script = Action.Repeat (Times (numExpr "2"), ActionElm.ActionRef ({ actionRefLabel = ActionLabel "top" }, []))
    let p = Progress.initial script
    let ex =
      Assert.Throws<System.Exception>(fun () ->
        Sim.run env state (stepRepeat noResolvers script p FireContext.zero) |> ignore)
    ex.Message |> should equal "repeatCommand: repeat の子が action ではない"

  /// 撃たれた弾の action に、輪で解けなかった actionRef がそのまま残ることがある
  /// （bullet の中の自己参照）。その台本を Step.step が回すと actionElm の
  /// actionRef の腕へ入る。同梱の 227 本 では踏まないので、ここで直に押さえる。
  ///
  /// 台本まるごとを渡していた頃は action が children = [] で走り、
  /// Ended と PAction (true, None, []) を返していた。ほどいて渡す形に
  /// 変えたあとも同じ値を返すことを固定する。**Ended だけを見ては足りない**
  /// —— PNoop を返す実装でも Ended になるが、PNoop は isDone が false なので
  /// 呼ぶ側の「終わったか」の判定が変わる
  [<Test>]
  member _.``解けなかった actionRef を台本として回すと、子が空の action として終わる``() =
    let script = ActionElm.ActionRef ({ actionRefLabel = ActionLabel "unresolved" }, [])
    let p = Progress.initialActionElm script
    p |> should equal PNoop
    let (r, p', fc'), _, w = Sim.run env state (Step.actionElm noResolvers script p FireContext.zero)
    r |> should equal Step.Ended
    p' |> should equal (PAction (true, None, []))
    fc' |> should equal FireContext.zero
    w |> should be Empty

  [<Test>]
  member _.``times が 0 なら、子が Action でなくても while に入らず落ちない``() =
    // 旧は times を while の外で引くが、while の中でしか actionElm を
    // 見ない。times に 1 度も届かなければ（times = 0、または num0 が
    // 既に times 以上）match そのものへ到達しないので落ちない
    let script = Action.Repeat (Times (numExpr "0"), ActionElm.ActionRef ({ actionRefLabel = ActionLabel "top" }, []))
    let p = Progress.initial script
    let (r, _, _), _, _ = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    r |> should equal Step.Ended

  // ------------------------------------------------------------------
  // final review 1: 大きな times が simForTests { } の while を通ってクラッシュする
  // ------------------------------------------------------------------

  [<Test>]
  member _.``repeat の times が 9999 でも、末尾再帰でない再帰を積まずに走り切る``() =
    // 旧 BulletRunner.repeatCommand の while は 1 周が定数のスタックで
    // 済む（4077ed6 の repeatCommand 参照）。stepRepeat も見た目は
    // 同じ手続きループへ書き換えてあるが、その while が simForTests { } の
    // ブロックの中に書かれていると、コンパイラが builder.While へ
    // 書き換えてしまう。SimBuilder.While は
    //   guard() が真なら Sim.bindForTests (fun () -> While(guard,body)) (body())
    // で、1 周につき Sim.bindForTests を 1 段 積む再帰（末尾再帰ではない）。
    // times が万のオーダーだとここで StackOverflow する
    // （$"[G_DARIUS]_homing_laser.xml" のような実物にも times=9999 が
    // あるが、corpus は全部 body に wait を持つので 1 コマに 1 周しか
    // 進まず、この不具合を誰も踏んでいなかった）。
    //
    // wait を置かず、1 回の呼び出しで times ぶん全部を回し切らせる
    let bullet = BulletElm.Bullet ({ bulletLabel = None }, None, None, [])
    let fire = Action.Fire ({ fireLabel = None }, None, None, bullet)
    let body = ActionElm.Action ({ actionLabel = None }, [ fire ])
    let script = Action.Repeat (Times (numExpr "9999"), body)
    let p = Progress.initial script
    let (r, _, _), _, w = Sim.run env state (stepRepeat noResolvers script p FireContext.zero)
    r |> should equal Step.Ended
    List.length w |> should equal 9999
