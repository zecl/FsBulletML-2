/// **語彙を引いて候補と hover を出す 1 本。表記を知らない。**
///
/// v0.8 まで、この中身は `Languages/Xml.fs` に在った。**実装が 1 本 のうちは、
/// どこまでが XML の話でどこからが表記に依らない話かが分からない** ——
/// sxml を足して初めて割れた。
///
/// 割れ目は `Shape` に挙げたものだけで、**残りは全部 共通。**
/// 語彙の引き方も、根をどう見つけるかも、hover に何を並べるかも、
/// 表記の話ではなかった。
module FsBulletML2.LanguageService.Languages.Lookup

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// 表記ごとに違うところ。**これだけ。**
type Shape =
  { Kind: SourceKind
    /// エディタ側の language id。表記と 1 対 1 とは限らない
    EditorLanguageId: string
    TriggerCharacters: string list
    ContextAt: string -> int -> Context
    TokenAt: string -> int -> Token
    /// 本文の中の要素を全部。**カーソルを見ない側**で、
    /// 「同じ名前がどこに在るか」を数えるのに要る（`Usages`）。
    ///
    /// **表記ごとの 1 本 は `XxxScan.tags`。** host 側の `ISourceReader.Tags` が
    /// 同じものを指している —— 波線と rename が同じ数え方を見る
    Tags: string -> TagHit list
    /// 式が書ける要素の中身（v4.1）。受け取るのは `#PCDATA` を取る要素名。
    ///
    /// **取り方は表記ごとに違う** —— XML は札のあいだ、sxml は括弧の中の
    /// 引用符、fsb は `:` のあとの引用符。**F# の CE は空**（下の但し書き）。
    ///
    /// **要素名は渡してもらう。** 正本は語彙の `Text` で、それは Core の
    /// DTD から来る —— 器に表を書くと、DTD が動いたとき黙って古びる
    Texts: string -> string list -> TextHit list
    /// 属性を入れるときの字。`$0` がカーソルの置き場
    AttrSnippet: string -> string
    /// 雛形をその表記の字にする（v2.6）。**字下げは 0 段 から** ——
    /// 入れる場所の桁は Monaco が合わせる（snippet は行頭を揃える）。
    ///
    /// **`SourceWriter` と 2 か所 になる。** 骨から字を作る手が、
    /// 焼く側とここに 1 つ ずつ在る形なので、**門で突き合わせる**
    /// （`FrameWrite.Tests`）—— 片方 だけ直すのを止める
    WriteFrame: Frame -> string
    /// 属性を入れるとき、手前 何文字 を置き換えるか。
    ///
    /// **XML は名前のぶんだけ。sxml は手前の `(` も食う** —— 食わないと
    /// `((label "…")` になる。`$rand` の `$` を食うのと同じ形で、
    /// **何が語かを知っているのは表記のほう。**
    AttrReplace: string -> int -> int
    /// hover の見出し（要素）。その表記の書き方で
    ElementTitle: string -> string
    /// hover の見出し（属性値）。同上
    AttrValueTitle: string -> string -> string
    /// 無い定義を**根の直下**に作る。受け取るのは
    /// (本文, 作る要素名, 名前の属性名, 名前)。
    ///
    /// 返すのは **(挿す位置（0 起点 の文字数）, そこへ入れる字)**。
    /// 置き換えではなく挿し込みなので、範囲は幅 0。
    ///
    /// **根が閉じていなければ `None`。** 打っている途中の本文はふつうに
    /// 閉じていないので、そこで場所を決め打つと本文の外に出る。
    ///
    /// **字下げは本文から測る。** 根の直下 に既に在る札の桁に合わせる ——
    /// 書き手が 4 で焼いても、人が 2 で書き直していることは在る。
    ///
    /// 挿す先を根の直下 にしたのは、**同梱 176 本 の定義 767 個 のうち
    /// 766 個 がそこに在る**から（v1.8 の頭で数えた）
    DefinitionAt: string -> string -> string -> string -> (int * string) option }

/// hover に出す markdown を組む 1 本。**`Token` から先は表記を知らない。**
///
/// **DTD の行は必ずコードフェンスに入れる。** markdown は `<` をタグとして
/// 食うので、素で渡すと `<!ELEMENT ...>` が丸ごと消える。**消えても hover は
/// 浮く**ので、目でも試験でも「出ていない」には見えない。
///
/// **フェンスの言語は表記に依らず `xml`。** 中に入るのは DTD の行で、
/// それは sxml で書いても XML の DTD のまま（語彙の正本は `Core/DTD.fs`）。
///
/// 重複させない —— 「置ける子」も「取る値」も「既定」も、DTD の行が既に
/// 言っている。属性値のときだけ「省いたときはこれ」を足す（あの行は
/// 属性の hover にしか出ないので）。
///
/// **見出しだけ呼ぶ側が決める。** 表記ごとに書き方が違い、F# の CE では
/// **要素名ですらない**（`aim` と打つと `<direction type="aim">` になる）。
/// 組み立てを表記ごとに持つと、同じ語彙から出た同じ hover が表記の差に見える
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

/// 同じ名前が本文のどこに書いてあるか。**表記を知らない 1 本。**
///
/// 要るのは「いま何の上に居るか」（`token`）と「本文に何が在るか」（`tags`）の
/// 2 つ だけ。**どちらも表記ごとの数え方が作る**が、そこから先は共通 ——
/// v1.2 の頭で 3 表記 に同じ弾幕を通し、**対象の数が完全に一致する**ことを
/// 測ってからこの形にした。
///
/// v1.9 で **F# の CE も同じ 1 本 を通る**ようになった。あちらは要素名を
/// 打たないので `Tags` が空だったが、**無いのは要素名であって名前ではない**
/// （`defAction "x"` の `x` は `<action label="x">` の `x` そのもの）。
///
/// 走る先の対は語彙から引く（`Refs.pairs`）。**host 側の波線と同じ 1 本。**
///
/// **定義側からも参照側からも引ける。** 片方 だけだと
/// 「参照からしか直せない」ことになる。
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
      // **定義側の要素名。** 参照側と重ならないことは測ってある
      // （`Usage.IsDefinition` の但し書き）—— 重なっていたら、
      // 札の名前だけではどちら側か言えない
      let defs = related |> List.map (fun (_, d, _) -> d) |> Set.ofList
      tags source
      // **閉じ札を数えない。** XML だけが返すもので、属性を持たない
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

/// カーソルの下の「無い参照」を、どう直せるか。**表記を知らない 1 本。**
///
/// **波線に紐づけない。** 本文から数え直す（`Refs.missing`）——
/// 波線は 1 文字 打った時点で消えるので、紐づけると
/// **Apply の直後の窓でしか出ない**（v1.3 の頭で現物に当てた）。
///
/// **近さは 1 まで。** コーパスで数えたら、候補が 2 個 以上 在る弾幕でも
/// 距離 1 以内 がちょうど 1 個 に絞れた（23 / 23）。
///
/// 表記ごとに渡すのは 2 つ だけ —— 定義を作る場所（`definitionAt`）と、
/// 見出しの書き方（`elementTitle`）。**作れない表記は `None` を返す。**
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
  // カーソルがその名前の上に在るものだけ。**本文の全部 を出さない** ——
  // 直すのはいま見ているところで、他所の分は他所で押す
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
       // **綴りの直しが在っても出す。** 近い名前が在ることと、
       // その名前を使いたいことは別 —— 打ち間違いではなく
       // 「まだ書いていない」ことのほうが多い
       let create =
         match definitionAt source m.DefName m.AttrName m.Hit.Value with
         | None -> []
         | Some (at, text) ->
           let struct (line, column) = Scan.lineColumn source at
           [ { Title = m.Hit.Value + " の " + elementTitle m.DefName + " を作る"
               Line = line
               Column = column
               // **幅 0。** 置き換えではなく挿し込み
               EndColumn = column
               Text = text } ]
       renames @ create)

/// 定義の行の上に出す字（v4.3）。**表記を知らない 1 本。**
///
/// v4.8 で `VocabularyLanguage` の中から出した —— **CE もここを通る。**
/// 中に置いたままだと、CE 側 に同じ組み立てを書くことになり、
/// 「参照 0 か所」の文面が 2 通り に割れる。
///
/// 表記ごとに渡すのは `Tags` だけ。
let lenses (v: Vocab) (tags: string -> TagHit list) (source: string) : Lens list =
  let pairs =
    Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
  Refs.uses pairs (tags source)
  |> List.map (fun d ->
       // **根から走る定義は「0 か所」ではない。** そこは入口
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

/// 語彙を引いて候補を出す。**語彙は引数で受け取る** ——
/// このクラスが host を知らないので、次の表記も同じ形で書ける
type VocabularyLanguage(shape: Shape, vocabulary: unit -> Vocab) =

  // **同じ本文を、1 打鍵 で 4 回 走査していた**（v4.9 で測った）——
  // 打鍵ごとに走る口はどれも `Tags` を自分で呼ぶので、いちばん長い本
  // （29,190 字）で 1.3 ms x 4 本。`Texts` も 2 本 が呼んで 1.5 ms x 2。
  // **合計 10.7 ms のうち 8.2 ms が、同じ走査の焼き直し**だった。
  //
  // **直前の 1 本 だけ覚える。** Monaco は 1 打鍵 で、全部 の provider を
  // **同じ本文で**呼ぶ —— 本文が変われば捨てる。
  //
  // 覚えるのが 1 つ なので、2 つ の欄（v2.1 の並べて見る）を行き来すると
  // 毎回 数え直す。**答えは変わらない**（速いか遅いかだけ）。
  let mutable tagSource: string = null
  let mutable tagCache: TagHit list = []

  let tagsOf (source: string) =
    if tagSource <> source then
      tagSource <- source
      tagCache <- shape.Tags source
    tagCache

  /// 式が書ける要素の名前。**`Findings` と `Hints` が同じものを見る。**
  ///
  /// v4.9 まで 2 か所 に同じ式が書いてあった —— 片方 だけ直すと
  /// 「波線は出るのに値が出ない」（逆も）になる。
  let exprElementNames (v: Vocab) =
    v.Elements |> List.filter (fun e -> e.Text) |> List.map (fun e -> e.Name)

  // **名前の並びも鍵に入れる。**
  //
  // ### この守りは冗長で、外しても答えが変わらない
  //
  // 較正で当てて **0 点** だった。渡す側は上の 1 本 しか無いので、
  // 名前の並びは呼ぶたびに同じ。
  //
  // **それでも残す。** この覚える口 が正しいのは「鍵が同じなら答えも同じ」
  // だからで、`Texts` は名前の並びにも依る —— 3 つ 目 の呼ぶ側が
  // 別の並びを渡した日に、**答えが古いまま返る**。
  // **残す理由を書いておく。**
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
      // 根の外。置けるのは根の要素だけ。
      // **名前を書かない** —— 根は「誰の子にもなっていない要素」で引ける。
      // 書くと、この段だけが BulletML を知っていることになる
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
        // 雛形（v2.6）。**要素名の候補の前 に出す** —— 書き始めで止まるのは
        // 「何が置けるか」ではなく「どう書くか」のほうなので、
        // 形が先に見えるほうがよい
        let frames =
          (vocabulary ()).Frames
          |> List.filter (fun s -> List.contains parent s.In)
          |> List.map (fun s ->
               { Label = s.Label
                 Insert = shape.WriteFrame s.Frame
                 Snippet = true
                 IsFrame = true
                 Replace = Scan.nameLenBefore source offset })
        // #PCDATA を取る要素の中では式も書ける
        if not e.Text then frames @ children
        else
          let exprLen = Scan.exprLenBefore source offset
          frames @ children @ ((vocabulary ()).Expressions |> List.map (Completion.plain exprLen))
    | InStartTag element ->
      match find element with
      | None -> []
      | Some e ->
        // **値まで入れて、引用符の中へカーソルを置く。**
        // 名前だけ入れると、必ず手で数文字 足すことになる
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

  /// 見出しは表記ごと。**`Token` から先は `Lookup.hover` の 1 本。**
  ///
  /// 属性の見出しだけ表記に依らない —— `fire/@label` は道しるべであって、
  /// その表記で打つ字ではない
  member private _.Title(token: Token) =
    match token with
    | Element name -> shape.ElementTitle name
    | Attribute (el, at) -> el + "/@" + at
    | AttrValue (_, at, value) -> shape.AttrValueTitle at value
    | Nothing -> ""

  member this.HoverAt(source: string, offset: int) : string option =
    hover (vocabulary ()) this.Title (shape.TokenAt source offset)

  /// カーソルの下の名前が、本文のどこに書いてあるか。
  /// **中身は `Lookup.usages` の 1 本**（表記を知らない）。
  ///
  /// ここが渡すのは `TokenAt`（いま何の上に居るか）と `Tags`（本文に何が
  /// 在るか）の 2 つ だけ —— **どちらも表記ごとの 1 本 を指しているだけ。**
  member _.UsagesAt(source: string, offset: int) : Usage list =
    usages (vocabulary ()) tagsOf source (shape.TokenAt source offset)

  /// カーソルの下の「無い参照」を、どう直せるか。
  /// **中身は `Lookup.fixes` の 1 本**（表記を知らない）。
  ///
  /// 表記ごとに渡すのは、定義を作る場所と見出しの書き方の 2 つ。
  member _.FixesAt(source: string, offset: int) : Fix list =
    fixes (vocabulary ()) tagsOf shape.DefinitionAt shape.ElementTitle source offset

  /// 読めて・組めても走らないもの（v2.3）。
  /// **中身は `Semantics.findings` の 1 本**（表記を知らない）。
  ///
  /// 表記ごとに渡すのは `Tags` だけ。対も `top` の綴りも語彙から引く ——
  /// **どちらも書き写さない**（対は `Refs.pairs`、綴りは Core が持つ）
  /// 定義の行の上に出す字（v4.3）。**中身は `Lookup.lenses` の 1 本**
  /// （表記を知らない）。
  ///
  /// `Semantics.UnusedDefinition`（v2.3 の青い波線）と**同じ材料**を見ている ——
  /// あちらは `Refs.pairs`、こちらは `Refs.uses`。
  /// 同梱で別々 に数えて、**どちらも同じ 6 件**を指した。
  member _.LensesIn(source: string) : Lens list =
    lenses (vocabulary ()) tagsOf source

  /// 値を横に出す先（v4.4）。**`Texts` の 1 本 の上**（v4.1 と同じ材料）。
  member _.HintsIn(source: string) : ExprSpot list =
    let v = vocabulary ()
    let names = exprElementNames v
    // **絞ってから行桁へ直す。** 1 つ ずつ `lineColumn` を呼ぶと
    // 本文を出す先の数ぶん 走る（71 個 の本で 1.08 ms。実測）——
    // `lineColumnsAscending` は本文を 1 巡 する
    let kept =
      textsOf source names
      |> List.choose (fun h ->
           let t = h.Text.Trim()
           if t = "" then None
           elif not (ExprCheck.readable t) then None
           elif ExprCheck.plainNumber t then None
           // **`$rand` を含む式は出さない。** 毎回 変わるので、字の横に
           // 固定の数を出すと嘘になる —— 同梱 6,042 件 のうち 349 件（5.8%）
           elif t.Contains "$rand" then None
           else Some (h.TagName, t, h.Stop))
    let places = Scan.lineColumnsAscending source (kept |> List.map (fun (_, _, stop) -> stop))
    List.map2
      (fun (element, text, _) (struct (line, column)) ->
        { Text = text; Element = element; Line = line; Column = column })
      kept
      places

  /// 参照が渡す引数の形（v4.5）。**`Refs.arity` の 1 本 の上。**
  member _.SignatureIn(source: string, offset: int) : Signature option =
    let v = vocabulary ()
    let pairs =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    let tags = tagsOf source |> List.toArray
    // **いちばん内側 の `◯◯Ref`。** 入れ子に成りうる（`fireRef` の中の `bulletRef`）
    let mutable found = -1
    let mutable stop = source.Length
    for i in 0 .. tags.Length - 1 do
      let t = tags.[i]
      if not t.Closing && pairs |> List.exists (fun (refName, _, _) -> refName = t.TagName) then
        // 中身の終わり。**その札より深くない札が次に出てくるところ**
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
      // その参照が渡している引数の並び（**直下 だけ**）。
      //
      // **引数の要素名を書かない。** `◯◯Ref` に置ける子は語彙が持っている
      // （正本は Core の DTD）—— ここに綴りを書くと、器に要素の表が
      // 1 つ 増える（`guard-playground-boundaries` が拾う。実際に拾われた）
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
          // その引数の中身の終わり
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
      // 見出しは「定義が取る数」と「いま渡している数」の大きいほう まで並べる ——
      // **多く渡していることも形として見える**
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
    // 式（v4.1）。**別の 1 本**（材料が `TextHit` で、`TagHit` ではない）
    let exprNames = exprElementNames v
    let expr = Semantics.exprFindings source (textsOf source exprNames)
    // **並べ直す。** 2 本 から来るので、混ぜたら本文の順に戻す ——
    // `NoEntryPoint` だけは本文全体の話なので先頭 に残す
    let sem = Semantics.findings pairs v.TopPrefix (tagsOf source)
    match sem with
    | first :: rest when first.Kind = Semantics.NoEntryPoint ->
        first :: (rest @ expr |> List.sortBy (fun f -> f.Line, f.Column))
    | _ -> sem @ expr |> List.sortBy (fun f -> f.Line, f.Column)

  /// 本文の構造（v2.4）。**中身は `Outline.build` の 1 本**（表記を知らない）。
  ///
  /// 表記ごとに渡すのは `Tags` だけ —— 入れ子は `TagHit.Depth` が持っていて、
  /// その数え方はそれぞれの Scan に閉じている。
  /// 添え字に使う属性も語彙から引く（`Refs.pairs`）
  member _.OutlineOf(source: string) : Outline.Node list =
    let v = vocabulary ()
    let detailAttr =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      |> List.tryHead
      |> Option.map (fun (_, _, attr) -> attr)
      |> Option.defaultValue ""
    // **`Scan.lineColumn` を渡さない**（v4.9）—— 札 1 つ につき 2 回 引くので、
    // 本文を札の数だけ走り直すことになる（焼いた JS で 40 ms 出た）
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
    /// **3 表記 とも同じ 1 本。** 数え方（札）は表記ごとだが、
    /// 「開き札のうちノードになるものの k 番目」は表記に依らない
    member _.NodeSpans source nodes =
      Scan.nodeSpans source (tagsOf source) nodes
