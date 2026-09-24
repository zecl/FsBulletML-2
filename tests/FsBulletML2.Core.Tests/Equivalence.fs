namespace FsBulletML2.Core.Tests

open System.IO
open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsUnit

/// 227 本の実物を突き合わせる橋。
type BridgeReport =
    {
        Ok: int
        Ng: int
        Skipped: int
        Total: int
        Text: string
    }

[<TestFixture>]
type Equivalence() =

    /// 走らせ方を 2 つ受け取り、227 本ぜんぶを突き合わせた報告を返す。
    static member RunBoth (runA: string -> string) (runB: string -> string) : BridgeReport =
        Equivalence.RunBothWith (fun _path xml -> runA xml) runB

    /// 基準側（A）にパスも渡す形。
    static member RunBothWith (runA: string -> string -> string) (runB: string -> string) : BridgeReport =
        let samples = CorpusData.uniqueSamples ()
        let mutable ok, ng, skipped = 0, 0, 0
        let diffs = StringBuilder()
        // 比べられず（両方が同じ例外で落ちた）の中身。数だけでは「何が」
        // 比べられなかったのか報告から読み戻せない
        let skippedNames = ResizeArray<string>()

        let tryRun (f: string -> string) xml =
            try
                Ok(f xml)
            with e ->
                Error(Equivalence.RenderException e)

        for path in samples do
            let name = CorpusData.relative path
            let xml = File.ReadAllText path

            match tryRun (runA path) xml, tryRun runB xml with
            | Ok a, Ok b ->
                match Divergence.firstDivergence a b with
                | None -> ok <- ok + 1
                | Some msg ->
                    ng <- ng + 1
                    diffs.AppendLine(sprintf "%s\n%s" name msg) |> ignore
            | Error ea, Error eb when ea = eb ->
                skipped <- skipped + 1
                skippedNames.Add(sprintf "%s（%s）" name ea)
            | Error ea, Error eb ->
                ng <- ng + 1

                diffs.AppendLine(sprintf "%s\n両方とも例外で落ちたが中身が違う\n  A: %s\n  B: %s" name ea eb)
                |> ignore
            | Error ea, Ok _ ->
                ng <- ng + 1
                diffs.AppendLine(sprintf "%s\n片方だけ例外で落ちた\n  A: %s\n  B: 例外なし" name ea) |> ignore
            | Ok _, Error eb ->
                ng <- ng + 1
                diffs.AppendLine(sprintf "%s\n片方だけ例外で落ちた\n  A: 例外なし\n  B: %s" name eb) |> ignore

        let skippedBlock =
            if skippedNames.Count = 0 then
                ""
            else
                "比べられず（両方とも同じ例外で落ちた）:\n"
                + (skippedNames |> Seq.map (sprintf "  %s") |> String.concat "\n")
                + "\n"

        let text =
            sprintf
                "一致 %d / 割れ %d / 比べられず %d（母数 %d）\n%s%s"
                ok
                ng
                skipped
                (List.length samples)
                skippedBlock
                (diffs.ToString())

        {
            Ok = ok
            Ng = ng
            Skipped = skipped
            Total = List.length samples
            Text = text
        }

    /// 例外の中身を、比べられて・表示もできる 1 行の文字列に畳む。
    static member RenderException(e: exn) =
        let rec inner (x: exn) =
            if isNull x.InnerException then
                x
            else
                inner x.InnerException

        let i = inner e
        sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))

    /// 軌跡そのものではなく、名前・撃った数・生存数・指紋の 4 つに畳む。
    static member private FoldTrace(t: string) =
        let lines = t.Split('\n')
        let fired = lines |> Array.filter (fun l -> l.Contains "  +b") |> Array.length

        let alive =
            match lines |> Array.tryFindIndexBack (fun l -> l.StartsWith "f") with
            | Some i -> lines.[i + 1 ..] |> Array.filter (fun l -> l.StartsWith "  b") |> Array.length
            | None -> 0

        use h = SHA256.Create()

        let digest =
            h.ComputeHash(Encoding.UTF8.GetBytes t)
            |> Array.take 6
            |> Array.map (fun b -> b.ToString("x2"))
            |> String.concat ""

        fired, alive, digest

    /// 凍結データを読む。
    static member private LoadFrozenVarying() =
        let path =
            Path.GetFullPath(
                Path.Combine(__SOURCE_DIRECTORY__, "..", "TestData", "trace", "corpus-trace-varying-old-4077ed6.tsv")
            )

        File.ReadAllLines path
        |> Array.filter (fun l -> l.Trim() <> "" && not (l.StartsWith "#"))
        |> Array.map (fun line ->
            let cols = line.Split('\t')
            let name = cols.[0]

            if cols.Length >= 3 && cols.[1] = "ERROR" then
                name, Error cols.[2]
            else
                name, Ok(int cols.[1], int cols.[2], cols.[3]))
        |> Map.ofArray

    /// 公開 API だけで 227 本 が走り、Step.step 直接呼びと一致する。
    [<Test>]
    member _.``227 本を、公開 API と Step.step 直接呼びで突き合わせると全部 一致する``() =
        let rand, rank, px, py = 0.5f, 0.5f, 30.0f, 100.0f

        let viaApi (xml: string) =
            TraceApi.run (fun () -> rand) rank px py xml 60

        let direct (xml: string) =
            TraceNew.run (fun () -> rand) rank px py xml 60

        let report = Equivalence.RunBoth viaApi direct
        TestContext.WriteLine report.Text
        // 部分文字列一致だけだと、220 本が同じ例外へ吸われても通ってしまう。実測した数を門にする。
        report.Total |> should equal 227
        report.Ok |> should equal 224
        report.Ng |> should equal 0
        report.Skipped |> should equal 3

    /// 定数の $rand（上のテスト）は「引く回数・引く順」の割れを見せない。
    [<Test>]
    member _.``227 本を、凍結した旧エンジン（4077ed6）の軌跡と突き合わせると全部 一致する``() =
        let rank, px, py = 0.5f, 30.0f, 100.0f
        let stream = SharedRandomStream()
        let samples = CorpusData.uniqueSamples ()
        let frozen = Equivalence.LoadFrozenVarying()
        let mutable ok, ng, skipped = 0, 0, 0
        let diffs = StringBuilder()
        // 比べられず（両方が同じ例外で落ちた）の中身。数だけでは読み戻せない
        let skippedNames = ResizeArray<string>()

        for path in samples do
            let name = CorpusData.relative path
            let xml = File.ReadAllText path
            stream.Reset()

            let newResult =
                try
                    Ok(TraceApi.run (fun () -> stream.Next()) rank px py xml 60)
                with e ->
                    Error(Equivalence.RenderException e)

            match Map.tryFind name frozen, newResult with
            | None, _ ->
                ng <- ng + 1

                diffs.AppendLine(
                    sprintf "%s\n凍結データに無い名前。サンプルが増減した？ %s を作り直すこと" name "corpus-trace-varying-old-4077ed6.tsv"
                )
                |> ignore
            | Some(Error oldMsg), Error newMsg when oldMsg = newMsg ->
                skipped <- skipped + 1
                skippedNames.Add(sprintf "%s（%s）" name oldMsg)
            | Some(Error oldMsg), Error newMsg ->
                ng <- ng + 1

                diffs.AppendLine(sprintf "%s\n両方とも例外で落ちたが中身が違う\n  旧(4077ed6): %s\n  新(HEAD):    %s" name oldMsg newMsg)
                |> ignore
            | Some(Error oldMsg), Ok _ ->
                ng <- ng + 1

                diffs.AppendLine(sprintf "%s\n片方だけ例外で落ちた\n  旧(4077ed6): %s\n  新(HEAD):    例外なし" name oldMsg)
                |> ignore
            | Some(Ok(oldFired, oldAlive, oldDigest)), Error newMsg ->
                ng <- ng + 1

                diffs.AppendLine(
                    sprintf
                        "%s\n片方だけ例外で落ちた\n  旧(4077ed6): 例外なし（撃った %d 発 残り %d 本 %s）\n  新(HEAD):    %s"
                        name
                        oldFired
                        oldAlive
                        oldDigest
                        newMsg
                )
                |> ignore
            | Some(Ok(oldFired, oldAlive, oldDigest)), Ok newText ->
                let newFired, newAlive, newDigest = Equivalence.FoldTrace newText

                if newDigest = oldDigest then
                    ok <- ok + 1
                else
                    ng <- ng + 1

                    diffs.AppendLine(
                        sprintf
                            "%s\n  旧(4077ed6) 撃った %4d 発 残り %3d 本 %s\n  新(HEAD)    撃った %4d 発 残り %3d 本 %s"
                            name
                            oldFired
                            oldAlive
                            oldDigest
                            newFired
                            newAlive
                            newDigest
                    )
                    |> ignore

        let skippedBlock =
            if skippedNames.Count = 0 then
                ""
            else
                "比べられず（両方とも同じ例外で落ちた）:\n"
                + (skippedNames |> Seq.map (sprintf "  %s") |> String.concat "\n")
                + "\n"

        let report =
            sprintf
                "一致 %d / 割れ %d / 比べられず %d（母数 %d）\n%s%s"
                ok
                ng
                skipped
                (List.length samples)
                skippedBlock
                (diffs.ToString())

        TestContext.WriteLine report
        // 部分文字列一致だけだと、220 本が同じ例外へ吸われても通ってしまう。実測した数を門にする。
        List.length samples |> should equal 227
        ok |> should equal 224
        ng |> should equal 0
        skipped |> should equal 3

    [<Test>]
    member _.``橋は、1 か所だけ違う軌跡を見つける``() =
        let run (xml: string) =
            TraceApi.run (fun () -> 0.5f) 0.5f 30.0f 100.0f xml 60
        // 片方の軌跡の、真ん中あたりの 1 行だけを書き換える
        let broken (xml: string) =
            let lines = (run xml).Split('\n')
            let i = lines |> Array.tryFindIndex (fun l -> l.StartsWith "  b")

            match i with
            | Some i ->
                lines.[i] <- lines.[i] + " (細工)"
                System.String.Join("\n", lines)
            | None -> run xml

        let report = Equivalence.RunBoth run broken
        report.Ng |> should be (greaterThan 0)
        report.Text |> should haveSubstring "(細工)"
