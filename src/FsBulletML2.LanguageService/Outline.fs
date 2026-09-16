/// 本文の構造を、深さの並びから木にする。
///
/// この 1 本 は表記を知らない —— 受け取るのは深さの付いた札の並びだけ。
/// 閉じ札が在るなら、それも中身のうち。 無い表記では中身の最後の開き札が終わり。
///
/// 最後の子が親の閉じ札を飲む形は捨てた（`</bulletml>` が畳んだ中に隠れる）。
/// `Fable.Core` に依存しない（host と ブラウザ側 の両方 で走る）。
module FsBulletML2.LanguageService.Outline

/// 木の節 1 つ。子を持たない ——
/// 入れ子は `Depth` で表す。木に積み直すのは受け取る側（Monaco へ渡す 1 か所）
/// で、そこまでは並びのまま運ぶ。2 runtime で突き合わせているのもこの並び。
type Node =
  { /// 要素の名前
    Name: string
    /// 名前に添える字（`label` の値など）。無ければ空
    Detail: string
    /// 入れ子の深さ。根が 0
    Depth: int
    /// この要素が始まる行（1 起点）
    Line: int
    /// 名前の桁（1 起点）と、その終わり
    Column: int
    EndColumn: int
    /// この要素が覆う最後の行（子を含む）。折りたたみが読む
    EndLine: int }

/// 深さの付いた札の並びから、木を組む。開始札だけ見る。
/// detailAttr が空なら添え字を出さない（属性名を決め打ちすると表記ごとに割れる）。
let build
  (detailAttr: string)
  (positionOf: int -> struct (int * int))
  (tags: TagHit list)
  : Node list =
  // 閉じ札も残す。 在る表記では、それが中身の最後になる
  let arr = List.toArray tags
  // 内包表記の中で `while` を回さない（v4.9）——
  // Fable は内包の中の `while` を enumerator の鎖に焼くので、
  // 1 歩 進むごとに物が 3 つ 増える。ここは札の数だけ内側を歩くので、
  // 同じソースで .NET は速く、焼いた JS だけが 56 ms だった。
  // 素の `while` と `ResizeArray` なら、どちらも同じ歩数 で済む
  let out = ResizeArray<Node>()
  for i in 0 .. arr.Length - 1 do
    let t = arr.[i]
    if not t.Closing then
      let struct (line, column) = positionOf t.NameStart
      // この要素の中身が終わるところ。
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
      out.Add
        { Name = t.TagName
          Detail = detail
          Depth = t.Depth
          Line = line
          Column = column
          // 名前の終わりの桁。`AttrHit` と同じ 1 起点
          EndColumn = column + (t.NameStop - t.NameStart)
          EndLine = endLine }
  List.ofSeq out

/// その行を囲む節を、内側から外側へ。
///
/// 「囲む要素を選ぶ」（v2.4.5）が読む。`registerSelectionRangeProvider` は
/// この順で「広げる」を積む —— 先頭がいちばん内側で、末尾が根。
///
/// 行だけで見る。桁は見ない。 版の頭で数えたら、同じ行に節の始まりが
/// 2 つ 以上 在る行は 3 表記 とも 0 件（13121 行）だった。
/// だから行で選んでも隣の要素は飲まない。
///
/// 深さで並べる。 範囲が重なるのは親子だけなので、深いほうが内側になる
/// —— 兄弟は同じ行を共有しないので、同じ深さが 2 つ 出ることは無い。
let enclosing (nodes: Node list) (line: int) : Node list =
  nodes
  |> List.filter (fun n -> n.Line <= line && line <= n.EndLine)
  |> List.sortByDescending (fun n -> n.Depth)

/// 2 つ の runtime で同じ答えが返ることを見る口（`guard-fable-parity`）。
///
/// 組み立てはここ 1 か所。 表記は XML で固定する ——
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

/// 囲みの列を字にする口（`guard-fable-parity`）。
///
/// `describe` と分ける。 あちらは並び全部 を出すので、囲みを混ぜると
/// 行が長くなり、割れたときにどちらが割れたか読めない。
let describeEnclosing (source: string) (line: int) : string =
  let sb = System.Text.StringBuilder()
  let add (s: string) = sb.Append s |> ignore
  let nodes = build "label" (Scan.lineColumn source) (XmlScan.tags source)
  match enclosing nodes line with
  | [] -> add "-"
  | ns ->
    ns
    |> List.iteri (fun i n ->
         if i > 0 then add ">"
         add n.Name
         add "@"
         add (string n.Line)
         add "-"
         add (string n.EndLine))
  sb.ToString()