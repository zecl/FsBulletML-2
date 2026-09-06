namespace FsBulletML2.Sample.Browser

open FsBulletML2.Front

/// Canvas 上の自機（マウス）と乱数。**弾 1 個 につき 1 個 は要らない。**
/// 狙う相手を覚えず、v0.1 は `TryTarget*` が常に false。
///
/// `Rand` の関数値は 1 個。毎コマ作らない（`IFrontEnv` の但し書き）。
[<Sealed>]
type BrowserEnv() =

  let rng = System.Random()
  let rand : unit -> float32 = fun () -> float32 (rng.NextDouble())

  let mutable playerX = Stage.PlayerX0
  let mutable playerY = Stage.PlayerY0

  member _.SetPlayer (x: float32) (y: float32) =
    playerX <- x
    playerY <- y

  member _.ClearPlayer () =
    playerX <- Stage.PlayerX0
    playerY <- Stage.PlayerY0

  interface IFrontEnv with
    member _.Rand = rand
    member _.Rank = 0.5f
    member _.PlayerX = playerX
    member _.PlayerY = playerY
    member _.TryTargetFrom (_, _, ex, ey) =
      ex <- 0.0f
      ey <- 0.0f
      false
    member _.TrySpawnTargetFrom (_, _, ex, ey) =
      ex <- 0.0f
      ey <- 0.0f
      false
