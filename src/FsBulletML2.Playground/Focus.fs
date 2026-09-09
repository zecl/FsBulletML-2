namespace FsBulletML2.Playground

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open FsBulletML2
open FsBulletML2.LanguageService

/// 選んだ弾の**再開点**を、走る木から読んだ木まで戻す（v3.1 の段 3）。
///
/// 段 1 の受け口（`NodeTrace.stop`）が渡すのは**走る木のノード**で、字に結べるのは
/// **読んだ木のノード**。あいだの鎖は段 2 が `NodeOrigin.pair` で作る ——
/// **表を持つのはここ。** Core は口を開けるだけで、誰の表かを知らない
/// （`Trace.fs` の但し書き：付け替える責任は呼ぶ側に在る）。
///
/// **繋ぐのは面 1 つ につき 1 か所。** `Playfield` が、選んだ弾の `Driver.step` の
/// 前後 でだけ繋ぐ —— 選んでいないときは受け口が `ignore` に戻っている。
[<Sealed>]
type Focus () =

  /// 作った物 -> 元の物。**参照が鍵**（同じ台本の同じ要素は、弾をまたいで同じ物）
  let origins = Dictionary<obj, obj>(HashIdentity.Reference)

  /// 読んだ木のノード -> 通し番号。**毎コマ 字を境界越しに渡さないため** ——
  /// 呼ぶ側は番号が変わったときだけ名前を引きに来る（`Main.ResumeName`）
  let serials = Dictionary<obj, int>(HashIdentity.Reference)

  /// 読んだ木のノード -> **書いてある順の添字**（v3.1 の段 4）。
  ///
  /// 字の位置は木に無いので、光らせる先は順番でしか言えない ——
  /// 木の k 番目 と札の k 番目 が揃うことは v2.9 で測ってある
  /// （3 表記 で 176 / 176。F# の CE は結べない）。
  let order = Dictionary<obj, int>(HashIdentity.Reference)

  /// 並びに 2 度 出るノード。**そこは光らせない。**
  ///
  /// 2 通り の理由で起きる —— 引数なしの腕は singleton（`vanish`）で、
  /// CE の木は空の `bullet` を共有している。**どちらも再開点には来ない**
  /// （`stop` に渡ってくるのは `action` / `wait` / `repeat` の 3 腕 だけ）が、
  /// **来ないことに寄りかからない** —— 来たときに黙って隣を光らせるより、
  /// 光らせないほうが読める
  let ambiguous = HashSet<obj>(HashIdentity.Reference)

  /// そのコマに `stop` が呼ばれた数。**0 か 2 以上。1 は出ない** ——
  /// 子が止まると親も `Stopped` を返すので、入れ子の深さだけ積み上がる
  /// （`Trace.fs` の但し書き。同梱 176 本 の最大は 21）
  let mutable stops = 0
  /// 列の**先頭**（いちばん内側）。呼ばれる順は葉が先・根が後 なので、
  /// **2 件目 以降 は祖先。捨てる**（光らせると 21 行 が一度に光る）
  let mutable first : obj = null
  /// 道の本数。**渡ってきた `wait` の数がそのまま道の本数。**
  ///
  /// `Stopped` が生まれるのは 2 か所 しかない —— `wait`（`Step.fs:58`）と、
  /// **輪を打ち切ったとき**（`621`）。`wait` は葉なので祖先にならず、
  /// 1 本 の道は必ず 1 つ の `wait` で止まる。同梱 176 本 / 383,655 コマ で
  /// **列の先頭は 100.00% が `wait`**（`docs/local/measure/measure-stop-head.fsx`）。
  ///
  /// **段 3 が出すのは 1 本 目 だけ。** 2 本 目 以降 が在ることは、
  /// この数が 2 以上 になることで見える（黙って落とさない）
  let mutable paths = 0
  /// 読んだ木まで戻した先。**戻せなければ null**
  let mutable resumed : obj = null
  /// 辿った鎖の段数。**0 は「1 段 も辿らなかった」** ——
  /// 読んだ木に同じ物が在ったということで、戻れなかったのとは別
  let mutable depth = 0
  let mutable serial = -1
  /// 再開点の**書いてある順の添字**。**無ければ -1**（決まらないときも）
  let mutable orderIndex = -1
  /// 名前を組んだときのノード。**参照で見る** —— 同じノードなら組み直さない
  let mutable named : obj = null
  let mutable name = ""

  let onStop =
    fun (node: obj) ->
      stops <- stops + 1
      if isNull first then first <- node
      match node with
      | :? Action as a ->
          match a with
          | Action.Wait _ -> paths <- paths + 1
          | _ -> ()
      | _ -> ()

  /// そのコマに走査した**撃つ腕**（v3.2）。並びは走査した順。
  ///
  /// **`Spawned` の並びと 1 対 1**（同梱 176 本 / 撃ったコマ 17,945 で 100.00%。
  /// 手書きの 4 fire でも並びが一致した）。数が食い違うコマでは結ばない ——
  /// もう撃った `fire` も走査はされるので、原理的には食い違いうる
  let fired = ResizeArray<obj>()

  let onVisit =
    fun (node: obj) ->
      match node with
      | :? Action as a ->
          match a with
          | Action.Fire _ | Action.FireRef _ -> fired.Add node
          | _ -> ()
      | _ -> ()

  let onPair = fun (created: obj) (origin: obj) -> origins.[created] <- origin
  /// 戻す先。**その場で作らない** —— 毎コマ 閉包が 1 個 ヒープに乗る
  let noPair = fun (_: obj) (_: obj) -> ()

  /// 鎖の上限。段 2 で測った最大は 3 段 だが、**輪を書いた本では走行中にも
  /// 対ができる。** 上限が無いと、万一 環ができたときに
  /// **赤くならずに止まらなくなる**（門も目も何も出ない）
  let maxChain = 16

  /// 走る木のノードから、読んだ木のノードへ。
  /// 戻りは (読んだ木のノード, 辿った段数)。**上限は `maxChain`**（環の疑い）
  let followTo (node: obj) =
    let mutable cur = node
    let mutable n = 0
    let mutable go = true
    while go && n < maxChain do
      match origins.TryGetValue cur with
      | true, o ->
          cur <- o
          n <- n + 1
      | _ -> go <- false
    struct (cur, n)

  let follow (node: obj) =
    let struct (r, n) = followTo node
    resumed <- r
    depth <- n

  /// 読んだ木のノードを、**書いてある順の添字**へ。**決まらなければ -1**
  let indexOfRead (node: obj) =
    if isNull node then -1
    elif ambiguous.Contains node then -1
    else
      match order.TryGetValue node with
      | true, k -> k
      | _ -> -1

  let serialOf (node: obj) =
    match serials.TryGetValue node with
    | true, v -> v
    | _ ->
        let v = serials.Count
        serials.[node] <- v
        v

  /// 腕の名前の頭を小文字に。**`Vocabulary` と同じ規則**（正本は `Core/DTD.fs`）——
  /// 要素名の表をここに書くと、DTD を直したときここだけが古びる
  let camel (s: string) =
    if String.IsNullOrEmpty s then s
    else string (Char.ToLowerInvariant s.[0]) + s.Substring 1

  let caseName (ty: Type) (o: obj) =
    let info, _ = FSharpValue.GetUnionFields(o, ty)
    camel info.Name

  /// 再開点の要素名。**`stop` が渡すのは 2 型**（`Step.fs` の 3 か所 ——
  /// 608 が `Action`、714 と 981 が `ActionElm`）。
  ///
  /// **鎖の先は 3 型 目 になりうる。** 根の直下 の `<action>` は
  /// `BulletmlElm.Action` で、`getAction` が作った `ActionElm` の元がそこ
  /// （段 2 で 1.00% を落としていた穴）。同梱では先頭が 100% `wait` なので
  /// ここには来ないが、**来たときに空文字を返すと「再開点なし」と同じ顔になる**
  let elementName (node: obj) =
    match node with
    | :? Action as a -> caseName typeof<Action> a
    | :? ActionElm as e -> caseName typeof<ActionElm> e
    | :? BulletmlElm as b -> caseName typeof<BulletmlElm> b
    | _ -> ""

  /// 組む段の対を拾う。**`Runner.load` を囲む** —— 畳みの対はそこでしか作れない。
  ///
  /// **選ぶのは走り出したあと**なので、選ばれてから作ろうとすると
  /// 面を建て直すことになる（見ていたコマが頭へ戻ってしまう）。
  /// 表は面ごとなので、建て直すたびに空にする
  member _.Collect (bulletml: Bulletml) (build: unit -> 'a) : 'a =
    origins.Clear()
    serials.Clear()
    order.Clear()
    ambiguous.Clear()
    // **書いてある順の並びも、ここで 1 回 だけ組む。** 走行中には要らない
    // （引くのは選んだ弾の 1 コマ に 1 回）ので、毎コマ 歩かない
    let walk = NodeOrder.walk bulletml
    for k in 0 .. walk.Count - 1 do
      let (_, node) = walk.[k]
      if order.ContainsKey node then ambiguous.Add node |> ignore
      else order.[node] <- k
    NodeOrigin.enabled <- true
    NodeOrigin.pair <- onPair
    try build ()
    finally
      NodeOrigin.enabled <- false
      NodeOrigin.pair <- noPair

  /// 撃った場所を拾う窓（v3.2）。**1 コマ の頭で 1 回 だけ繋ぐ** ——
  /// 弾ごとに付け替えると、1,500 弾 x 60 コマ ぶんの付け替えを毎秒 払う。
  ///
  /// **これは全部 の弾に掛かる。** 押す前に撃たれているので、選んだ弾だけでは
  /// 足りない —— 確保が増えないことは測ってある（同梱 176 本 / 287 万 コマ で
  /// 素との差 0.08%。素どうしの差より小さい）
  member _.BeginFrame() = NodeTrace.visit <- onVisit

  /// 1 コマ の後。**素へ戻す**
  member _.EndFrame() = NodeTrace.visit <- ignore

  /// 1 弾 の 1 コマ の前。**撃つ腕の並びを空にする**
  member _.BeginBullet() = fired.Clear()

  /// そのコマに走査した撃つ腕の数
  member _.FiredCount = fired.Count

  /// k 番目 の撃つ腕を、読んだ木の**書いてある順の添字**へ。
  /// **決まらなければ -1**
  member _.FiredIndex(k: int) =
    if k < 0 || k >= fired.Count then -1
    else
      let struct (r, _) = followTo fired.[k]
      indexOfRead r

  /// 選んだ弾の 1 コマ の前。**`NodeOrigin` もここで繋ぐ** ——
  /// 輪を書いた本は走行中にも新しいノードを作る（同梱では 1 件 も出ないが、
  /// Playground は本文を打てるので輪はいつでも書ける）
  member _.Begin() =
    stops <- 0
    paths <- 0
    first <- null
    NodeTrace.stop <- onStop
    NodeOrigin.enabled <- true
    NodeOrigin.pair <- onPair

  /// 選んだ弾の 1 コマ の後。**素へ戻すのはここ** ——
  /// 戻さないと、選んでいない弾も受け口を通ることになる
  member _.End() =
    NodeTrace.stop <- ignore
    NodeOrigin.enabled <- false
    NodeOrigin.pair <- noPair
    if isNull first then
      resumed <- null
      depth <- 0
      serial <- -1
      orderIndex <- -1
    else
      follow first
      serial <- serialOf resumed
      orderIndex <- indexOfRead resumed

  /// 選ぶのをやめたとき。**数も消す** —— 消さないと、
  /// 選んでいないのに前のコマの数が残る
  member _.Clear() =
    stops <- 0
    paths <- 0
    first <- null
    resumed <- null
    depth <- 0
    serial <- -1
    orderIndex <- -1

  /// そのコマに `stop` が呼ばれた数
  member _.Stops = stops

  /// 道の本数。**2 以上 なら、出しているのは 1 本 目 だけ**
  member _.Paths = paths

  /// 辿った鎖の段数
  member _.Depth = depth

  /// 再開点の通し番号。**無ければ -1**（同じノードなら同じ数）
  member _.Serial = serial

  /// 再開点が、読んだ木を**書いてある順**に歩いたときの何番目 か。
  /// **無ければ -1** —— 再開点が無いときと、並びに 2 度 出て決まらないとき。
  ///
  /// 呼ぶ側は**この数だけを字の側へ渡す**（`Main.StepFrame`）。
  /// 字の位置はあちらにしか無く、木はこちらにしか無い
  member _.OrderIndex = orderIndex

  /// 読んだ木まで戻した再開点そのもの。**戻せなければ null。**
  ///
  /// 番号や名前ではここが**読んだ木のノードか**を確かめられない ——
  /// 「戻れる」と「正しい位置に戻れる」は別なので、
  /// 試験が読んだ木と参照で突き合わせられる形で出す
  member _.Resumed = resumed

  /// 再開点の要素名。**番号が変わったときだけ組み直す**（reflection なので
  /// 毎コマ 呼ぶと、引数の並びがコマごとにヒープへ乗る）
  member _.Name =
    if not (obj.ReferenceEquals(named, resumed)) then
      named <- resumed
      name <- if isNull resumed then "" else elementName resumed
    name