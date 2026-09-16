namespace FsBulletML2.Core.Tests

open System
open System.IO
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection
open NUnit.Framework
open FsUnit
open FsBulletML2

/// 公式 DTD と `Core/DTD.fs` を突き合わせる。（v2.4.1）
///
/// `DTD.fs` は doc コメントに `<!ELEMENT ...>` / `<!ATTLIST ...>` を写し、
/// 既定値は `[<BulletmlDefault>]` で持っている。
[<TestFixture>]
type OfficialDtd() =

  static let repoRoot = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))
  static let dtdPath = Path.Combine(repoRoot, "license", "bulletml", "relax", "bulletml.dtd")
  static let oursPath = Path.Combine(repoRoot, "src", "FsBulletML2.Core", "DTD.fs")

  /// 承知の違い。 数に出すが、赤にしない
  static let known : Map<string * string, string> =
    Map.ofList
      [ ("bulletml", "type"),
        "DTD の既定は none。こちらの札は「省いたときに走る値」で vertical。"
        + "公式 Demo も none では向きを変えない（初期値のまま）ので効果は同じ。"
        + "Parser.Tests/AttributeDefaults.fs が走りで固定している" ]

  /// 空白の詰め方だけ揃える。字は変えない
  static let norm (s: string) = Regex.Replace(s.Trim(), @"\s+", " ")

  static let isDtdLine (s: string) = s.StartsWith "<!ELEMENT" || s.StartsWith "<!ATTLIST"

  static let officialLines =
    if not (File.Exists dtdPath) then [||]
    else File.ReadAllLines dtdPath |> Array.map norm |> Array.filter isDtdLine

  static let ourLines =
    if not (File.Exists oursPath) then [||]
    else
      File.ReadAllLines oursPath
      |> Array.choose (fun l ->
           let t = l.Trim()
           if t.StartsWith "///" then
             let body = norm (t.Substring 3)
             if isDtdLine body then Some body else None
           else None)

  /// 公式 ATTLIST を (要素, 属性, 取れる値, 既定) に割る
  static let attlists =
    officialLines
    |> Array.filter (fun l -> l.StartsWith "<!ATTLIST")
    |> Array.map (fun l ->
         let m = Regex.Match(l, @"^<!ATTLIST (\w+) (\w+) (.+)>$")
         let elem = m.Groups.[1].Value
         let attr = m.Groups.[2].Value
         let rest = m.Groups.[3].Value.Trim()
         let values =
           let v = Regex.Match(rest, @"^\(([^)]*)\)")
           if v.Success then v.Groups.[1].Value.Split '|' |> Array.map (fun s -> s.Trim()) else [||]
         let deflt =
           let d = Regex.Match(rest, "\"([^\"]*)\"")
           if d.Success then Some d.Groups.[1].Value else None
         elem, attr, values, deflt)

  static let core = typeof<DTD.BulletmlDefaultAttribute>.Assembly

  static let rec unwrap (t: Type) =
    if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>> then
      unwrap (t.GetGenericArguments().[0])
    else t

  /// 名前で型を探さない。 綴りを組み立てて探すと `bulletml/@type` だけ
  /// 見つからない（あちらは `ShootingDirection` で規則から外れている）——
  /// 「型が無い」と「名前が違う」が同じ顔になる。属性の欄から辿る
  static let duCasesOf (elem: string) (attr: string) =
    let attrsName = string (Char.ToUpper elem.[0]) + elem.Substring 1 + "Attrs"
    let fieldName = elem + string (Char.ToUpper attr.[0]) + attr.Substring 1
    match core.GetTypes() |> Array.tryFind (fun t -> t.Name = attrsName) with
    | None -> Error(sprintf "レコード %s が無い" attrsName)
    | Some rec_ ->
      match rec_.GetProperties() |> Array.tryFind (fun p -> p.Name = fieldName) with
      | None -> Error(sprintf "%s に欄 %s が無い" attrsName fieldName)
      | Some p ->
        let t = unwrap p.PropertyType
        if FSharpType.IsUnion t then
          let cases = FSharpType.GetUnionCases t
          let names = cases |> Array.map (fun c -> c.Name)
          let deflt =
            cases
            |> Array.tryFind (fun c ->
                 (c.GetCustomAttributes typeof<DTD.BulletmlDefaultAttribute>).Length > 0)
            |> Option.map (fun c -> c.Name)
          Ok(names, deflt)
        else Error(sprintf "%s.%s は DU ではない" attrsName fieldName)

  /// 腕の名前が公式の値に当たるか（`BulletVertical` -> `vertical`）
  static let matches (officialValue: string) (caseName: string) =
    caseName.ToLowerInvariant().EndsWith(officialValue.ToLowerInvariant())

  // --- 材料 --------------------------------------------------------------------

  [<Test>]
  member _.``公式 DTD が repo に在る``() =
    File.Exists dtdPath |> should be True

  [<Test>]
  member _.``材料が読めている``() =
    // 0 件 を緑にしない。 下の点は、行が 1 行 も取れなくても通る
    officialLines.Length |> should greaterThan 0
    ourLines.Length |> should greaterThan 0
    attlists.Length |> should greaterThan 0

  // --- 1. 写した行 --------------------------------------------------------------

  [<Test>]
  member _.``写した DTD 行 が公式と 1 文字 ずつ同じ``() =
    let official = Set.ofArray officialLines
    let ours = Set.ofArray ourLines
    // 公式に在って写していない / 写しに在って公式に無い、の両向き
    (Set.difference official ours |> Set.toList) |> should be Empty
    (Set.difference ours official |> Set.toList) |> should be Empty

  // --- 2 と 3. 値と既定 ---------------------------------------------------------

  [<Test>]
  member _.``取れる値が公式と一致する``() =
    let bad =
      attlists
      |> Array.filter (fun (_, _, values, _) -> values.Length > 0)
      |> Array.choose (fun (elem, attr, values, _) ->
           match duCasesOf elem attr with
           | Error why -> Some(elem, attr, why)
           | Ok(caseNames, _) ->
             let missing = values |> Array.filter (fun v -> not (caseNames |> Array.exists (matches v)))
             let extra = caseNames |> Array.filter (fun c -> not (values |> Array.exists (fun v -> matches v c)))
             if missing.Length = 0 && extra.Length = 0 then None
             else Some(elem, attr, sprintf "公式だけ %A / こちらだけ %A" missing extra))
    bad |> Array.toList |> should be Empty

  [<Test>]
  member _.``既定値が公式と一致する（承知の違いを除く）``() =
    let bad =
      attlists
      |> Array.filter (fun (_, _, values, _) -> values.Length > 0)
      |> Array.choose (fun (elem, attr, values, deflt) ->
           if known.ContainsKey((elem, attr)) then None
           else
             match duCasesOf elem attr with
             | Error why -> Some(elem, attr, why)
             | Ok(_, ourCase) ->
               let ourValue = ourCase |> Option.bind (fun c -> values |> Array.tryFind (fun v -> matches v c))
               if deflt = ourValue then None
               else Some(elem, attr, sprintf "公式 %A / こちら %A" deflt ourValue))
    bad |> Array.toList |> should be Empty

  [<Test>]
  member _.``承知の違いは、まだ違いのまま``() =
    // 表が古びたら落ちる。 直したのに表に残っていると、
    // 次に本当に割れたときへ気づけなくなる
    let stale =
      known
      |> Map.toList
      |> List.filter (fun ((elem, attr), _) ->
           match attlists |> Array.tryFind (fun (e, a, _, _) -> e = elem && a = attr) with
           | None -> true   // 公式から消えた
           | Some(_, _, values, deflt) ->
             match duCasesOf elem attr with
             | Error _ -> true
             | Ok(_, ourCase) ->
               let ourValue = ourCase |> Option.bind (fun c -> values |> Array.tryFind (fun v -> matches v c))
               deflt = ourValue)   // 一致してしまった = 表から外す時
    stale |> List.map fst |> should be Empty
