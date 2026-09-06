/// XML の補完。**v0.3 で中身が在る唯一の言語モジュール。**
///
/// 語彙は持たない —— host が `Core/DTD.fs` から焼いたものを受け取る。
/// ここが持つのは「カーソルがどこに居るか」の判定だけ。
///
/// **精度より、止まらないこと。** 打っている途中の XML は必ず壊れているので、
/// パーサは使わずに `<` から左へ数えるだけにする。閉じていないタグも
/// 引用符の中の `>` も、そのぶんだけ数えて先へ進む。
module FsBulletML2.Playground.Languages.Xml

open FsBulletML2.Playground
open FsBulletML2.Playground.SourceLanguage

/// 字を数えるのは `XmlScan` の 1 本。**ここが持つのは語彙の引き方だけ。**
/// 名前をここへ引き直しているのは、呼ぶ側（試験と `Playground.fs`）が
/// 表記のモジュールだけを見ていれば済むようにするため
let contextAt = XmlScan.contextAt

let private nameLenBefore = XmlScan.nameLenBefore
let private exprLenBefore = XmlScan.exprLenBefore

/// 語彙を引いて候補を出す。**語彙は引数で受け取る** ——
/// このモジュールが host を知らないので、次の言語も同じ形で書ける
type XmlLanguage(vocabulary: unit -> Vocab) =

  let find name = (vocabulary ()).Elements |> List.tryFind (fun e -> e.Name = name)

  member _.Candidates(source: string, offset: int) : Completion list =
    let nameLen = nameLenBefore source offset
    let plain = Completion.plain nameLen
    match contextAt source offset with
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
          let exprLen = exprLenBefore source offset
          children @ ((vocabulary ()).Expressions |> List.map (Completion.plain exprLen))
    | InStartTag element ->
      match find element with
      | None -> []
      | Some e ->
        // **`=""` まで入れて、引用符の中へカーソルを置く。**
        // 名前だけ入れると、必ず手で 3 文字 足すことになる
        e.Attrs
        |> List.map (fun a ->
             { Label = a.Name
               Insert = a.Name + "=\"$0\""
               Snippet = true
               Replace = nameLen })
    | InAttrValue (element, attr) ->
      match find element with
      | None -> []
      | Some e ->
        match e.Attrs |> List.tryFind (fun a -> a.Name = attr) with
        | Some a -> a.Values |> List.map plain
        | None -> []

  interface ISourceLanguage with
    member _.Kind = SourceKind.Xml
    member _.MonacoLanguage = "xml"
    // `<` の直後は要素、`"` の直後は属性値。**空白は入れない** ——
    // 本文のどこで空白を打っても候補が出ることになる。
    // 属性名は 1 文字 打つか Ctrl+Space で出る
    member _.TriggerCharacters = [ "<"; "\"" ]
    member this.Complete source offset = this.Candidates(source, offset)
