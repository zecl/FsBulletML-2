namespace FsBulletML2.Core.Tests

open System
open System.IO
open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsBulletML2.Processable

/// Not part of any build at HEAD. Compiles only against the pre-deletion API. Run from a worktree at that commit.
[<TestFixture>]
type DumpFrozenCorpus() =

    // Must match Equivalence.fs's frozen-corpus parameters exactly, or the sides are not comparable.
    let sourceCommit = "4077ed6"
    let rank, px, py = 0.5f, 30.0f, 100.0f
    let frames = 60

    // Copied verbatim from Equivalence.RenderException at HEAD
    let renderException (e: exn) =
        let rec inner (x: exn) =
            if isNull x.InnerException then
                x
            else
                inner x.InnerException

        let i = inner e
        sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))

    // Copied from Equivalence.FoldTrace. Also strip tabs; this column is written into a TSV.
    let digest (s: string) =
        use h = SHA256.Create()

        h.ComputeHash(Encoding.UTF8.GetBytes s)
        |> Array.take 6
        |> Array.map (fun b -> b.ToString("x2"))
        |> String.concat ""

    let aliveAtEnd (lines: string[]) =
        match lines |> Array.tryFindIndexBack (fun l -> l.StartsWith "f") with
        | Some i -> lines.[i + 1 ..] |> Array.filter (fun l -> l.StartsWith "  b") |> Array.length
        | None -> 0

    [<Test>]
    member _.``変動する乱数列での軌跡を、名前・撃った数・生存数・指紋に畳んで凍結する``() =
        let stream = SharedRandomStream()
        let samples = CorpusData.uniqueSamples ()
        let sb = StringBuilder()

        let headerLines =
            [
                sprintf "# source-commit: %s" sourceCommit
                sprintf
                    "# params: rank=%s px=%s py=%s frames=%d rand=SharedRandomStream(i -> (i*37 mod 101)/101, reset per sample)"
                    (string rank)
                    (string px)
                    (string py)
                    frames
                "# producer: tools/frozen-corpus/DumpFrozenCorpus.fs"
                "# format: name\\tfired\\talive\\tdigest  OR  name\\tERROR\\ttype: message"
                "# consumer: tests/FsBulletML2.Core.Tests/Equivalence.fs (LoadFrozenVarying)"
            ]

        for h in headerLines do
            sb.AppendLine(h) |> ignore

        for path in samples do
            let name = CorpusData.relative path
            let xml = File.ReadAllText path
            stream.Reset()
            BulletMLManager.Init(StreamManager(stream, rank, px, py))

            let row =
                try
                    let t = Trace.run xml frames
                    let lines = t.Split('\n')
                    let fired = lines |> Array.filter (fun l -> l.Contains "  +b") |> Array.length
                    let alive = aliveAtEnd lines
                    sprintf "%s\t%d\t%d\t%s" name fired alive (digest t)
                with e ->
                    sprintf "%s\tERROR\t%s" name ((renderException e).Replace("\t", " "))

            sb.AppendLine(row) |> ignore

        let outPath = Path.Combine(Path.GetTempPath(), "fsb-frozen-corpus-varying.tsv")
        File.WriteAllText(outPath, sb.ToString())
        TestContext.WriteLine(sprintf "wrote %s (%d samples)" outPath (List.length samples))
        Assert.Pass()
