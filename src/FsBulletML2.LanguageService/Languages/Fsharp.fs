/// F# の CE の形。**hover は出す。候補はまだ出さない。**
///
/// --- なぜ `Lookup` の `Shape` に載せないか
///
/// XML も sxml も fsb も、`Shape` の中身は「字の数え方」と「どう書くか」で、
/// **どれも要素名を打つ表記**だった。CE はそこが違う ——
///
///     要素名    action / fire / bullet / changeSpeed …
///     CE の名前  top / defAction / refBullet / doActs / speedSeq / aim …
///
/// `Shape.ContextAt` も `AttrSnippet` も `AttrReplace` も、**候補を出すため
/// だけに在る。** hover しか出さないここに載せると「呼ぶ経路の無い口」を
/// 4 つ 抱えることになる（設計書がリスクに挙げている形）。
///
/// --- hover の中身は 1 本 のまま
///
/// **`Token` から先は `Lookup.hover`** —— ほかの 3 表記 と同じ関数を通る。
/// 違うのは `Token` の作り方だけで、
///
///     ほかの 3 つ  字を数えて「要素名・属性・属性値」を直に取る
///     CE          名前を 1 つ 取り、それが何を作るかを語彙（`Vocab.Ce`）で引く
///
/// **散文を新しく書いていない。** CE で書いていても読んでいるのは BulletML で、
/// `fire` の意味は表記が変わっても変わらない。出す字は DTD 由来の
/// 既存の散文（正本は `Core/DTD.fs` と host の `Spec`）。
///
/// --- 1 つ の名前が 2 つ を指すことがある
///
///     changeDirectionAbs  向きを変える要素と、その中の向きの型
///     vertical            根の画面の向きと、`accel` の中の縦の加速度
///
/// **要素名をここに書かない**（門が見ている）—— 語彙は host が渡す。
///
/// **どちらか に決めない。** 決めるには入れ子の型を追うことになり、それは
/// 字を数える話ではなくなる。**両方 並べる**ほうが、読む人が選べる。
///
/// --- 候補（Complete）を出さない
///
/// hover は「いま在る名前」を引くだけだが、候補は「その場所に置ける名前」が
/// 要る。CE の置ける場所は**入れ子の型**で決まる（`bullet` の中と `fire` の
/// 中で置ける CustomOperation が違う）—— 字の数え方では出せないので、
/// **ここで出さないと決めている。**
///
/// --- rename と直し方は出す（v1.9）
///
/// v1.6 まで空だった。理由は「参照を数える側が探すのは要素名 + label 属性で、
/// CE はそこが DSL の名前だから」と書いてあったが、**別なのは名前の載せ方で
/// あって名前ではない** —— `defAction "x"` の `x` は `<action label="x">` の
/// `x` そのもの。
///
/// **中身はほかの 3 表記 と同じ 1 本**（`Lookup.usages` / `Lookup.fixes`）。
/// この表記が渡すのは「名前をどう数えるか」と「定義をどこに作るか」だけ。
///
/// 同梱 176 本 を CE と XML の両方 で書いて、**定義と参照の数が一致する**
/// ことを門が見ている（`Parser.Tests/FsharpUsages.fs`）。
///
/// --- エディタの色分けは付く
///
/// `EditorLanguageId` は `fsharp`。Monaco に組み込みで在るので、
/// 候補が無くても色と括弧の対応は効く。
module FsBulletML2.LanguageService.Languages.Fsharp

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

type FsharpLanguage(vocabulary: unit -> Vocab) =

  /// 語彙の表を、字を数える側へ渡す形に落とす。
  /// **`FsharpScan` は要素名も属性名も知らない**（門が見ている）
  let labelTable (v: Vocab) =
    v.CeLabels |> List.map (fun c -> c.Name, c.Element, c.LabelArg, c.Fixed) |> List.toArray

  /// 名前を載せる属性。**字を書かない** —— 語彙から引いた対が持っている。
  /// 対が 1 組 も無ければ空（語彙が引けていない印）
  let labelAttr (v: Vocab) =
    Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    |> List.tryHead
    |> Option.map (fun (_, _, attr) -> attr)

  /// 本文の中の「名前を載せている CE」。**ほかの 3 表記 の `Tags` と同じ形。**
  let tagsOf (v: Vocab) (source: string) =
    match labelAttr v with
    | None -> []
    | Some attr -> FsharpScan.tags (labelTable v) attr source

  /// カーソルの下が名前の中なら、それが何の名前か。
  ///
  /// **`wordAt` では出せない。** あちらは CE の名前を返すもので、
  /// 名前（`"center"`）は文字列の中に在る —— `wordAt` は文字列の中を見ない。
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

  /// 無い定義を**根のブロックの直下**に作る。
  ///
  /// ほかの 3 表記 と同じで、挿す先は「根が閉じるところ」——
  /// CE では**いちばん外の `{ }` の閉じ**で、`FsharpScan.blockEnd` が返す。
  ///
  /// **どの名前で書くかは表が決める。** 同じ要素を作る名前が複数 在り、
  /// `defAction` は根の直下、`nestAs` は `action` の中、`bodyAs` は
  /// `bullet` の中 —— **要素だけでは選べない**ので `Root` の欄で絞る。
  /// 根の直下 に置ける名前が無ければ作らない。
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
        // **中身を空にできない。** ほかの 3 表記 は空の定義がそのまま読めるが、
        // F# の CE は `{ }` の中に何か要る（`defAction には { } が要る`）——
        // ブラウザで当てて Apply して初めて出た。
        //
        // `()` は「中身が空」をこの DSL で書く形（根の builder の但し書きと同じ）。
        // **入れる字はこれ以上 増やさない** —— 何を書くかは人が決めること
        let body =
          pad + c.Name + " \"" + value + "\" {\n" + pad + pad + "()\n" + pad + "}\n"
        if Scan.blankBefore source at
        then Some(Scan.lineStart source at, body)
        else Some(at, "\n" + body)

  /// 見出し。**打った字と、それが作るものを並べる** ——
  /// `aim` だけ出しても、それが `<direction>` の話だと分からない
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
    /// **空のまま。** 名前は語の頭から打つので、Monaco が自分で出す
    /// （`Ctrl+Space` と 1 文字 目 で開く）—— XML の `<` や sxml の `(` に
    /// 当たる「語ではないが直後に候補が要る字」が CE には無い
    member _.TriggerCharacters = []

    /// その場所に置ける CE の名前。
    ///
    /// v1.6 まで空だった。理由は「置ける場所が入れ子の型で決まるので、
    /// 字の数え方では出せない」と書いてあった。**入れ子の型は `{ }` の対で
    /// 出せる** —— 版の頭で数えたら、`{` の手前 に名前が無いものは
    /// 同梱 3259 個 中 0 個 だった。**FCS は要らない。**
    ///
    /// 置ける先は要素ではなく**入れ物の種類**（`Vocab.CePlaces`）——
    /// `repeat` の中に置けるものは `action` の中と同じ。
    ///
    /// **知らない入れ物なら空。** 打っている途中で `{` の手前 が
    /// CE でない字のことは在る（`let x = seq {` など）
    member _.Complete source offset =
      let v = vocabulary ()
      // **同じ綴りが 2 つ の意味を持つことが在る。** `vertical` は根の
      // builder（`vertical "名" { }`）でもあり、`accel` の中の操作でもある ——
      // `{ }` の手前 に在るのだから、**開く側を採る**
      let opensOf name =
        let rows = v.CePlaces |> List.filter (fun p -> p.Name = name)
        match rows |> List.tryFind (fun p -> p.Opens <> "") with
        | Some p -> Some p.Opens
        | None -> rows |> List.tryHead |> Option.map (fun p -> p.Opens)
      // **いちばん外は「誰も開かない入れ物」。** 綴りをここに書かない ——
      // 入れ物の名前は host が決めていて、器はその字を知らないでよい
      let outerSlot () =
        let opened =
          v.CePlaces |> List.map (fun p -> p.Opens) |> List.filter (fun s -> s <> "") |> Set.ofList
        v.CePlaces
        |> List.map (fun p -> p.In)
        |> List.filter (fun s -> s <> "" && not (opened.Contains s))
        |> List.tryHead
      // いま開いている入れ物の種類。**どこにも入っていなければ「いちばん外」**
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

    /// カーソルの下の名前を引く。**知らない名前なら `None`** ——
    /// 本文には CE でない字も混ざる（`let` も `[]` も F# の一部）ので、
    /// 「名前の上に居ること」と「その名前が CE であること」は別
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
          // **語彙に無いものしか引けなければ `None`。** 空の字を返さない ——
          // 空でも枠は浮くので、出ていないことと見分けがつかなくなる
          match blocks with
          | [] -> None
          | _ -> Some(String.concat "\n\n---\n\n" blocks)

    /// 名前が本文のどこに書いてあるか。**中身は `Lookup.usages` の 1 本** ——
    /// ほかの 3 表記 と同じ関数を通る。
    ///
    /// v1.6 まで空だった。**「CE には要素名が無いから」と書いてあったが、
    /// 無いのは要素名であって名前ではない** —— `defAction "x"` の `x` は
    /// `<action label="x">` の `x` そのもので、数え方が違うだけだった。
    member _.Usages source offset =
      let v = vocabulary ()
      Lookup.usages v (tagsOf v) source (tokenAt v source offset)

    /// 無い参照の直し方。**中身は `Lookup.fixes` の 1 本。**
    ///
    /// 綴りの直しも「定義を作る」も、ほかの 3 表記 と同じ道を通る ——
    /// 表記ごとに渡すのは、定義を作る場所と見出しの書き方の 2 つ だけ。
    member _.Fixes source offset =
      let v = vocabulary ()
      // 見出しは**その表記で打つ字**。CE は要素名を打たないので、
      // 根の直下 に書く名前をそのまま出す（無ければ要素名で代える）
      let elementTitle (element: string) =
        v.CeLabels
        |> List.tryFind (fun c -> c.Element = element && c.Root && c.LabelArg >= 0)
        |> function
           | Some c -> c.Name
           | None -> element
      Lookup.fixes v (tagsOf v) (definitionAt v) elementTitle source offset

    /// 読めて・組めても走らないもの（v2.3）。
    ///
    /// **ほかの 3 表記 と同じ 1 本 を通る**（`Semantics.findings`）——
    /// 渡す `Tags` が CE の名前を数える側になるだけ。
    /// 対も `top` の綴りも語彙から引くので、ここには何も書かない
    ///
    /// **式は出ない**（v4.1）。ほかの 3 表記 は「その要素の `#PCDATA`」で
    /// 式を拾えるが、**CE は要素名で書かない** ——
    /// `absolute "180+$rand*30"` が `<direction type="absolute">` で、
    /// `repeat "10-$rank*2"` の字は `<repeat>` ではなく**中の `<times>`**。
    /// **名前から要素へは引けるが、引数の何番目 が式かは引けない**
    /// （`VocabCeLabel.LabelArg` が言うのは label の位置だけ）。
    ///
    /// **推定で光らせない。** label の字を式と読み違えると、
    /// **正しい弾幕が赤くなる** —— v2.3 が引いた線に掛かる。
    /// CE で効かないものを数え直すのは v4.8
    member _.Findings source =
      let v = vocabulary ()
      let pairs =
        Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      Semantics.findings pairs v.TopPrefix (tagsOf v source)

    /// 本文の構造（v2.4）。**ほかの 3 表記 と同じ 1 本 を通る。**
    ///
    /// ただし CE の深さは `{ }` の段で、**要素の入れ子とは別物**
    /// （同梱 176 本 中 145 本 で違う。`◯◯Ref` は `{ }` を開かず、
    /// `doActs (body { … })` のような包みが 1 段 増える）。
    /// **それが正しい** —— ここで見ているのは CE の本文であって XML ではない
    /// **結べない。** CE は要素名で書かないので `FsharpScan.tags` は
    /// 名前しか返さず、入れ子も `{ }` の段（v2.4.5 / v2.9 の 5d で測った）。
    /// **推定で光らせない** —— 隣を光らせるより、光らせないほうが読める
    member _.NodeSpans _ _ = []

    member _.Outline source =
      let v = vocabulary ()
      let detailAttr =
        Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
        |> List.tryHead
        |> Option.map (fun (_, _, attr) -> attr)
        |> Option.defaultValue ""
      Outline.build detailAttr (Scan.lineColumn source) (tagsOf v source)
