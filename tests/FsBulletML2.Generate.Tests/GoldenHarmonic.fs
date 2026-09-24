namespace FsBulletML2.Generate.Tests

open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 花 が出す 字 を まるごと 固定 する（`GoldenShape` も 収録 の ゴールデン も 花 を 通らない）。
/// 変える つもり の直し で 赤 なら、指紋 を置き直して 何 を変えた かを commit に書く
[<TestFixture>]
type GoldenHarmonic() =

    /// 3 札 x Folds 4 x Spin 2 x Vanishing 2 x Amplitude 3 x Blooms 3 x Phase 2 = 864 通り。
    /// `Phase` を 振る のは 層 が そこ に 足し込む から —— 固定 だと 扱い を壊して も 緑 のまま
    static let all () =
        seq {
            for figure in [ Petal; Star; Heart ] do
                for folds in [ 3; 5; 7; 8 ] do
                    for spin in [ 0.0; 1.5 ] do
                        for vanishing in [ false; true ] do
                            for amplitude in [ 0.0; 1.2; 2.0 ] do
                                for blooms in [ 1; 2; 5 ] do
                                    for phase in [ 0.0; 0.7 ] ->
                                        HarmonicSpec.create (fun a ->
                                            { a with
                                                Figure = figure
                                                Folds = folds
                                                Speed = 2.0
                                                Amplitude = amplitude
                                                Phase = phase
                                                Spin = spin
                                                Blooms = blooms
                                                Density = 1.0
                                                Vanishing = vanishing
                                            })
        }

    static let digestOf (specs: HarmonicSpec seq) =
        let sb = StringBuilder()

        for s in specs do
            sb.Append(BulletmlWriter.toIndentedXml 4 (Harmonic.generate s).Bulletml)
            |> ignore

        use h = SHA256.Create()

        h.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))
        |> Array.take 8
        |> Array.map (fun b -> b.ToString("x2"))
        |> String.concat ""

    [<Test>]
    member _.``花 の字 が動かない``() =
        let specs = all () |> Seq.toList
        List.length specs |> should equal 864
        let got = digestOf specs
        TestContext.Out.WriteLine(sprintf "指紋 %s" got)
        got |> should equal "791d49e66d324fe5"

    [<Test>]
    member _.``1 波 の弾数 が動かない``() =
        let got = all () |> Seq.sumBy Harmonic.shotsPerWave
        TestContext.Out.WriteLine(sprintf "弾数 %d" got)
        got |> should equal 63120
