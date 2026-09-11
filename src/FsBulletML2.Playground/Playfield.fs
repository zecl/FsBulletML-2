namespace FsBulletML2.Playground

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

/// 弾 1 発 の、**生涯 で 1 度 しか触らないもの**（v4.9.1 の段 2）。
///
/// 座標と差分はここに置かない —— あちらは毎コマ 全部 の弾をなめるので、
/// **成分ごとの配列**（`Playfield` の `pos` / `vel`）に居る。
///
/// **`Parent` は参照のまま握る。** 添字にすると消しで詰めたときに壊れ、
/// 世代を付けると**消えた親を辿れなくなる** —— v3.6 の系譜が変わる。
/// 握ると消えた親が解放されないのは前からで、深さのぶんだけ残る。
[<Sealed; AllowNullLiteral>]
type Born(run: BulletRun, isRoot: bool, from: int, parent: Born) =
  member val Run = run with get, set
  member _.IsRoot = isRoot
  /// この弾を撃った `fire` が、読んだ木の**書いてある順**の何番目 か（v3.2）。
  /// **撃たれていなければ -1**（根の敵）。決まらないときも -1
  member _.From = from
  /// この弾を撃った弾（v3.6）。**根は null。**
  ///
  /// **`From` を辿っても系譜にならない** —— あれは「撃った `fire` の添字」で、
  /// 同じ `fire` から撃たれた弾は全部 同じ数になる。親そのものが要る。
  member _.Parent = parent

/// 生きている弾の一覧。Bolero を知らない。
///
/// 1 コマの順は MonoGame の `RunTask` と同じ
/// （`Driver.step` → Delta を足す → Spawn は次コマ → Finished なら restart）。
/// **回している最中に足さない。** 今コマの Spawn は溜めて、消しのあとで足す。
///
/// ## 弾は成分ごとの配列に居る（v4.9.1 の段 2）
///
///     pos    1 点 3 つ —— x / y / 撃った腕の添字。**JS が読むのはこれ**
///     vel    1 点 2 つ —— 速い道の 1 コマ の差分
///     quiet  速い道の残り —— **-1 生涯 / 正 あと n コマ / 0 素の道**
///     born   生涯 で 1 度 だけ触るもの（`Born`）
///
/// **詰めた並びそのものを置き場にしてある。** 以前は弾 1 発 が
/// `Live` オブジェクトで、毎コマ
///
///     Tick          1 巡（位置を足す）
///     Pack          1 巡（3 つ 組 へ写す）
///     BulletCount   1 巡（根を除いて数える）
///
/// と **3 回 なめて**いた。1,706 発 のところで `Tick` が 1 発 46 ns ——
/// `x += dx` に 120 サイクル は出ないので、**散ったオブジェクトを
/// 引きに行く待ち**だった。
///
/// 置き場を詰めた並びそのものにすると、**`Pack` は数を返すだけ**になる。
[<Sealed>]
type Playfield private (front: IFrontEnv, field: Field, focus: Focus, rootRun: BulletRun) =

  // **1 点 につき 3 つ。** x / y / 撃った腕の添字（v4.7）——
  // **撃たれていない弾は -1**（根の敵がそう）。JS 側はそこを外す
  let mutable pos = Array.zeroCreate<float32> (256 * 3)
  // 速い道の 1 コマ の差分。**JS へは渡らない**
  let mutable vel = Array.zeroCreate<float32> (256 * 2)
  /// 速い道の残り。**-1 は生涯**（台本が空。v4.9.1 の段 1）、
  /// **正 は あと n コマ**（`wait` で止まっている。v4.9.2）、**0 は素の道**。
  ///
  /// 1 本 に畳んである —— 2 本 に分けると「どちらの道に居るか」を
  /// 2 か所 で持つことになり、消しで詰めるときに片方 だけ直す事故が出る
  let mutable quiet = Array.zeroCreate<int> 256
  let mutable born : Born[] = Array.zeroCreate 256
  let mutable n = 0

  // 今コマ に産まれた弾。**回している最中に足さない**
  let spawned = ResizeArray<struct (Born * float32 * float32)>()

  let mutable frame = 0
  let mutable pin = Unchecked.defaultof<GCHandle>

  /// 追っている弾（v3.1 の段 3）。**添字で持ち、消しで詰めたら追随させる。**
  ///
  /// 以前は参照で握っていた（添字はそのコマのものでしかないため）が、
  /// 弾がオブジェクトでなくなったので握る先が無い ——
  /// 代わりに**消すところで 1 か所 だけ直す**。追っていなければ -1
  let mutable picked = -1

  let rePin () =
    if pin.IsAllocated then pin.Free()
    pin <- GCHandle.Alloc(pos, GCHandleType.Pinned)

  /// 場所を確かめる。**足りなければ倍にする** —— pin し直しもここ
  let ensure (need: int) =
    if quiet.Length < need then
      let cap = max (need * 2) 256
      let p = Array.zeroCreate<float32> (cap * 3)
      let v = Array.zeroCreate<float32> (cap * 2)
      let f = Array.zeroCreate<int> cap
      let b : Born[] = Array.zeroCreate cap
      Array.blit pos 0 p 0 (n * 3)
      Array.blit vel 0 v 0 (n * 2)
      Array.blit quiet 0 f 0 n
      Array.blit born 0 b 0 n
      pos <- p
      vel <- v
      quiet <- f
      born <- b
      rePin ()
    elif not pin.IsAllocated then rePin ()

  let add (b: Born) (x: float32) (y: float32) =
    ensure (n + 1)
    pos.[n * 3] <- x
    pos.[n * 3 + 1] <- y
    pos.[n * 3 + 2] <- float32 b.From
    vel.[n * 2] <- 0.0f
    vel.[n * 2 + 1] <- 0.0f
    quiet.[n] <- 0
    born.[n] <- b
    n <- n + 1

  /// 死んだ枠に末尾を移す。**追っている弾の添字もここで直す** ——
  /// 直さないと、詰めたコマに黙って別の弾を指す
  let removeAt (i: int) (onUnpick: unit -> unit) =
    let last = n - 1
    if picked = i then
      picked <- -1
      onUnpick ()
    elif picked = last then picked <- i
    if i <> last then
      pos.[i * 3] <- pos.[last * 3]
      pos.[i * 3 + 1] <- pos.[last * 3 + 1]
      pos.[i * 3 + 2] <- pos.[last * 3 + 2]
      vel.[i * 2] <- vel.[last * 2]
      vel.[i * 2 + 1] <- vel.[last * 2 + 1]
      quiet.[i] <- quiet.[last]
      born.[i] <- born.[last]
    // **末尾の参照を落とす。** 残すと、消えた弾を配列が握り続ける。
    //
    // ### この守りは振る舞いに出ない
    //
    // 較正で当てて **赤 0 点**。読むのは `0 .. n - 1` だけなので、
    // 末尾より後ろ に何が残っていても答えは変わらない ——
    // **変わるのは、消えた弾（と その親の鎖）がいつ解放されるか**だけ。
    //
    // **それでも残す。** 面は建て直すまで生き続けるので、落とさないと
    // **いちばん多かったコマの弾が全部 残る**。門では見えないので書いておく。
    born.[last] <- null
    n <- last

  // 根の敵は撃たれていないので、撃った場所は無い
  do add (Born(rootRun, true, -1, null)) field.EnemyX field.EnemyY

  static member Create (front: IFrontEnv) (bulletml: Bulletml) =
    // **対の表は組む段でしか作れない**（`Focus.Collect` の但し書き）。
    // 選ぶのは走り出したあとなので、ここで作らないと、選ばれた時点で
    // 面を建て直すことになる —— 見ていたコマが頭へ戻ってしまう
    let focus = Focus()
    let script = focus.Collect bulletml (fun () -> Runner.load front.Rand front.Rank bulletml)
    let run = Runner.newRoot BulletType.Enemy script
    // **面の形は弾幕が決める。** 横画面と名乗る弾幕は、縦の面に置くと
    // 弾が横へ抜けていく（`Stage.landscape` の但し書き）
    let field = Stage.ofDirection script.ShootingDirection
    Playfield(front, field, focus, run)

  /// `Pack` が返す点の数。**根の敵が入る** —— 描く側はこの数で回す
  member _.Count = n

  /// **弾の数。根の敵は入らない**（v4.0.1）。
  ///
  /// `Count` と 1 だけ違う。**その 1 を呼ぶ側に引かせない** ——
  /// `Count - 1` と書くと「根がちょうど 1 個 で、必ず生きている」が
  /// 呼ぶ側の知識になる。数えれば、根を 2 つ にしても増やしても合う。
  ///
  /// **字の上の合計と一致するのはこちら。** 腕ごとの生き残りを足すと
  /// この数になる（同梱 176 本 / 600 コマ で 1 度 も外れない。版の頭で数えた）
  member _.BulletCount =
    let mutable c = 0
    for i in 0 .. n - 1 do
      if not born.[i].IsRoot then c <- c + 1
    c

  /// 速い道に居る弾の数（v4.9.1）。**エンジンを呼ばない弾。**
  ///
  /// **門がこれを数える** —— 0 件 のまま「毎コマ 呼ぶ面と一致した」と
  /// 言えてしまうと、**速い道を 1 度 も通らずに緑**になる
  member _.ConstCount =
    let mutable c = 0
    for i in 0 .. n - 1 do
      if quiet.[i] < 0 then c <- c + 1
    c

  /// `wait` で止まっているので速い道に居る弾の数（v4.9.2）。
  ///
  /// **`ConstCount` と分けてある。** あちらは「台本が空」で戻ってこない弾、
  /// こちらは「あと n コマ で素の道へ戻る」弾 —— **同じ配列の別の意味**で、
  /// 門がどちらを見ているかが分からなくなると、片方 が 0 件 のまま緑になる
  member _.QuietCount =
    let mutable c = 0
    for i in 0 .. n - 1 do
      if quiet.[i] > 0 then c <- c + 1
    c

  /// 生きている弾を、**撃った腕の添字**で束ねる（v4.0.1）。
  /// 戻りは (書いてある順の添字, いま生きている数)。**多い順**。
  ///
  /// **鎖は辿らない。** `From` は撃たれた時点で引いてある（v3.2）ので、
  /// ここは束ねるだけ —— 弾数ぶんの走査 1 回 で済む。
  ///
  /// **`From` が -1 の弾は入らない。** 根の敵と、結べなかったコマで
  /// 産まれた弾がそれ（`AliveUnattributed` で数えられる）——
  /// **黙って落とさない。** 落とすと、字の上の合計と `Bullets:` が
  /// 食い違う理由が読めなくなる
  member _.AliveByFire() : struct (int * int)[] =
    let byIndex = Dictionary<int, int>()
    for i in 0 .. n - 1 do
      let from = born.[i].From
      if from >= 0 then
        byIndex.[from] <- (match byIndex.TryGetValue from with | true, v -> v | _ -> 0) + 1
    byIndex
    |> Seq.sortByDescending (fun kv -> kv.Value)
    |> Seq.map (fun kv -> struct (kv.Key, kv.Value))
    |> Seq.toArray

  /// どの腕にも結べない、生きている弾の数（v4.0.1）。**根の敵を含む。**
  ///
  /// `Count` から `AliveByFire` の合計を引いた数と同じ ——
  /// **引き算を呼ぶ側にさせない**（`Count` に根が入っていることを
  /// 知っている必要が出る）
  member _.AliveUnattributed =
    let mutable c = 0
    for i in 0 .. n - 1 do
      if born.[i].From < 0 then c <- c + 1
    c

  /// 生きている弾の座標。**外へは出さない** —— `Differ` が使うだけ。
  /// 1 点 3 つ の並びで、長さは `Count * 3` 以上
  member private _.Positions = struct (pos, n)

  /// 2 つ の面が分かれているか（v3.8）。**弾の数か、同じ添字の座標が違うか。**
  ///
  /// **添字で突き合わせられるのは「最初の食い違い」まで。** そこまでは 2 つ の
  /// 走行が同じ順に弾を作っているので、並びも同じ物を指す。食い違ったあとの
  /// 並びは意味を持たないが、**要るのは最初の 1 コマ だけ**なのでそれで足りる。
  ///
  /// **数だけでは足りない。** 同梱 176 本 で種を変えたとき、座標が分かれるのは
  /// 中央 6 コマ 目 なのに、弾の数が食い違うのは中央 134 コマ 目
  /// （**22.3 倍 遅れる**）。数だけ見ると、位置は 6 コマ 目 から違うのに
  /// 134 コマ 目 まで気づかない。
  ///
  /// **`Pack` を呼ばなくてよくなった**（v4.9.1 の段 2）—— 座標は詰めた並び
  /// そのものに居るので、2 つ の面を続けて読んでも上書きが起きない
  static member Differ (a: Playfield) (b: Playfield) : bool =
    let struct (xs, na) = a.Positions
    let struct (ys, nb) = b.Positions
    if na <> nb then true
    else
      let mutable i = 0
      let mutable diff = false
      while not diff && i < na do
        if xs.[i * 3] <> ys.[i * 3] || xs.[i * 3 + 1] <> ys.[i * 3 + 1] then diff <- true
        i <- i + 1
      diff

  /// この面の形。**建てたあとは変わらない** —— 向きは弾幕が決めるので、
  /// 変わるときは弾幕が変わったとき、つまり建て直すとき
  member _.Field = field

  /// 進めたコマ数。**この面を建ててから何回 `Tick` したか**であって、
  /// 弾幕の中の時間ではない。建て直せば 0 に戻る（Reset / Apply / 選び直し）。
  ///
  /// 面が持つのは、飛ぶ側（`Seek`）が「いまどこか」を要るから ——
  /// 呼ぶ側で数えると、`Tick` を呼ぶ道が増えたときに数え漏れる。
  member _.Frame = frame

  member _.Tick() =
    spawned.Clear()
    // 撃った場所を拾う窓（v3.2）。**1 コマ の頭で 1 回 だけ** ——
    // 弾ごとに付け替えると、そのぶんを毎コマ 払う
    focus.BeginFrame()
    let w = field.Width
    let h = field.Height
    let unpick () = focus.Clear()
    let mutable i = 0
    while i < n do
      // --- 台本の無い弾は、位置を足すだけ（v4.9.1）------------------------
      //
      // **エンジンを呼ばない。** 差分はもう変わらないので
      // （`BulletRun.ConstantDelta` の但し書き）、撃たないし消えないし
      // 走らせ直しも空を歩くだけ —— 残る仕事は足し算と面の外の判定。
      //
      // 同梱 176 本 x 300 コマ で**弾コマ の 84.0%** がここを通り、
      // **座標は 1 ビット も変わらない**（`Front.Tests/ConstStep.fs`）。
      //
      // **成分の配列の上で回す**（段 2）—— 読むのは 5 つ の float32 だけで、
      // オブジェクトを 1 つ も引きに行かない
      if quiet.[i] <> 0 then
        // **あと n コマ の道（v4.9.2）は 1 つ 減らす。**
        // -1（生涯。段 1）は減らさない —— 戻ってこないので数える意味が無い
        if quiet.[i] > 0 then quiet.[i] <- quiet.[i] - 1
        let b = i * 3
        let a = i * 2
        let x = pos.[b] + vel.[a]
        let y = pos.[b + 1] + vel.[a + 1]
        pos.[b] <- x
        pos.[b + 1] <- y
        // **追っている弾なら、再開点は「無い」に戻す**（v3.1）。
        // 素の道では `Begin` -> step -> `End` を通り、台本が空の弾は
        // 受け口を 1 度 も鳴らさないので `End` が全部 消していた ——
        // ここを飛ばすと**前のコマの再開点が残る**。
        // **1 コマ に 1 発 だけなので、値段はここに乗らない**
        if i = picked then
          focus.Begin()
          focus.End()
        if x < 0.0f || x > w || y < 0.0f || y > h then removeAt i unpick
        else i <- i + 1
      else

      let it = born.[i]
      let m0 = it.Run.Motion
      let motion =
        { Pos = { X = pos.[i * 3]; Y = pos.[i * 3 + 1] }
          Speed = m0.Speed
          Dir = m0.Dir
          Accel = m0.Accel }
      focus.BeginBullet()
      // **追っている弾のときだけ繋ぐ。** 呼びを 1 本 にまとめて try/finally を
      // 全部 の弾に掛けると、選んでいない人にもその分 を払わせる
      let f =
        if i = picked then
          focus.Begin()
          try
            Driver.step front Space.YDown SpawnOrigin.AtShooter it.Run motion
          finally
            focus.End()
        else Driver.step front Space.YDown SpawnOrigin.AtShooter it.Run motion
      if not it.IsRoot then
        pos.[i * 3] <- pos.[i * 3] + f.Delta.X
        pos.[i * 3 + 1] <- pos.[i * 3 + 1] + f.Delta.Y
      it.Run <- if f.Finished then Driver.restart front f.Run else f.Run
      // 撃った場所を弾に持たせる（v3.2）。**走査した撃つ腕の並びと
      // `Spawned` の並びは 1 対 1**（同梱 176 本 の撃ったコマ 17,945 で
      // 100.00%。手書きの 4 fire でも並びが一致した）。
      //
      // 数が食い違うコマでは結ばない。ただし **この守りが働くところは
      // 作れなかった** —— もう撃った `fire` は走査されないので、
      // `<fire/><wait>1</wait><fire/>` でも数は一致する。
      // **外しても緑のまま**なので、これは網ではなく現状固定
      let pairable = focus.FiredCount = List.length f.Spawned
      let mutable k = 0
      for child in f.Spawned do
        let p = child.Motion.Pos
        let from = if pairable then focus.FiredIndex k else -1
        spawned.Add(struct (Born(child, false, from, it), p.X, p.Y))
        // 撃った腕ごとに数える（v3.3 の段 1）。**引いた添字を使い回す** ——
        // 数えるためにもう一度 鎖を辿ると、弾 1 発 につき 2 度 辿ることになる
        focus.TallyAt from
        k <- k + 1
      let x = pos.[i * 3]
      let y = pos.[i * 3 + 1]
      let dead =
        not it.IsRoot && (
          f.Vanished || f.Retired
          || x < 0.0f || x > w
          || y < 0.0f || y > h)
      // **台本が空になったら、次のコマから速い道へ移す**（v4.9.1）。
      //
      // ### 根を外す守りは冗長で、外しても答えが変わらない
      //
      // 較正で当てて **0 点** だった。台本を持たない根は速さが 0 なので、
      // `ConstantDelta` も (0, 0) —— 速い道へ入れても動かない。
      //
      // **それでも残す。** 素の道は `IsRoot` のとき差分を**足さない**と
      // 決めていて、速い道にその枝は無い。根に速さが入る形
      // （フロントが `WithMotion` で入れる）ができた日に、
      // **根だけが静かに動き出す。** 残す理由を書いておく。
      //
      // ### `wait` で止まっているだけの弾も移す（v4.9.2）
      //
      // 台本が残っていても、**全部 の top が `wait` の途中 なら、その残りの
      // あいだ は何も起こさない**（`BulletRun.QuietFrames`）。
      // 素の道を通っていた弾コマ の 45.7% がそれ。
      //
      // **差分はここで組み直さない** —— 出口 が返した `f.Delta` をそのまま
      // 持つ（同じ式を 3 か所 目 に書かないため）。
      //
      // **`SkipQuiet` で先に `wait` を消化する。** 速い道はエンジンを
      // 呼ばないので、放っておくと飛ばしたあいだ `wait` が減らず、
      // その弾だけ n コマ 余分 に待つ。
      if not dead && not it.IsRoot then
        match it.Run.ConstantDelta with
        | ValueSome (d: Vec2) ->
          vel.[i * 2] <- d.X
          vel.[i * 2 + 1] <- d.Y
          quiet.[i] <- -1
        | ValueNone ->
          let q = it.Run.QuietFrames
          if q > 0 then
            vel.[i * 2] <- f.Delta.X
            vel.[i * 2 + 1] <- f.Delta.Y
            quiet.[i] <- q
            it.Run <- it.Run.SkipQuiet q
      if dead then
        // 追っていた弾が消えたら追うのをやめる。**数も消す** ——
        // 残すと、消えた弾の最後のコマの数がそのまま出続ける
        removeAt i unpick
      else i <- i + 1
    for struct (b, x, y) in spawned do add b x y
    focus.EndFrame()
    frame <- frame + 1

  /// WASM ヒープ上の `float32[]`。JS は JSON せず `localHeapViewF32` で読む。
  ///
  /// **1 点 につき 3 つ**（v4.7）—— x / y / **撃った腕の添字**。
  ///
  /// **写す仕事が消えた**（v4.9.1 の段 2）—— 詰めた並びそのものが弾の置き場に
  /// なったので、ここは数を返して pin を確かめるだけ。
  /// 以前は毎コマ 弾の数だけ `live.[i]` を引いて 3 つ ずつ書いていた
  /// （1,709 点 で 10.9 us。1 コマ の 3 巡 のうち 10.3%）。
  ///
  /// **撃たれていない弾は -1**（根の敵がそう）。JS 側はそこを外す
  member _.Pack() =
    ensure (max n 1)
    n

  /// `Pack` が読む並びそのもの（v4.7）。**本番は使わない** ——
  /// JS はポインタ（`PackedPtr`）で読む。
  ///
  /// **中身を字で確かめられる口が要る。** 3 つ 組 の並びは
  /// JS と .NET のあいだの取り決めで、崩れても走行は落ちない
  /// （弾がおかしな場所に描かれるだけ）—— **門が届く形にしておく。**
  ///
  /// 長さは `Count * 3` 以上（先に確保してあるので、それより長い）
  member _.Packed = pos

  member _.PackedPtr =
    if not pin.IsAllocated then rePin ()
    float (pin.AddrOfPinnedObject().ToInt64())

  /// 追っている弾の再開点（v3.1 の段 3）。**選んでいなければ何も出ない**
  member _.Focus = focus

  /// `Pack` した並びの添字で選ぶ。**添字をそのまま覚える**（v4.9.1 の段 2）——
  /// 消しで詰めるところが 1 か所 しか無いので、そこで追随させれば足りる。
  /// 範囲の外なら追うのをやめる
  member _.Pick(i: int) =
    if i >= 0 && i < n then picked <- i
    else
      picked <- -1
      focus.Clear()

  member _.Unpick() =
    picked <- -1
    focus.Clear()

  /// 追っている弾を撃った `fire` が、読んだ木の書いてある順の何番目 か（v3.2）。
  /// **追っていないときと、撃たれていない弾（根の敵）は -1**
  member _.PickedFrom =
    if picked < 0 then -1 else born.[picked].From

  /// 追っている弾の系譜（v3.6）。撃った `fire` の**書いてある順の添字**を、
  /// **根に近い順**で並べる。追っていなければ空。
  ///
  /// **上限を置く。** 環はできないはずだが、置かないと万一のとき
  /// **赤くならずに止まらなくなる**（門も目も何も出ない）
  member _.PickedLineage : int[] =
    if picked < 0 then Array.empty
    else
      let acc = ResizeArray<int>()
      let mutable cur = born.[picked]
      let mutable k = 0
      while not (obj.ReferenceEquals(cur, null)) && k < 64 do
        if cur.From >= 0 then acc.Add cur.From
        cur <- cur.Parent
        k <- k + 1
      acc.Reverse()
      acc.ToArray()

  /// 系譜の深さ（v3.6）。**辿れた段の数**で、`PickedLineage` の長さとは別 ——
  /// 撃った場所が決まらなかった段（`From` が -1）も 1 段 として数える
  member _.PickedDepth =
    if picked < 0 then 0
    else
      let mutable cur = born.[picked]
      let mutable k = 0
      while not (obj.ReferenceEquals(cur, null)) && k < 64 do
        cur <- cur.Parent
        k <- k + 1
      k

  /// 追っている弾が `Pack` の並びの何番目 か。**無ければ -1**（消えたときも）。
  ///
  /// **探さなくなった**（v4.9.1 の段 2）—— 添字そのものを持っていて、
  /// 消しで詰めるところで直している。以前は毎コマ 弾の数だけ
  /// 参照を突き合わせていた
  member _.PickedIndex = picked
