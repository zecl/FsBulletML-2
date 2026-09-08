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
    usages (vocabulary ()) shape.Tags source (shape.TokenAt source offset)

  /// カーソルの下の「無い参照」を、どう直せるか。
  /// **中身は `Lookup.fixes` の 1 本**（表記を知らない）。
  ///
  /// 表記ごとに渡すのは、定義を作る場所と見出しの書き方の 2 つ。
  member _.FixesAt(source: string, offset: int) : Fix list =
    fixes (vocabulary ()) shape.Tags shape.DefinitionAt shape.ElementTitle source offset

  interface ISourceLanguage with
    member _.Kind = shape.Kind
    member _.EditorLanguageId = shape.EditorLanguageId
    member _.TriggerCharacters = shape.TriggerCharacters
    member this.Complete source offset = this.Candidates(source, offset)
    member this.Hover source offset = this.HoverAt(source, offset)
    member this.Usages source offset = this.UsagesAt(source, offset)
    member this.Fixes source offset = this.FixesAt(source, offset)
