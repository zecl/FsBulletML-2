namespace FsBulletML2.Sample.MonoGame.FSharp

open System
open FsBulletML2

type BulletFunctions() =
    static let rand = new Random()

    interface IBulletMLManager with
        member this.GetRandom() =
            Math.Round(rand.NextDouble() * 10000.) / 10000. |> float32

        member this.GetRank() = 0.f
        member this.GetPlayerPosX() = FsBulletML2SampleGame.Player.pos.X
        member this.GetPlayerPosY() = FsBulletML2SampleGame.Player.pos.Y
