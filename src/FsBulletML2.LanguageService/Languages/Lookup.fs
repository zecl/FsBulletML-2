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
    /// 属性を入れるときの字。`$0` がカーソルの置き場
    AttrSnippet: string -> string
    /// 属性を入れるとき、手前 何文字 を置き換えるか。
    ///
    /// **XML は名前のぶんだけ。sxml は手前の `(` も食う** —— 食わないと
    /// `((label "…")` になる。`$rand` の `$` を食うのと同じ形で、
    /// **何が語かを知っているのは表記のほう。**
    AttrReplace: string -> int -> int
    /// hover の見出し（要素）。その表記の書き方で
    ElementTitle: string -> string
    /// hover の見出し（属性値）。同上
    AttrValueTitle: string -> string -> string }

/// 語彙を引いて候補を出す。**語彙は引数で受け取る** ——
/// このクラスが host を知らないので、次の表記も同じ形で書ける
type VocabularyLanguage(shape: Shape, vocabulary: unit -> Vocab) =

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
        // #PCDATA を取る要素の中では式も書ける
        if not e.Text then children
        else
          let exprLen = Scan.exprLenBefore source offset
          children @ ((vocabulary ()).Expressions |> List.map (Completion.plain exprLen))
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
               Replace = replace })
    | InAttrValue (element, attr) ->
      match find element with
      | None -> []
      | Some e ->
        match e.Attrs |> List.tryFind (fun a -> a.Name = attr) with
        | Some a -> a.Values |> List.map plain
        | None -> []

  /// hover に出す markdown を組む。
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
  /// 属性の hover にしか出ないので）
  member private _.Block(title: string, prose: string, lines: string list) =
    let head = "**`" + title + "`**\n\n" + prose
    match lines |> List.filter (fun l -> l <> "") with
    | [] -> head
    | ls -> head + "\n\n```xml\n" + String.concat "\n" ls + "\n```"

  member this.HoverAt(source: string, offset: int) : string option =
    let v = vocabulary ()
    let element name = v.Elements |> List.tryFind (fun e -> e.Name = name)
    let attribute el at =
      element el |> Option.bind (fun e -> e.Attrs |> List.tryFind (fun a -> a.Name = at))
    match shape.TokenAt source offset with
    | Nothing -> None
    | Element name ->
      element name
      |> Option.map (fun e ->
           this.Block(shape.ElementTitle e.Name, e.Spec, e.Dtd :: (e.Attrs |> List.map (fun a -> a.Dtd))))
    | Attribute (el, at) ->
      // **見出しは表記に依らない。** `fire/@label` は道しるべであって、
      // その表記で打つ字ではない
      attribute el at |> Option.map (fun a -> this.Block(el + "/@" + a.Name, a.Spec, [ a.Dtd ]))
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
                this.Block(shape.AttrValueTitle a.Name value, spec, [])))

  interface ISourceLanguage with
    member _.Kind = shape.Kind
    member _.EditorLanguageId = shape.EditorLanguageId
    member _.TriggerCharacters = shape.TriggerCharacters
    member this.Complete source offset = this.Candidates(source, offset)
    member this.Hover source offset = this.HoverAt(source, offset)
