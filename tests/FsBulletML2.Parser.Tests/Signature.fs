namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 参照が渡す引数の形（v4.5）。
/// 食い違いは正しい弾幕にも在る。残す理由は `Refs.maxParamIn` の但し書き。
[<TestFixture>]
type Signature() =

  static let vocab = VocabForTests.vocab

  static let lang : SourceLanguage.ISourceLanguage =
    Languages.Xml.XmlLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

  static let pairs =
    Refs.pairs (vocab.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))

  static let corpus =
    Bullets.Dsl.All.bullets
    |> List.map (fun i -> i.Name, BulletmlWriter.toIndentedXml 4 i.Bulletml)

  static let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <actionRef label="shot">
      <param>10</param>
      <param>20</param>
    </actionRef>
  </action>
  <action label="shot">
    <fire><direction type="absolute">$1</direction><speed>$2</speed><bullet/></fire>
    <wait>$rank</wait>
  </action>
  <action label="nopar">
    <wait>1</wait>
  </action>
</bulletml>"""

  /// その字が本文のどこに在るか（0 起点）
  static let at (needle: string) = src.IndexOf(needle: string)

  // --- 数える ---------------------------------------------------------------

  [<Test>]
  member _.``対が引けている``() =
    pairs |> should not' (be Empty)

  [<Test>]
  member _.``$n の最大を取る``() =
    let ar = Refs.arity pairs src (XmlScan.tags src)
    (ar |> List.find (fun a -> a.Name = "shot")).Takes |> should equal 2
    (ar |> List.find (fun a -> a.Name = "nopar")).Takes |> should equal 0
    // `top` の中身には `$n` が無い（参照の中の `param` は 10 と 20）
    (ar |> List.find (fun a -> a.Name = "top")).Takes |> should equal 0

  [<Test>]
  member _.``$rank と $rand は数えない``() =
    // `shot` の本文には `$rank` が在るが、取る数は 2 のまま
    let ar = Refs.arity pairs src (XmlScan.tags src)
    (ar |> List.find (fun a -> a.Name = "shot")).Takes |> should equal 2
    let only = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="a"><wait>$rank+$rand</wait></action>
</bulletml>"""
    (Refs.arity pairs only (XmlScan.tags only) |> List.head).Takes |> should equal 0

  /// 最大であって、最後ではない。大きいほうが先に出てくる本文で当てる。
  /// 「最後を取る」に変えても、`$1` から `$2` の並びでは同じ答えになる。
  [<Test>]
  member _.``大きいほうが先に出てきても最大を取る``() =
    let back = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="a"><wait>$3</wait><wait>$1</wait></action>
</bulletml>"""
    (Refs.arity pairs back (XmlScan.tags back) |> List.head).Takes |> should equal 3

  [<Test>]
  member _.``二桁 の $n も読む``() =
    let big = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="a"><wait>$12</wait></action>
</bulletml>"""
    (Refs.arity pairs big (XmlScan.tags big) |> List.head).Takes |> should equal 12

  // --- 形を出す --------------------------------------------------------------

  [<Test>]
  member _.``参照の中なら形が出る``() =
    match lang.Signature src (at "<param>10") with
    | Some s ->
        s.Label |> should equal "shot($1, $2)"
        s.Params |> List.length |> should equal 2
    | None -> failwith "出ない"

  [<Test>]
  member _.``参照の外なら出ない``() =
    // 空を返さない。 空でも枠が浮く
    lang.Signature src (at "<action label=\"nopar\"") |> should equal None

  [<Test>]
  member _.``いま何番目 かを言う``() =
    match lang.Signature src (at "10</param>") with
    | Some s -> s.Active |> should equal 0
    | None -> failwith "出ない"
    match lang.Signature src (at "20</param>") with
    | Some s -> s.Active |> should equal 1
    | None -> failwith "出ない"

  [<Test>]
  member _.``数が合っていればそう言う``() =
    match lang.Signature src (at "<param>10") with
    | Some s -> s.Detail |> should equal "2 つ 取って、2 つ 渡している"
    | None -> failwith "出ない"

  [<Test>]
  member _.``足りなければそう言う``() =
    // 改行ごと消さない。 この file の改行は CRLF なので `\n` では当たらず、
    // 「消したつもりで消えていない」形になる（1 度 踏んだ）
    let few = src.Replace("<param>20</param>", "")
    match lang.Signature few (few.IndexOf "<param>10") with
    | Some s ->
        s.Detail |> should equal "2 つ 使っているが、1 つ しか渡していない"
        // 見出しは定義の側に合わせる —— 足りないことが形で見える
        s.Label |> should equal "shot($1, $2)"
    | None -> failwith "出ない"

  // --- 同梱 ------------------------------------------------------------------

  [<Test>]
  member _.``同梱で引数を使う定義が在る``() =
    // 0 件 だと、上の点が全部「当てる先が無くて緑」になる
    corpus
    |> List.sumBy (fun (_, t) ->
         Refs.arity pairs t (XmlScan.tags t) |> List.filter (fun a -> a.Takes > 0) |> List.length)
    |> should be (greaterThan 100)

  [<Test>]
  member _.``同梱に食い違いが在る``() =
    // 在ることを門が数える。 0 件 になったら「波線にしない」の根拠が消える
    // —— そのときは判断を見直すべきなので、ここが赤くなるのが正しい
    let mutable bad = 0
    for (_, text) in corpus do
      let tags = XmlScan.tags text
      let ar = Refs.arity pairs text tags
      let takesOf = System.Collections.Generic.Dictionary<string, int>()
      for a in ar do
        match takesOf.TryGetValue a.Name with
        | true, v -> if a.Takes > v then takesOf.[a.Name] <- a.Takes
        | _ -> takesOf.[a.Name] <- a.Takes
      let all = tags |> List.toArray
      for i in 0 .. all.Length - 1 do
        let t = all.[i]
        if not t.Closing && pairs |> List.exists (fun (r, _, _) -> r = t.TagName) then
          let mutable k = i + 1
          let mutable n = 0
          while k < all.Length && all.[k].Depth > t.Depth do
            if not all.[k].Closing && all.[k].TagName = "param" && all.[k].Depth = t.Depth + 1 then
              n <- n + 1
            k <- k + 1
          match t.Attrs |> List.tryFind (fun a -> a.AttrName = "label") with
          | Some a ->
            match takesOf.TryGetValue a.Value with
            | true, takes when takes <> n -> bad <- bad + 1
            | _ -> ()
          | None -> ()
    bad |> should be (greaterThan 0)
