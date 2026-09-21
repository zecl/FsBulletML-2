namespace FsBulletML2.Generate.Tests

open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2.Generate

/// 1 波 を 絵 に して 1 枚 に並べる。
///
/// 形 の判断 を 8787 の 目視 だけ で やって いて、ハート で 2 周・星 で 2 周・
/// 薔薇 で 4 周 外した。絵 が 試験 の中 で 出れば その 全部 が 1 回 で済む
[<TestFixture>]
type HarmonicGrid() =

  static let waveOf (h: HarmonicSpec) =
    Harmonic.generate h |> fun info -> Felt.run 90 info.Bulletml |> Felt.Draw.fullest

  static let write (name: string) (cols: int) (cells: (string * Felt.Snapshot) list) =
    let dir = Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "artifacts")
    Directory.CreateDirectory dir |> ignore
    let path = Path.Combine(dir, name)
    File.WriteAllText(path, Felt.Draw.grid cols cells)
    TestContext.Out.WriteLine(sprintf "絵 %s" (Path.GetFullPath path))
    path

  [<Test>]
  member _.``1 波 の 点 が 絵 に なる``() =
    let h = HarmonicSpec.create (fun a -> { a with Folds = 5; Speed = 2.0; Amplitude = 1.2 })
    let w = waveOf h
    // 1 波 が まるごと 揃って いる こと。ここ が 割れる と 絵 が 輪郭 に ならない
    Felt.Draw.outline w |> List.length |> should equal (Harmonic.shotsPerWave h)
    let svg = Felt.Draw.grid 4 [ "petal-5", w ]
    svg |> should startWith "<svg"
    svg |> should endWith "</svg>\n"
    svg |> should haveSubstring "petal-5"

  [<Test>]
  member _.``3 札 を 1 枚 に書き出す``() =
    let cells =
      [ for figure, name in [ Petal, "petal"; Star, "star"; Heart, "heart" ] do
          for folds in [ 3; 5; 7; 8 ] ->
            sprintf "%s-%d" name folds,
            waveOf (HarmonicSpec.create (fun a ->
              { a with Figure = figure; Folds = folds; Speed = 2.0; Amplitude = 1.2 })) ]
    write "harmonic-grid.svg" 4 cells |> File.Exists |> should equal true
