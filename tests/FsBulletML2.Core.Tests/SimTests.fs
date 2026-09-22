namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// Sim は Reader + State + Writer を 1 本に畳んだもの。
/// 手で 3 つ組を持ち回るのをやめるための道具なので、
/// 「落とさない」「順序が保たれる」「読むだけの環境は変わらない」を見る。
[<TestFixture>]
type SimTests() =

  let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; Aim = { ToPlayer = 0.f; ToEnemy = 0.f }; Spawn = { ToPlayer = 0.f; ToEnemy = 0.f } }

  let st0 =
    { Pos = { X = 0.f; Y = 0.f }
      Speed = 1.f
      Dir = 0.f
      Accel = { X = 0.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = [] }

  /// 効果と状態の両方 を動かす台本。どちらも動かないものを当てると、
  /// モナド則の 4 本 は bind を壊しても緑のままになる
  let moving =
    simForTests {
      do! Sim.put { st0 with Speed = 7.0f }
      do! Sim.emit Vanished
      let! s = Sim.get
      return s.Speed
    }

  /// 値・状態・効果の 3 つ とも一致するか。Sim は 3 つ 持ち回るので、
  /// 値だけ見ると State や Emit を落とす壊れ方が通る
  let same (a: Sim<float32>) (b: Sim<float32>) =
    let va, sa, wa = Sim.run env st0 a
    let vb, sb, wb = Sim.run env st0 b
    va |> should equal vb
    sa |> should equal sb
    wa |> should equal wb

  [<Test>]
  member _.``ask は環境を読む。状態も効果も動かない``() =
    let a, st, w = Sim.run env st0 Sim.ask
    // Env が [<Struct>] になったので obj.ReferenceEquals は使えない
    // （box した時点で別のオブジェクトになる）。値型では「同じオブジェクト」
    // という問いに意味が無いので、中の値が全部 素通しかを見る。
    //
    // 構造的等価（a |> should equal env）も使えない。Rand が関数で、
    // F# の関数は比較できず実行時に落ちる。関数だけ参照で、残りは値で見る。
    obj.ReferenceEquals(a.Rand, env.Rand) |> should equal true
    a.Rank |> should equal env.Rank
    a.Aim.ToPlayer |> should equal env.Aim.ToPlayer
    a.Aim.ToEnemy |> should equal env.Aim.ToEnemy
    a.Spawn.ToPlayer |> should equal env.Spawn.ToPlayer
    a.Spawn.ToEnemy |> should equal env.Spawn.ToEnemy
    st |> should equal st0
    w |> should be Empty

  [<Test>]
  member _.``put した状態が、次の get に見える``() =
    let m = simForTests {
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
    let m = simForTests {
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
    let inner = simForTests { do! Sim.emit Vanished }
    let m = simForTests {
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
    let m = simForTests {
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
  member _.``モナド則: 左単位元 —— bind f (ret x) は f x``() =
    let f x = simForTests { do! Sim.emit Vanished
                            return x * 2.0f }
    same (simForTests.Bind (Sim.ret 21.0f, f)) (f 21.0f)

  [<Test>]
  member _.``モナド則: 右単位元 —— bind ret m は m``() =
    same (simForTests.Bind (moving, Sim.ret)) moving

  [<Test>]
  member _.``モナド則: 結合則 —— 繋ぐ順を変えても同じ``() =
    let f (x: float32) = simForTests { do! Sim.emit (Spawn { st0 with Speed = x })
                                       return x + 1.0f }
    let g (x: float32) = simForTests { do! Sim.put { st0 with Speed = x }
                                       return x * 3.0f }
    same
      (simForTests.Bind (simForTests.Bind (moving, f), g))
      (simForTests.Bind (moving, fun x -> simForTests.Bind (f x, g)))

  /// BindReturn は Bind + Return の速い道。答えが変わってはいけない。
  /// 速い道を足したら、遅い道と突き合わせる
  [<Test>]
  member _.``BindReturn は Bind + Return と同じ答えを返す``() =
    same
      (simForTests.BindReturn (moving, fun x -> x * 2.0f))
      (simForTests.Bind (moving, fun x -> Sim.ret (x * 2.0f)))

  [<Test>]
  member _.``状態は前から後ろへ渡る``() =
    // bind が r1.State を f へ渡すこと。位置ではなく名前で受けるので
    // 取り違えが起きない形になっているかを見る
    let m = simForTests {
      do! Sim.put { st0 with Speed = 2.0f }
      let! s1 = Sim.get
      do! Sim.put { s1 with Speed = s1.Speed * 3.0f }
      let! s2 = Sim.get
      return s2.Speed
    }
    let a, _, _ = Sim.run env st0 m
    a |> should equal 6.0f
