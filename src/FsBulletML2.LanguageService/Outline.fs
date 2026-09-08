/// **本文の構造を、深さの並びから木にする。**
///
/// アウトライン（どこに何が在るか）と折りたたみ（範囲）が載る。
///
/// ## 深さは `TagHit` が持っている
///
/// 知り方は表記ごとに違う（XML は閉じ札、sxml は再帰、fsb は字下げ、
/// CE は `{ }`）が、**この 1 本 はそれを知らない** ——
/// 受け取るのは深さの付いた札の並びだけ。
///
/// 版の頭で測ったら、同梱 176 本 を 3 表記 に通して (名前, 深さ) の並びが
/// **176 / 176 本 一致**した。だから**表記を切り替えてもアウトラインは同じ。**
///
/// F# の CE だけは `{ }` の段なので、要素の入れ子とは別物（176 本 中
/// 145 本 で違う）。**それが正しい** —— あちらで見ているのは CE の本文。
///
/// ## 範囲の終わりは「中身の最後の札」
///
/// **閉じ札が在るなら、それも中身のうち**（XML の `</action>`）。
/// 無い 3 表記 では、中身の最後の開き札がそのまま終わりになる ——
/// **どちらも同じ 1 本 で出る**ので、表記ごとの分岐を持たない。
///
/// 最初は「次に同じか浅いものが来るまで」で、行が尽きたら本文の終わりまで、
/// にしていた。**最後の子が親の閉じ札を飲む** ——
/// `</bulletml>` が畳んだ中に隠れる。**畳んで初めて見えた**（並びは正しく、
/// 2 runtime も一致していたので、突き合わせの門は緑のまま）。
///
/// ## `Fable.Core` に依存しない
///
/// host（.NET）と ブラウザ側（Fable が焼いた JS）の**両方 で走る。**
module FsBulletML2.LanguageService.Outline

/// 木の節 1 つ。**子を持たない** ——
/// 入れ子は `Depth` で表す。木に積み直すのは受け取る側（Monaco へ渡す 1 か所）
/// で、そこまでは並びのまま運ぶ。**2 runtime で突き合わせているのもこの並び。**
type Node =
  { /// 要素の名前
    Name: string
    /// 名前に添える字（`label` の値など）。**無ければ空**
    Detail: string
    /// 入れ子の深さ。根が 0
    Depth: int
    /// この要素が始まる行（1 起点）
    Line: int
    /// 名前の桁（1 起点）と、その終わり
    Column: int
    EndColumn: int
    /// **この要素が覆う最後の行**（子を含む）。折りたたみが読む
    EndLine: int }

/// 深さの付いた札の並びから、木を組む。
///
/// **開始札だけ見る**（閉じ札は XML にしか無い）。
///
/// `detailAttr` はその表記で名前を載せる属性（`Refs.pairs` から来る）。
/// **空なら添え字を出さない** —— 語彙が引けていないときに、
/// 属性名を決め打ちで探すと表記ごとに割れる。
///
/// 行と桁は `positionOf`（本文の位置 -> 1 起点 の (行, 桁)）で引く。
/// **この 1 本 は本文を持たない** —— 位置から行桁を出すのは `Scan` の仕事
/// （`Scan.lineColumn` がその 1 本）。本文の行数も要らない
/// （**終わりは札から出す**）。
let build
  (detailAttr: string)
  (positionOf: int -> struct (int * int))
  (tags: TagHit list)
  : Node list =
  // **閉じ札も残す。** 在る表記では、それが中身の最後になる
  let arr = List.toArray tags
  [ for i in 0 .. arr.Length - 1 do
      let t = arr.[i]
      if not t.Closing then
        let struct (line, column) = positionOf t.NameStart
        // **この要素の中身が終わるところ。**
        // 終わらせるのは「同じか浅い開き札」と「浅い閉じ札」（＝親が閉じた）——
        // 自分の閉じ札（同じ深さ）はまだ中身のうち
        let mutable j = i + 1
        while j < arr.Length
              && not (if arr.[j].Closing then arr.[j].Depth < t.Depth
                      else arr.[j].Depth <= t.Depth) do
          j <- j + 1
        // 1 つ 手前 の札が中身の最後。中身が無ければ自分自身
        let struct (inner, _) = positionOf arr.[j - 1].NameStart
        let endLine = max line inner
        let detail =
          if detailAttr = "" then ""
          else
            t.Attrs
            |> List.tryFind (fun a -> a.AttrName = detailAttr)
            |> function
               | Some a -> a.Value
               | None -> ""
        yield
          { Name = t.TagName
            Detail = detail
            Depth = t.Depth
            Line = line
            Column = column
            // 名前の終わりの桁。**`AttrHit` と同じ 1 起点**
            EndColumn = column + (t.NameStop - t.NameStart)
            EndLine = endLine } ]

/// 2 つ の runtime で同じ答えが返ることを見る口（`guard-fable-parity`）。
///
/// **組み立てはここ 1 か所。** 表記は XML で固定する ——
/// どの表記で数えるかを引数にすると、表のほうが 2 runtime で割れうる
let describe (source: string) : string =
  let sb = System.Text.StringBuilder()
  let add (s: string) = sb.Append s |> ignore
  let nodes = build "label" (Scan.lineColumn source) (XmlScan.tags source)
  if List.isEmpty nodes then add "-"
  else
    nodes
    |> List.iteri (fun i n ->
         if i > 0 then add ","
         add (String.replicate n.Depth " ")
         add n.Name
         if n.Detail <> "" then
           add "("
           add n.Detail
           add ")"
         add "@"
         add (string n.Line)
         add "-"
         add (string n.EndLine))
  sb.ToString()
