namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Xml.Linq
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 補完の語彙が、書き出す側と食い違っていないか。
///
/// 語彙（`Vocabulary`）は `Core/DTD.fs` の DU を reflection で読んで作る。
/// 読み方には当てずっぽうが 3 つ 入っている ——
/// 腕の名前を小文字にする / field 名から要素名を剥がす /
/// 全部の腕が同じ語で始まるならその語を落とす。
[<TestFixture>]
type VocabularyCoverage() =

  /// コーパスを読んで書き戻し、出てきた名前を集める。
  /// 読めなかったものは飛ばす（読めない形も置いてある試験用の並び）
  static let observed =
    lazy
      let root = Path.Combine(AppContext.BaseDirectory, "TestData", "xml")
      let elements = System.Collections.Generic.HashSet<string>()
      let attrs = System.Collections.Generic.HashSet<string * string>()
      let attrValues = System.Collections.Generic.HashSet<string * string * string>()
      let mutable parsed = 0
      if Directory.Exists root then
        for file in Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories) do
          let written =
            try
              match tryReadXmlString (File.ReadAllText file) with
              | Some bulletml -> Some(bulletml.ToIndentedXmlString())
              | None -> None
            with _ -> None
          match written with
          | None -> ()
          | Some xml ->
            parsed <- parsed + 1
            let doc = XDocument.Parse xml
            for e in doc.Descendants() do
              let name = e.Name.LocalName
              elements.Add name |> ignore
              for a in e.Attributes() do
                let attrName = if a.IsNamespaceDeclaration then "xmlns" else a.Name.LocalName
                attrs.Add(name, attrName) |> ignore
                if not a.IsNamespaceDeclaration then
                  attrValues.Add(name, attrName, a.Value) |> ignore
      parsed, elements, attrs, attrValues

  let byName = Vocabulary.elements |> Array.map (fun e -> e.Name, e) |> dict

  [<Test>]
  member _.``語彙が空でない``() =
    // reflection が効いていない印。ここが 0 だと下の包含は全部 意味を失う
    Vocabulary.elements.Length |> should greaterThan 0
    Vocabulary.elements |> Array.map (fun e -> e.Name) |> should contain "bulletml"

  [<Test>]
  member _.``コーパスが読めていて、名前が出てくる``() =
    let parsed, els, ats, _ = observed.Value
    parsed |> should greaterThan 0
    els.Count |> should greaterThan 0
    ats.Count |> should greaterThan 0

  [<Test>]
  member _.``書き出した要素名は全部 語彙に在る``() =
    let _, els, _, _ = observed.Value
    let missing = els |> Seq.filter (byName.ContainsKey >> not) |> Seq.sort |> Seq.toList
    missing |> should be Empty

  [<Test>]
  member _.``語彙に在ってコーパスに出ない要素は無い``() =
    // 在ってよいとは限らない。 出ないなら、コーパスが使っていないか、
    // 語彙が作りすぎているかのどちらか。どちらも見ておきたい
    let _, els, _, _ = observed.Value
    let unused =
      Vocabulary.elements
      |> Array.map (fun e -> e.Name)
      |> Array.filter (els.Contains >> not)
      |> Array.sort
    unused |> should be Empty

  [<Test>]
  member _.``書き出した属性名は全部 その要素の語彙に在る``() =
    let _, _, ats, _ = observed.Value
    let missing =
      ats
      |> Seq.filter (fun (el, at) ->
          match byName.TryGetValue el with
          | true, v -> v.Attrs |> Array.exists (fun a -> a.Name = at) |> not
          | _ -> true)
      |> Seq.map (fun (el, at) -> sprintf "%s/@%s" el at)
      |> Seq.sort
      |> Seq.toList
    missing |> should be Empty

  [<Test>]
  member _.``書き出した属性値は語彙の並びに在る``() =
    // 値の並びを持つ属性だけ見る（label / name / xmlns は自由記述）
    let _, _, _, vals = observed.Value
    let missing =
      vals
      |> Seq.choose (fun (el, at, value) ->
          match byName.TryGetValue el with
          | true, v ->
            match v.Attrs |> Array.tryFind (fun a -> a.Name = at) with
            | Some a when a.Values.Length > 0 && not (Array.contains value a.Values) ->
              Some(sprintf "%s/@%s = %s（語彙は %s）" el at value (String.concat "|" a.Values))
            | _ -> None
          | _ -> None)
      |> Seq.sort
      |> Seq.toList
    missing |> should be Empty
