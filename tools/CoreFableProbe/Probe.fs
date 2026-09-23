module Probe

open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Dsl

/// `Core` と `Dsl` だけ。パーサは Fable で焼けない。
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

/// 整数だけ返す。`float32` は倍精度のままなので座標では割れる。撃たれた弾も回す。
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

/// 形はここ 1 か所。node と .NET で別々に組むと、食い違いの色が嘘になる。
let line () =
  let spawned, shootingFrames, finished = run ()
  sprintf "spawned=%d shootingFrames=%d finished=%d" spawned shootingFrames finished
