/// `open FsBulletML2` が 使う人 の 名前 を奪わない か。
/// AutoOpen の module に ありふれた 名前 の active pattern を置く と、
/// 先 に open した 型 の同じ 名前 の腕 が 型 違い で 通らなく なる。
/// FsBulletML2 の名前空間 の外 に置く —— 中 だと 親 が 暗黙 に open されて 順 が崩れる
module OpenDoesNotShadow

open NUnit.Framework
open FsUnit

module Mine =
    type Precision =
        | Single of int
        | Double of int

    type Stamp =
        | Date of int
        | Decimal of int
        | Int32 of int
        | Int64 of int

open Mine
open FsBulletML2

[<TestFixture>]
type OpenDoesNotShadow() =

    [<Test>]
    member _.``open の あと も 自分 の型 の腕 で 照合 できる``() =
        let precision p =
            match p with
            | Single n -> n
            | Double n -> n * 2

        let stamp s =
            match s with
            | Date n
            | Decimal n
            | Int32 n
            | Int64 n -> n

        precision (Double 32) |> should equal 64
        stamp (Decimal 7) |> should equal 7
