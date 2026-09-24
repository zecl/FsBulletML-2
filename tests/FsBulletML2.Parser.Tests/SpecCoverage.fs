namespace FsBulletML2.Parser.Tests

open System.Reflection
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// 散文の表が、語彙と過不足なく一致するか。
/// `Spec.fs` は手書き、`Vocabulary` は reflection。中身の正しさは測れない。
[<TestFixture>]
type SpecCoverage() =

    static let elementKeys = Vocabulary.elements |> Array.map (fun e -> e.Name) |> Set.ofArray

    static let attributeKeys =
        [|
            for e in Vocabulary.elements do
                for a in e.Attrs -> sprintf "%s/@%s" e.Name a.Name
        |]
        |> Set.ofArray

    static let attrValueKeys =
        [|
            for e in Vocabulary.elements do
                for a in e.Attrs do
                    for v in a.Values -> sprintf "%s/@%s=%s" e.Name a.Name v
        |]
        |> Set.ofArray

    static let both (name: string) (fromVocab: Set<string>) (table: (string * string) list) =
        let fromTable = table |> List.map fst |> Set.ofList
        let missing = Set.difference fromVocab fromTable |> Set.toList
        let extra = Set.difference fromTable fromVocab |> Set.toList

        [
            if not missing.IsEmpty then
                yield sprintf "%s: 語彙に在って散文が無い -> %s" name (String.concat ", " missing)
            if not extra.IsEmpty then
                yield sprintf "%s: 散文が在って語彙に無い -> %s" name (String.concat ", " extra)
        ]

    /// `FsBulletML2.Dsl` の CE の名前。reflection で舐める。
    /// 表と突き合わせる相手を手で書くと、突き合わせにならない。
    static let ceKeys =
        let asm = typeof<FsBulletML2.Dsl.BulletmlBuilder>.Assembly

        let dslModule =
            asm.GetTypes() |> Array.find (fun t -> t.FullName = "FsBulletML2.Dsl")

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

    // CE の表は散文ではない。名前が `Dsl` を覆うことと、指す先が語彙に在ること。
    [<Test>]
    member _.``CE の名前も表も空でない``() =
        // どちらかが空だと、下の突き合わせは「両方 空で緑」になる
        ceKeys.Count |> should greaterThan 0
        Spec.ce.Length |> should greaterThan 0

    [<Test>]
    member _.``CE の名前が 過不足なく 一致する``() =
        // `Dsl` は配る package。 名前が増えたときに hover が黙って
        // 出なくなる（表に無い名前は `None`）のを、ここで赤にする
        let fromTable = Spec.ce |> List.map (fun (n, _, _, _) -> n) |> Set.ofList
        let missing = Set.difference ceKeys fromTable |> Set.toList
        let extra = Set.difference fromTable ceKeys |> Set.toList

        [
            if not missing.IsEmpty then
                yield sprintf "Dsl に在って表が無い -> %s" (String.concat ", " missing)
            if not extra.IsEmpty then
                yield sprintf "表が在って Dsl に無い -> %s" (String.concat ", " extra)
        ]
        |> should be Empty

    [<Test>]
    member _.``CE の指す先が 語彙に在る``() =
        // 要素名を打ち間違えても hover が黙って出なくなるだけなので、字で見る
        [
            for (name, element, attr, value) in Spec.ce do
                if not (elementKeys.Contains element) then
                    yield sprintf "%s: 要素 %s が語彙に無い" name element
                elif attr <> "" then
                    let key = sprintf "%s/@%s=%s" element attr value

                    if not (attrValueKeys.Contains key) then
                        yield sprintf "%s: %s が語彙に無い" name key
        ]
        |> should be Empty

    [<Test>]
    member _.``CE の属性と値は 揃って書く``() =
        // 片方 だけだと `Token` を組めない（`Attribute` は CE の hover に出さない）
        [
            for (name, _, attr, value) in Spec.ce do
                if (attr = "") <> (value = "") then
                    yield sprintf "%s: attr=%s value=%s" name attr value
        ]
        |> should be Empty

    [<Test>]
    member _.``CE の行が重なっていない``() =
        // 同じ名前が何行 在ってもよいが、同じ行が 2 度 在ってはいけない ——
        // hover に同じ塊が 2 つ 並ぶ
        Spec.ce
        |> List.countBy id
        |> List.filter (fun (_, n) -> n > 1)
        |> should be Empty

    // --- CE の名前が載せる label（v1.9）--------------------------------------

    [<Test>]
    member _.``label の表も空でない``() =
        // 空だと、下の点は全部「1 行 も回さずに緑」になる
        Spec.ceLabels |> should not' (be Empty)

    [<Test>]
    member _.``label の表の名前は CE の表にも在る``() =
        // 2 つ の表は同じ名前の集合の上に載る。 片方 にしか無い名前が在ると、
        // 「rename は当たるのに hover が出ない」（逆も）になる
        let ceNames = Spec.ce |> List.map (fun (n, _, _, _) -> n) |> Set.ofList

        [
            for (name, _, _, _, _) in Spec.ceLabels do
                if not (ceNames.Contains name) then
                    yield name
        ]
        |> should be Empty

    [<Test>]
    member _.``label の付く要素は 参照の対に在る``() =
        // 定義側は `action` / `bullet` / `fire`、参照側は `Ref` の付いたほう。
        // 対に無い要素を書くと、名前を数えても誰も引かない
        let refNames = References.pairs |> Array.map (fun (r, _, _) -> r) |> Set.ofArray
        let defNames = References.pairs |> Array.map (fun (_, d, _) -> d) |> Set.ofArray
        let known = Set.union refNames defNames

        [
            for (name, element, _, _, _) in Spec.ceLabels do
                if not (known.Contains element) then
                    yield name + " -> " + element
        ]
        |> should be Empty

        known |> should not' (be Empty)

    [<Test>]
    member _.``定義側の要素には 根の直下 に書く名前が 1 つ 在る``() =
        // 「定義を作る」がこれで選ぶ。 無いとその要素の定義を作れない
        let defNames = References.pairs |> Array.map (fun (_, d, _) -> d) |> Set.ofArray

        [
            for element in defNames do
                let roots =
                    Spec.ceLabels
                    |> List.filter (fun (_, e, arg, _, root) -> e = element && root && arg >= 0)

                if roots.Length <> 1 then
                    yield sprintf "%s: %d 個" element roots.Length
        ]
        |> should be Empty

    [<Test>]
    member _.``label の表の行が重なっていない``() =
        // 同じ名前を 2 度 書くと、先に見つかったほうだけが効く
        Spec.ceLabels
        |> List.countBy (fun (n, _, _, _, _) -> n)
        |> List.filter (fun (_, n) -> n > 1)
        |> should be Empty

    [<Test>]
    member _.``名前を取らないのは 固定の名前を持つものだけ``() =
        // `-1` は「引数を取らない」の印。そのときは名前が要る
        [
            for (name, _, arg, fixedName, _) in Spec.ceLabels do
                if (arg < 0) <> (fixedName <> "") then
                    yield sprintf "%s: arg=%d fixed=%s" name arg fixedName
        ]
        |> should be Empty

    [<Test>]
    member _.``散文が空でない``() =
        // 鍵だけ足して中身を書き忘れると、上の突き合わせは緑のまま通る
        let blank =
            [
                for (k, v) in Spec.elements @ Spec.attributes @ Spec.attrValues do
                    if System.String.IsNullOrWhiteSpace v then
                        yield k
            ]

        blank |> should be Empty

    [<Test>]
    member _.``鍵が重なっていない``() =
        // 2 度 書くと、あとから直す人がどちらを直したか分からなくなる。
        // 突き合わせは Set を通すので、重なりだけでは赤くならない
        let dup (table: (string * string) list) =
            table |> List.countBy fst |> List.filter (fun (_, n) -> n > 1) |> List.map fst

        let bad = dup Spec.elements @ dup Spec.attributes @ dup Spec.attrValues
        bad |> should be Empty
