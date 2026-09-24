/// 本文の構造を、深さの並びから木にする。表記を知らない。
/// 最後の子に親の閉じ札を飲ませるな。`</bulletml>` が畳んだ中に隠れる。
module FsBulletML2.LanguageService.Outline

/// 木の節 1 つ。子を持たない。入れ子は `Depth`。
type Node =
    {
        /// 要素の名前
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
        EndLine: int
    }

/// 深さの付いた札の並びから、木を組む。開始札だけ見る。
/// detailAttr が空なら添え字を出さない（属性名を決め打ちすると表記ごとに割れる）。
let build (detailAttr: string) (positionOf: int -> struct (int * int)) (tags: TagHit list) : Node list =
    // 閉じ札も残す。 在る表記では、それが中身の最後になる
    let arr = List.toArray tags
    // 内包の中で `while` を回さない。Fable が enumerator の鎖に焼く。
    let out = ResizeArray<Node>()

    for i in 0 .. arr.Length - 1 do
        let t = arr.[i]

        if not t.Closing then
            let struct (line, column) = positionOf t.NameStart
            // 同じか浅い開き札、または浅い閉じ札で終わる。自分の閉じ札はまだ中身。
            let mutable j = i + 1

            while j < arr.Length
                  && not (
                      if arr.[j].Closing then
                          arr.[j].Depth < t.Depth
                      else
                          arr.[j].Depth <= t.Depth
                  ) do
                j <- j + 1

            let struct (inner, _) = positionOf arr.[j - 1].NameStart
            let endLine = max line inner

            let detail =
                if detailAttr = "" then
                    ""
                else
                    t.Attrs
                    |> List.tryFind (fun a -> a.AttrName = detailAttr)
                    |> function
                        | Some a -> a.Value
                        | None -> ""

            out.Add
                {
                    Name = t.TagName
                    Detail = detail
                    Depth = t.Depth
                    Line = line
                    Column = column
                    // 名前の終わりの桁。`AttrHit` と同じ 1 起点
                    EndColumn = column + (t.NameStop - t.NameStart)
                    EndLine = endLine
                }

    List.ofSeq out

/// その行を囲む節を、内側から外側へ。先頭がいちばん内側。
/// 行だけで見る。桁は見ない。
let enclosing (nodes: Node list) (line: int) : Node list =
    nodes
    |> List.filter (fun n -> n.Line <= line && line <= n.EndLine)
    |> List.sortByDescending (fun n -> n.Depth)

/// 2 runtime の突き合わせ口（`guard-fable-parity`）。
/// 表記は XML で固定する。引数にすると表のほうが割れうる。
let describe (source: string) : string =
    let sb = System.Text.StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    let nodes = build "label" (Scan.lineColumn source) (XmlScan.tags source)

    if List.isEmpty nodes then
        add "-"
    else
        nodes
        |> List.iteri (fun i n ->
            if i > 0 then
                add ","

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

/// 囲みの列を字にする口。`describe` と分ける。
/// 混ぜると、割れたときにどちらが割れたか読めない。
let describeEnclosing (source: string) (line: int) : string =
    let sb = System.Text.StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    let nodes = build "label" (Scan.lineColumn source) (XmlScan.tags source)

    match enclosing nodes line with
    | [] -> add "-"
    | ns ->
        ns
        |> List.iteri (fun i n ->
            if i > 0 then
                add ">"

            add n.Name
            add "@"
            add (string n.Line)
            add "-"
            add (string n.EndLine))

    sb.ToString()
