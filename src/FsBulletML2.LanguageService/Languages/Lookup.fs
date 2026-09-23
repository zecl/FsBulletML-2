/// 語彙を引いて候補と hover を出す 1 本。表記を知らない。
/// 割れ目は `Shape` だけ。残りを表記ごとに分けない。
module FsBulletML2.LanguageService.Languages.Lookup

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// 表記ごとに違うところ。これだけ。
type Shape =
  { Kind: SourceKind
    /// エディタ側の language id。表記と 1 対 1 とは限らない
    EditorLanguageId: string
    TriggerCharacters: string list
    ContextAt: string -> int -> Context
    TokenAt: string -> int -> Token
    /// 本文の中の要素を全部。カーソルを見ない側。
    /// `XxxScan.tags` と host の `Tags` は同じものを指す。分けると波線と rename がずれる。
    Tags: string -> TagHit list
    /// 式が書ける要素の中身（v4.1）。要素名は渡してもらう。
    /// 器に表を書くと、DTD が動いたとき古びる。
    Texts: string -> string list -> TextHit list
    /// 属性を入れるときの字。`$0` がカーソルの置き場
    AttrSnippet: string -> string
    /// 雛形をその表記の字にする（v2.6）。字下げは 0 段 から。
    /// `SourceWriter` と 2 か所。片方 だけ直すと `FrameWrite.Tests` が落ちる。
    WriteFrame: Frame -> string
    /// 属性を入れるとき、手前 何文字 を置き換えるか。
    /// sxml は手前の `(` も食う。食わないと `((label "…")` になる。
    AttrReplace: string -> int -> int
    /// hover の見出し（要素）。その表記の書き方で
    ElementTitle: string -> string
    /// hover の見出し（属性値）。同上
    AttrValueTitle: string -> string -> string
    /// 無い定義を根の直下に作る。根が閉じていなければ None。
    /// 返すのは (挿す位置, 入れる字)。字下げは本文から測る。
    DefinitionAt: string -> string -> string -> string -> (int * string) option }

/// hover の markdown。`Token` から先は表記を知らない。
/// DTD の行はコードフェンスへ。素だと `<` が食われ、消えても hover は浮く。
/// 見出しだけ呼ぶ側が決める。組み立てを表記ごとに持たない。
let hover (v: Vocab) (title: Token -> string) (token: Token) : string option =
  let element name = v.Elements |> List.tryFind (fun e -> e.Name = name)
  let attribute el at =
    element el |> Option.bind (fun e -> e.Attrs |> List.tryFind (fun a -> a.Name = at))
  let block (prose: string) (lines: string list) =
    let head = "**`" + title token + "`**\n\n" + prose
    match lines |> List.filter (fun l -> l <> "") with
    | [] -> head
    | ls -> head + "\n\n```xml\n" + String.concat "\n" ls + "\n```"
  match token with
  | Nothing -> None
  | Element name ->
    element name
    |> Option.map (fun e -> block e.Spec (e.Dtd :: (e.Attrs |> List.map (fun a -> a.Dtd))))
  | Attribute (el, at) -> attribute el at |> Option.map (fun a -> block a.Spec [ a.Dtd ])
  | AttrValue (el, at, value) ->
    attribute el at
    |> Option.bind (fun a ->
         a.ValueSpecs
         |> List.tryFind (fun (v, _) -> v = value)
         |> Option.map (fun (_, spec) ->
              let spec =
                if List.contains value a.Defaults
                then spec + "\n\n**省いたときはこれ。**"
                else spec
              block spec []))

/// 同じ名前が本文のどこに書いてあるか。表記を知らない 1 本。
/// 走る先は `Refs.pairs`。定義側と参照側の両方から引く。
let usages (v: Vocab) (tags: string -> TagHit list) (source: string) (token: Token) : Usage list =
  match token with
  | AttrValue (element, attr, value) ->
    let related =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      |> List.filter (fun (refName, defName, a) ->
           a = attr && (refName = element || defName = element))
    match related with
    | [] -> []
    | _ ->
      let names = related |> List.collect (fun (r, d, _) -> [ r; d ]) |> Set.ofList
      let defs = related |> List.map (fun (_, d, _) -> d) |> Set.ofList
      tags source
      // 閉じ札を数えない。 XML だけが返すもので、属性を持たない
      |> List.filter (fun t -> not t.Closing && names.Contains t.TagName)
      |> List.collect (fun t ->
           t.Attrs
           |> List.filter (fun a -> a.AttrName = attr && a.Value = value)
           |> List.map (fun a -> t.TagName, a))
      |> List.map (fun (tagName, a) ->
           { Line = a.Line
             Column = a.Column
             EndColumn = a.EndColumn
             Text = a.Value
             IsDefinition = defs.Contains tagName })
  | _ -> []

/// カーソルの下の無い参照を、どう直せるか。表記を知らない 1 本。
/// 波線に紐づけない。本文から数え直す。紐づけると Apply の直後しか出ない。
/// 近さは 1 まで。作れない表記は `None`。
let fixes
  (v: Vocab)
  (tags: string -> TagHit list)
  (definitionAt: string -> string -> string -> string -> (int * string) option)
  (elementTitle: string -> string)
  (source: string)
  (offset: int)
  : Fix list =
  let pairs =
    Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
  Refs.missing pairs (tags source)
  // カーソルがその名前の上だけ。本文の全部 は出さない。
  |> List.filter (fun m -> offset >= m.Hit.ValueStart && offset <= m.Hit.ValueStop)
  |> List.collect (fun m ->
       let renames =
         m.Defined
         |> List.filter (fun d -> Distance.within1 m.Hit.Value d)
         |> List.map (fun d ->
              { Title = m.Hit.Value + " を " + d + " に直す"
                Line = m.Hit.Line
                Column = m.Hit.Column
                EndColumn = m.Hit.EndColumn
                Text = d })
       // 綴りの直しが在っても「定義を作る」は出す。近い名前と使いたい名前は別。
       let create =
         match definitionAt source m.DefName m.AttrName m.Hit.Value with
         | None -> []
         | Some (at, text) ->
           let struct (line, column) = Scan.lineColumn source at
           [ { Title = m.Hit.Value + " の " + elementTitle m.DefName + " を作る"
               Line = line
               Column = column
               // 幅 0。 置き換えではなく挿し込み
               EndColumn = column
               Text = text } ]
       renames @ create)

/// 定義の行の上に出す字（v4.3）。表記を知らない 1 本。
/// CE もここを通す。中に戻すと「参照 0 か所」の文面が割れる。
let lenses (v: Vocab) (tags: string -> TagHit list) (source: string) : Lens list =
  let pairs =
    Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
  Refs.uses pairs (tags source)
  |> List.map (fun d ->
       // 根から走る定義は「0 か所」ではない。 そこは入口
       let isEntry = v.TopPrefix <> "" && d.Name.StartsWith v.TopPrefix
       if isEntry && d.Uses = 0 then
         { Title = "根から走る"; Name = d.Name; Line = d.Hit.Line; Column = d.Hit.Column; Clickable = false }
       elif d.Uses = 0 then
         { Title = "どこからも参照されていない"; Name = d.Name; Line = d.Hit.Line; Column = d.Hit.Column; Clickable = false }
       else
         { Title = string d.Uses + " か所 から参照"
           Name = d.Name
           Line = d.Hit.Line
           Column = d.Hit.Column
           Clickable = true })

/// 語彙を引いて候補を出す。語彙は引数で受け取る ——
/// このクラスが host を知らないので、次の表記も同じ形で書ける
type VocabularyLanguage(shape: Shape, vocabulary: unit -> Vocab) =

  // 直前の 1 本 だけ覚える。本文が変われば捨てる。
  // 欄を行き来すると数え直す。答えは変わらない。
  let mutable tagSource: string = null
  let mutable tagCache: TagHit list = []

  let tagsOf (source: string) =
    if tagSource <> source then
      tagSource <- source
      tagCache <- shape.Tags source
    tagCache

  /// 式が書ける要素の名前。`Findings` と `Hints` が同じものを見る
  /// （2 か所 に書くと「波線は出るのに値が出ない」になる）
  let exprElementNames (v: Vocab) =
    v.Elements |> List.filter (fun e -> e.Text) |> List.map (fun e -> e.Name)

  // 名前の並びも鍵に入れる。冗長だが残す。
  // `Texts` は並びにも依る。別の並びを渡すと古い答えが返る。
  let mutable textSource: string = null
  let mutable textNames: string list = []
  let mutable textCache: TextHit list = []

  let textsOf (source: string) (names: string list) =
    if textSource <> source || textNames <> names then
      textSource <- source
      textNames <- names
      textCache <- shape.Texts source names
    textCache

  let find name = (vocabulary ()).Elements |> List.tryFind (fun e -> e.Name = name)

  member _.Shape = shape

  member _.Candidates(source: string, offset: int) : Completion list =
    let plain = Completion.plain (Scan.nameLenBefore source offset)
    match shape.ContextAt source offset with
    | InContent None ->
      // 根の外。名前を書かない。根は誰の子でもない要素で引ける。
      let elements = (vocabulary ()).Elements
      let children = elements |> List.collect (fun e -> e.Children) |> Set.ofList
      elements
      |> List.filter (fun e -> not (children.Contains e.Name))
      |> List.map (fun e -> plain e.Name)
    | InContent (Some parent) ->
      match find parent with
      | None -> []
      | Some e ->
        let children = e.Children |> List.map plain
        // 雛形は要素名の前 に出す。書き始めは「どう書くか」で止まる。
        let frames =
          (vocabulary ()).Frames
          |> List.filter (fun s -> List.contains parent s.In)
          |> List.map (fun s ->
               { Label = s.Label
                 Insert = shape.WriteFrame s.Frame
                 Snippet = true
                 IsFrame = true
                 Replace = Scan.nameLenBefore source offset })
        if not e.Text then frames @ children
        else
          let exprLen = Scan.exprLenBefore source offset
          frames @ children @ ((vocabulary ()).Expressions |> List.map (Completion.plain exprLen))
    | InStartTag element ->
      match find element with
      | None -> []
      | Some e ->
        // 値まで入れて、引用符の中へカーソルを置く。名前だけだと手で足すことになる。
        let replace = shape.AttrReplace source offset
        e.Attrs
        |> List.map (fun a ->
             { Label = a.Name
               Insert = shape.AttrSnippet a.Name
               Snippet = true
               IsFrame = false
               Replace = replace })
    | InAttrValue (element, attr) ->
      match find element with
      | None -> []
      | Some e ->
        match e.Attrs |> List.tryFind (fun a -> a.Name = attr) with
        | Some a -> a.Values |> List.map plain
        | None -> []

  /// 見出しは表記ごと。`Token` から先は `Lookup.hover` の 1 本。
  /// 属性の見出しだけ表記に依らない。`fire/@label` は道しるべであって打つ字ではない。
  member private _.Title(token: Token) =
    match token with
    | Element name -> shape.ElementTitle name
    | Attribute (el, at) -> el + "/@" + at
    | AttrValue (_, at, value) -> shape.AttrValueTitle at value
    | Nothing -> ""

  member this.HoverAt(source: string, offset: int) : string option =
    hover (vocabulary ()) this.Title (shape.TokenAt source offset)

  /// カーソルの下の名前が、本文のどこに書いてあるか。
  /// 中身は `Lookup.usages` の 1 本（表記を知らない）。
  member _.UsagesAt(source: string, offset: int) : Usage list =
    usages (vocabulary ()) tagsOf source (shape.TokenAt source offset)

  /// カーソルの下の「無い参照」を、どう直せるか。
  /// 中身は `Lookup.fixes` の 1 本（表記を知らない）。
  member _.FixesAt(source: string, offset: int) : Fix list =
    fixes (vocabulary ()) tagsOf shape.DefinitionAt shape.ElementTitle source offset

  /// 定義の行の上に出す字（v4.3）。中身は `Lookup.lenses` の 1 本。
  /// `Semantics.UnusedDefinition` と同じ材料。別々に数えるな。
  member _.LensesIn(source: string) : Lens list =
    lenses (vocabulary ()) tagsOf source

  /// 値を横に出す先（v4.4）。`Texts` の 1 本 の上（v4.1 と同じ材料）。
  member _.HintsIn(source: string) : ExprSpot list =
    let v = vocabulary ()
    let names = exprElementNames v
    // 絞ってから行桁へ直す。1 つ ずつ `lineColumn` すると本文を何度も走る。
    let kept =
      textsOf source names
      |> List.choose (fun h ->
           let t = h.Text.Trim()
           if t = "" then None
           elif not (ExprCheck.readable t) then None
           elif ExprCheck.plainNumber t then None
           // `$rand` を含む式は出さない。毎回変わるので、横の固定値は嘘になる。
           elif t.Contains "$rand" then None
           else Some (h.TagName, t, h.Stop))
    let places = Scan.lineColumnsAscending source (kept |> List.map (fun (_, _, stop) -> stop))
    List.map2
      (fun (element, text, _) (struct (line, column)) ->
        { Text = text; Element = element; Line = line; Column = column })
      kept
      places

  /// 参照が渡す引数の形（v4.5）。`Refs.arity` の 1 本 の上。
  member _.SignatureIn(source: string, offset: int) : Signature option =
    let v = vocabulary ()
    let pairs =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    let tags = tagsOf source |> List.toArray
    // いちばん内側 の `◯◯Ref`。 入れ子に成りうる（`fireRef` の中の `bulletRef`）
    let mutable found = -1
    let mutable stop = source.Length
    for i in 0 .. tags.Length - 1 do
      let t = tags.[i]
      if not t.Closing && pairs |> List.exists (fun (refName, _, _) -> refName = t.TagName) then
        let mutable k = i + 1
        while k < tags.Length && tags.[k].Depth > t.Depth do k <- k + 1
        let until = if k < tags.Length then tags.[k].Start else source.Length
        if offset >= t.Start && offset <= until then
          found <- i
          stop <- until
    if found < 0 then None
    else
      let t = tags.[found]
      let attrName =
        pairs |> List.pick (fun (refName, _, a) -> if refName = t.TagName then Some a else None)
      let label =
        t.Attrs |> List.tryPick (fun a -> if a.AttrName = attrName then Some a.Value else None)
      let label = defaultArg label ""
      // 引数の要素名を書かない。書くと器に表が増えて `guard-playground-boundaries` が拾う。
      let argNames =
        v.Elements
        |> List.tryFind (fun e -> e.Name = t.TagName)
        |> Option.map (fun e -> e.Children)
        |> Option.defaultValue []
      let mutable given = []
      let mutable active = -1
      let mutable k = found + 1
      let mutable n = 0
      while k < tags.Length && tags.[k].Depth > t.Depth do
        let u = tags.[k]
        if not u.Closing && List.contains u.TagName argNames && u.Depth = t.Depth + 1 then
          let mutable m = k + 1
          while m < tags.Length && tags.[m].Depth > u.Depth do m <- m + 1
          let uEnd = if m < tags.Length then tags.[m].Start else stop
          given <- n :: given
          if offset >= u.Start && offset <= uEnd then active <- n
          n <- n + 1
        k <- k + 1
      let takes =
        Refs.arity pairs source (List.ofArray tags)
        |> List.filter (fun a -> a.Name = label)
        |> List.fold (fun best a -> max best a.Takes) 0
      // 見出しは大きいほう まで並べる —— 多く渡していることも形として見える
      let shown = max takes n
      let sb = System.Text.StringBuilder()
      sb.Append(label).Append("(") |> ignore
      let ranges =
        [ for i in 1 .. shown do
            if i > 1 then sb.Append(", ") |> ignore
            let from = sb.Length
            sb.Append("$").Append(i) |> ignore
            yield (from, sb.Length) ]
      sb.Append(")") |> ignore
      let detail =
        if label = "" then "名前がまだ無い"
        elif takes = 0 && n = 0 then "この定義は引数を使わない"
        elif takes = n then string takes + " つ 取って、" + string n + " つ 渡している"
        else
          string takes + " つ 使っているが、" + string n + " つ しか渡していない"
          |> fun s -> if n > takes then string takes + " つ 使っているのに、" + string n + " つ 渡している" else s
      Some { Label = sb.ToString(); Params = ranges; Active = active; Detail = detail }

  member _.FindingsIn(source: string) : Semantics.Finding list =
    let v = vocabulary ()
    let pairs =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    // 式は別の 1 本。材料は `TextHit` で `TagHit` ではない。
    let exprNames = exprElementNames v
    let expr = Semantics.exprFindings source (textsOf source exprNames)
    // 混ぜたら本文の順。`NoEntryPoint` だけ先頭に残す。
    let sem = Semantics.findings pairs v.TopPrefix (tagsOf source)
    match sem with
    | first :: rest when first.Kind = Semantics.NoEntryPoint ->
        first :: (rest @ expr |> List.sortBy (fun f -> f.Line, f.Column))
    | _ -> sem @ expr |> List.sortBy (fun f -> f.Line, f.Column)

  /// 本文の構造（v2.4）。中身は `Outline.build`。表記を知らない。
  member _.OutlineOf(source: string) : Outline.Node list =
    let v = vocabulary ()
    let detailAttr =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      |> List.tryHead
      |> Option.map (fun (_, _, attr) -> attr)
      |> Option.defaultValue ""
    // `Scan.lineColumn` を渡さない。札ごとに引くと本文を札の数だけ走る。
    Outline.build detailAttr (Scan.lineColumnLookup source) (tagsOf source)

  interface ISourceLanguage with
    member this.Lenses source = this.LensesIn source
    member this.Hints source = this.HintsIn source
    member this.Signature source offset = this.SignatureIn(source, offset)
    member _.Kind = shape.Kind
    member _.EditorLanguageId = shape.EditorLanguageId
    member _.TriggerCharacters = shape.TriggerCharacters
    member this.Complete source offset = this.Candidates(source, offset)
    member this.Hover source offset = this.HoverAt(source, offset)
    member this.Usages source offset = this.UsagesAt(source, offset)
    member this.Fixes source offset = this.FixesAt(source, offset)
    member this.Findings source = this.FindingsIn source
    member this.Outline source = this.OutlineOf source
    /// 3 表記 とも同じ 1 本。k 番目は表記に依らない。
    member _.NodeSpans source nodes =
      Scan.nodeSpans source (tagsOf source) nodes
