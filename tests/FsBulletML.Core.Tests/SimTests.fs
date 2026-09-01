namespace FsBulletML.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML
open FsBulletML.DTD
open FsBulletML.Domain

/// Sim は Reader + State + Writer を 1 本に畳んだもの。
/// 手で 3 つ組を持ち回るのをやめるための道具なので、
/// 「落とさない」「順序が保たれる」「読むだけの環境は変わらない」を見る。
[<TestFixture>]
type SimTests() =

  let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; AimDir = 0.f; EnemyAimDir = 0.f }

  let st0 =
    { Pos = { X = 0.f; Y = 0.f }
      Speed = 1.f
      Dir = 0.f
      Accel = { X = 0.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = []
      PendingBulletAim = false }

  [<Test>]
  member _.``ask は環境を読む。状態も効果も動かない``() =
    let a, st, w = Sim.run env st0 Sim.ask
    obj.ReferenceEquals(a, env) |> should equal true
    st |> should equal st0
    w |> should be Empty

  [<Test>]
  member _.``put した状態が、次の get に見える``() =
    let m = sim {
      do! Sim.put { st0 with Speed = 42.0f }
      let! s = Sim.get
      return s.Speed
    }
    let a, st, _ = Sim.run env st0 m
    a |> should equal 42.0f
    st.Speed |> should equal 42.0f

  [<Test>]
  member _.``emit した効果は、書いた順に並ぶ``() =
    let child n = { st0 with Speed = float32 n }
    let m = sim {
      do! Sim.emit (Spawn (child 1))
      do! Sim.emit Vanished
      do! Sim.emit (Spawn (child 3))
    }
    let _, _, w = Sim.run env st0 m
    match w with
    | [ Spawn a; Vanished; Spawn c ] ->
        a.Speed |> should equal 1.0f
        c.Speed |> should equal 3.0f
    | _ -> Assert.Fail (sprintf "順序が違う: %A" w)

  [<Test>]
  member _.``入れ子にしても効果を落とさない``() =
    let inner = sim { do! Sim.emit Vanished }
    let m = sim {
      do! Sim.emit (Spawn { st0 with Speed = 1.0f })
      do! inner
      do! Sim.emit (Spawn { st0 with Speed = 3.0f })
    }
    let _, _, w = Sim.run env st0 m
    List.length w |> should equal 3
    match w with
    | [ Spawn a; Vanished; Spawn b ] ->
        a.Speed |> should equal 1.0f
        b.Speed |> should equal 3.0f
    | _ -> Assert.Fail (sprintf "順序が違う: %A" w)

  [<Test>]
  member _.``効果を 1000 個 積んでも順序が保たれる``() =
    // 差分リストにしているので、@ で繋いだときの O(n^2) にならないことも兼ねる
    let m = sim {
      for i in 1 .. 1000 do
        do! Sim.emit (Spawn { st0 with Speed = float32 i })
    }
    let _, _, w = Sim.run env st0 m
    List.length w |> should equal 1000
    match List.head w, List.last w with
    | Spawn a, Spawn b ->
        a.Speed |> should equal 1.0f
        b.Speed |> should equal 1000.0f
    | _ -> Assert.Fail "先頭か末尾が Spawn ではない"

  [<Test>]
  member _.``状態は前から後ろへ渡る``() =
    // bind が r1.State を f へ渡すこと。位置ではなく名前で受けるので
    // 取り違えが起きない形になっているかを見る
    let m = sim {
      do! Sim.put { st0 with Speed = 2.0f }
      let! s1 = Sim.get
      do! Sim.put { s1 with Speed = s1.Speed * 3.0f }
      let! s2 = Sim.get
      return s2.Speed
    }
    let a, _, _ = Sim.run env st0 m
    a |> should equal 6.0f
