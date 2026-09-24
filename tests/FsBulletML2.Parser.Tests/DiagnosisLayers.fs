namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// Apply が落ちたとき、どこまで分かるか。
/// 位置が在るのは 1 層 目 だけで、波線を引くかどうかがそれで決まる。
[<TestFixture>]
type DiagnosisLayers() =

    let rand () = 0.5f
    let rank = 0.5f

    /// host がやるのと同じ「載せる」。木を組むところまで通す
    let build (b: Bulletml) = Runner.load rand rank b |> ignore

    let apply xml = Diagnosis.apply build xml

    let failure xml =
        match apply xml with
        | Some f -> f
        | None -> failwithf "読めてしまった: %s" xml

    // --- 位置が在る層 --------------------------------------------------------

    [<Test>]
    member _.``閉じていない``() =
        let f = failure "<bulletml><action label=\"top\"><fire>"
        f.Line |> should greaterThan 0
        f.Column |> should greaterThan 0

    [<Test>]
    member _.``タグが合っていない``() =
        // 4 行目 で閉じ札が合わない。行が 1 でないことまで見る ——
        // いつも 1 を返す壊れ方が緑で通らないように
        let f = failure "<bulletml>\n<action label=\"top\">\n<fire>\n</action>\n</bulletml>"
        f.Line |> should equal 4
        f.Column |> should greaterThan 0

    [<Test>]
    member _.``属性値が閉じていない``() =
        let f = failure "<bulletml><action label=\"top></action></bulletml>"
        f.Line |> should greaterThan 0

    [<Test>]
    member _.``空文字でも 0 を返さない``() =
        // XmlException は 行 0 桁 0 を返す。そのまま渡すと範囲が壊れるし、
        // 位置なし（Line = 0）と見分けが付かなくなる
        let f = failure ""
        f.Line |> should equal 1
        f.Column |> should equal 1

    // --- 位置が無い層 --------------------------------------------------------

    [<Test>]
    member _.``XML だが BulletML でない``() =
        let f = failure "<foo><bar/></foo>"
        f.Line |> should equal 0
        f.Message |> should not' (be EmptyString)

    [<Test>]
    member _.``根が bulletml でない``() =
        (failure "<action label=\"top\"><wait>1</wait></action>").Line |> should equal 0

    [<Test>]
    member _.``無い label を指す``() =
        // 木は読めるが組めない。label 名は文面に在るが、位置は無い ——
        // 本文を探して当てにいかない（同じ label が 2 つ 在ると嘘を指す）
        let f =
            failure "<bulletml><action label=\"top\"><actionRef label=\"nope\"/></action></bulletml>"

        f.Line |> should equal 0
        f.Message |> should not' (be EmptyString)

    [<Test>]
    member _.``読めない式``() =
        (failure "<bulletml><action label=\"top\"><wait>1+</wait></action></bulletml>").Line
        |> should equal 0

    // --- 落ちない側 ----------------------------------------------------------

    [<Test>]
    member _.``正しいものは通る``() =
        apply "<bulletml><action label=\"top\"><wait>1</wait></action></bulletml>"
        |> should equal None

    [<Test>]
    member _.``輪 と 重複 label と top 無し は、ここでは落ちない``() =
        // 落ちるのは走行のほう。 Apply で何も出ないのが正しい。
        // ここが赤くなったら、落ちる場所が動いたということ
        apply "<bulletml><action label=\"top\"><actionRef label=\"top\"/></action></bulletml>"
        |> should equal None

        apply
            "<bulletml><action label=\"top\"><wait>1</wait></action><action label=\"top\"><wait>1</wait></action></bulletml>"
        |> should equal None

        apply "<bulletml><action label=\"x\"><wait>1</wait></action></bulletml>"
        |> should equal None

    // --- 層が潰れていないこと ------------------------------------------------

    [<Test>]
    member _.``位置が在る側と無い側が両方 在る``() =
        // 全部 が「位置なし」に落ちる壊れ方（例外の分け方を消す）と、
        // 全部 に位置が付く壊れ方（0 を素通しする）を、まとめて塞ぐ
        let withPos =
            [
                "<bulletml><action label=\"top\"><fire>"
                "<bulletml>\n<action label=\"top\">\n<fire>\n</action>\n</bulletml>"
                ""
            ]
            |> List.map (failure >> fun f -> f.Line)

        let without =
            [
                "<foo/>"
                "<bulletml><action label=\"top\"><actionRef label=\"nope\"/></action></bulletml>"
            ]
            |> List.map (failure >> fun f -> f.Line)

        withPos |> List.forall (fun l -> l > 0) |> should be True
        without |> List.forall (fun l -> l = 0) |> should be True
