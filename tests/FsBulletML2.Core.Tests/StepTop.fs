namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.DTD
open FsBulletML2.Domain

/// top* の走査と、1 コマの結果の組み立て。
[<TestFixture>]
type StepTop() =

  let env = { Rand = (fun () -> 0.5f); Rank = 0.5f; AimDir = 0.f; EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }

  let noResolvers : Step.Resolvers =
    { Bullet = (fun _ _ -> None); Action = fun _ _ -> None }

  let stateWith tops =
    { Pos = { X = 0.f; Y = 0.f }
      Speed = 2.f
      Dir = 0.f
      Accel = { X = 1.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = tops }

  let top children =
    let s = ActionElm.Action ({ actionLabel = Some (ActionLabel "top") }, children)
    s, Progress.initialActionElm s, FireContext.zero

  [<Test>]
  member _.``差分は、加速度と 速さ かける 向き の和``() =
    let t = top [ Action.Wait (numExpr "10") ]
    let r = Step.step noResolvers env (stateWith [ t ])
    // dir = 0 なので sin 0 = 0、-cos 0 = -1。速さ 2 なので (0, -2)。加速度 (1, 0) を足す
    r.Delta.X |> should (equalWithin 0.0001) 1.0f
    r.Delta.Y |> should (equalWithin 0.0001) -2.0f

  [<Test>]
  member _.``ある top が止まっても、後ろの top は同じフレームで回る``() =
    let t1 = top [ Action.Wait (numExpr "10") ]
    let t2 = top [ Action.Vanish ]
    let r = Step.step noResolvers env (stateWith [ t1; t2 ])
    r.Effects |> should equal [ Vanished ]

  [<Test>]
  member _.``全部の top が終わったら Finished``() =
    let t = top [ Action.Vanish ]
    let r = Step.step noResolvers env (stateWith [ t ])
    r.Finished |> should equal true

  [<Test>]
  member _.``止まっている top があるうちは Finished ではない``() =
    let t = top [ Action.Wait (numExpr "10") ]
    let r = Step.step noResolvers env (stateWith [ t ])
    r.Finished |> should equal false

  [<Test>]
  member _.``撃たれた弾で自分も撃っていれば、終わったときに回収される``() =
    let t = top [ Action.Vanish ]
    let st = { stateWith [ t ] with IsBullet = true; HasFired = true }
    let r = Step.step noResolvers env st
    r.Retired |> should equal true

  [<Test>]
  member _.``根の弾は、終わっても回収されない``() =
    let t = top [ Action.Vanish ]
    let st = { stateWith [ t ] with IsBullet = false; HasFired = true }
    let r = Step.step noResolvers env st
    r.Retired |> should equal false

  /// Sim.bindForTests は r2.Emit >> r1.Emit で合成する（Sim.fs 参照）ので、複数 top を
  /// 畳むときも並び順を取り違えやすい。前の top の効果が後ろの top より先に
  /// 出ることを、種類の違う 2 つの効果（Spawn と Vanished）で確かめる。
  /// 同じ効果 2 つ（例えば Vanish を 2 本）では、逆順に足しても結果の並びが
  /// 偶然一致してしまい、この門は働かない
  [<Test>]
  member _.``複数 top の効果は、top の並び順のまま出る``() =
    let bullet = BulletElm.Bullet ({ bulletLabel = None }, None, None, [])
    let fireTop = top [ Action.Fire ({ fireLabel = None }, None, None, bullet) ]
    let vanishTop = top [ Action.Vanish ]
    let r = Step.step noResolvers env (stateWith [ fireTop; vanishTop ])
    match r.Effects with
    | [ Spawn _; Vanished ] -> ()
    | other -> Assert.Fail (sprintf "top の並び順で出るはずが %A" other)

  /// ここまでの 6 本はどれも Step.step を 1 回しか呼ばない。fold が
  /// 返す Progress / FireContext を Tops へ書き戻さず、コマの前の値を
  /// そのまま積み直しても、1 回しか呼ばないテストにはその違いが出ない
  /// （書き戻し先を誰も読み返さないため）。前のコマの `r.State` を
  /// 実際に次の `Step.step` へ渡して、初めて書き戻しの有無が見える。
  ///
  /// wait "1" は 1 コマ目で Stopped、2 コマ目で Ended になる（stepWait 参照）。
  /// Progress が持ち越らなければ、2 コマ目も PWait(false, 0.0) から
  /// 振り出しに戻り、いつまでも Ended にならない
  [<Test>]
  member _.``Progress は次のコマへ持ち越される: wait は 2 コマ目で終わる``() =
    let t = top [ Action.Wait (numExpr "1") ]
    let r1 = Step.step noResolvers env (stateWith [ t ])
    r1.Finished |> should equal false
    let r2 = Step.step noResolvers env r1.State
    r2.Finished |> should equal true

  /// FireContext（SrcSpeed の積み上がりと SpeedInit の掛け金）も同じ
  /// Tops のスロットへ持ち越る。1 発め（bullet 側の絶対値 5 を、掛け金が
  /// まだ立っていないので latch として採用）と 2 発め（fire 側の
  /// sequence "3"）の間に wait を 1 つ挟み、2 発めが実際に「次のコマ」
  /// で走るようにしてある。
  ///
  /// Progress が持ち越らなければ wait が毎回振り出しに戻り、2 発めへは
  /// 一度も届かず 1 発めが毎コマ撃ち直される（撃たれた弾の速さは
  /// bullet 側の絶対値 5 のまま）。FireContext が持ち越らなければ
  /// 2 発めは届いても SrcSpeed の基準が 0 に戻っていて 0 + 3 = 3 になる。
  /// どちらか片方でも欠けると 8 にはならない
  [<Test>]
  member _.``FireContext は次のコマへ持ち越される: 2 発めの sequence は 1 発めの速さに積む``() =
    let bullet d s = BulletElm.Bullet ({ bulletLabel = None }, d, s, [])
    let fire1 =
      Action.Fire ({ fireLabel = None }, None,
                        None,
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "5"))))
    let fire2 =
      Action.Fire ({ fireLabel = None }, None,
                        Some (Speed (Some { speedType = SpeedType.Sequence }, numExpr "3")),
                        bullet None None)
    let t = top [ fire1; Action.Wait (numExpr "1"); fire2 ]
    let r1 = Step.step noResolvers env (stateWith [ t ])
    match r1.Effects with
    | [ Spawn b1 ] -> b1.Speed |> should (equalWithin 0.0001) 5.0f
    | other -> Assert.Fail (sprintf "1 発めの Spawn のはずが %A" other)
    let r2 = Step.step noResolvers env r1.State
    match r2.Effects with
    | [ Spawn b2 ] -> b2.Speed |> should (equalWithin 0.0001) 8.0f
    | other -> Assert.Fail (sprintf "2 発めの Spawn のはずが %A" other)

  /// final review 1: 実物で踏んだ形（top* から辿れる repeat の times=9999）を
  /// Step.step 経由（BulletRunner.run が実際に呼ぶのと同じ関数）で 1 コマ回す。
  /// StepCommands.fs の門は stepRepeat を直接見ているが、ここは top* の
  /// 走査（action → command → repeat）を経由しても壊れないことを確かめる
  [<Test>]
  member _.``top 直下の repeat 9999 も、1 コマで走り切って StackOverflow しない``() =
    let bullet = BulletElm.Bullet ({ bulletLabel = None }, None, None, [])
    let fire =
      Action.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Absolute }, numExpr "0")),
                        Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "1")),
                        bullet)
    let body = ActionElm.Action ({ actionLabel = None }, [ fire ])
    let t = top [ Action.Repeat (Times (numExpr "9999"), body) ]
    let r = Step.step noResolvers env (stateWith [ t ])
    r.Effects |> List.length |> should equal 9999
    r.Finished |> should equal true

  /// BulletRunner.run は「生きている top が 1 本 も無いコマ」で aim 4 本 を
  /// 組まずに 0 で済ませる（BulletRunner.envWithoutAim）。その前提 ——
  /// Step.step が env を触るのは top を回すループの中だけで、ループの外
  /// （差分の計算・FireContext の積み直し・Finished の判定）は env を
  /// 見ない —— をここで門にする。
  ///
  /// 見るのは「aim を変えても結果が 1 ビットも動かないこと」。step が
  /// ループの外で env を読むようになったら、毒入りの env の側だけ答えが
  /// ずれて赤くなる。
  ///
  /// 較正: step の差分に env.AimDir を足す変異を入れるとこの門は赤くなり、
  /// 同じファイルの他の門は緑のままだった（top が生きているコマを見ている
  /// ので、そちらは両方の env で同じだけずれる）。
  [<Test>]
  member _.``終わった top しか無いコマは、aim を読まない``() =
    let poisoned =
      { env with
          AimDir = 1.25f
          EnemyAimDir = -2.5f
          SpawnAimDir = 3.0f
          SpawnEnemyAimDir = -0.75f }
    // vanish は 1 コマで終わる。2 コマめが「生きている top が無い」コマ
    let t = top [ Action.Vanish ]
    let first = Step.step noResolvers env (stateWith [ t ])
    first.Finished |> should equal true
    // 前提そのもの: この状態では List.exists (not << isDone) が false
    first.State.Tops |> List.exists (fun (_, p, _) -> not (Step.isDone p)) |> should equal false
    let withZero = Step.step noResolvers env first.State
    let withPoison = Step.step noResolvers poisoned first.State
    withPoison.Delta |> should equal withZero.Delta
    withPoison.State |> should equal withZero.State
    withPoison.Effects |> should equal withZero.Effects
    withPoison.Finished |> should equal withZero.Finished
    withPoison.Retired |> should equal withZero.Retired
