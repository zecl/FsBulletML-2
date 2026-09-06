namespace FsBulletML2.Parser.Tests

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
