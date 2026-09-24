namespace FsBulletML2.Front.Tests

open System
open NUnit.Framework
open FsUnit
open FsBulletML2.Front

/// `Aiming.toward` が、同梱の 2 つ のフロントが持っていた式と float32 のビットまで同じ答えを返すこと。
/// 畳んだ側を直したら、こちらが赤くなる。
[<TestFixture>]
type AimingFreeze() =

    /// MonoGame（`src/FsBulletML2.MonoGame/FrontEnv.fs` の `aimAtPlayer` と、
    /// `BaseBullet.EnemyAimDirAt` が持っていた式）
    let monoGame (fx: float32) (fy: float32) (tx: float32) (ty: float32) =
        float32 (Math.Atan2(float (tx - fx), -1.0 * float (ty - fy)))

    /// Unity2D（`src/FsBulletML2.Unity2D/DefaultBullet.fs` の `AimDir` /
    /// `EnemyAimDir` が持っていた式。`Mathf.Atan2` は中身が同じ）
    let unity2D (fx: float32) (fy: float32) (tx: float32) (ty: float32) =
        float32 (Math.Atan2(float (tx - fx), 1.0 * float (ty - fy)))

    /// 0 を挟み、正負が非対称で、同じ値が 2 度 出ない 11 点
    let grid =
        [|
            -97.5f
            -40.0f
            -13.25f
            -3.5f
            -0.25f
            0.0f
            0.75f
            6.5f
            22.0f
            61.25f
            180.0f
        |]

    let bits (v: float32) = BitConverter.SingleToInt32Bits v

    [<Test>]
    member _.``14,641 組 とも YDown が MonoGame の式とビット一致``() =
        let mutable compared = 0
        let mutable mismatch = 0
        let mutable firstBad = ""

        for fx in grid do
            for fy in grid do
                for tx in grid do
                    for ty in grid do
                        compared <- compared + 1
                        let a = Aiming.toward Space.YDown fx fy tx ty
                        let b = monoGame fx fy tx ty

                        if bits a <> bits b then
                            mismatch <- mismatch + 1

                            if firstBad = "" then
                                firstBad <- sprintf "(%f, %f) -> (%f, %f) : %f <> %f" fx fy tx ty a b

        compared |> should equal 14641

        if mismatch <> 0 then
            Assert.Fail(sprintf "%d 組 ずれた。最初: %s" mismatch firstBad)

    [<Test>]
    member _.``14,641 組 とも YUp が Unity2D の式とビット一致``() =
        let mutable compared = 0
        let mutable mismatch = 0
        let mutable firstBad = ""

        for fx in grid do
            for fy in grid do
                for tx in grid do
                    for ty in grid do
                        compared <- compared + 1
                        let a = Aiming.toward Space.YUp fx fy tx ty
                        let b = unity2D fx fy tx ty

                        if bits a <> bits b then
                            mismatch <- mismatch + 1

                            if firstBad = "" then
                                firstBad <- sprintf "(%f, %f) -> (%f, %f) : %f <> %f" fx fy tx ty a b

        compared |> should equal 14641

        if mismatch <> 0 then
            Assert.Fail(sprintf "%d 組 ずれた。最初: %s" mismatch firstBad)

    /// 較正。 上の 2 本 が「どんな式でも通る」門になっていないことを見る。
    /// 2 つ の座標系は実際に違う答えを出すので、取り違えたら落ちるはず
    [<Test>]
    member _.``較正: YDown と YUp は同じ答えではない``() =
        let mutable differ = 0

        for fx in grid do
            for fy in grid do
                for tx in grid do
                    for ty in grid do
                        if
                            bits (Aiming.toward Space.YDown fx fy tx ty)
                            <> bits (Aiming.toward Space.YUp fx fy tx ty)
                        then
                            differ <- differ + 1
        // Y が同じ点（fy = ty）では両方 0 を挟むので一致しうる。
        // 大半でずれることだけを見る
        differ |> should be (greaterThan 10000)
        TestContext.WriteLine(sprintf "14,641 組 中 %d 組 でずれた" differ)
