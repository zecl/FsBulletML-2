/// F# の CE の形。`Lookup` の `Shape` に載せない。
/// `Token` から先は `Lookup.hover`。要素名をここに書かない。
module FsBulletML2.LanguageService.Languages.Fsharp

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

type FsharpLanguage(vocabulary: unit -> Vocab) =

  /// 語彙の表を、字を数える側へ渡す形に落とす。
  /// `FsharpScan` は要素名も属性名も知らない（門が見ている）
  let labelTable (v: Vocab) =
    v.CeLabels |> List.map (fun c -> c.Name, c.Element, c.LabelArg, c.Fixed) |> List.toArray

  /// 名前を載せる属性。字を書かない —— 語彙から引いた対が持っている。
  /// 対が 1 組 も無ければ空（語彙が引けていない印）
  let labelAttr (v: Vocab) =
    Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    |> List.tryHead
    |> Option.map (fun (_, _, attr) -> attr)

  // 同じ本文を 1 打鍵 で何度も走査しない。語彙も鍵に入れる。
  // 同じ物かで見る。中身で比べると表を丸ごと辿る。
  let mutable tagVocab: Vocab = Unchecked.defaultof<Vocab>
  let mutable tagSource: string = null
  let mutable tagCache: TagHit list = []

  /// 本文の中の「名前を載せている CE」。ほかの 3 表記 の `Tags` と同じ形。
  let tagsOf (v: Vocab) (source: string) =
    if not (LanguagePrimitives.PhysicalEquality tagVocab v) || tagSource <> source then
      tagVocab <- v
      tagSource <- source
      tagCache <-
        match labelAttr v with
        | None -> []
        | Some attr -> FsharpScan.tags (labelTable v) attr source
    tagCache

  /// カーソルの下が名前の中なら、それが何の名前か。
  /// `wordAt` は使うな。文字列の中を見ない。
  let tokenAt (v: Vocab) (source: string) (offset: int) =
    match labelAttr v with
    | None -> Nothing
    | Some attr ->
      tagsOf v source
      |> List.collect (fun t -> t.Attrs |> List.map (fun a -> t.TagName, a))
      |> List.tryFind (fun (_, a) -> offset >= a.ValueStart && offset <= a.ValueStop)
      |> function
         | Some (element, a) -> AttrValue(element, attr, a.Value)
         | None -> Nothing

  /// 無い定義を根のブロックの直下に作る。挿す先はいちばん外の `{ }` の閉じ。
  /// どの名前で書くかは表が決める。要素だけでは選べない。
  let definitionAt (v: Vocab) (source: string) (defName: string) (_attr: string) (value: string) =
    match
      v.CeLabels
      |> List.tryFind (fun c -> c.Element = defName && c.Root && c.LabelArg >= 0)
      with
    | None -> None
    | Some c ->
      match FsharpScan.blockEnd source with
      | None -> None
      | Some at ->
        let indent = FsharpScan.rootChildIndent source
        let pad = System.String(' ', indent)
        // 中身を空にできない。F# の CE は `{ }` の中に何か要る。入れる字は増やすな。
        let body =
          pad + c.Name + " \"" + value + "\" {\n" + pad + pad + "()\n" + pad + "}\n"
        if Scan.blankBefore source at
        then Some(Scan.lineStart source at, body)
        else Some(at, "\n" + body)

  /// 見出し。打った字と、それが作るものを並べる。
  static let title (name: string) (token: Token) =
    match token with
    | Element element -> name + " → <" + element + ">"
    | Attribute (element, attr) -> name + " → <" + element + " " + attr + "=…>"
    | AttrValue (element, attr, value) ->
      name + " → <" + element + " " + attr + "=\"" + value + "\">"
    | Nothing -> name

  interface ISourceLanguage with
    member _.Kind = SourceKind.FSharpDsl
    member _.EditorLanguageId = "fsharp"
    /// 空のまま。名前は語の頭から打つので Monaco が自分で出す。
    /// XML の `<` に当たる字が CE には無い。
    member _.TriggerCharacters = []

    /// その場所に置ける CE の名前。置ける先は要素ではなく入れ物。
    /// 知らない入れ物なら空。`{` の手前が CE でないことは在る。
    member _.Complete source offset =
      let v = vocabulary ()
      // 同じ綴りが 2 つ の意味を持つことがある。`{ }` の手前なので開く側を採る。
      let opensOf name =
        let rows = v.CePlaces |> List.filter (fun p -> p.Name = name)
        match rows |> List.tryFind (fun p -> p.Opens <> "") with
        | Some p -> Some p.Opens
        | None -> rows |> List.tryHead |> Option.map (fun p -> p.Opens)
      // いちばん外は誰も開かない入れ物。綴りをここに書かない。
      let outerSlot () =
        let opened =
          v.CePlaces |> List.map (fun p -> p.Opens) |> List.filter (fun s -> s <> "") |> Set.ofList
        v.CePlaces
        |> List.map (fun p -> p.In)
        |> List.filter (fun s -> s <> "" && not (opened.Contains s))
        |> List.tryHead
      // いま開いている入れ物の種類。どこにも入っていなければ「いちばん外」
      let slot =
        match FsharpScan.blockAt source offset with
        | None -> outerSlot ()
        | Some name -> opensOf name
      match slot with
      | None -> []
      | Some "" -> []
      | Some slot ->
        let replace = Scan.nameLenBefore source offset
        v.CePlaces
        |> List.filter (fun p -> p.In = slot)
        |> List.map (fun p -> p.Name)
        |> List.distinct
        |> List.map (Completion.plain replace)

    /// カーソルの下の名前を引く。知らない名前なら `None`。
    /// 名前の上に居ることと、その名前が CE であることは別。
    member _.Hover source offset =
      match FsharpScan.wordAt source offset with
      | None -> None
      | Some name ->
        let v = vocabulary ()
        match v.Ce |> List.filter (fun c -> c.Name = name) with
        | [] -> None
        | hits ->
          let blocks =
            hits
            |> List.choose (fun c ->
                 let token =
                   if c.Attr = "" then Element c.Element
                   else AttrValue(c.Element, c.Attr, c.Value)
                 Lookup.hover v (title name) token)
          // 語彙に無いものしか引けなければ `None`。空の字を返すな。空でも枠は浮く。
          match blocks with
          | [] -> None
          | _ -> Some(String.concat "\n\n---\n\n" blocks)

    /// 名前が本文のどこに書いてあるか。中身は `Lookup.usages` の 1 本。
    member _.Usages source offset =
      let v = vocabulary ()
      Lookup.usages v (tagsOf v) source (tokenAt v source offset)

    /// 無い参照の直し方。中身は `Lookup.fixes` の 1 本。
    member _.Fixes source offset =
      let v = vocabulary ()
      // 見出しはその表記で打つ字。CE は要素名を打たない。
      let elementTitle (element: string) =
        v.CeLabels
        |> List.tryFind (fun c -> c.Element = element && c.Root && c.LabelArg >= 0)
        |> function
           | Some c -> c.Name
           | None -> element
      Lookup.fixes v (tagsOf v) (definitionAt v) elementTitle source offset

    /// 読めて・組めても走らないもの（v2.3）。`Semantics.findings` を通る。
    /// 式は出さない。引数の何番目 が式かは引けない。推定で光らせるな。
    member _.Findings source =
      let v = vocabulary ()
      let pairs =
        Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      Semantics.findings pairs v.TopPrefix (tagsOf v source)

    /// 結べない。CE は要素名で書かず、入れ子も `{ }` の段。
    /// 推定で光らせるな。隣を光らせるより、光らせないほうが読める。
    member _.NodeSpans _ _ = []

    /// 定義の行の上に出す字（v4.3）。中身は `Lookup.lenses` の 1 本。
    member _.Lenses source =
      let v = vocabulary ()
      Lookup.lenses v (tagsOf v) source

    /// 値を横に出す先（v4.4）。CE では出さない。式の取り出しが無い。
    member _.Hints _ = []

    /// 参照が渡す引数の形（v4.5）。CE では出さない。
    /// 渡している数の札が出ない。0 と出すと正しい弾幕が赤くなる。
    member _.Signature _ _ = None

    member _.Outline source =
      let v = vocabulary ()
      let detailAttr =
        Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
        |> List.tryHead
        |> Option.map (fun (_, _, attr) -> attr)
        |> Option.defaultValue ""
      // 表を 1 度 だけ作る（v4.9。ほかの 3 表記 と同じ）
      Outline.build detailAttr (Scan.lineColumnLookup source) (tagsOf v source)
