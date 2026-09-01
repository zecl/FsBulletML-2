namespace FsBulletML

open FsBulletML.DTD
open FsBulletML.Domain
open FsBulletML.Processable

/// 命令を 1 つずつ進める。現行の BulletRunner.runCommand を写したもの。
///
/// 走査を止めるかどうかの 3 値は現行と同じ意味。
///   Stopped   走査を止める。次のフレームも同じところから
///   Continue  走査は止めない。ただし終わりにもしないので、次のフレームでも走る
///   Ended     終わり。走査の側が finish を立てる
module internal Step =

  type RunState =
    | Continue
    | Ended
    | Stopped

  /// 角度を 0 〜 2π に丸める。現行の BulletRunner.calcDir と同じ式。
  ///
  /// BulletRunner.fs はこのファイルより後に compile されるので、
  /// BulletRunner.calcDir はここを指すだけの別名にしてある。
  /// 式そのものはここが唯一の置き場所
  let internal calcDir (dir: float32) =
    if (float dir > 2. * System.Math.PI) then dir - float32 (2. * System.Math.PI)
    elif (float dir < 0.) then dir + float32 (2. * System.Math.PI)
    else dir

  /// wait。現行の waitCommand を写す。
  ///
  ///   term >= 0 なら 1 減らす
  ///   その後まだ term >= 0 なら Stopped、そうでなければ Ended
  ///
  /// term の評価は初回だけ。現行は Init() が入れているが、こちらは
  /// initial が評価しないので started で 1 度だけ評価する
  ///
  /// term を 1 減らしてから判定する形は、現行の off-by-one を写した。
  /// 単純化してはいけない
  let wait (script: RecBulletml) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let started, left =
        match p with
        | PWait (s, l) -> s, l
        | _ -> false, 0.0f
      let left =
        if started then left
        else
          match script with
          | RecBulletml.Wait s -> getValue env s
          | _ -> 0.0f
      let left = if left >= 0.0f then left - 1.0f else left
      if left >= 0.0f then
        return Stopped, PWait (true, left)
      else
        return Ended, PWait (true, left)
    }

  /// vanish。現行の vanishCommand を写す
  let vanish (p: Progress) : Sim<RunState * Progress> =
    sim {
      do! Sim.emit Vanished
      return Ended, PVanish true
    }

  /// accel。現行の accelCommand を写す。
  ///
  /// 終わり方が changeDirection / changeSpeed と違う。
  /// term < 0 で終わり、そのとき加算しない
  ///
  /// 省略された軸は convertRecBulletmlEx が { Absolute, "0" } で埋めるので、
  /// 木を組む段では常に値を持つ。ここで None を処理するのは、
  /// RecBulletml.Accel が option を持つため。その場合も現行と同じく
  /// "0" として扱い、catch-all の計算をする：
  /// 現状維持ではなく、既存の加速度を term フレームかけて 0 へ寄せる。
  /// getValue を通すこと自体にも意味がある（式の中身に関わらず乱数を進める）
  let accel (script: RecBulletml) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let h, v, term =
        match script with
        | RecBulletml.Accel (h, v, Term t) -> h, v, t
        | _ -> None, None, "0"
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
                let value = getValue env "0"
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
                let value = getValue env "0"
                (value - self.Accel.Y) / t
          t, dx, dy
      let left = left - 1.0f
      if left < 0.0f then
        return Ended, PAccel (true, left, dx, dy)
      else
        do! Sim.put { self with Accel = { X = self.Accel.X + dx; Y = self.Accel.Y + dy } }
        return Continue, PAccel (true, left, dx, dy)
    }

  /// changeDirection。現行の changeDirection を写す。
  ///
  /// accel と終わり方が違う。term <= 0 で終わり、そのときも加算してから終わる。
  /// 終わるときは term を getValue initTerm に戻す。repeat の中で回り直すときに
  /// この値が効くので、消すと 2 周目の挙動が変わる
  let changeDirection (script: RecBulletml) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let dir, term =
        match script with
        | RecBulletml.ChangeDirection (d, Term t) -> d, t
        | _ -> Direction (None, "0"), "0"
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
                    let aim = if self.Kind = BulletType.Player then env.EnemyAimDir else env.AimDir
                    fold (aim + value - self.Dir)
            | None ->
                let aim = if self.Kind = BulletType.Player then env.EnemyAimDir else env.AimDir
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

  /// changeSpeed。現行の changeSpeed を写す。term <= 0 で終わり、そのときも
  /// 加算してから終わる。終わるときに term を戻すのも changeDirection と同じ
  let changeSpeed (script: RecBulletml) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let spd, term =
        match script with
        | RecBulletml.ChangeSpeed (s, Term t) -> s, t
        | _ -> Speed (None, "0"), "0"
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
        // term を戻すのは repeat の次周のため。戻すと left がまた正になるので、
        // 終わったことは left でなく done_ で覚えておく
        return Ended, PChangeSpeed (true, true, getValue env term, delta)
      else
        return Continue, PChangeSpeed (true, false, left, delta)
    }

  /// repeat が次の周へ進むときの、running 1 要素ぶんの t.Init(env) に当たる。
  ///
  /// 現行の running |> Seq.iter Init は、1 つの並びを順番に辿りながら
  /// 要素ごとに Init を呼ぶ。中身によって「引いた値をそのまま次の周の
  /// 値として使う」（wait）か「引くだけ引いて値は捨てる」（changeDirection /
  /// changeSpeed。term は本人の Ended 分岐で既に一度戻しており、これは
  /// 二度めの引き直し。値は次の周の first 判定でさらに引き直されて
  /// 上書きされるので使われない）かが変わるが、どちらも getValue を
  /// 呼ぶことに変わりはなく、しかも「同じ 1 回の辿りの中で、並びの順に」
  /// 呼ばれる。wait の引きだけ後回し（次に実際に動くとき）にすると、
  /// 同じ並びでも wait と changeDirection / changeSpeed とで、どの物理
  /// 呼び出しがどちらの値になるかが入れ替わってしまう。定数式では
  /// 見えないが、式に $rand が入ると別の弾道になる
  ///
  /// nested な Action / Repeat の loop は、ここで毎回 None に戻してよい
  /// （現行の Init も pa.loop <- None を通ってから children を辿る。
  /// これは repeat 直下の body だけ特別扱いする対象で、resetBody 側で
  /// 持ち越す。入れ子の loop はここで素直に消してよい）。
  /// fire は撃たれた弾の action へ潜って値を引くだけで、Progress に
  /// 進行状態を持たない（現行はテンプレートの mutable な木へ書き戻すが、
  /// 新しい弾は fire のたびに新しい Progress で始まるので、ここでは
  /// 引いた分を捨ててよい）
  /// internal（private ではない）にしてあるのは、橋（TraceNew.fs）が
  /// 全 top 終了時の引き直しにこれを使うため（現行の
  /// task.Init(envOfGlobal o)。Original が None の task の Init は
  /// この歩き方と同じ。Domain 5.6 参照）。木を組む段の wait だけの
  /// 引きは別の歩き方が要るので rootProgress に分けてある
  let rec internal resetChild (env: Env) (script: RecBulletml) : Progress =
    match script with
    | RecBulletml.Wait s -> PWait (true, getValue env s)
    | RecBulletml.ChangeDirection (_, Term t) ->
        getValue env t |> ignore
        PChangeDir (false, false, 0.0f, 0.0f)
    | RecBulletml.ChangeSpeed (_, Term t) ->
        getValue env t |> ignore
        PChangeSpeed (false, false, 0.0f, 0.0f)
    | RecBulletml.Action (_, children) ->
        PAction (false, None, children |> List.map (resetChild env))
    | RecBulletml.Repeat (_, body) ->
        PRepeat (0, false, resetChild env body)
    | RecBulletml.Fire (_, _, _, RecBulletml.Bullet (_, _, _, actions)) ->
        actions |> List.iter (resetChild env >> ignore)
        PFire false
    | other -> Progress.initial other

  /// 木を組む段（現行の convertRecBulletmlEx）の wait だけの引き直し。
  ///
  /// IntermediateParser.fs の RecBulletml.Wait の腕は、撃つ弾ごとの Env が
  /// まだ無い木構築の段で、AimDir / EnemyAimDir を 0 に固定した Env で
  /// getValue を呼ぶ。兄弟の changeDirection / changeSpeed は placeholder
  /// （term = 1.f、first = true）を置くだけで、この段では getValue を
  /// 呼ばない。降りる腕（Action / Repeat / Fire / Bullet）は resetChild と同じ。
  ///
  /// accel だけは placeholder が first = false（他の 2 つは first = true）。
  /// 根の弾は Init を一度も通らない（設計文書 5.3「走らせ直す」）ので、
  /// この first = false がそのまま最初のフレームへ持ち越る。first = true
  /// の側（changeDirection / changeSpeed、そして Progress.initial の既定）は
  /// 「まだ評価前」を意味するので、Init を経ないまま最初のフレームへ入っても
  /// 値が合うが、accel の first = false は逆の「もう評価済み」を意味するので、
  /// ここを素通りすると取り違える。accel だけ明示の腕で PAccel (true, 1.0f,
  /// 0.0f, 0.0f) を組み、getValue は一切呼ばない。この状態で Step.accel を
  /// 呼ぶと、1 回め（term 1.0 -> 0.0、0.0 は 0.0 未満でないので Continue、
  /// 加速度は動かない）→ 2 回め（term 0.0 -> -1.0、Ended）という、乱数を
  /// 1 回も引かず加速度も一度も変えない「2 フレームの no-op」になる
  /// （旧 accelCommand を first = false のまま読んだときと同じ）。
  ///
  /// resetChild（現行の Init の写し）とは別の歩き方が要る理由は Domain 5.6。
  /// 撃たれた弾は Step.fire が resetChild（createTask の Init(env) の写し）を
  /// 通すのでここを通らない。根の BulletState を組むときにだけ使う
  let rec internal rootProgress (env: Env) (script: RecBulletml) : Progress =
    match script with
    | RecBulletml.Wait s -> PWait (true, getValue env s)
    | RecBulletml.Accel _ -> PAccel (true, 1.0f, 0.0f, 0.0f)
    | RecBulletml.Action (_, children) ->
        PAction (false, None, children |> List.map (rootProgress env))
    | RecBulletml.Repeat (_, body) ->
        PRepeat (0, false, rootProgress env body)
    | RecBulletml.Fire (_, _, _, RecBulletml.Bullet (_, _, _, actions)) ->
        actions |> List.iter (rootProgress env >> ignore)
        PFire false
    | other -> Progress.initial other

  /// repeat 直下の body（現行の actionElm）だけの特別扱い。
  ///
  /// 現行の running = match pa.loop with Some t -> t | None -> tasks を
  /// この周ざかりの reset でも同じ並びに使い、かつ pa.loop 自身は
  /// 触らずに持ち越す（現行の running |> Seq.iter Init は running の
  /// 要素ごとに Init を呼ぶだけで、actionElm 自身の Init は呼ばない
  /// ので pa.loop は変わらない）。actionRef の輪が一度解ければ、
  /// その周から先はもう解き直さない
  let private resetBody (env: Env) (body: RecBulletml) (finished: Progress) : Progress =
    match body, finished with
    | RecBulletml.Action (_, staticChildren), PAction (_, loop, _) ->
        let running = match loop with Some l -> l | None -> staticChildren
        PAction (false, loop, running |> List.map (resetChild env))
    | _ -> resetChild env body

  /// 輪のために展開を止めた bulletRef / actionRef を、走らせる側から
  /// 1 段だけ解くための入口。実装は fire（bulletRef）と action（actionRef）
  type Resolvers =
    { Bullet : string -> string list -> RecBulletml option
      Action : string -> string list -> RecBulletml option }

  /// Progress が「終わった」を持っているか。現行の getFinish。
  ///
  /// wait / accel は term が尽きたら二度と正に戻らないので left の符号だけで
  /// 判定できるが、changeDirection / changeSpeed は終わるフレームで term を
  /// 戻す（repeat の次周のため）ので left の符号では判定できない。
  /// PChangeDir / PChangeSpeed の done_ を明示で読む（Domain.Progress 参照）
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

  /// 終わりの印を立てる。現行の setFinish
  let setDone (p: Progress) =
    match p with
    | PAction (_, loop, cs) -> PAction (true, loop, cs)
    | PRepeat (n, _, c) -> PRepeat (n, true, c)
    | PFire _ -> PFire true
    | PVanish _ -> PVanish true
    | PChangeDir (s, _, l, d) -> PChangeDir (s, true, l, d)
    | PChangeSpeed (s, _, l, d) -> PChangeSpeed (s, true, l, d)
    | other -> other

  /// 命令 1 つを振り分ける。現行の runCommand の match に当たる。
  ///
  /// fire だけ FireContext を書き換えるので戻り値に含めてあるが、fire の腕は
  /// まだここには無い（fire を足す Task で足す）。他の 5 つは FireContext に
  /// 触らないので、渡された fc をそのまま返す
  let rec command (rs: Resolvers) (script: RecBulletml) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    match script with
    | RecBulletml.Wait _ ->
        sim {
          let! r, p' = wait script p
          return r, p', fc
        }
    | RecBulletml.Vanish ->
        sim {
          let! r, p' = vanish p
          return r, p', fc
        }
    | RecBulletml.Accel _ ->
        sim {
          let! r, p' = accel script p
          return r, p', fc
        }
    | RecBulletml.ChangeDirection _ ->
        sim {
          let! r, p' = changeDirection script p
          return r, p', fc
        }
    | RecBulletml.ChangeSpeed _ ->
        sim {
          let! r, p' = changeSpeed script p
          return r, p', fc
        }
    | RecBulletml.Action _ -> action rs script p fc
    | RecBulletml.Repeat _ -> repeat rs script p fc
    | RecBulletml.Fire _ -> fire rs script p fc
    | _ -> sim { return Ended, p, fc }

  /// action。現行の actionCommand を写す。
  ///
  /// Stopped は走査を止める。Continue は止めない（i は進む）が終わりにも
  /// しないので、次のフレームでも同じ命令が走る。
  ///
  /// actionRef（輪を解いた並び）：展開していない actionRef は Progress.initial で
  /// PNoop になる（actionRef 自身は状態を持たないため）。輪を解いた並びは、
  /// その actionRef 自身ではなく親の action の PAction.loop に積む。展開できたら、
  /// 展開した中身の後ろに残りの兄弟を繋いで loop に差し替え、その場で走査を
  /// 止める（済んだ手前は捨てるので、並びは解くたびに伸びない）。
  /// 自己参照の actionRef はパーサが展開せず残すので、輪はここへ毎フレーム
  /// 届き、そのたびに 1 段だけ解けて回り続ける。
  /// 解決できない actionRef（ラベルが見つからない等）は何もしない。走査は
  /// 止めず、終わりにもしない
  and action (rs: Resolvers) (script: RecBulletml) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    sim {
      let! env = Sim.ask
      let children =
        match script with
        | RecBulletml.Action (_, cs) -> cs
        | _ -> []
      let done_, loop, progs =
        match p with
        | PAction (d, l, ps) -> d, l, ps
        | _ -> false, None, children |> List.map Progress.initial
      if done_ then
        return Ended, p, fc
      else
        // loop が立っていれば、輪を解いた並びを走らせる。現行の
        // 「pa.loop があればそちらを running にする」と同じ
        let running = match loop with Some l -> l | None -> children
        let len = min (List.length progs) (List.length running)
        // 走査 1 マスぶんの処理。stopped または loopHit が立った後の呼び出しは
        // 何もしない（while が stop で抜けるのを、fold の中の早期リターンで書いた形）
        let step
            (accSim: Sim<Progress list * bool * bool * FireContext * (RecBulletml list * Progress list) option>)
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
                | RecBulletml.ActionRef (attrs, prams) ->
                    match rs.Action attrs.actionRefLabel prams with
                    | Some (RecBulletml.Action (_, expanded)) ->
                        let newRunning = expanded @ (running |> List.skip (idx + 1))
                        // 旧 expandActionRefOnce（= expandActionRefOnceRec を
                        // convertRecBulletmlEx へ通したもの）は、輪を 1 段
                        // 解いた瞬間に展開した中身の wait をまとめて引いていた
                        // （IntermediateParser.fs の convertRecBulletmlEx。
                        // Domain 5.3「木を組む段」と同じ、AimDir / EnemyAimDir
                        // を 0 に固定した Env）。ここが 5 つめの draw site
                        // （設計文書 5.3 参照）。bulletRef（Step.fire）と違い、
                        // actionRef はこのあと Init 相当を挟まないので、
                        // ここで引いた 1 回だけが最終値になる。展開していない
                        // 残りの兄弟（running の idx+1 から先）は今まで通り
                        // Progress.initial で組む——rootProgress は木を組む段の
                        // 引きを写すもので、ここではもう関係ない。
                        //
                        // 旧の pa.loop <- Some (expanded @ (tasks |> List.skip (num+1)))
                        // は残りの兄弟を「同じ ProcessableBulletml オブジェクトの
                        // まま」繋いでいた——もしその兄弟が前の解決ラウンドで
                        // すでに何か引いていれば、その状態（term など）を
                        // 保ったまま持ち越る。ここは Progress.initial で
                        // 毎回 作り直すので、その保持は写していない。
                        // それでも安全なのは、輪を解くたびに展開結果
                        // （expanded）の末尾に必ず同じ自己参照 actionRef が
                        // 再び現れ、actionCommand の走査がそこで Stop する
                        // ため——残りの兄弟（tail）へ実際に到達する形が
                        // 構成できない（輪でない actionRef は 1 段で実体へ
                        // 展開し尽くされ、輪はここでしか作られないため）。
                        // 到達しない枝の状態が保たれるかどうかは観測できない
                        // ので、直さずに歩き方だけ記録しておく

                        let refEnv = { env with AimDir = 0.0f; EnemyAimDir = 0.0f }
                        let expandedProgs = expanded |> List.map (rootProgress refEnv)
                        let remainingProgs =
                          running |> List.skip (idx + 1) |> List.map Progress.initial
                        return ps, true, cont, curFc, Some (newRunning, expandedProgs @ remainingProgs)
                    | _ ->
                        return ps, stopped, cont, curFc, loopHit
                | childScript ->
                    let! r, p', fc' = command rs childScript cur curFc
                    let p'' = if r = Ended then setDone p' else p'
                    let ps' = ps |> List.mapi (fun j x -> if j = idx then p'' else x)
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

  /// repeat。現行の repeatCommand を写す。
  ///
  /// times は呼ばれるたびに評価し直す（現行も while の外、呼び出しのたびに
  /// 引き直している）。子（action）が End を返すたびに周を 1 つ数え、
  /// times に届いたら子に finish を立てて終わり、届いていなければ次の周へ。
  /// 子が Stop / Continue を返したら、その場でこの呼び出しを終える
  /// （現行の while が continue' で止まるのと同じ）。
  ///
  /// times = 0 は while に 1 度も入らず、そのまま自分に finish を立てて
  /// 終わる。現行の癖をそのまま写した
  and repeat (rs: Resolvers) (script: RecBulletml) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    // 周を手続き的なループで回す。以前は [1..cycles] |> List.fold で
    // Sim.bind を cycles 回 積んでいたが、Sim.bind / Sim.run は
    // 末尾再帰ではないので、積んだ層ぶんだけ呼び出しスタックを消費する。
    // times に 9999 のような値を書く実物の弾幕があり（例:
    // [G_DARIUS]_homing_laser.xml）、1 コマ目にその周ぶんが丸ごと
    // 積まれて StackOverflow になる。現行の while は 1 周が定数の
    // スタックで済むので、ここも mutable な状態を直接持ち回るループに
    // 書き換える —— はずだったが、この while を sim { } の中に書くと、
    // コンパイラが while キーワードを builder.While（= SimBuilder.While）
    // へ展開してしまう。SimBuilder.While は「guard が真なら Sim.bind
    // (fun () -> While(guard,body)) (body ()) を返す」形で、1 周につき
    // Sim.bind を 1 段 積む再帰であり、これは末尾再帰にならない
    // （bind の中で f r1.Value を評価する時点で While が再帰的に評価される）。
    // 見た目が手続きループでも、sim { } の中に置いた時点で元の
    // StackOverflow へ戻っていた。
    //
    // 避けるため、この関数は sim { } を使わず Sim を直接組み立てる。
    // 中身は素の F# 関数本体になるので、while は CE を経由しない
    // 本物の手続きループとしてコンパイルされる
    Sim (fun env self0 ->
      let timesStr, body =
        match script with
        | RecBulletml.Repeat (Times t, b) -> t, b
        | _ -> "0", RecBulletml.NotCommand
      let times = getValue env timesStr |> int
      let num0, done0, child0 =
        match p with
        | PRepeat (n, d, c) -> n, d, c
        | _ -> 0, false, Progress.initial body
      // 1 回の呼び出しで進む周の数は、多くても times - num0。子が End を
      // 返すたびに 1 つ数え、それ以外（Stop / Continue）は go を落として
      // その場で抜ける
      let cycles = max 0 (times - num0)
      // 現行の repeatCommand は while の中で actionElm を Action へ
      // パターンマッチし、それ以外なら failwith する。while が 1 度も
      // 回らなければ（times に届かない・num0 が既に times 以上）この
      // チェックへは到達しないので、cycles > 0 のときだけ見る。
      //
      // DTD は repeat (times, (action | actionRef)) で actionRef も許すが、
      // パーサ（IntermediateParser.convertRefBulletmlIn）は自己参照だけ
      // 展開せずに残す（輪を解くのは走らせる側の仕事のため）。repeat の
      // 直下が actionRef になって残るのは、その actionRef が自分を
      // 直接包む action への自己参照であるときだけ —— 通常の（自己参照で
      // ない）actionRef は 1 段めで実体の Action へ展開し尽くされる。
      // 例: <action label="X">...<repeat><times>N</times>
      // <actionRef label="X"/></repeat></action>。この形は DTD 上 合法で、
      // 現行はここへ実際に到達して落ちる（equivalence の橋は「片方だけ
      // 例外」を割れとして拾うので、ここを黙って通すと後で橋の側から
      // 誤診断される）
      if cycles > 0 then
        match body with
        | RecBulletml.Action _ -> ()
        | _ -> failwith "repeatCommand: repeat の子が action ではない"
      let mutable num = num0
      let mutable child = child0
      let mutable curFc = fc
      let mutable st = self0
      // effects <- effects @ w は cycles について二乗になる（左辺がコマを
      // 重ねるたびに伸びる）。ResizeArray への追加は償却定数時間なので、
      // ここへ積んでから最後に 1 回だけ list へ畳む。並びは変えない
      let effectsAcc = ResizeArray<Effect>()
      let mutable stopped = false
      let mutable cont = false
      let mutable go = true
      let mutable i = 0
      while go && i < cycles do
        i <- i + 1
        if isDone child then
          // 現行の「子が既に finish なら、走らせずに repeatNum だけ増やす」。
          // num が times に届くのと同時に子へも finish が立つので、
          // この枝には実際には届かない。それでも現行の形のまま残す
          let num' = num + 1
          num <- num'
          go <- num' < times
        else
          let (r, child', fc'), st', w = Sim.run env st (action rs body child curFc)
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
                // 次の周のために子の並びを作り直す。現行の
                // running |> Seq.iter Init に当たる（resetBody / resetChild 参照）
                resetBody env body child'
            go <- num' < times
      let value =
        if stopped then Stopped, PRepeat (num, done0, child), curFc
        elif cont then Continue, PRepeat (num, done0, child), curFc
        else Ended, PRepeat (num, true, child), curFc
      { Value = value; State = st; Emit = fun rest -> (List.ofSeq effectsAcc) @ rest })

  /// fire。現行の fireCommand と createTask の両方を写す。
  ///
  /// fireCommand が撃つ側の FireContext（SrcDir / SrcSpeed）を先に決め、
  /// createTask がそれを使って撃たれた弾の Dir / Speed を組む。この 2 つは
  /// 現行では同じ値をそれぞれの場所で入れ直しており、順序（fire 側の
  /// direction/speed → bullet 側の direction/speed → 未指定なら fire 側の
  /// 値で埋める）に意味がある。
  ///
  /// 撃たれた弾の Tops は Progress.initial ではなく resetChild を通す。
  /// 現行の createTask 冒頭 bulletElm.Init(env) が Wait / ChangeDirection /
  /// ChangeSpeed の値を Action / Repeat / Fire / Bullet を辿って先に引いて
  /// おり（Init の実装は Processable.fs 参照）、resetChild は repeat の
  /// 周ざかりで使っている、それと同じ辿り方をする既存の写し。ここを
  /// Progress.initial に戻すと、その分の乱数消費が丸ごと消えて弾が
  /// 撃たれた瞬間から乱数列がずれる
  and fire (rs: Resolvers) (script: RecBulletml) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      let dirOpt, spdOpt, bulletSrc =
        match script with
        | RecBulletml.Fire (_, d, s, b) -> d, s, b
        | _ -> None, None, RecBulletml.NotCommand
      // bulletRef は 1 段だけ解く。fire のたびに新しい弾ができるので 1 段で足りる
      let bulletElm =
        match bulletSrc with
        | RecBulletml.BulletRef (attrs, prams) ->
            match rs.Bullet attrs.bulletRefLabel prams with
            | Some (RecBulletml.Bullet (_, _, _, actions) as x) ->
                // 旧 expandBulletRefOnce（= expandBulletRefOnceRec を
                // convertRecBulletmlEx へ通したもの）は、解決した瞬間に
                // bullet 本体の中の wait をまとめて引いていた（設計文書 5.3
                // 「木を組む段」と同じ、AimDir / EnemyAimDir を 0 に固定した
                // Env）。この直後、下の resetChild が createTask の
                // bulletElm.Init(env) に当たる引き直しをもう一度するので、
                // wait 1 個につき乱数を 2 回 引くのが正しい（1 回めの値は
                // 2 回めで上書きされて捨てられるが、消費そのものは残す）。
                // リテラルで埋め込んだ bullet はこの 1 回めの引きを
                // rootProgress の Fire の腕（根の弾を組むとき）または
                // 文書読み込み時の convertRecBulletmlEx（撃たれた弾のテンプレ
                // 自身が根の top* の中に literal で書いてある場合）で
                // 既に済ませているので、ここへは bulletRef で解決したときだけ来る
                actions |> List.iter (rootProgress { env with AimDir = 0.0f; EnemyAimDir = 0.0f } >> ignore)
                x
            | Some x -> x
            // ここへは実際には来ない。ラベルが存在しない bulletRef は
            // convertRefBulletmlIn（IntermediateParser.fs）が構文解析の時点で
            // BulletmlDTDViolationException を投げて弾く。ここまで RecBulletml.BulletRef
            // のまま残るのは輪（自己参照）だけで、輪は tryFindBullet が一度
            // Some を返した相手同士でしか作られない。rs.Bullet（= 同じ document・
            // 同じラベルで tryFindBullet をもう一度呼ぶだけの expandBulletRefOnce）が
            // ここで None を返すことは無い
            | None -> bulletSrc
        | _ -> bulletSrc
      let revise = (float32 System.Math.PI) / 180.f
      let aim = if self.Kind = BulletType.Player then env.EnemyAimDir else env.AimDir
      // fire 側の direction で SrcDir を決める。現行の fireCommand と同じ順
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
        | RecBulletml.Bullet (_, d, s, acts) -> d, s, acts
        | _ -> None, None, []
      // 撃たれた弾を組む。現行の createTask に当たる
      let mutable child =
        { Pos = self.Pos
          Speed = 0.0f
          Dir = 0.0f
          Accel = { X = 0.0f; Y = 0.0f }
          Kind = self.Kind
          IsBullet = true
          HasFired = false
          Tops = bActions |> List.map (fun a -> a, resetChild env a, FireContext.zero)
          PendingBulletAim = false }
      // bullet の direction。type ごとに基準が変わる。
      //
      // aim 系（type 省略 or "aim"）だけが撃つ側（fire 側）と違う基準を使う。
      // 旧 createTask (bulletElm) (bulletmlTask) (bullet: IBulletmlObject) は
      // fireCommand から newBullet を渡されて呼ばれ、中で bullet.GetAimDir() /
      // GetEnemyAimDir() を読む —— この bullet は newBullet 自身であり、
      // GetNewBullet() 直後でまだ位置が (0, 0) のまま（撃った側の位置を
      // コピーするのはそのあと）。fire 側の aim（この関数の env から出す
      // 上の aim）は撃った側の位置に依るので、同じ変数を bullet 側にも
      // 使うと「新しい弾の aim のつもりで撃った側の aim を使う」取り違えに
      // なる。撃たれた弾のオブジェクトはこの時点では存在しない（Spawn は
      // 値で、実体は BulletRunner.applySpawn が newBullet として後で作る）ので、
      // ここでは revise 済みの角度だけを Dir に留めて PendingBulletAim を
      // 立て、実際に aim を足す仕上げは applySpawn（newBullet を得た直後・
      // 位置をコピーする前）に委ねる
      match bDir with
      | Some (Direction (attrs, v)) ->
          let value = getValue env v * revise
          match attrs with
          | Some a ->
              match a.directionType with
              | DirectionType.Sequence -> child <- { child with Dir = calcDir (fc.SrcDir + value) }
              | DirectionType.Absolute -> child <- { child with Dir = calcDir value }
              | DirectionType.Relative -> child <- { child with Dir = calcDir (child.Dir + value) }
              | _ -> child <- { child with Dir = value; PendingBulletAim = true }
          | None -> ()
      | None -> ()
      // bullet の speed。現行はこの式を 2 回 getValue で読む
      // （createTask、続けて fireCommand）。
      // 1 回め（createTask 相当）は撃たれた弾自身のまだ 0 の Speed を
      // relative の基準にして書き込むが、この書き込みは直後の 2 回めで
      // 必ず上書きされるので値そのものは使われない。それでも getValue は
      // 式の中身に関わらず env.Rand() を無条件に呼ぶ（$rand の有無を
      // 問わない）ので、この 1 回ぶんの乱数消費だけは消してはいけない。
      // 2 回め（fireCommand 相当）が実際に使う値で、relative の基準は
      // 撃った側 self.Speed になる（1 回めと基準が違う点も現行のまま）
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
      // 撃つ側の SrcSpeed / SpeedInit を決める（現行の fireCommand の対応する枝）。
      //
      // fc.SpeedInit は「この top から一度でも bullet 側の speed を SrcSpeed に
      // 採用したか」を覚える、撃つ側 1 つにつき一生ものの latch（現行は
      // ここを一度も false へ戻していない。一度 true になったら二度と
      // 立て直さない）。PFire の done_ や PChangeDir の done_ と違い、
      // 撃つたびに引き継がれる状態なので Progress ではなく FireContext 側に乗る。
      //
      // latch がまだ false で、かつ今回 bullet 側に speed が書いてあるなら
      // （bSpd.IsSome）、bullet 側の解決済みの速さ（child.Speed）をそのまま
      // SrcSpeed に採用して latch を立てる。このとき fire 側の speed
      // （pf.speed 相当の spdOpt）は getValue すら呼ばれない
      // ——fire に speed を書いても、この分岐では丸ごと無視される（現行の
      // fireCommand も if 全体を先に見る作りで、spdOpt の有無を後から
      // 見る作りではない点が、前回の版の間違いだった）。
      // それ以外（latch が既に true、または今回 bullet 側に speed が無い）は
      // spdOpt をそのまま見る。ここでは latch には触れない（現行もここでは
      // SpeedInit へ書き込まない）
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

  /// 1 コマ進める。現行の BulletRunner.runWithEnv を写す。
  ///
  /// runWithEnv であって run ではない。run は envOfGlobal でグローバルから
  /// Env を組んでから runWithEnv を呼ぶだけの 1 行の橋渡しで、ここが写して
  /// いる中身（top* の走査・endCount・差分の組み立て）は runWithEnv 側にある。
  /// step は Env を引数で受け取るので、その橋渡しの分は要らない
  ///
  /// top* は 1 本ずつ独立に回す。ある top が wait で止まっても、
  /// それはその top の話なので、後ろの top はこのフレームでも回す
  let step (rs: Resolvers) (env: Env) (self: BulletState) : StepResult =
    let mutable st = self
    let mutable effects : Effect list = []
    let mutable tops = []
    let mutable endCount = 0
    // FireContext は現行では弾 1 つにつき 1 個しかない
    // （BulletmlTask.FireData.[ActiveTaskIndex]。ActiveTaskIndex は生成時の
    // 0 から一度も変わらない —— BulletRunner.fs の convertBulletmlTask /
    // createTask を見ても ActiveTaskIndex への代入は 0 しかない）。
    // top* が複数あっても fireCommand は常に同じ FireData.[0] を読み書きする
    // ので、sequence の積み上がりは top をまたいで続く。Tops の組が
    // fc を 1 つずつ持つ形は「添字の対応を構造で保証する」ためのものだが、
    // 値そのものは全 top で同じでなければならない。フレームの頭で
    // 先頭の組から読み、走査のあいだ 1 個の変数で持ち回って、
    // 書き戻すときは全部の組に同じ値を積み直すことでこれを保つ
    let mutable sharedFc =
      match self.Tops with
      | (_, _, fc0) :: _ -> fc0
      | [] -> FireContext.zero
    for (script, prog, _) in self.Tops do
      if not (isDone prog) then
        let (r, prog', fc'), st', w = Sim.run env st (action rs script prog sharedFc)
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
    // sharedFc は走査が進むごとに更新されるので、途中で組へ積むと
    // 先に処理した top ほど古い値のまま固まる（実際にこれで壊れた：
    // top1 が自分の処理直後の値だけを持ち越し、次のコマで top1 単独の
    // 続きから再開してしまい、top2 が足した分が丸ごと落ちた）。
    // 走査が終わったあとの最終値を、全部の組へ一括で積み直す
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
