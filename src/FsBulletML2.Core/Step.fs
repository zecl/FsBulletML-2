namespace FsBulletML2

open FsBulletML2.Domain
open FsBulletML2.Eval

/// 命令を 1 つずつ進める。落とした `BulletRunner.runCommand` を写したもの。
///
/// 走査を止めるかどうかの 3 値も、あちらと同じ意味。
///   Stopped   走査を止める。次のフレームも同じところから
///   Continue  走査は止めない。ただし終わりにもしないので、次のフレームでも走る
///   Ended     終わり。走査の側が finish を立てる
module internal Step =

  type RunState =
    | Continue
    | Ended
    | Stopped

  /// 角度を 0 〜 2π に丸める。落とした `BulletRunner.calcDir` と同じ式
  let internal calcDir (dir: float32) =
    if (float dir > 2. * System.Math.PI) then dir - float32 (2. * System.Math.PI)
    elif (float dir < 0.) then dir + float32 (2. * System.Math.PI)
    else dir

  /// 型の上では届くが実際には来ない腕の既定値。
  ///
  /// ここを定数 0.0f に置き換えてはいけない。 getValue は式の中身に
  /// よらず乱数を 1 回 引くので、呼び出しを消すと引く回数が変わり、
  /// 全弾幕の軌跡がずれる
  let private zeroExpr = numExpr "0"

  /// wait。旧の waitCommand を写す。
  ///
  ///   term >= 0 なら 1 減らす
  ///   その後まだ term >= 0 なら Stopped、そうでなければ Ended
  ///
  /// term を 1 減らしてから判定する形は、旧の off-by-one を写した。
  /// 単純化してはいけない。
  let wait (waitExpr: Expr.NumExpr) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let started, left =
        match p with
        | PWait (s, l) -> s, l
        | _ -> false, 0.0f
      let left = if started then left else getValue env waitExpr
      let left = if left >= 0.0f then left - 1.0f else left
      if left >= 0.0f then
        return Stopped, PWait (true, left)
      else
        return Ended, PWait (true, left)
    }

  /// vanish。旧の vanishCommand を写す
  let vanish (p: Progress) : Sim<RunState * Progress> =
    sim {
      do! Sim.emit Vanished
      return Ended, PVanish true
    }

  /// accel。旧の accelCommand を写す。
  ///
  /// 終わり方が changeDirection / changeSpeed と違う ——
  /// term < 0 で終わり、そのとき加算しない。
  ///
  /// 軸が省略されていても旧と同じく "0" として扱い、catch-all の計算をする
  /// （現状維持ではなく、既存の加速度を term フレームかけて 0 へ寄せる）。
  /// getValue を通すこと自体にも意味がある（式の中身に関わらず乱数を進める）。
  let accel (h: Horizontal option) (v: Vertical option) (Term term) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let started, left, dx, dy =
        match p with
        | PAccel (s, l, x, y) -> s, l, x, y
        | _ -> false, 0.0f, 0.0f, 0.0f
      let left, dx, dy =
        if started then left, dx, dy
        else
          let t = getValue env term
          let dx =
            match h with
            | Some (Horizontal (attrs, value)) ->
                let value = getValue env value
                match attrs with
                | Some a ->
                    match a.horizontalType with
                    | HorizontalType.Sequence -> value
                    | HorizontalType.Relative -> value / t
                    | _ -> (value - self.Accel.X) / t
                | None -> (value - self.Accel.X) / t
            | None ->
                let value = getValue env zeroExpr
                (value - self.Accel.X) / t
          let dy =
            match v with
            | Some (Vertical (attrs, value)) ->
                let value = getValue env value
                match attrs with
                | Some a ->
                    match a.verticalType with
                    | VerticalType.Sequence -> value
                    | VerticalType.Relative -> value / t
                    | _ -> (value - self.Accel.Y) / t
                | None -> (value - self.Accel.Y) / t
            | None ->
                let value = getValue env zeroExpr
                (value - self.Accel.Y) / t
          t, dx, dy
      let left = left - 1.0f
      if left < 0.0f then
        return Ended, PAccel (true, left, dx, dy)
      else
        do! Sim.put { self with Accel = { X = self.Accel.X + dx; Y = self.Accel.Y + dy } }
        return Continue, PAccel (true, left, dx, dy)
    }

  /// changeDirection。旧の changeDirection を写す。
  ///
  /// accel と終わり方が違う —— term <= 0 で終わり、そのときも加算してから終わる。
  /// 終わるときは term を getValue initTerm に戻す。repeat の中で回り直すときに
  /// この値が効くので、消すと 2 周目の挙動が変わる。
  let changeDirection (dir: Direction) (Term term) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let started, left, delta =
        match p with
        | PChangeDir (s, _, l, d) -> s, l, d
        | _ -> false, 0.0f, 0.0f
      let left, delta =
        if started then left, delta
        else
          let t = getValue env term
          let attrs, valueStr = match dir with Direction (a, v) -> a, v
          let value = float32 ((getValue env valueStr |> float) * System.Math.PI / 180.0)
          // 目標との差を ±π に畳んでから term で割る。sequence だけは通さない
          let fold (d: float32) =
            let d = if float d > System.Math.PI then d - 2.0f * float32 System.Math.PI else d
            let d = if float d < -System.Math.PI then d + 2.0f * float32 System.Math.PI else d
            d / t
          let delta =
            match attrs with
            | Some a ->
                match a.directionType with
                | DirectionType.Sequence -> value
                | DirectionType.Absolute -> fold (value - self.Dir)
                | DirectionType.Relative -> fold value
                | _ ->
                    let aim = if self.Kind = BulletType.Player then env.Aim.ToEnemy else env.Aim.ToPlayer
                    fold (aim + value - self.Dir)
            | None ->
                let aim = if self.Kind = BulletType.Player then env.Aim.ToEnemy else env.Aim.ToPlayer
                fold (aim + value - self.Dir)
          t, delta
      let left = left - 1.0f
      do! Sim.put { self with Dir = calcDir (self.Dir + delta) }
      if left <= 0.0f then
        // term を戻すのは repeat の次周のため。戻すと left がまた正になるので、
        // 終わったことは left でなく done_ で覚えておく
        return Ended, PChangeDir (true, true, getValue env term, delta)
      else
        return Continue, PChangeDir (true, false, left, delta)
    }

  /// changeSpeed。旧の changeSpeed を写す。term <= 0 で終わり、そのときも
  /// 加算してから終わる（終わるときに term を戻すのも changeDirection と同じ）
  let changeSpeed (spd: Speed) (Term term) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let started, left, delta =
        match p with
        | PChangeSpeed (s, _, l, d) -> s, l, d
        | _ -> false, 0.0f, 0.0f
      let left, delta =
        if started then left, delta
        else
          let t = getValue env term
          let attrs, valueStr = match spd with Speed (a, v) -> a, v
          let value = getValue env valueStr
          let delta =
            match attrs with
            | Some a ->
                match a.speedType with
                | SpeedType.Sequence -> value
                | SpeedType.Relative -> value / t
                | _ -> (value - self.Speed) / t
            | None -> (value - self.Speed) / t
          t, delta
      let left = left - 1.0f
      do! Sim.put { self with Speed = self.Speed + delta }
      if left <= 0.0f then
        return Ended, PChangeSpeed (true, true, getValue env term, delta)
      else
        return Continue, PChangeSpeed (true, false, left, delta)
    }

  /// repeat が次の周へ進むときの、running 1 要素ぶんの t.Init(env) に当たる。
  ///
  /// 引く順を変えてはいけない。 旧は 1 回 の辿りの中で並びの順に getValue を
  /// 呼ぶので、wait の引きだけ後回しにするとどの物理呼び出しがどちらの値になるかが
  /// 入れ替わる（定数式では見えないが、式に `$rand` が入ると別の弾道になる）。
  ///
  /// internal（private ではない）にしてあるのは、橋（TraceNew.fs）が
  /// 全 top 終了時の引き直しにこれを使うため。
  ///
  /// 木を組む段の wait だけの引きは別の歩き方が要るので rootProgress に分けてある。
  let rec internal resetChild (env: Env) (script: Action) : Progress =
    match script with
    | Action.Wait s -> PWait (true, getValue env s)
    | Action.ChangeDirection (_, Term t) ->
        getValue env t |> ignore
        PChangeDir (false, false, 0.0f, 0.0f)
    | Action.ChangeSpeed (_, Term t) ->
        getValue env t |> ignore
        PChangeSpeed (false, false, 0.0f, 0.0f)
    | Action.Action (_, children) ->
        PAction (false, None, children |> List.map (resetChild env))
    | Action.Repeat (_, body) ->
        PRepeat (0, false, resetChildActionElm env body)
    | Action.Fire (_, _, _, BulletElm.Bullet (_, _, _, actions)) ->
        // fire は値を引くだけ（撃たれた弾は fire のたびに新しい Progress で始まる）
        actions |> List.iter (resetChildActionElm env >> ignore)
        PFire false
    | Action.Fire (_, _, _, BulletElm.BulletRef _) -> PFire false
    | Action.Accel _ | Action.Vanish
    | Action.FireRef _ | Action.ActionRef _ -> Progress.initial script

  /// repeat / bullet の子（action か actionRef）ぶん
  and internal resetChildActionElm (env: Env) (a: ActionElm) : Progress =
    match a with
    | ActionElm.Action (_, children) ->
        PAction (false, None, children |> List.map (resetChild env))
    | ActionElm.ActionRef _ -> PNoop

  /// 木を組む段（`BulletmlRead.foldConstants`）の wait だけの引き直し。
  ///
  /// accel だけ明示の腕が要る。 placeholder が first = false（他の 2 つ は true）で、
  /// true は「まだ評価前」・false は「もう評価済み」を意味するので、
  /// 素通りすると取り違える —— 明示の腕で getValue を一切呼ばずに組む。
  ///
  /// 撃たれた弾は Step.fire が resetChild を通すのでここを通らない。
  let rec internal rootProgress (env: Env) (script: Action) : Progress =
    match script with
    | Action.Wait s -> PWait (true, getValue env s)
    | Action.Accel _ -> PAccel (true, 1.0f, 0.0f, 0.0f)
    | Action.Action (_, children) ->
        PAction (false, None, children |> List.map (rootProgress env))
    | Action.Repeat (_, body) ->
        PRepeat (0, false, rootProgressActionElm env body)
    | Action.Fire (_, _, _, BulletElm.Bullet (_, _, _, actions)) ->
        actions |> List.iter (rootProgressActionElm env >> ignore)
        PFire false
    | Action.Fire (_, _, _, BulletElm.BulletRef _) -> PFire false
    | Action.ChangeDirection _ | Action.ChangeSpeed _ | Action.Vanish
    | Action.FireRef _ | Action.ActionRef _ -> Progress.initial script

  /// repeat / bullet の子ぶん
  and internal rootProgressActionElm (env: Env) (a: ActionElm) : Progress =
    match a with
    | ActionElm.Action (_, children) ->
        PAction (false, None, children |> List.map (rootProgress env))
    | ActionElm.ActionRef _ -> PNoop

  /// repeat 直下の body（旧の actionElm）だけの特別扱い。
  ///
  /// `pa.loop` 自身は触らずに持ち越す —— actionRef の輪が一度解ければ、
  /// その周から先はもう解き直さない。
  let private resetBody (env: Env) (body: ActionElm) (finished: Progress) : Progress =
    match body, finished with
    | ActionElm.Action (_, staticChildren), PAction (_, loop, _) ->
        let running = match loop with Some l -> l | None -> staticChildren
        PAction (false, loop, running |> List.map (resetChild env))
    | _ -> resetChildActionElm env body

  /// 輪のために展開を止めた bulletRef / actionRef を、走らせる側から
  /// 1 段だけ解くための入口。実装は fire（bulletRef）と action（actionRef）
  ///
  /// 名前を BulletLabel / ActionLabel で受けるので、bullet の名前を
  /// Action の解決子へ渡す形が組めない。
  type Resolvers =
    { Bullet : BulletLabel -> string list -> BulletElm option
      Action : ActionLabel -> string list -> ActionElm option }

  /// Progress が「終わった」を持っているか。旧の getFinish。
  ///
  /// changeDirection / changeSpeed は left の符号では判定できない
  /// （終わるフレームで term を戻すため）ので done_ を明示で読む。
  let isDone (p: Progress) =
    match p with
    | PAction (d, _, _) -> d
    | PRepeat (_, d, _) -> d
    | PFire d -> d
    | PVanish d -> d
    | PWait (_, l) -> l < 0.0f
    | PAccel (s, l, _, _) -> s && l < 0.0f
    | PChangeDir (_, d, _, _) -> d
    | PChangeSpeed (_, d, _, _) -> d
    | PNoop -> false

  /// この位置が、これから何コマ 何も起こさないか。
  /// 現在位置だけを見る。走査は action の中の step と同じ順。
  /// ブロックするのは wait だけ。
  let rec quietOf (p: Progress) : Quiet =
    match p with
    | PWait (true, l) -> if l >= 1.0f then QWait l else QNone
    | PAccel (true, l, _, _) -> if l >= 0.0f then QBusy else QNone
    | PChangeDir (true, false, l, _) -> if l >= 0.0f then QBusy else QNone
    | PChangeSpeed (true, false, l, _) -> if l >= 0.0f then QBusy else QNone
    | PAction (false, _, children) -> quietFirst children
    // `repeat` は静かではない。 中の wait が進行中でも毎コマ `times` を
    // 評価するので、`$rand` が在ると位置は 1 ミリ も動かないのに乱数の列が進む。
    // 位置・撃つ・消える を見る検算は 80 万 コマ で 1 件 も拾わず、
    // `$rand` を読んだ回数を数えて初めて出た（176 本 中 46 本 が割れていた）
    | _ -> QNone

  /// 頭から見て、最初の「終わっていない子」だけを見る。
  /// そこが現在位置で、その先は走らない
  and private quietFirst (xs: Progress list) : Quiet =
    match xs with
    | [] -> QNone
    | h :: t -> if isDone h then quietFirst t else quietOf h

  /// 1 ずつ n 回 引く。 `l - float32 n` と 1 行 で書くと
  /// float32 の丸めが素の道と違う —— 素の道は毎コマ `left - 1.0f` を
  /// 通るので、同じ順で引かないと下の桁がずれる。
  /// 答えをビット で突き合わせているので、その 1 ビット が割れになる。
  let private minusOnes (n: int) (l: float32) =
    let mutable v = l
    for _ in 1 .. n do v <- v - 1.0f
    v

  /// この top が、これから何コマ 一定の割合で変わるか（v4.9.3）
  ///
  /// 頭から `wait` で止まるまでをなめて、その途中 に
  ///
  ///     進行中の accel          足す量 を集める
  ///     進行中の changeSpeed    足す量 を集める
  ///     それ以外               その場でやめる（`LNone`）
  ///
  /// 入れ子の action には入らない —— `repeat` は `times` を評価して
  /// `$rand` を読むし、まだ始まっていない `wait` も `getValue` を通る。
  let linearOf (p: Progress) : Linear =
    match p with
    | PAction (false, _, children) ->
        let mutable frames = System.Int32.MaxValue
        let mutable sstep = 0.0f
        let mutable ax = 0.0f
        let mutable ay = 0.0f
        // 足すものが 1 つ も無ければ乗せない。 定数なら段 2 の仕事
        let mutable moving = false
        let mutable stopped = false
        let mutable bad = false
        let mutable rest = children
        while not bad && not stopped && not (List.isEmpty rest) do
          let h = List.head rest
          rest <- List.tail rest
          if isDone h then () else
          match h with
          | PWait (true, l) when l >= 1.0f ->
              stopped <- true
              frames <- min frames (int l)
          | PAccel (true, l, dx, dy) when l >= 0.0f ->
              moving <- true
              ax <- ax + dx
              ay <- ay + dy
              frames <- min frames (int l)
          | PChangeSpeed (true, false, l, d) when l >= 0.0f ->
              moving <- true
              sstep <- sstep + d
              // 終わるコマ は飛ばさない。 changeSpeed は足してから終わるので、
              // floor L コマ 飛ばすと `done_` が立たず次の step で 1 回 余分 に
              // 足される（同梱 10 本 が割れた形）。accel は足さずに終わるので不要
              frames <- min frames (int l - 1)
          | _ -> bad <- true
        // `stopped` は要らない。 足すものが在れば `Continue` で同じ位置に
        // 留まる —— 要るのは「途中 に知らない子が無い」ことだけ
        ignore stopped
        if bad || not moving || frames < 1 then LNone
        else LStep (frames, sstep, ax, ay)
    | _ -> LNone

  /// 線形の n コマ を飛ばした後の実行位置（v4.9.3）。
  ///
  /// `linearOf` と同じ道を同じ順で辿る（食い違うと、その弾だけ余分 に走る）。
  /// `Speed` / `Accel` はここでは触らない —— 呼ぶ側が n 回 分 進める。
  let rec skipLinear (n: int) (p: Progress) : Progress =
    match p with
    | PAction (false, loop, children) ->
        let mutable stopped = false
        PAction (false, loop, children |> List.map (fun h ->
          if stopped || isDone h then h
          else
            match h with
            | PWait (true, l) when l >= 1.0f ->
                stopped <- true
                PWait (true, minusOnes n l)
            | PAccel (true, l, dx, dy) when l >= 0.0f -> PAccel (true, minusOnes n l, dx, dy)
            | PChangeSpeed (true, false, l, d) when l >= 0.0f -> PChangeSpeed (true, false, minusOnes n l, d)
            | _ -> h))
    | _ -> p

  /// 静かな n コマ を飛ばした後の実行位置。（v4.9.2）
  ///
  /// 速い道はエンジンを呼ばないので、飛ばしたあいだ `wait` が減らない
  /// （減らさずに戻すと n コマ 余分 に待つ —— 176 本 のうち 118 本 が割れた）。
  ///
  /// `quietOf` と対で読むこと。 同じ「現在位置」を辿って、そこの `wait` だけを
  /// n 減らす。`quietOf` が返した数より大きい n を渡してはいけない。
  let rec skipQuiet (n: int) (p: Progress) : Progress =
    match p with
    | PWait (true, l) when l >= 1.0f -> PWait (true, minusOnes n l)
    | PAction (false, loop, children) -> PAction (false, loop, skipQuietFirst n children)
    // `repeat` の中へは入らない（`quietOf` が repeat を静かと言わないので
    // ここへ来ない）—— 2 つ が同じ道を辿ることが、この対の正しさの条件
    | _ -> p

  /// `quietFirst` と同じ順で辿る。終わった子は素通り、最初の
  /// 「終わっていない子」だけを書き換えて、尻尾はそのまま共有する
  and private skipQuietFirst (n: int) (xs: Progress list) : Progress list =
    match xs with
    | [] -> []
    | h :: t -> if isDone h then h :: skipQuietFirst n t else skipQuiet n h :: t

  /// 終わりの印を立てる。旧の setFinish
  let setDone (p: Progress) =
    match p with
    | PAction (_, loop, cs) -> PAction (true, loop, cs)
    | PRepeat (n, _, c) -> PRepeat (n, true, c)
    | PFire _ -> PFire true
    | PVanish _ -> PVanish true
    | PChangeDir (s, _, l, d) -> PChangeDir (s, true, l, d)
    | PChangeSpeed (s, _, l, d) -> PChangeSpeed (s, true, l, d)
    | other -> other

  /// 命令 10 通りを漏れなく振り分ける。中身をほどいて渡す
  /// （受け取る側それぞれの「自分の腕でなければ既定値」という届かない match が消える）。
  let rec command (rs: Resolvers) (script: Action) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    NodeTrace.visit (box script)
    match script with
    | Action.Wait s ->
        sim {
          let! r, p' = wait s p
          return r, p', fc
        }
    | Action.Vanish ->
        sim {
          let! r, p' = vanish p
          return r, p', fc
        }
    | Action.Accel (h, v, term) ->
        sim {
          let! r, p' = accel h v term p
          return r, p', fc
        }
    | Action.ChangeDirection (dir, term) ->
        sim {
          let! r, p' = changeDirection dir term p
          return r, p', fc
        }
    | Action.ChangeSpeed (spd, term) ->
        sim {
          let! r, p' = changeSpeed spd term p
          return r, p', fc
        }
    | Action.Action (attrs, children) -> action rs attrs children p fc
    | Action.Repeat (times, body) -> repeat rs times body p fc
    | Action.Fire (attrs, dirOpt, spdOpt, bulletSrc) -> fire rs attrs dirOpt spdOpt bulletSrc p fc
    // 展開していない参照。走査は止めず、終わりにする
    | Action.ActionRef _ | Action.FireRef _ -> sim { return Ended, p, fc }

  /// repeat / bullet の子、および top* の台本ぶん。
  ///
  /// actionRef のときは「子が空の action」として通す（repeat の子に
  /// actionRef を書く弾幕があり、旧の振る舞いをそのまま写す）。
  and actionElm (rs: Resolvers) (script: ActionElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    NodeTrace.visit (box script)
    match script with
    | ActionElm.Action (attrs, children) -> action rs attrs children p fc
    | ActionElm.ActionRef _ -> action rs { actionLabel = None } [] p fc

  /// action。旧の actionCommand を写す。
  ///
  /// `Stopped` は走査を止める。`Continue` は止めない（i は進む）が終わりにも
  /// しないので、次のフレームでも同じ命令が走る。
  ///
  /// 輪を解いた並びは親の `PAction.loop` に積む（actionRef 自身は状態を持たない）。
  /// 展開できたら中身の後ろに残りの兄弟を繋いで loop に差し替え、その場で走査を止める
  /// —— 済んだ手前は捨てるので、並びは解くたびに伸びない。
  ///
  /// `ps` を配列にする案は測って戻した（上界 0.4〜6.7% で、いちばん大きいのは
  /// 子の少ない台本だった）。
  and action (rs: Resolvers) (_attrs: ActionAttrs) (children: Action list)
             (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    sim {
      let! env = Sim.ask
      let done_, loop, progs =
        match p with
        | PAction (d, l, ps) -> d, l, ps
        | _ -> false, None, children |> List.map Progress.initial
      if done_ then
        return Ended, p, fc
      else
        // loop が立っていれば、輪を解いた並びを走らせる
        let running = match loop with Some l -> l | None -> children
        let len = min (List.length progs) (List.length running)
        // 走査 1 マスぶん。stopped または loopHit が立った後の呼び出しは何もしない
        let step
            (accSim: Sim<Progress list * bool * bool * FireContext * (Action list * Progress list) option>)
            (idx: int) =
          sim {
            let! (ps, stopped, cont, curFc, loopHit) = accSim
            if stopped then
              return ps, stopped, cont, curFc, loopHit
            else
              let cur = List.item idx ps
              if isDone cur then
                return ps, stopped, cont, curFc, loopHit
              else
                match List.item idx running with
                | Action.ActionRef (attrs, prams) ->
                    match rs.Action attrs.actionRefLabel prams with
                    | Some (ActionElm.Action (_, expanded)) ->
                        let newRunning = expanded @ (running |> List.skip (idx + 1))
                        // ここが 5 つめの draw site。 actionRef は このあと
                        // Init 相当を挟まないので、ここで引いた 1 回 だけが最終値になる
                        let expandedProgs = expanded |> List.map (rootProgress env)
                        let remainingProgs =
                          running |> List.skip (idx + 1) |> List.map Progress.initial
                        return ps, true, cont, curFc, Some (newRunning, expandedProgs @ remainingProgs)
                    | _ ->
                        return ps, stopped, cont, curFc, loopHit
                | childScript ->
                    let! r, p', fc' = command rs childScript cur curFc
                    if r = Stopped then NodeTrace.stop (box childScript)
                    let p'' = if r = Ended then setDone p' else p'
                    // 1 マスだけ差し替える。尻尾は共有する（idx+1 セル）
                    let ps' = ps |> List.updateAt idx p''
                    return ps', (r = Stopped), (cont || r = Continue), fc', loopHit
          }
        let! (ps, stopped, cont, fcOut, loopHit) =
          [ 0 .. len - 1 ] |> List.fold step (Sim.ret (progs, false, false, fc, None))
        match loopHit with
        | Some (newRunning, newProgs) ->
            return Stopped, PAction (false, Some newRunning, newProgs), fcOut
        | None ->
            if stopped then return Stopped, PAction (false, loop, ps), fcOut
            elif cont then return Continue, PAction (false, loop, ps), fcOut
            else return Ended, PAction (true, loop, ps), fcOut
    }

  /// repeat。旧の repeatCommand を写す。
  ///
  /// times は呼ばれるたびに評価し直す。子が End を返すたびに周を 1 つ数え、
  /// times に届いたら子に finish を立てて終わる。
  /// times = 0 は while に 1 度も入らない（旧の癖をそのまま写した）。
  ///
  /// この関数は `sim { }` を使わない。 `while` を CE の中に書くと
  /// コンパイラが `SimBuilder.While` へ展開し、1 周 につき `Sim.bind` を 1 段 積む
  /// 再帰になる —— 見た目が手続きループでも StackOverflow へ戻る
  /// （`times` に 9999 を書く実物の弾幕がある）。
  and repeat (rs: Resolvers) (Times timesStr) (body: ActionElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    fun env self0 ->
      let times = getValue env timesStr |> int
      let num0, done0, child0 =
        match p with
        | PRepeat (n, d, c) -> n, d, c
        | _ -> 0, false, Progress.initialActionElm body
      let cycles = max 0 (times - num0)
      // while が 1 度も回らなければこのチェックへは到達しないので、
      // cycles > 0 のときだけ見る（旧もそうだった）。
      // repeat の直下に actionRef が残るのは自己参照のときだけ で、
      // その形は DTD 上 合法・旧はここへ到達して落ちる ——
      // 黙って通すと、橋（equivalence）が「片方だけ例外」を割れとして拾う
      if cycles > 0 then
        match body with
        | ActionElm.Action _ -> ()
        | _ -> failwith "repeatCommand: repeat の子が action ではない"
      let mutable num = num0
      let mutable child = child0
      let mutable curFc = fc
      let mutable st = self0
      // `effects @ w` は cycles について二乗になるので ResizeArray に積んで
      // 最後に 1 回 だけ list へ畳む。並びは変えない
      let effectsAcc = ResizeArray<Effect>()
      let mutable stopped = false
      let mutable cont = false
      let mutable go = true
      let mutable i = 0
      while go && i < cycles do
        i <- i + 1
        if isDone child then
          // 実際には届かない（num が times に届くのと同時に子へも finish が
          // 立つ）。それでも旧の形のまま残す
          let num' = num + 1
          num <- num'
          go <- num' < times
        else
          let (r, child', fc'), st', w = Sim.run env st (actionElm rs body child curFc)
          if r = Stopped then NodeTrace.stop (box body)
          st <- st'
          effectsAcc.AddRange w
          child <- child'
          curFc <- fc'
          if r = Stopped then
            stopped <- true
            go <- false
          elif r = Continue then
            cont <- true
            go <- false
          else
            let num' = num + 1
            num <- num'
            child <-
              if num' >= times then setDone child'
              else
                // 次の周のために子の並びを作り直す（旧の running |> Seq.iter Init）
                resetBody env body child'
            go <- num' < times
      let value =
        if stopped then Stopped, PRepeat (num, done0, child), curFc
        elif cont then Continue, PRepeat (num, done0, child), curFc
        else Ended, PRepeat (num, true, child), curFc
      // 揃えてある理由は性能ではなく、`Emit` を読む側が「空かどうか」を
      // 場所ごとに疑わなくて済むようにするため（効きは move -1,440 B 程度）
      let emit =
        if effectsAcc.Count = 0 then ValueNone
        else ValueSome (fun rest -> (List.ofSeq effectsAcc) @ rest)
      { Value = value; State = st; Emit = emit }

  /// fire。旧の fireCommand と createTask の両方を写す。
  ///
  /// 順序に意味がある —— fire 側の direction/speed -> bullet 側の
  /// direction/speed -> 未指定なら fire 側の値で埋める。
  ///
  /// 撃たれた弾の Tops は `Progress.initial` ではなく `resetChild` を通す。
  /// 戻すとその分の乱数消費が丸ごと消えて、弾が撃たれた瞬間から乱数列がずれる。
  and fire (rs: Resolvers) (_attrs: FireAttrs) (dirOpt: Direction option) (spdOpt: Speed option)
           (bulletSrc: BulletElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      // bulletRef は 1 段だけ解く（fire のたびに新しい弾ができるので 1 段で足りる）
      let bulletElm =
        match bulletSrc with
        | BulletElm.BulletRef (attrs, prams) ->
            match rs.Bullet attrs.bulletRefLabel prams with
            | Some (BulletElm.Bullet (_, _, _, actions) as x) ->
                // wait 1 個 につき乱数を 2 回 引くのが正しい（1 回めの値は
                // 下の resetChild で上書きされて捨てられるが、消費そのものは残す）
                actions |> List.iter (rootProgressActionElm env >> ignore)
                x
            | Some x -> x
            // ここへは実際には来ない（ラベルの無い bulletRef は構文解析で弾かれ、
            // 残るのは輪だけ。輪は tryFindBullet が Some を返した相手同士でしか作られない）
            | None -> bulletSrc
        | _ -> bulletSrc
      let revise = (float32 System.Math.PI) / 180.f
      let aim = if self.Kind = BulletType.Player then env.Aim.ToEnemy else env.Aim.ToPlayer
      // fire 側の direction で SrcDir を決める。旧の fireCommand と同じ順
      let srcDir =
        match dirOpt with
        | Some (Direction (attrs, v)) ->
            let value = getValue env v
            match attrs with
            | Some a ->
                match a.directionType with
                | DirectionType.Sequence -> fc.SrcDir + value * revise
                | DirectionType.Absolute -> value * revise
                | DirectionType.Relative -> value * revise + self.Dir
                | _ -> value * revise + aim
            | None -> value * revise + aim
        | None -> aim
      let fc = { fc with SrcDir = srcDir }
      // bullet の中の direction / speed
      let bDir, bSpd, bActions =
        match bulletElm with
        | BulletElm.Bullet (_, d, s, acts) -> d, s, acts
        | _ -> None, None, []
      // 撃たれた弾を組む。旧の createTask に当たる
      let mutable child =
        { Pos = self.Pos
          Speed = 0.0f
          Dir = 0.0f
          Accel = { X = 0.0f; Y = 0.0f }
          Kind = self.Kind
          IsBullet = true
          HasFired = false
          Tops = bActions |> List.map (fun a -> a, resetChildActionElm env a, FireContext.zero) }
      // bullet の direction。aim 系 だけ撃つ側 と違う基準を使う
      // （「撃たれた弾から見た向き」。fire 側 の aim を使うと基準の取り違えになる）
      match bDir with
      | Some (Direction (attrs, v)) ->
          let value = getValue env v * revise
          match attrs with
          | Some a ->
              match a.directionType with
              | DirectionType.Sequence -> child <- { child with Dir = calcDir (fc.SrcDir + value) }
              | DirectionType.Absolute -> child <- { child with Dir = calcDir value }
              | DirectionType.Relative -> child <- { child with Dir = calcDir (child.Dir + value) }
              | _ ->
                  // 撃たれた弾がどこに出るかはフロントが知っているので env.Spawn で受ける。
                  // 振り分けは撃った側の種別で行う（撃たれた弾は複写なので同じ）
                  let spawnAim =
                    if child.Kind = BulletType.Player then env.Spawn.ToEnemy else env.Spawn.ToPlayer
                  child <- { child with Dir = calcDir (spawnAim + value) }
          | None -> ()
      | None -> ()
      // bullet の speed。旧はこの式を 2 回 読む。
      // 1 回め の書き込みは直後に上書きされて使われないが、
      // getValue は式の中身に関わらず乱数を引くので、その 1 回 は消せない
      match bSpd with
      | Some (Speed (attrs, v)) ->
          getValue env v |> ignore
          let value = getValue env v
          let s =
            match attrs with
            | Some a ->
                match a.speedType with
                | SpeedType.Sequence -> fc.SrcSpeed + value
                | SpeedType.Relative -> self.Speed + value
                | _ -> value
            | None -> value
          child <- { child with Speed = s }
      | None -> ()
      // `fc.SpeedInit` は撃つ側 1 つ につき一生ものの latch（旧は一度も
      // false へ戻していない）。立っていない かつ bullet 側 に speed が在るなら、
      // fire 側 の speed は getValue すら呼ばれない（旧も if 全体を先に見る）
      let fc =
        if not fc.SpeedInit && bSpd.IsSome then
          { fc with SrcSpeed = child.Speed; SpeedInit = true }
        else
          match spdOpt with
          | Some (Speed (attrs, v)) ->
              let value = getValue env v
              let s =
                match attrs with
                | Some a ->
                    match a.speedType with
                    | SpeedType.Sequence -> fc.SrcSpeed + value
                    | SpeedType.Relative -> value + self.Speed
                    | _ -> value
                | None -> value
              { fc with SrcSpeed = s }
          | None ->
              if bSpd.IsNone then { fc with SrcSpeed = 1.0f }
              else { fc with SrcSpeed = child.Speed }
      // bullet に書いていなければ、fire 側の値を入れる
      let child = if bDir.IsNone then { child with Dir = calcDir fc.SrcDir } else child
      let child = if bSpd.IsNone then { child with Speed = fc.SrcSpeed } else child
      do! Sim.emit (Spawn child)
      do! Sim.put { self with HasFired = true }
      return Ended, PFire true, fc
    }

  /// 1 コマ進める。落とした `BulletRunner.runWithEnv` を写したもの。
  ///
  /// top* は 1 本ずつ独立に回す。ある top が wait で止まっても、
  /// それはその top の話なので、後ろの top はこのフレームでも回す。
  ///
  /// ここの `effects @ w` / `tops @ [...]` は ResizeArray にしない。
  /// n は top* の本数で、コーパス 227 本 のうち 204 本 が 1 本 ——
  /// `[] @ [x]` は 1 セルで、逆順に積んで戻す形は 2 セル。直すと負ける。
  ///
  /// 当てる先を測らずに「@ だから遅い」で直すと、効かないどころか逆に振れる。
  let step (rs: Resolvers) (env: Env) (self: BulletState) : StepResult =
    let mutable st = self
    let mutable effects : Effect list = []
    let mutable tops = []
    let mutable endCount = 0
    // FireContext は弾 1 つ につき 1 個。 top* が複数あっても sequence の
    // 積み上がりは top をまたいで続くので、値そのものは全 top で同じでなければ
    // ならない —— 頭で読み、1 個 の変数で持ち回り、最後に全部 の組へ積み直す
    let mutable sharedFc =
      match self.Tops with
      | (_, _, fc0) :: _ -> fc0
      | [] -> FireContext.zero
    for (script, prog, _) in self.Tops do
      if not (isDone prog) then
        let (r, prog', fc'), st', w = Sim.run env st (actionElm rs script prog sharedFc)
        if r = Stopped then NodeTrace.stop (box script)
        st <- st'
        effects <- effects @ w
        sharedFc <- fc'
        if r = Ended then
          endCount <- endCount + 1
          tops <- tops @ [ script, setDone prog' ]
        else
          tops <- tops @ [ script, prog' ]
      else
        endCount <- endCount + 1
        tops <- tops @ [ script, prog ]
    // 途中で組へ積むと、先に処理した top ほど古い値のまま固まる
    // （実際にこれで壊れた —— top2 が足した分が丸ごと落ちた）
    let tops = tops |> List.map (fun (s, p) -> s, p, sharedFc)
    let st = { st with Tops = tops }
    let speed = float st.Speed
    let dir = float st.Dir
    let dx = st.Accel.X + float32 (System.Math.Sin dir * speed)
    let dy = st.Accel.Y + float32 (-System.Math.Cos dir * speed)
    let finished = endCount >= List.length self.Tops
    { State = st
      Effects = effects
      Delta = { X = dx; Y = dy }
      Finished = finished
      Retired = finished && st.IsBullet && st.HasFired }
