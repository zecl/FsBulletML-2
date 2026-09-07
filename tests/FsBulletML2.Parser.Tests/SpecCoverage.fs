namespace FsBulletML2.Parser.Tests

open System.Reflection
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// **散文の表が、語彙と過不足なく一致するか。**
///
/// `Spec.fs` は手で書いた表。`Vocabulary` は `Core/DTD.fs` から
/// reflection で出る。**片方 だけが動くのを止める** ——
///
///   足りない   Core に要素が増えて書き忘れ    -> 赤
///   余る       Core から要素が消えて表が残る  -> 赤
///
/// **中身の正しさは測れない。** ここが見るのは「在ること」だけ。
/// 散文が仕様と合っているかは `docs/local/bulletml-spec-draft.md` の
/// レビューだけが頼りで、そのことは認めたうえで置いている。
///
/// --- 属性値は、同じ綴りでも別々に数える
///
/// `absolute` は direction / speed / horizontal / vertical の 4 つ に在り、
/// **計算が違う**。鍵を `要素/@属性=値` にしてあるので、1 つ 書けば
/// 4 つ 埋まったことにはならない。
///
/// --- 0 件 を緑にしない
///
/// 突き合わせだけだと両方 空でも緑。語彙が空でないこと・表が空でないことを
/// 別の点に置く。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   要素の鍵を 1 つ 改名     要素が過不足なく一致する
///   属性の鍵を 1 つ 改名     属性が過不足なく一致する
///   属性値の鍵を 1 つ 改名   属性値が過不足なく一致する
///   要素を 1 つ 足す         要素が過不足なく一致する
///   散文を空文字にする       散文が空でない
///   同じ鍵を 2 度 書く       鍵が重なっていない
///
/// **消すのでなく改名する。** 消す変異は組の書き方ごと壊れて、
/// 較正ではなくコンパイルエラーになる。改名なら「足りない」と「余る」が
/// 同時に立つので、両向きを 1 回 で当てられる。
///
/// --- 借りているファイル
///
/// `Spec.fs` も `Vocabulary.fs` も `Link` で借りている。**そのファイルだけを
/// 触った PR ではこの試験が選ばれない**（`VocabularyCoverage` と同じ穴）。
/// Core を触ると全部 走るので、守りたい向きは塞がっている。
[<TestFixture>]
type SpecCoverage() =

  static let elementKeys =
    Vocabulary.elements |> Array.map (fun e -> e.Name) |> Set.ofArray

  static let attributeKeys =
    [| for e in Vocabulary.elements do
         for a in e.Attrs -> sprintf "%s/@%s" e.Name a.Name |]
    |> Set.ofArray

  static let attrValueKeys =
    [| for e in Vocabulary.elements do
         for a in e.Attrs do
           for v in a.Values -> sprintf "%s/@%s=%s" e.Name a.Name v |]
    |> Set.ofArray

  static let both (name: string) (fromVocab: Set<string>) (table: (string * string) list) =
    let fromTable = table |> List.map fst |> Set.ofList
    let missing = Set.difference fromVocab fromTable |> Set.toList
    let extra = Set.difference fromTable fromVocab |> Set.toList
    [ if not missing.IsEmpty then
        yield sprintf "%s: 語彙に在って散文が無い -> %s" name (String.concat ", " missing)
      if not extra.IsEmpty then
        yield sprintf "%s: 散文が在って語彙に無い -> %s" name (String.concat ", " extra) ]

  /// `FsBulletML2.Dsl` の CE の名前。**reflection で舐める** ——
  /// 表と突き合わせる相手を手で書くと、突き合わせにならない。
  ///
  /// 2 通り 在る —— module の公開 `let`（`fire` / `defAction` …）と、
  /// builder の `[<CustomOperation>]`（`aim` / `speedSeq` …）。
  /// **どちらも CE の中で打つ字**なので、hover の当てる先はこの和集合
  static let ceKeys =
    let asm = typeof<FsBulletML2.Dsl.BulletmlBuilder>.Assembly
    let dslModule = asm.GetTypes() |> Array.find (fun t -> t.FullName = "FsBulletML2.Dsl")
    let lets =
      dslModule.GetMembers(BindingFlags.Public ||| BindingFlags.Static)
      |> Array.choose (fun m ->
           match m with
           | :? MethodInfo as mi when not mi.IsSpecialName -> Some mi.Name
           | :? PropertyInfo as pi -> Some pi.Name
           | _ -> None)
    let ops =
      asm.GetTypes()
      |> Array.collect (fun t -> t.GetMethods(BindingFlags.Public ||| BindingFlags.Instance))
      |> Array.choose (fun m ->
           m.GetCustomAttributes(typeof<CustomOperationAttribute>, false)
           |> Array.tryHead
           |> Option.map (fun a -> (a :?> CustomOperationAttribute).Name))
    Array.append lets ops |> Set.ofArray

  [<Test>]
  member _.``語彙も散文も空でない``() =
    // どちらかが空だと、下の突き合わせは「両方 空で緑」になる
    elementKeys.Count |> should greaterThan 0
    attributeKeys.Count |> should greaterThan 0
    attrValueKeys.Count |> should greaterThan 0
    Spec.elements.Length |> should greaterThan 0
    Spec.attributes.Length |> should greaterThan 0
    Spec.attrValues.Length |> should greaterThan 0

  [<Test>]
  member _.``要素が 過不足なく 一致する``() =
    both "要素" elementKeys Spec.elements |> should be Empty

  [<Test>]
  member _.``属性が 過不足なく 一致する``() =
    both "属性" attributeKeys Spec.attributes |> should be Empty

  [<Test>]
  member _.``属性値が 過不足なく 一致する``() =
    both "属性値" attrValueKeys Spec.attrValues |> should be Empty

  // --- F# の CE の表 --------------------------------------------------------
  //
  // ここは**散文の表ではなく対応の表**（CE の名前 -> 要素・属性値）。
  // 散文は上の 3 つ から引くので、当てるのは 2 つ ——
  // 名前が `Dsl` を過不足なく覆うことと、指す先が語彙に在ること。

  [<Test>]
  member _.``CE の名前も表も空でない``() =
    // どちらかが空だと、下の突き合わせは「両方 空で緑」になる
    ceKeys.Count |> should greaterThan 0
    Spec.ce.Length |> should greaterThan 0

  [<Test>]
  member _.``CE の名前が 過不足なく 一致する``() =
    // **`Dsl` は配る package。** 名前が増えたときに hover が黙って
    // 出なくなる（表に無い名前は `None`）のを、ここで赤にする
    let fromTable = Spec.ce |> List.map (fun (n, _, _, _) -> n) |> Set.ofList
    let missing = Set.difference ceKeys fromTable |> Set.toList
    let extra = Set.difference fromTable ceKeys |> Set.toList
    [ if not missing.IsEmpty then
        yield sprintf "Dsl に在って表が無い -> %s" (String.concat ", " missing)
      if not extra.IsEmpty then
        yield sprintf "表が在って Dsl に無い -> %s" (String.concat ", " extra) ]
    |> should be Empty

  [<Test>]
  member _.``CE の指す先が 語彙に在る``() =
    // 要素名を打ち間違えても hover が黙って出なくなるだけなので、字で見る
    [ for (name, element, attr, value) in Spec.ce do
        if not (elementKeys.Contains element) then
          yield sprintf "%s: 要素 %s が語彙に無い" name element
        elif attr <> "" then
          let key = sprintf "%s/@%s=%s" element attr value
          if not (attrValueKeys.Contains key) then
            yield sprintf "%s: %s が語彙に無い" name key ]
    |> should be Empty

  [<Test>]
  member _.``CE の属性と値は 揃って書く``() =
    // 片方 だけだと `Token` を組めない（`Attribute` は CE の hover に出さない）
    [ for (name, _, attr, value) in Spec.ce do
        if (attr = "") <> (value = "") then
          yield sprintf "%s: attr=%s value=%s" name attr value ]
    |> should be Empty

  [<Test>]
  member _.``CE の行が重なっていない``() =
    // 同じ名前が何行 在ってもよいが、**同じ行が 2 度 在ってはいけない** ——
    // hover に同じ塊が 2 つ 並ぶ
    Spec.ce |> List.countBy id |> List.filter (fun (_, n) -> n > 1) |> should be Empty

  [<Test>]
  member _.``散文が空でない``() =
    // 鍵だけ足して中身を書き忘れると、上の突き合わせは緑のまま通る
    let blank =
      [ for (k, v) in Spec.elements @ Spec.attributes @ Spec.attrValues do
          if System.String.IsNullOrWhiteSpace v then yield k ]
    blank |> should be Empty

  [<Test>]
  member _.``鍵が重なっていない``() =
    // 2 度 書くと、あとから直す人がどちらを直したか分からなくなる。
    // 突き合わせは Set を通すので、重なりだけでは赤くならない
    let dup (table: (string * string) list) =
      table
      |> List.countBy fst
      |> List.filter (fun (_, n) -> n > 1)
      |> List.map fst
    let bad = dup Spec.elements @ dup Spec.attributes @ dup Spec.attrValues
    bad |> should be Empty
