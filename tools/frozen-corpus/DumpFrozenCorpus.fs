namespace FsBulletML2.Core.Tests

open System
open System.IO
open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsBulletML2.Processable

/// Producer of `tests/TestData/trace/corpus-trace-varying-old-4077ed6.tsv`.
///
/// This file is **not** part of any build at HEAD. It exists to make the
/// frozen corpus (the branch's only surviving old-engine evidence, since
/// commit 9d98954 deleted `ProcessableBulletml`) reproducible by someone who
/// was not here. It only compiles against the pre-deletion API
/// (`Processable.ProcessableBulletml`, `BulletmlTask.Init()` with no `Env`
/// argument), so it has to be run from a worktree checked out at the old
/// engine's last commit, not from this branch.
///
/// The fold (`digest`, `aliveAtEnd`) and the exception rendering below are
/// copied verbatim from `FsBulletML2.Core.Tests.Equivalence`'s private
/// `FoldTrace` / `RenderException` members as they stand at HEAD when this
/// file was written. If those change, this file has drifted and must be
/// re-synced by hand before the next regeneration — there is no shared
/// source between the two, because one only compiles against the deleted
/// engine and the other only against the current one.
///
/// ---
/// HOW TO REGENERATE `corpus-trace-varying-old-4077ed6.tsv`
/// ---
///
/// 1. Add a worktree at the commit named in the frozen file's header
///    (currently `4077ed6`), at a plain path.
///
///      git worktree add C:\Code\FsBulletML-old-4077ed6 4077ed6
///
///    TRAP 1 (produces a confident wrong result — zero samples, green test):
///    `CorpusData.isBuildOutput` (`tests/FsBulletML2.Core.Tests/Corpus.fs`)
///    rejects any path containing the literal segment `/Temp/`, to keep
///    build-output copies out of the sample count. A worktree placed under
///    a temp directory (`$env:TEMP`, `/tmp`, anything with `\Temp\` or
///    `/Temp/` in its absolute path) is filtered out entirely by that same
///    rule, since it does not distinguish "this Temp is a build artifact"
///    from "this Temp is where I happened to put my worktree." The
///    resulting run reports **0 samples and the smoke test still passes**
///    (it only asserts the fixture is non-empty across the *whole* corpus
///    dir, and an empty dump test here would report nothing failing) —
///    nothing here says "you measured the wrong tree." Use a path with no
///    `Temp` component anywhere in it, and check `%d samples` in the
///    written-output message before trusting the result.
///
/// 2. Copy this file into that worktree's test project and wire it in:
///
///      Copy this file to
///        <worktree>\tests\FsBulletML2.Core.Tests\DumpFrozenCorpus.fs
///      Add one line to
///        <worktree>\tests\FsBulletML2.Core.Tests\FsBulletML2.Core.Tests.fsproj
///      inside the existing `<ItemGroup>` of `<Compile Include=...>` lines:
///        <Compile Include="DumpFrozenCorpus.fs" />
///
/// 3. Run it and capture the written file:
///
///      cd <worktree>
///      dotnet test tests\FsBulletML2.Core.Tests\FsBulletML2.Core.Tests.fsproj -v:n --filter "FullyQualifiedName~DumpFrozenCorpus"
///
///    The test writes to `%TEMP%\fsb-frozen-corpus-varying.tsv` and prints
///    the path and sample count via `TestContext.WriteLine`.
///
/// 4. Copy the written file back over
///    `tests/TestData/trace/corpus-trace-varying-old-4077ed6.tsv` in the
///    branch (not the worktree), **prepending the provenance header** this
///    file writes as its own first lines (see `headerLines` below) if the
///    parameters changed, and re-add the file's own `#` header lines if
///    your copy step stripped them.
///
///    TRAP 2 (produces a confident wrong result — measures a stale binary):
///    if you restore a mutated source file with a straight file copy
///    (PowerShell `Copy-Item source.fs dest.fs`, or equivalent) instead of
///    `git checkout -- <path>` / an editor save, the destination can end up
///    with the **source's own older `LastWriteTime`**, which can be older
///    than the already-built `bin/obj` output. MSBuild's incremental build
///    compares timestamps, sees the `.fs` file as "not newer than the last
///    build," and skips recompilation — `dotnet build`/`dotnet test`
///    reports success while silently running the previous binary. This is
///    a general MSBuild trap, not specific to this file, but it is exactly
///    how a wrong regeneration would slip through unnoticed: fix a bug,
///    revert it to re-measure the "before" number with a raw copy, get the
///    "after" number back by mistake, and record it as if it were new. If
///    a build finishes suspiciously fast, or a number does not move after
///    an edit that should have moved it, force a clean rebuild
///    (`dotnet build --no-incremental`, or delete `obj`/`bin`) before
///    trusting the result.
///
/// 5. Delete the worktree when done: `git worktree remove <path>`.
[<TestFixture>]
type DumpFrozenCorpus() =

  // Parameters. Must match Equivalence.fs's
  // ``227 本を、凍結した旧エンジン（4077ed6）の軌跡と突き合わせると全部 一致する``
  // exactly, or the two sides are not comparable
  let sourceCommit = "4077ed6"
  let rank, px, py = 0.5f, 30.0f, 100.0f
  let frames = 60

  // Copied verbatim from Equivalence.RenderException at HEAD
  let renderException (e: exn) =
    let rec inner (x: exn) = if isNull x.InnerException then x else inner x.InnerException
    let i = inner e
    sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))

  // Copied verbatim from Equivalence.FoldTrace at HEAD (the SHA256-first-6-bytes
  // digest and the fired/alive counting), with one addition: exception messages
  // are also stripped of literal tabs before being written, since this is the
  // one place that has to survive being written into a TSV column
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
      [ sprintf "# source-commit: %s" sourceCommit
        sprintf "# params: rank=%s px=%s py=%s frames=%d rand=SharedRandomStream(i -> (i*37 mod 101)/101, reset per sample)"
          (string rank) (string px) (string py) frames
        "# producer: tools/frozen-corpus/DumpFrozenCorpus.fs"
        "# format: name\\tfired\\talive\\tdigest  OR  name\\tERROR\\ttype: message"
        "# consumer: tests/FsBulletML2.Core.Tests/Equivalence.fs (LoadFrozenVarying)" ]
    for h in headerLines do sb.AppendLine(h) |> ignore
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
