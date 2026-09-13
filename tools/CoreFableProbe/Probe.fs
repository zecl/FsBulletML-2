module Probe

open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Dsl

/// **`Core` と `Dsl` だけ で書いた弾幕。**
///
/// パーサ（`FsBulletML2.Parser`）は Fable で焼けない —— FParsec が F# の
/// ソースを同梱しないパッケージで、`System.Xml` も Fable に無い。
/// だから**この門が当てる先は「読む段を通らない道」**になる。
/// Fable から使う側も同じ形（CE で書くか、型プロバイダで引くか、読んだ木を渡す）。
let private script =
  verticalAnon {
    top {
      repeat "3" {
        nest {
          fire {
            sequence "36"
            speed "2"
            plain
          }
          wait "1"
        }
      }
    }
  }

/// 走らせて、**整数だけ**を返す。
///
/// **座標や速さで突き合わせない。** JS に単精度が無いので、Fable は
/// `float32` を倍精度のまま持つ —— 同じソース・同じ弾幕でも
/// 単精度の桁数を超えたところから値が割れる（実測で 7 桁 目）。
/// 撃った数・撃ったフレーム・終わった数は丸めを跨がないので一致する。
///
/// 撃たれた弾も回す。**親 1 体 だけ回しても、撃たれた側の道は 1 度 も通らない。**
let run () =
  let rand = fun () -> 0.5f
  let s = Runner.load rand 0.5f script
  let env =
    { Rand = rand
      Rank = 0.5f
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
  let mutable root = Runner.newRoot BulletType.Enemy s
  let mutable shots : BulletRun list = []
  let mutable spawned = 0
  let mutable shootingFrames = 0
  let mutable finished = 0
  for _ in 1 .. 60 do
    let f = Runner.stepWith env root root.Motion
    root <- f.Run
    if not (List.isEmpty f.Spawned) then shootingFrames <- shootingFrames + 1
    spawned <- spawned + List.length f.Spawned
    shots <- f.Spawned @ shots
    shots <-
      shots
      |> List.map (fun r ->
        let g = Runner.stepWith env r r.Motion
        if g.Finished then finished <- finished + 1
        g.Run)
  spawned, shootingFrames, finished

/// 突き合わせる 1 行。**形を 1 か所 にする** ——
/// node 側と .NET 側で別々に組むと、そちらが食い違って
/// 「中身は同じなのに赤」「違うのに緑」になる。
let line () =
  let spawned, shootingFrames, finished = run ()
  sprintf "spawned=%d shootingFrames=%d finished=%d" spawned shootingFrames finished
