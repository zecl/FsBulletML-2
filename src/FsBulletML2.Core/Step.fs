namespace FsBulletML2

open FsBulletML2.Domain
open FsBulletML2.Eval

/// 命令を 1 つずつ進める。**落とした `BulletRunner.runCommand` を写したもの。**
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

  /// 角度を 0 〜 2π に丸める。落とした `BulletRunner.calcDir` と同じ式。
  ///
  /// **いまはここが唯一の置き場所。** あちらが在ったころは、compile 順の都合で
  /// ここを指すだけの別名を向こうに置いていた
  let internal calcDir (dir: float32) =
    if (float dir > 2. * System.Math.PI) then dir - float32 (2. * System.Math.PI)
    elif (float dir < 0.) then dir + float32 (2. * System.Math.PI)
    else dir

  /// 型の上では届くが実際には来ない腕の既定値。"0" を毎回 読み直さないよう
  /// 1 つ 持っておく。
  ///
  /// **ここを定数 0.0f に置き換えてはいけない。** getValue は式の中身に
  /// よらず乱数を 1 回 引くので、呼び出しを消すと引く回数が変わり、
  /// 全弾幕の軌跡がずれる
  let private zeroExpr = numExpr "0"

  /// wait。旧の waitCommand を写す。
  ///
  ///   term >= 0 なら 1 減らす
  ///   その後まだ term >= 0 なら Stopped、そうでなければ Ended
  ///
  /// term の評価は初回だけ。旧は Init() が入れているが、こちらは
  /// initial が評価しないので started で 1 度だけ評価する
  ///
  /// term を 1 減らしてから判定する形は、旧の off-by-one を写した。
  /// 単純化してはいけない
  let wait (waitExpr: Expr.NumExpr) (p: Progress) : Sim<RunState * Progress> =
    sim {
      let! env = Sim.ask
      let started, left =
        match p with
        | PWait (s, l) -> s, l
        | _ -> false, 0.0f
      // 台本まるごとでなく wait の中身を受けるので、
      // 「wait 以外が来たら 0」という届かない腕が要らない
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
  /// 終わり方が changeDirection / changeSpeed と違う。
  /// term < 0 で終わり、そのとき加算しない
  ///
  /// 旧は木を組む段（convertRecBulletmlEx）が省略された軸を
  /// { Absolute, "0" } で埋めていたので、そこから先は常に値が入っていた。
  /// **いまは埋めない** —— `Action.Accel` が option をそのまま持ち回るので、
  /// ここで None を処理する。その場合も旧と同じく
  /// "0" として扱い、catch-all の計算をする：
  /// 現状維持ではなく、既存の加速度を term フレームかけて 0 へ寄せる。
  /// getValue を通すこと自体にも意味がある（式の中身に関わらず乱数を進める）
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
  /// accel と終わり方が違う。term <= 0 で終わり、そのときも加算してから終わる。
  /// 終わるときは term を getValue initTerm に戻す。repeat の中で回り直すときに
  /// この値が効くので、消すと 2 周目の挙動が変わる
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
  /// 加算してから終わる。終わるときに term を戻すのも changeDirection と同じ
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
        // term を戻すのは repeat の次周のため。戻すと left がまた正になるので、
        // 終わったことは left でなく done_ で覚えておく
        return Ended, PChangeSpeed (true, true, getValue env term, delta)
      else
        return Continue, PChangeSpeed (true, false, left, delta)
    }

  /// repeat が次の周へ進むときの、running 1 要素ぶんの t.Init(env) に当たる。
  ///
  /// 旧の running |> Seq.iter Init は、1 つの並びを順番に辿りながら
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
  /// （旧の Init も pa.loop <- None を通ってから children を辿る。
  /// これは repeat 直下の body だけ特別扱いする対象で、resetBody 側で
  /// 持ち越す。入れ子の loop はここで素直に消してよい）。
  /// fire は撃たれた弾の action へ潜って値を引くだけで、Progress に
  /// 進行状態を持たない（旧はテンプレートの mutable な木へ書き戻すが、
  /// 新しい弾は fire のたびに新しい Progress で始まるので、ここでは
  /// 引いた分を捨ててよい）
  /// internal（private ではない）にしてあるのは、橋（TraceNew.fs）が
  /// 全 top 終了時の引き直しにこれを使うため（旧の
  /// task.Init(envOfGlobal o)。Original が None の task の Init は
  /// この歩き方と同じ。Domain 5.6 参照）。木を組む段の wait だけの
  /// 引きは別の歩き方が要るので rootProgress に分けてある
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
        actions |> List.iter (resetChildActionElm env >> ignore)
        PFire false
    // 以前は `| other -> Progress.initial other` だった。その other には
    // 「引かないでよい命令」と「そもそも命令の位置に来ない要素」が
    // 混ざっていた。型が分かれたので前者だけが残る
    | Action.Fire (_, _, _, BulletElm.BulletRef _) -> PFire false
    | Action.Accel _ | Action.Vanish
    | Action.FireRef _ | Action.ActionRef _ -> Progress.initial script

  /// repeat / bullet の子（action か actionRef）ぶん
  and internal resetChildActionElm (env: Env) (a: ActionElm) : Progress =
    match a with
    | ActionElm.Action (_, children) ->
        PAction (false, None, children |> List.map (resetChild env))
    | ActionElm.ActionRef _ -> PNoop

  /// 木を組む段（`BulletmlRead.foldConstants`。旧の名前は
  /// convertRecBulletmlEx）の wait だけの引き直し。
  ///
  /// BulletmlRead.fs の wait の腕は、撃つ弾ごとの Env が
  /// まだ無い木構築の段で、Aim を 0 に固定した Env で
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
  /// resetChild（旧の Init の写し）とは別の歩き方が要る理由は Domain 5.6。
  /// 撃たれた弾は Step.fire が resetChild（createTask の Init(env) の写し）を
  /// 通すのでここを通らない。根の BulletState を組むときにだけ使う
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
  /// 旧の running = match pa.loop with Some t -> t | None -> tasks を
  /// この周ざかりの reset でも同じ並びに使い、かつ pa.loop 自身は
  /// 触らずに持ち越す（旧の running |> Seq.iter Init は running の
  /// 要素ごとに Init を呼ぶだけで、actionElm 自身の Init は呼ばない
  /// ので pa.loop は変わらない）。actionRef の輪が一度解ければ、
  /// その周から先はもう解き直さない
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
  /// Action の解決子へ渡す形が組めない。以前はどちらも string を受けていて、
  /// 取り違えても型が通っていた
  type Resolvers =
    { Bullet : BulletLabel -> string list -> BulletElm option
      Action : ActionLabel -> string list -> ActionElm option }

  /// Progress が「終わった」を持っているか。旧の getFinish。
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

  /// 命令 1 つを振り分ける。旧の runCommand の match に当たる。
  ///
  /// fire だけ FireContext を書き換えるので戻り値に含めてあるが、fire の腕は
  /// まだここには無い（fire を足す Task で足す）。他の 5 つは FireContext に
  /// 触らないので、渡された fc をそのまま返す
  /// 命令 10 通りを漏れなく振り分ける。**中身をほどいて渡す。**
  ///
  /// 以前は台本まるごとを渡していたので、受け取る側それぞれが
  /// 「自分の腕でなければ既定値」という届かない match を持っていた。
  /// ほどいて渡すと、その match が要らなくなる
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
    // 展開していない参照。走査は止めず、終わりにする（以前の `| _ ->` と同じ）
    | Action.ActionRef _ | Action.FireRef _ -> sim { return Ended, p, fc }

  /// action。旧の actionCommand を写す。
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
  /// repeat / bullet の子、および top* の台本ぶん。
  ///
  /// actionRef のときは「子が空の action」として通す。以前は台本まるごとを
  /// action へ渡していて、Action でなければ children が [] になっていた。
  /// その振る舞いをそのまま写す（repeat の子に actionRef を書く弾幕がある）
  and actionElm (rs: Resolvers) (script: ActionElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    NodeTrace.visit (box script)
    match script with
    | ActionElm.Action (attrs, children) -> action rs attrs children p fc
    | ActionElm.ActionRef _ -> action rs { actionLabel = None } [] p fc

  /// **ps / running を配列にして idx で触る形は、試して戻した。
  /// ただし却下の範囲は「子が 2〜3 個 の台本」まで。**
  ///
  /// 形の上では二乗になっている —— `List.item idx` が O(idx)、書き戻しの
  /// `List.mapi` が毎回 全体を作り直すので、子が n 個 の action は走査で
  /// O(n^2) のセルを作る。
  ///
  /// だが実物の action は子が数個 しかない。`COUNTS.md` の
  /// 「List.mapi 作ったセル」は homing で 9,168、`action` 訪問が 3,233 で、
  /// **平均 2.8 個**。配列にすると `List.toArray` を 2 本 と `List.ofArray` を
  /// 1 本、action を訪れるたびに作るので、そちらのほうが高くつく。
  ///
  /// 実測（`--alloc`）で確保が 4 本 とも増えた ——
  /// move +0.9% / 5way +0.3% / 10Way +0.3% / homing +1.1%。
  ///
  /// ### その 4 本 に、狙っている形は載っていなかった
  ///
  /// 手が当てようとしているのは**子の多い action**。ところが測った 4 本 の
  /// action は、子が最大 2 / 2 / 2 / 3 個 しかない。
  ///
  /// **却下そのものは 4 本 については正しい。一般化されていたのが誤り。**
  ///
  /// コーパス 227 本 を静的に数えると、`<action>` は 1,695 個 で子は平均
  /// 3.09 個、**最大 32 個**（`[OtakuTwo]_dis_bee_1`）。子 10 個 以上 の
  /// action が 69 個 あり、子全体の 17.9% を占める。
  ///
  /// `--alloc` / `--counts` に `wide`（その最大 32 個 の台本）を足した。
  /// **これで「子の多い action」が物差しに載った。**
  ///
  /// ### 載せて上界を測った。**当てる先が無い**
  ///
  /// 使い捨ての計数を入れて、この `List.mapi` が作るセルを数えた。
  /// **`COUNTS.md` の 4 本 と完全に一致したので、計数は軸の上に乗っている。**
  ///
  /// **バイトに直すときの 32 B は、測った数ではなく型の並びから出した数**
  /// （x64 のヘッダ 16 + Head 8 + Tail 8）。24 でも 40 でも下の結論
  /// （上界 0.4〜6.7%、いちばん大きいのは子の少ない台本）は変わらない。
  ///
  ///     台本      mapi セル   訪問   セル/訪問      バイト   走行の確保に占める
  ///     move            386    131      2.95      12,352            3.84%
  ///     5way            480    300      1.60      15,360            0.42%
  ///     10Way         1,500    780      1.92      48,000            0.64%
  ///     homing        9,168  3,233      2.84     293,376            6.71%
  ///     wide          3,332    361      9.23     106,624            3.30%
  ///
  /// **wide は狙いどおり セル/訪問 が 3 倍（9.23）ある。形は在った。**
  /// ところが走行に占める割合は 3.30% で、**子が最大 3 個 の homing
  /// （6.71%）より小さい。** wide は弾を撃つので他の確保のほうが大きい。
  ///
  /// **上界は 0.4〜6.7%。しかも いちばん大きいのは子の少ない台本。**
  /// 「子の多い action が重い」という見立てのほうが逆だった。
  ///
  /// ### 費用の形は O(n^2) ではない
  ///
  /// 走査は最初の Stopped で抜けるので、1 回 の走査で書き換わるのは
  /// たいてい 1 マス。費用は **(書き換えた数) x n** で、n^2 になるのは
  /// 止まらずに全部 走ったときだけ。実測の セル/訪問 1.6〜9.2 が
  /// それを言っている（n^2 なら n の 2 乗 に近い数が出る）。
  ///
  /// ### n 以上 を払う形はどれも負ける。**が、n 未満 の形があった**
  ///
  ///     配列にする         toArray x2 + ofArray x1 の固定費（実測で 4 本 とも増）
  ///     通過分を逆順に積む   2n（revAppend で 1 パスにしても）
  ///     尻尾を共有する      **idx+1**（List.updateAt）
  ///
  /// **いちど「既知の書き換えはどれも高くつく」と書いた。n 以上 を払う形
  /// しか見ていなかったのが誤り。** Progress は不変なので、書き換えた位置
  /// より後ろは作り直さずそのまま繋げる。走査は最初の Stopped で抜けるので
  /// idx は小さいほうに偏り、そこがそのまま効きになる。
  ///
  /// updateAt に替えて実測（--alloc）:
  ///
  ///     台本         前          後        差     mapi の上界に対して
  ///     move      321,520    313,360   -2.54%          66%
  ///     5way    3,619,032  3,605,592   -0.37%          87%
  ///     10Way   7,530,640  7,490,320   -0.54%          84%
  ///     homing  4,372,632  4,122,872   -5.71%          85%
  ///     wide    3,233,256  3,163,432   -2.16%          65%
  ///
  /// **上界を超えていない。** 残りは updateAt が払う idx+1 のぶん。
  ///
  /// まだ残っている当てる先は Progress の節そのもの（command / action /
  /// repeat の訪問ごとに 1 個、40 B 以下）。homing で 342,240 B ＝ 走行の
  /// 7.8%。取るには Progress を可変にすることになる（別議題。F# の DU は
  /// 可変フィールドを持てないので型ごと作り替えになる。橋が 227 本 あるので
  /// 軌跡では確かめられるが、いまは弾どうしが節を共有していない前提で
  /// Progress.initial が組んでいるところが効いてくる）。
  ///
  /// **形が O(n^2) であることと、その台本でそこが効くことは別。**
  /// **効かないと測った台本に、その形が載っていたかも別。**
  /// **そして形が載っていても、上界が小さければやはり当てる先は無い。**
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
        // loop が立っていれば、輪を解いた並びを走らせる。旧の
        // 「pa.loop があればそちらを running にする」と同じ
        let running = match loop with Some l -> l | None -> children
        let len = min (List.length progs) (List.length running)
        // 走査 1 マスぶんの処理。stopped または loopHit が立った後の呼び出しは
        // 何もしない（while が stop で抜けるのを、fold の中の早期リターンで書いた形）
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
                        // 旧 expandActionRefOnce は、輪を 1 段
                        // 解いた瞬間に展開した中身の wait をまとめて引いていた
                        // （BulletmlRead.fs の foldConstants。
                        // Domain 5.3「木を組む段」と同じ、Aim
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

                        // **aim を 0 に潰した Env を作って渡していたが、外した。**
                        // rootProgress が Env から読むのは getValue 経由の
                        // Rand と Rank だけ（Eval.fs）で、aim へ届く腕が無い。
                        // 潰しても潰さなくても同じ値になる
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
                    // **1 マスだけ差し替える。尻尾は共有する。**
                    // List.mapi は毎回 n セル 作り直していた。updateAt は
                    // idx までを作り直して idx+1 から先を共有するので idx+1 セル。
                    // 走査は最初の Stopped で抜けるので idx は小さいほうに偏る
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
  /// times は呼ばれるたびに評価し直す（旧も while の外、呼び出しのたびに
  /// 引き直している）。子（action）が End を返すたびに周を 1 つ数え、
  /// times に届いたら子に finish を立てて終わり、届いていなければ次の周へ。
  /// 子が Stop / Continue を返したら、その場でこの呼び出しを終える
  /// （旧の while が continue' で止まるのと同じ）。
  ///
  /// times = 0 は while に 1 度も入らず、そのまま自分に finish を立てて
  /// 終わる。旧の癖をそのまま写した
  and repeat (rs: Resolvers) (Times timesStr) (body: ActionElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    // 周を手続き的なループで回す。以前は [1..cycles] |> List.fold で
    // Sim.bind を cycles 回 積んでいたが、Sim.bind / Sim.run は
    // 末尾再帰ではないので、積んだ層ぶんだけ呼び出しスタックを消費する。
    // times に 9999 のような値を書く実物の弾幕があり（例:
    // [G_DARIUS]_homing_laser.xml）、1 コマ目にその周ぶんが丸ごと
    // 積まれて StackOverflow になる。旧の while は 1 周が定数の
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
    fun env self0 ->
      // times と body をほどいて受けるので、以前ここに在った
      // 「repeat 以外が来たら NotCommand」という届かない腕が要らない
      let times = getValue env timesStr |> int
      let num0, done0, child0 =
        match p with
        | PRepeat (n, d, c) -> n, d, c
        | _ -> 0, false, Progress.initialActionElm body
      // 1 回の呼び出しで進む周の数は、多くても times - num0。子が End を
      // 返すたびに 1 つ数え、それ以外（Stop / Continue）は go を落として
      // その場で抜ける
      let cycles = max 0 (times - num0)
      // 旧の repeatCommand は while の中で actionElm を Action へ
      // パターンマッチし、それ以外なら failwith する。while が 1 度も
      // 回らなければ（times に届かない・num0 が既に times 以上）この
      // チェックへは到達しないので、cycles > 0 のときだけ見る。
      //
      // DTD は repeat (times, (action | actionRef)) で actionRef も許すが、
      // パーサ（BulletmlRead.convertRefBulletmlIn）は自己参照だけ
      // 展開せずに残す（輪を解くのは走らせる側の仕事のため）。repeat の
      // 直下が actionRef になって残るのは、その actionRef が自分を
      // 直接包む action への自己参照であるときだけ —— 通常の（自己参照で
      // ない）actionRef は 1 段めで実体の Action へ展開し尽くされる。
      // 例: <action label="X">...<repeat><times>N</times>
      // <actionRef label="X"/></repeat></action>。この形は DTD 上 合法で、
      // 旧はここへ実際に到達して落ちる（equivalence の橋は「片方だけ
      // 例外」を割れとして拾うので、ここを黙って通すと後で橋の側から
      // 誤診断される）
      if cycles > 0 then
        match body with
        | ActionElm.Action _ -> ()
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
          // 旧の「子が既に finish なら、走らせずに repeatNum だけ増やす」。
          // num が times に届くのと同時に子へも finish が立つので、
          // この枝には実際には届かない。それでも旧の形のまま残す
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
                // 次の周のために子の並びを作り直す。旧の
                // running |> Seq.iter Init に当たる（resetBody / resetChild 参照）
                resetBody env body child'
            go <- num' < times
      let value =
        if stopped then Stopped, PRepeat (num, done0, child), curFc
        elif cont then Continue, PRepeat (num, done0, child), curFc
        else Ended, PRepeat (num, true, child), curFc
      // 1 個 も積まなかった周は ValueNone。SimResult.Emit の定義どおり
      // 「積むものが無い」を型で持つ。確保は --alloc のベンチ 4 本 で
      // move -1,440 B / homing -25,208 B（5way と 10Way は 0）。
      // **効きが小さい** —— repeat の周は効果を 1 個 は積むことが多い、
      // ということ。揃えてある主な理由は性能ではなく、Emit を読む側が
      // 「空かどうか」を場所ごとに疑わなくて済むようにするため
      let emit =
        if effectsAcc.Count = 0 then ValueNone
        else ValueSome (fun rest -> (List.ofSeq effectsAcc) @ rest)
      { Value = value; State = st; Emit = emit }

  /// fire。旧の fireCommand と createTask の両方を写す。
  ///
  /// fireCommand が撃つ側の FireContext（SrcDir / SrcSpeed）を先に決め、
  /// createTask がそれを使って撃たれた弾の Dir / Speed を組む。この 2 つは
  /// 旧では同じ値をそれぞれの場所で入れ直しており、順序（fire 側の
  /// direction/speed → bullet 側の direction/speed → 未指定なら fire 側の
  /// 値で埋める）に意味がある。
  ///
  /// 撃たれた弾の Tops は Progress.initial ではなく resetChild を通す。
  /// 旧の createTask 冒頭 bulletElm.Init(env) が Wait / ChangeDirection /
  /// ChangeSpeed の値を Action / Repeat / Fire / Bullet を辿って先に引いて
  /// おり、resetChild は repeat の
  /// 周ざかりで使っている、それと同じ辿り方をする既存の写し。ここを
  /// Progress.initial に戻すと、その分の乱数消費が丸ごと消えて弾が
  /// 撃たれた瞬間から乱数列がずれる
  and fire (rs: Resolvers) (_attrs: FireAttrs) (dirOpt: Direction option) (spdOpt: Speed option)
           (bulletSrc: BulletElm) (p: Progress) (fc: FireContext)
      : Sim<RunState * Progress * FireContext> =
    sim {
      let! env = Sim.ask
      let! self = Sim.get
      // bulletRef は 1 段だけ解く。fire のたびに新しい弾ができるので 1 段で足りる
      let bulletElm =
        match bulletSrc with
        | BulletElm.BulletRef (attrs, prams) ->
            match rs.Bullet attrs.bulletRefLabel prams with
            | Some (BulletElm.Bullet (_, _, _, actions) as x) ->
                // 旧 expandBulletRefOnce は、解決した瞬間に
                // bullet 本体の中の wait をまとめて引いていた（設計文書 5.3
                // 「木を組む段」と同じ、Aim を 0 に固定した
                // Env）。この直後、下の resetChild が createTask の
                // bulletElm.Init(env) に当たる引き直しをもう一度するので、
                // wait 1 個につき乱数を 2 回 引くのが正しい（1 回めの値は
                // 2 回めで上書きされて捨てられるが、消費そのものは残す）。
                // リテラルで埋め込んだ bullet はこの 1 回めの引きを
                // rootProgress の Fire の腕（根の弾を組むとき）または
                // 文書読み込み時の foldConstants（撃たれた弾のテンプレ
                // 自身が根の top* の中に literal で書いてある場合）で
                // 既に済ませているので、ここへは bulletRef で解決したときだけ来る
                actions |> List.iter (rootProgressActionElm env >> ignore)
                x
            | Some x -> x
            // ここへは実際には来ない。ラベルが存在しない bulletRef は
            // convertRefBulletmlIn（BulletmlRead.fs）が構文解析の時点で
            // BulletmlDTDViolationException を投げて弾く。ここまで BulletElm.BulletRef
            // のまま残るのは輪（自己参照）だけで、輪は tryFindBullet が一度
            // Some を返した相手同士でしか作られない。rs.Bullet（= 同じ document・
            // 同じラベルで tryFindBullet をもう一度呼ぶだけの expandBulletRefOnce）が
            // ここで None を返すことは無い
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
      // bullet の direction。type ごとに基準が変わる。
      //
      // aim 系（type 省略 or "aim"）だけが撃つ側（fire 側）と違う基準を使う。
      // 旧 createTask は fireCommand から newBullet を渡されて呼ばれ、中で
      // その newBullet 自身の GetAimDir() / GetEnemyAimDir() を読んでいた
      // ——「撃たれた弾から見た向き」であって、撃った側から見た向きではない。
      // fire 側の aim（この関数の env から出す上の aim）は撃った側の位置に
      // 依るので、同じ値を bullet 側にも使うと基準の取り違えになる
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
                  // 撃たれた弾の実体はこの時点では無いが、その弾がどこに出るかは
                  // フロントエンドが知っているので env.Spawn で受け取っている
                  // （旧の BulletRunner.envOfGlobal が GetSpawnAimDir を読んでいた）。
                  // これで Spawn は値として完結し、実体を見て仕上げる必要が無い。
                  // Player / Enemy の振り分けは撃った側の種別で行う——撃たれた弾の
                  // 種別は例外なく撃った側からその場で複写されるので同じになる
                  let spawnAim =
                    if child.Kind = BulletType.Player then env.Spawn.ToEnemy else env.Spawn.ToPlayer
                  child <- { child with Dir = calcDir (spawnAim + value) }
          | None -> ()
      | None -> ()
      // bullet の speed。旧はこの式を 2 回 getValue で読む
      // （createTask、続けて fireCommand）。
      // 1 回め（createTask 相当）は撃たれた弾自身のまだ 0 の Speed を
      // relative の基準にして書き込むが、この書き込みは直後の 2 回めで
      // 必ず上書きされるので値そのものは使われない。それでも getValue は
      // 式の中身に関わらず env.Rand() を無条件に呼ぶ（$rand の有無を
      // 問わない）ので、この 1 回ぶんの乱数消費だけは消してはいけない。
      // 2 回め（fireCommand 相当）が実際に使う値で、relative の基準は
      // 撃った側 self.Speed になる（1 回めと基準が違う点も旧のまま）
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
      // 撃つ側の SrcSpeed / SpeedInit を決める（旧の fireCommand の対応する枝）。
      //
      // fc.SpeedInit は「この top から一度でも bullet 側の speed を SrcSpeed に
      // 採用したか」を覚える、撃つ側 1 つにつき一生ものの latch（旧は
      // ここを一度も false へ戻していない。一度 true になったら二度と
      // 立て直さない）。PFire の done_ や PChangeDir の done_ と違い、
      // 撃つたびに引き継がれる状態なので Progress ではなく FireContext 側に乗る。
      //
      // latch がまだ false で、かつ今回 bullet 側に speed が書いてあるなら
      // （bSpd.IsSome）、bullet 側の解決済みの速さ（child.Speed）をそのまま
      // SrcSpeed に採用して latch を立てる。このとき fire 側の speed
      // （pf.speed 相当の spdOpt）は getValue すら呼ばれない
      // ——fire に speed を書いても、この分岐では丸ごと無視される（旧の
      // fireCommand も if 全体を先に見る作りで、spdOpt の有無を後から
      // 見る作りではない点が、前回の版の間違いだった）。
      // それ以外（latch が既に true、または今回 bullet 側に speed が無い）は
      // spdOpt をそのまま見る。ここでは latch には触れない（旧もここでは
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

  /// 1 コマ進める。落とした `BulletRunner.runWithEnv` を写したもの。
  ///
  /// runWithEnv であって run ではない。run は envOfGlobal でグローバルから
  /// Env を組んでから runWithEnv を呼ぶだけの 1 行の橋渡しで、ここが写して
  /// いる中身（top* の走査・endCount・差分の組み立て）は runWithEnv 側にあった。
  /// step は Env を引数で受け取るので、その橋渡しの分は要らない
  ///
  /// top* は 1 本ずつ独立に回す。ある top が wait で止まっても、
  /// それはその top の話なので、後ろの top はこのフレームでも回す
  /// **ここの `effects @ w` / `tops @ [...]` は ResizeArray にしない。**
  ///
  /// repeat の側（effectsAcc）は周の数だけ積むので二乗になるが、こちらが
  /// 回るのは top の本数ぶんだけ。ベンチの 4 本 は**どれも top が 1 本**で、
  /// そのとき `[] @ w` は w をそのまま返し（コピーなし）、`[] @ [x]` は
  /// cons 1 個 で済む。ResizeArray に替えると、毎コマ ResizeArray を 2 個
  /// 作るぶん**確保が増える**。
  ///
  /// top を何本も持つ台本でだけ効く形なので、そういう台本を物差しに
  /// 載せてから直すこと。**当てる先を測らずに「@ だから遅い」で直すと、
  /// 効かないどころか逆に振れる。**
  let step (rs: Resolvers) (env: Env) (self: BulletState) : StepResult =
    let mutable st = self
    let mutable effects : Effect list = []
    let mutable tops = []
    let mutable endCount = 0
    // FireContext は旧でも弾 1 つにつき 1 個しかなかった
    // （BulletmlTask.FireData.[ActiveTaskIndex]。ActiveTaskIndex は生成時の
    // 0 から一度も変わらない —— 落とした BulletRunner.fs の convertBulletmlTask /
    // createTask を見ても ActiveTaskIndex への代入は 0 しかなかった）。
    // top* が複数あっても fireCommand は常に同じ FireData.[0] を読み書きする
    // ので、sequence の積み上がりは top をまたいで続く。Tops の組が
    // fc を 1 つずつ持つ形は「添字の対応を構造で保証する」ためのものだが、
    // 値そのものは全 top で同じでなければならない。フレームの頭で
    // 先頭の組から読み、走査のあいだ 1 個の変数で持ち回って、
    // 書き戻すときは全部の組に同じ値を積み直すことでこれを保つ
    // **`tops @ [x]` と `effects @ w` は毎周 全体をコピーする。直さない。**
    //
    // 形は n^2 だが、n は top* の本数。コーパス 227 本 を数えると
    // **204 本 が 1 本**、残りは 2 本 13 / 3 本 9 / 4 本 1。
    // n=1 なら `[] @ [x]` は 1 セルで、逆順に積んで最後に戻す形は 2 セル。
    // **直すと負ける。**（`effects` も同じで `[] @ w` は w をそのまま返す）
    //
    // 走査の中の書き戻し（`ps` の 1 マス差し替え）は事情が違ったので
    // `List.updateAt` にしてある。あちらは n が子の数で、最大 32。
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
