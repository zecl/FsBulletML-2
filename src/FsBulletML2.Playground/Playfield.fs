namespace FsBulletML2.Playground

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

/// 参照型。struct だと `let it = live.[i]` がコピーになって書き戻せない。
[<Sealed; AllowNullLiteral>]
type Live(run: BulletRun, x: float32, y: float32, isRoot: bool, from: int, parent: Live) =
  member val Run = run with get, set
  member val X = x with get, set
  member val Y = y with get, set
  member _.IsRoot = isRoot
  /// この弾を撃った `fire` が、読んだ木の**書いてある順**の何番目 か（v3.2）。
  /// **撃たれていなければ -1**（根の敵）。決まらないときも -1
  member _.From = from
  /// この弾を撃った弾（v3.6）。**根は null。**
  ///
  /// **`From` を辿っても系譜にならない** —— あれは「撃った `fire` の添字」で、
  /// 同じ `fire` から撃たれた弾は全部 同じ数になる。親そのものが要る。
  ///
  /// **握ると、消えた親が解放されない。** 深さのぶんだけ残る ——
  /// そこは版の頭で数えた
  member _.Parent = parent

/// 生きている弾の一覧。Bolero を知らない。
///
/// 1 コマの順は MonoGame の `RunTask` と同じ
/// （`Driver.step` → Delta を足す → Spawn は次コマ → Finished なら restart）。
/// **回している最中に `live.Add` しない。** 今コマの Spawn は溜めて、消しのあとで足す。
[<Sealed>]
type Playfield private (front: IFrontEnv, live: ResizeArray<Live>, field: Field, focus: Focus) =

  let spawned = ResizeArray<Live>()
  let mutable frame = 0
  let mutable xs = Array.zeroCreate<float32> 256
  let mutable pin = Unchecked.defaultof<GCHandle>
  /// 追っている弾（v3.1 の段 3）。**参照で握る** —— 死んだ枠には末尾を移すので
  /// （`Tick`）、添字はそのコマのものでしかない
  let mutable picked = Unchecked.defaultof<Live>

  let rePin () =
    if pin.IsAllocated then pin.Free()
    pin <- GCHandle.Alloc(xs, GCHandleType.Pinned)

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
    let live = ResizeArray<Live>()
    // 根の敵は撃たれていないので、撃った場所は無い
    live.Add(Live(run, field.EnemyX, field.EnemyY, true, -1, null))
    Playfield(front, live, field, focus)

  member _.Count = live.Count

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
    let mutable i = 0
    while i < live.Count do
      let it = live.[i]
      let m0 = it.Run.Motion
      let motion =
        { Pos = { X = it.X; Y = it.Y }
          Speed = m0.Speed
          Dir = m0.Dir
          Accel = m0.Accel }
      focus.BeginBullet()
      // **追っている弾のときだけ繋ぐ。** 呼びを 1 本 にまとめて try/finally を
      // 全部 の弾に掛けると、選んでいない人にもその分 を払わせる
      let f =
        if obj.ReferenceEquals(it, picked) then
          focus.Begin()
          try
            Driver.step front Space.YDown SpawnOrigin.AtShooter it.Run motion
          finally
            focus.End()
        else Driver.step front Space.YDown SpawnOrigin.AtShooter it.Run motion
      if not it.IsRoot then
        it.X <- it.X + f.Delta.X
        it.Y <- it.Y + f.Delta.Y
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
        spawned.Add(Live(child, p.X, p.Y, false, from, it))
        // 撃った腕ごとに数える（v3.3 の段 1）。**引いた添字を使い回す** ——
        // 数えるためにもう一度 鎖を辿ると、弾 1 発 につき 2 度 辿ることになる
        focus.TallyAt from
        k <- k + 1
      let dead =
        not it.IsRoot && (
          f.Vanished || f.Retired
          || it.X < 0.0f || it.X > field.Width
          || it.Y < 0.0f || it.Y > field.Height)
      if dead then
        let last = live.Count - 1
        // 追っていた弾が消えたら追うのをやめる。**数も消す** ——
        // 残すと、消えた弾の最後のコマの数がそのまま出続ける
        if obj.ReferenceEquals(it, picked) then
          picked <- Unchecked.defaultof<Live>
          focus.Clear()
        if i <> last then live.[i] <- live.[last]
        live.RemoveAt last
      else i <- i + 1
    live.AddRange spawned
    focus.EndFrame()
    frame <- frame + 1

  /// WASM ヒープ上の `float32[]`。JS は JSON せず `localHeapViewF32` で読む。
  member _.Pack() =
    let n = live.Count
    let need = n * 2
    if xs.Length < need then
      xs <- Array.zeroCreate (max (need * 2) 256)
      rePin ()
    elif not pin.IsAllocated then
      rePin ()
    for i in 0 .. n - 1 do
      xs.[i * 2] <- live.[i].X
      xs.[i * 2 + 1] <- live.[i].Y
    n

  member _.PackedPtr =
    if not pin.IsAllocated then rePin ()
    float (pin.AddrOfPinnedObject().ToInt64())

  /// 追っている弾の再開点（v3.1 の段 3）。**選んでいなければ何も出ない**
  member _.Focus = focus

  /// `Pack` した並びの添字で選ぶ。**握るのは参照** ——
  /// 添字はそのコマのもので、次のコマには別の弾を指しうる。
  /// 範囲の外なら追うのをやめる
  member _.Pick(i: int) =
    if i >= 0 && i < live.Count then picked <- live.[i]
    else
      picked <- Unchecked.defaultof<Live>
      focus.Clear()

  member _.Unpick() =
    picked <- Unchecked.defaultof<Live>
    focus.Clear()

  /// 追っている弾を撃った `fire` が、読んだ木の書いてある順の何番目 か（v3.2）。
  /// **追っていないときと、撃たれていない弾（根の敵）は -1**
  member _.PickedFrom =
    if obj.ReferenceEquals(picked, null) then -1 else picked.From

  /// 追っている弾の系譜（v3.6）。撃った `fire` の**書いてある順の添字**を、
  /// **根に近い順**で並べる。追っていなければ空。
  ///
  /// **上限を置く。** 環はできないはずだが、置かないと万一のとき
  /// **赤くならずに止まらなくなる**（門も目も何も出ない）
  member _.PickedLineage : int[] =
    if obj.ReferenceEquals(picked, null) then Array.empty
    else
      let acc = ResizeArray<int>()
      let mutable cur = picked
      let mutable n = 0
      while not (obj.ReferenceEquals(cur, null)) && n < 64 do
        if cur.From >= 0 then acc.Add cur.From
        cur <- cur.Parent
        n <- n + 1
      acc.Reverse()
      acc.ToArray()

  /// 系譜の深さ（v3.6）。**辿れた段の数**で、`PickedLineage` の長さとは別 ——
  /// 撃った場所が決まらなかった段（`From` が -1）も 1 段 として数える
  member _.PickedDepth =
    if obj.ReferenceEquals(picked, null) then 0
    else
      let mutable cur = picked
      let mutable n = 0
      while not (obj.ReferenceEquals(cur, null)) && n < 64 do
        cur <- cur.Parent
        n <- n + 1
      n

  /// 追っている弾が `Pack` の並びの何番目 か。**無ければ -1**（消えたときも）。
  ///
  /// **添字を覚えない。** 覚えると、消しで詰めたコマに黙って別の弾を指す
  /// —— 絵は隣の弾に付き、再開点は追っている弾のものという食い違いが出る
  member _.PickedIndex =
    if obj.ReferenceEquals(picked, null) then -1
    else
      let mutable i = 0
      let mutable found = -1
      while found < 0 && i < live.Count do
        if obj.ReferenceEquals(live.[i], picked) then found <- i
        i <- i + 1
      found