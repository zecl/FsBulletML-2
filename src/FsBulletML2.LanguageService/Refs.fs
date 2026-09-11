/// **「どの要素が、どの要素を、どの属性で参照するか」の規則。1 本。**
///
/// 表を持たない —— 語彙の中で名前が `Ref` で終わり、`Ref` を落とした名前も
/// 語彙に在り、両方 が同じ属性を持つものだけを対にする。
///
/// ## 入口が 2 つ 在る
///
///     host 側     `References.fs`（波線。`Vocabulary.elements` から）
///     ブラウザ側   `Languages/Lookup.fs`（rename と Quick Fix。
///                 起動時にもらった `Vocab` から）
///
/// 型が違うので、受け取るのは**名前と属性名だけに落としたもの。**
///
/// **規則を 2 か所 に書かない。** 書くと、片方 だけ直したときに
/// 「波線は出るのに rename は当たらない」（逆も）という形になる ——
/// **どちらも単独では正しく見える。**
///
/// ## `Fable.Core` に依存しない
///
/// このソースは host（.NET）と ブラウザ側（Fable が焼いた JS）の
/// **両方 で走る。**
module FsBulletML2.LanguageService.Refs

/// 名前が `Ref` で終わるか。
///
/// **`EndsWith` を使わない。** 引数 1 つ の `String.EndsWith` は
/// .NET では現在のカルチャで比べる —— 綴りが ASCII なので実害は出にくいが、
/// 「どちらで比べているか」が字に出ない。F# の文字列の等値は序数なので、
/// 切って比べれば読んだとおりに動く
let private endsWithRef (name: string) =
  name.Length > 3 && name.Substring(name.Length - 3) = "Ref"

/// (参照する要素, 参照される要素, label の属性名) の並び。
///
/// 受け取るのは (要素名, その要素の属性名の並び) の並び。
///
/// **空なら、呼ぶ側は何も挙げない**（語彙が引けていない印。
/// `Parser.Tests` が 0 件 を赤にする）
let pairs (elements: (string * string list) list) : (string * string * string) list =
  let attrsOf name =
    elements |> List.tryPick (fun (n, attrs) -> if n = name then Some attrs else None)
  elements
  |> List.choose (fun (name, attrs) ->
      if not (endsWithRef name) then None
      else
        match attrsOf (name.Substring(0, name.Length - 3)) with
        | None -> None
        | Some defAttrs ->
          attrs
          |> List.tryPick (fun a ->
               if List.contains a defAttrs
               then Some(name, name.Substring(0, name.Length - 3), a)
               else None))

/// 定義に無い参照 1 つ。**字（人へ見せる文面）を持たない。**
///
/// 持たせると、host の波線とブラウザの Quick Fix で文面が割れる ——
/// **割れても、どちらも単独では正しく見える。**
/// 文面を組むのは、それぞれの呼ぶ側。
type Missing =
  { /// 参照している要素（`actionRef`）
    RefName: string
    /// 参照されるはずの要素（`action`）
    DefName: string
    /// 名前が載っている属性（`label`）
    AttrName: string
    /// その参照の属性そのもの。**位置も値もここに在る**
    Hit: AttrHit
    /// その本文に在る、その種類の定義の名前。**「近い名前」の材料** ——
    /// 空なら直し方は作れない（作る側がそう読む）
    Defined: string list }

/// 定義に無い参照を全部。**本文に出てくる順**で返す。
///
/// 同じ名前を 2 回 参照していたら 2 本 返る —— どちらも直す先なので。
///
/// **字を数えるところは受け取る**（表記ごとの 1 本）。
/// **語彙も受け取る**（`pairs` の返り）—— この 1 本 は
/// 「どこから語彙が来たか」を知らない。
let missing (pairs: (string * string * string) list) (tags: TagHit list) : Missing list =
  // **開始札だけ見る。** 閉じ札は XML だけが返すもので、属性を持たない
  let opens = tags |> List.filter (fun t -> not t.Closing)
  [ for (refName, defName, attrName) in pairs do
      let defined =
        opens
        |> List.filter (fun t -> t.TagName = defName)
        |> List.choose (fun t -> t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName))
        |> List.map (fun a -> a.Value)
        |> List.distinct
      for t in opens do
        if t.TagName = refName then
          match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
          | Some a when not (List.contains a.Value defined) ->
            yield
              { RefName = refName
                DefName = defName
                AttrName = attrName
                Hit = a
                Defined = defined }
          | _ -> () ]
  // **並べ直す。** 上は対ごとに走るので、対の順に並んでいる ——
  // 人へ見せる側も、直し方を選ぶ側も、本文の順で読む
  |> List.sortBy (fun m -> m.Hit.Line, m.Hit.Column)

/// 定義 1 つ と、それを指している参照の数（v4.3）。
///
/// **`missing` の裏返し。** あちらは「指す先が無い参照」、こちらは
/// 「指されている数が分かっている定義」—— 材料は同じ `TagHit` の並び。
///
/// **字を持たない**（`Missing` と同じ理由）。「3 か所 から参照」の文面は
/// 呼ぶ側が組む —— 表記ごとに書き方が違う。
type DefUse =
  { /// 定義の要素（`action`）
    DefName: string
    /// 参照する側の要素（`actionRef`）。**呼ぶ側が文面に使う**
    RefName: string
    /// その名前
    Name: string
    /// 名前が載っている属性そのもの。**位置はここに在る**
    Hit: AttrHit
    /// その名前を指している参照の数。**定義そのものは数えない**
    Uses: int }

/// 定義を全部、参照の数つきで。**本文に出てくる順**で返す。
///
/// **同じ名前の定義が 2 つ 在ることは在る**（別々 の要素が同じ label）——
/// そのときは 2 本 返り、どちらにも同じ数が乗る。
/// **数のほうは名前で数えるので、どちらを指しているかは言えない** ——
/// 言えないことを言わない（`Usages` も同じ形）。
let uses (pairs: (string * string * string) list) (tags: TagHit list) : DefUse list =
  let opens = tags |> List.filter (fun t -> not t.Closing)
  [ for (refName, defName, attrName) in pairs do
      // 参照側の名前を数える。**1 対 につき 1 回 走る**
      let used =
        opens
        |> List.filter (fun t -> t.TagName = refName)
        |> List.choose (fun t -> t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName))
        |> List.map (fun a -> a.Value)
      for t in opens do
        if t.TagName = defName then
          match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
          | Some a ->
            yield
              { DefName = defName
                RefName = refName
                Name = a.Value
                Hit = a
                Uses = used |> List.filter (fun v -> v = a.Value) |> List.length }
          | None -> () ]
  |> List.sortBy (fun d -> d.Hit.Line, d.Hit.Column)

/// 定義 1 つ が取る引数の数と、参照 1 つ が渡している数（v4.5）。
///
/// **`$n` を数えるのは新しい仕事。** 定義の本文を走って、
/// **いちばん大きい `$n`** を取る —— BulletML の `<param>` は
/// **並びで渡す**ので、`$3` が使われていれば 3 つ 要る。
///
/// **`$1` が飛んでいても数は減らない**（`$1` を使わず `$2` だけ使う定義は
/// 2 つ 要る）—— 数えるのは最大値であって個数ではない。
type Arity =
  { /// 定義の要素（`action`）
    DefName: string
    /// その名前
    Name: string
    /// 名前が載っている属性そのもの
    Hit: AttrHit
    /// 本文で使われているいちばん大きい `$n`。**1 つ も無ければ 0** ——
    /// `$rand` / `$rank` は数えない（`$` のあとが数字のものだけ）
    Takes: int }

/// 本文の一部 に出てくるいちばん大きい `$n`。
///
/// **`$rand` と `$rank` は数えない。** `$` のあとが数字のものだけ ——
/// Core の式の読み手（`$` + 数字 を 0 にする）と同じ形。
///
/// ### この守りは冗長で、外しても答えが変わらない
///
/// 較正で当てて **0 点** だった。`$` のあとが数字でなくても、内側の
/// while が 1 度 も回らないので `v` は 0 のまま —— `best` は動かず、
/// `i` の進み方も同じになる。
///
/// **それでも残す。** 意図が字に出るのはここだけで、
/// 内側を「1 つ でも数字を食べたら数える」形に変えたら
/// `$rank` の `r` を跨いで数えかねない。**残す理由を書いておく。**
let private maxParamIn (src: string) (from: int) (until: int) =
  let stop = min until src.Length
  let mutable i = max 0 from
  let mutable best = 0
  while i < stop do
    if src.[i] = '$' && i + 1 < stop && src.[i + 1] >= '0' && src.[i + 1] <= '9' then
      let mutable j = i + 1
      let mutable v = 0
      while j < stop && src.[j] >= '0' && src.[j] <= '9' do
        v <- v * 10 + int src.[j] - int '0'
        j <- j + 1
      if v > best then best <- v
      i <- j
    else i <- i + 1
  best

/// 定義を全部、取る引数の数つきで。**本文に出てくる順**で返す。
///
/// **本文の範囲は深さで決める。** その定義より深くない札が次に出てくる
/// ところまでが中身 —— **表記に依らない**（XML は閉じ札、sxml は括弧、
/// fsb は字下げ で、どれも `Depth` に落ちている）。
///
/// 最後の定義なら本文の終わりまで。
let arity (pairs: (string * string * string) list) (src: string) (tags: TagHit list) : Arity list =
  let all = tags |> List.toArray
  let defNames = pairs |> List.map (fun (_, defName, attrName) -> defName, attrName)
  // **内包表記の中で `while` を回さない**（v4.9）——
  // Fable は内包の中の `while` を enumerator の鎖に焼く（`Scan.fs` に書いた）
  let out = ResizeArray<Arity>()
  for i in 0 .. all.Length - 1 do
    let t = all.[i]
    if not t.Closing then
      match defNames |> List.tryFind (fun (n, _) -> n = t.TagName) with
      | None -> ()
      | Some (_, attrName) ->
        match t.Attrs |> List.tryFind (fun a -> a.AttrName = attrName) with
        | None -> ()
        | Some a ->
          // **中身の終わり。** その定義より深くない札が出てくるところ
          let mutable k = i + 1
          while k < all.Length && all.[k].Depth > t.Depth do k <- k + 1
          let until = if k < all.Length then all.[k].Start else src.Length
          out.Add
            { DefName = t.TagName
              Name = a.Value
              Hit = a
              Takes = maxParamIn src t.Stop until }
  List.ofSeq out