namespace FsBulletML2.Playground

open FsBulletML2.Front

/// Canvas 上の自機（マウス）と乱数。**弾 1 個 につき 1 個 は要らない。**
/// 狙う相手を覚えず、`TryTarget*` は常に false。
///
/// `Rand` の関数値は 1 個。毎コマ作らない（`IFrontEnv` の但し書き）。
///
/// --- 2 つ の軸が動く（v2.0）
///
///     $rank   173 / 176 本 が使う。**0.5 に固定していた**
///     $rand   115 / 176 本 が使う。**種が無く、毎回 別の走りだった**
///
/// 版の頭で測ったら、`$rank` を 0 と 1 に振ると **176 本 中 171 本 で
/// 走りが変わった**（弾数が 3 倍 から 7 倍 になるものが在る）。
///
/// --- どちらも面を建て直さないと効かない
///
/// `Runner.load` は**木を組む段で rank と rand を引く**（`wait` の term を
/// その場で畳む）。だから `Rank` を変えても、走っている面は変わらない ——
/// 変えた側が建て直す（`Main.fs`）。
///
/// **建て直すときは並びも頭へ戻す。** 戻さないと、同じ種でも
/// 「建て直したあと」が別の走りになる（`Restart`）。
[<Sealed>]
type BrowserEnv() =

  let mutable random = SeededRandom(SeededRandom.fromClock ())
  let rand : unit -> float32 = fun () -> random.Next()

  // **既定は真ん中。** 端に寄せると、開いた人が最初に見る絵が
  // 「その弾幕の顔」ではなくなる
  let mutable rank = 0.5f

  let mutable playerX = Stage.PlayerX0
  let mutable playerY = Stage.PlayerY0

  member _.SetPlayer (x: float32) (y: float32) =
    playerX <- x
    playerY <- y

  member _.ClearPlayer () =
    playerX <- Stage.PlayerX0
    playerY <- Stage.PlayerY0

  /// 難易度。**0 から 1。** 外から来る値なので丸める
  member _.SetRank (v: float32) =
    rank <- if v < 0.0f then 0.0f elif v > 1.0f then 1.0f else v

  member _.RankValue = rank

  /// 乱数の種。**Share URL に乗る** —— 同じ種なら同じ走り
  member _.Seed = random.Seed

  member _.SetSeed (n: int) = random <- SeededRandom(SeededRandom.clamp n)

  /// 並びを頭へ戻す。**面を建て直す手前 で呼ぶ**
  member _.RestartRandom() = random.Restart()

  interface IFrontEnv with
    member _.Rand = rand
    member _.Rank = rank
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
