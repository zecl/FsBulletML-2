namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 定義の行の上に参照の数（v4.3）。
/// 参照 0 をそのまま出すと、根から走る定義まで「どこからも参照されていない」になる。
[<TestFixture>]
type Lenses() =

    static let vocab = VocabForTests.vocab

    static let pairs =
        Refs.pairs (
            vocab.Elements
            |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name))
        )

    static let lang: SourceLanguage.ISourceLanguage =
        Languages.Xml.XmlLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

    static let corpus =
        Bullets.Dsl.All.bullets
        |> List.map (fun i -> i.Name, BulletmlWriter.toIndentedXml 4 i.Bulletml)

    static let src =
        """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <actionRef label="shot"/>
    <wait>10</wait>
    <actionRef label="shot"/>
  </action>
  <action label="shot">
    <fire><direction type="aim">0</direction><speed>2</speed><bullet/></fire>
  </action>
  <action label="tsukawanai">
    <wait>1</wait>
  </action>
</bulletml>"""

    // CE でも出す。打たないのは字で、`TagName` は語彙が引いた要素名。
    static let ce: SourceLanguage.ISourceLanguage =
        Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

    static let ceSrc =
        """let x =
  untypedXmlns "u" "n" {
    top {
      actionRef "loop" []
      actionRef "loop" []
    }
    defAction "loop" {
      wait "1"
    }
    defAction "tsukawanai" {
      wait "1"
    }
  }
"""

    [<Test>]
    member _.``対が引けている``() =
        // 0 件 だと、下の点が全部「当てる先が無くて緑」になる
        pairs |> should not' (be Empty)

    [<Test>]
    member _.``定義の数だけ出る``() =
        lang.Lenses src |> List.length |> should equal 3

    [<Test>]
    member _.``参照の数を数える``() =
        match lang.Lenses src |> List.tryFind (fun l -> l.Name = "shot") with
        | Some l ->
            l.Title |> should equal "2 か所 から参照"
            l.Clickable |> should equal true
        | None -> failwith "shot が無い"

    [<Test>]
    member _.``根から走る定義には数を出さない``() =
        match lang.Lenses src |> List.tryFind (fun l -> l.Name = "top") with
        | Some l ->
            l.Title |> should equal "根から走る"
            // 開く先が無いので押せない
            l.Clickable |> should equal false
        | None -> failwith "top が無い"

    [<Test>]
    member _.``呼ばれない定義はそう言う``() =
        match lang.Lenses src |> List.tryFind (fun l -> l.Name = "tsukawanai") with
        | Some l ->
            l.Title |> should equal "どこからも参照されていない"
            l.Clickable |> should equal false
        | None -> failwith "tsukawanai が無い"

    [<Test>]
    member _.``本文の順で返る``() =
        lang.Lenses src
        |> List.map (fun l -> l.Line)
        |> List.pairwise
        |> List.forall (fun (a, b) -> a <= b)
        |> should equal true

    /// v2.3 の青い波線と同じものを指す。 材料は同じ（`Refs`）だが
    /// 数え方が別（`pairs` と `uses`）—— 別々 に数えて同じ答えが出ることを見る
    [<Test>]
    member _.``呼ばれない定義は、青い波線と同じ集合``() =
        let byLens =
            [
                for (name, text) in corpus do
                    for l in lang.Lenses text do
                        if l.Title = "どこからも参照されていない" then
                            yield name, l.Name
            ]
            |> List.sort

        let byFindings =
            [
                for (name, text) in corpus do
                    for f in lang.Findings text do
                        if f.Kind = Semantics.UnusedDefinition then
                            yield name, f.Name
            ]
            |> List.sort
        // 当てる先が在ることを、門が自分で数える
        byLens |> should not' (be Empty)
        byLens |> should equal byFindings

    [<Test>]
    member _.``同梱では、参照が 0 の定義のほとんどが根から走る``() =
        let mutable entry = 0
        let mutable dead = 0
        let mutable used = 0

        for (_, text) in corpus do
            for l in lang.Lenses text do
                if l.Title = "根から走る" then entry <- entry + 1
                elif l.Title = "どこからも参照されていない" then dead <- dead + 1
                else used <- used + 1

        entry |> should be (greaterThan 100)
        dead |> should be (lessThan 20)
        used |> should be (greaterThan 100)
        // 根から走るほうが、本当に呼ばれていないものより桁で多い
        entry |> should be (greaterThan (dead * 10))

    [<Test>]
    member _.``CE でも定義の行に出る``() =
        ce.Lenses ceSrc |> List.length |> should equal 3

    [<Test>]
    member _.``CE でも参照の数を数える``() =
        match ce.Lenses ceSrc |> List.tryFind (fun l -> l.Name = "loop") with
        | Some l ->
            l.Title |> should equal "2 か所 から参照"
            l.Clickable |> should equal true
        | None -> failwith "loop が無い"

    [<Test>]
    member _.``CE でも根から走る定義と、呼ばれない定義を分ける``() =
        let titles = ce.Lenses ceSrc |> List.map (fun l -> l.Name, l.Title) |> List.sort
        titles |> should contain ("top", "根から走る")
        titles |> should contain ("tsukawanai", "どこからも参照されていない")

    /// 同梱を CE と XML の両方で書いて、名前と見出しの並びが一致すること。
    /// 位置は表記ごとに違うが、数も文面も違わない。
    [<Test>]
    member _.``同梱 全部 で、CE と XML の Lens が一致する``() =
        let write (kind: SourceKind) (b: Bulletml) =
            match SourceWriter.tryFind kind with
            | None -> Result.Error "書く口が無い"
            | Some w -> w.Write b

        let mutable compared = 0
        let bad = ResizeArray<string>()

        for info in Bullets.Dsl.All.bullets do
            match write SourceKind.FSharpDsl info.Bulletml, write SourceKind.Xml info.Bulletml with
            | Result.Ok c, Result.Ok x ->
                compared <- compared + 1

                let profile (l: SourceLanguage.ISourceLanguage) (src: string) =
                    l.Lenses src |> List.map (fun n -> n.Name, n.Title) |> List.sort

                if profile ce c <> profile lang x then
                    bad.Add info.Name
            | _ -> ()
        // 当てる先が在ることを、門が自分で数える
        compared |> should be (greaterThan 100)
        bad |> List.ofSeq |> should be Empty
