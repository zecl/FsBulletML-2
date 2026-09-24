namespace FsBulletML2.Generate.Tests

open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 生成器 が出す 字 を 連結 して 指紋 を取る。答え を変えない 組み直し の 網
/// 変える つもり の直し で 赤 に なったら、指紋 を置き直して 何 を変えた かを commit に書く
[<TestFixture>]
type GoldenShape() =

    /// 軸 の数。9 つ の段 ＋ 5 つ の真偽（`BoundTests` と同じ 畳み方）
    static let AXES = 14

    static let allExtremes () =
        let at (bits: int) (i: int) (hi: float) =
            if (bits >>> i) &&& 1 = 0 then 0.0 else hi

        let flag (bits: int) (i: int) = (bits >>> i) &&& 1 = 1

        seq {
            for bits in 0 .. (1 <<< AXES) - 1 ->
                PatternSpec.create (fun a ->
                    { a with
                        Kind = Spiral
                        Speed = at bits 0 3.0
                        Density = at bits 1 3.0
                        Symmetry = at bits 2 3.0
                        Layers = at bits 3 2.0
                        Jitter = at bits 4 2.0
                        Rhythm = at bits 5 2.0
                        Depth = at bits 6 2.0
                        BulletKinds = at bits 7 2.0
                        Cascade = at bits 8 3.0
                        Breathe = flag bits 9
                        Vanishing = flag bits 10
                        Aiming = flag bits 11
                        Pause = flag bits 12
                        Parametrized = flag bits 13
                        KindConfidence = 0.8
                    })
        }

    /// 型 と 名指し の 3 軸 も 回す。上 の 16,384 通り は `Spiral` / `Around` / `Plain` に固定 して いる
    static let named () =
        seq {
            for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
                for facing in [ Around; Forward; Backward; Sideways ] do
                    for motion in [ Plain; Laser; Missile ] do
                        for ways in [ 0; 3; 12 ] do
                            PatternSpec.create (fun a ->
                                { a with
                                    Kind = kind
                                    Speed = 2.0
                                    Density = 1.5
                                    Symmetry = 2.0
                                    Layers = 1.0
                                    Jitter = 1.0
                                    Rhythm = 1.0
                                    Depth = 1.0
                                    BulletKinds = 1.0
                                    Cascade = 1.0
                                    Vanishing = true
                                    Aiming = true
                                    Ways = ways
                                    Facing = facing
                                    Motion = motion
                                    KindConfidence = 0.8
                                })
        }

    static let digestOf (specs: PatternSpec seq) =
        let sb = StringBuilder()

        for s in specs do
            sb.Append(BulletmlWriter.toIndentedXml 4 (Generate.generate s).Bulletml)
            |> ignore

        use h = SHA256.Create()

        h.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))
        |> Array.take 8
        |> Array.map (fun b -> b.ToString("x2"))
        |> String.concat ""

    [<Test>]
    member _.``端 の組み合わせ 16,384 通り の字 が動かない``() =
        let specs = allExtremes () |> Seq.toList
        // 当てる 先 が 本当 に在る か を 先 に見る
        List.length specs |> should equal 16384
        let got = digestOf specs
        TestContext.Out.WriteLine(sprintf "指紋 %s" got)
        got |> should equal "88c57a32035e1a82"

    [<Test>]
    member _.``型 と 名指し の 180 通り の字 が動かない``() =
        let specs = named () |> Seq.toList
        List.length specs |> should equal 180
        let got = digestOf specs
        TestContext.Out.WriteLine(sprintf "指紋 %s" got)
        got |> should equal "21d7b412a4c2cb36"
