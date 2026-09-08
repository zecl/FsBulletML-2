namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Playground

/// **2 面 で見た絵は、1 面 で見た絵と同じでなければならない。**
///
/// 並べて比べる道具なので、並べたことで絵が変わったら比べられない。
/// これが**乱数を面ごとに分けてある理由**（`Main.fs` の `env2`）——
/// 1 個 の `SeededRandom` を共有すると、2 つ の面が並びを取り合って、
/// 片方 の弾数が変わるたびにもう片方 の引く値がずれる。
///
/// --- この点が届かないところ
///
/// **`Main.fs` で `env2` を `env` に書き換えても、ここは赤くならない。**
/// あのファイルは Bolero を参照するので門の外に居て、分けるという判断
/// そのものには当たらない。ここが当てているのは**その判断が正しいこと** ——
/// 共有すると走りが変わる、という数を残してある。
///
/// だから「実装の変異で赤くなる門」ではなく、
/// **「共有していいのでは」と思った人が答えを引ける場所**として置いてある。
/// 実装側は 1 か所（`Main.fs` の `env2`）で、そこは目で見るしかない。
///
/// --- 較正（当てた変異と、赤くなった点）
///
/// 変異はこの file の側にしか当てられない（上の理由）。それでも
/// **2 つ の点が互いの逆を見ている**ことは確かめてある ——
///
///   `paired` の `share` を常に true    別の env なら 1 面 のときと同じ
///   `paired` の `share` を常に false   env を共有すると走りが変わる
///   `f2.Tick()` を落とす               2 面 目 も進む
[<TestFixture>]
type TwoFields() =

  let envOf (seed: int) =
    let r = SeededRandom(seed)
    let rand = fun () -> r.Next()
    { new IFrontEnv with
        member _.Rand = rand
        member _.Rank = 0.5f
        member _.PlayerX = Stage.portrait.PlayerX
        member _.PlayerY = Stage.portrait.PlayerY
        member _.TryTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false
        member _.TrySpawnTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false }

  /// 2 面 目 に載せるほう。**`$rand` を使う** ——
  /// 使わない弾幕だと、env を共有しても走りが変わらず、
  /// **分ける理由の点が「変わらなくて緑」になる**
  [<Literal>]
  let XmlB = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>30</times>
      <action>
        <fire>
          <direction type="absolute">360 * $rand</direction>
          <speed>1.5</speed>
          <bullet/>
        </fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  /// 1 面 目 に載せるほう。**中身も弾数も B と違う** ——
  /// 同じものを並べると、取り合ってもずれ方が同じになりうる
  [<Literal>]
  let XmlA = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>7</times>
      <action>
        <fire>
          <direction type="absolute">180 * $rand</direction>
          <speed>0.8</speed>
          <bullet/>
        </fire>
        <wait>5</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  let a = Bulletml.readXmlString XmlA
  let b = Bulletml.readXmlString XmlB

  [<Literal>]
  let Frames = 150

  /// B を 1 面 だけで回した並び
  let alone (seed: int) =
    let f = Playfield.Create (envOf seed) b
    [ for _ in 1 .. Frames do
        f.Tick()
        yield f.Count ]

  /// A と B を並べて回して、B の並びを返す。
  /// `share` なら env は 1 個（分けない側）
  let paired (seed: int) (share: bool) =
    let e1 = envOf seed
    let e2 = if share then e1 else envOf seed
    let f1 = Playfield.Create e1 a
    let f2 = Playfield.Create e2 b
    [ for _ in 1 .. Frames do
        // **同じコマで進める**（`Main.fs` の `tick` と同じ順）
        f1.Tick()
        f2.Tick()
        yield f2.Count ]

  [<Test>]
  member _.``別の env なら 1 面 のときと同じ``() =
    paired 4242 false |> should equal (alone 4242)

  [<Test>]
  member _.``env を共有すると走りが変わる``() =
    // **これが分ける理由。** 上の点は、そもそも乱数が効いていなくても緑になる
    paired 4242 true |> should not' (equal (alone 4242))

  [<Test>]
  member _.``2 面 目 も進む``() =
    // 上の 2 点 は、**2 面 目 が 1 コマ も進まなくても**
    // 「両方 空で一致」「両方 空で不一致にならない」で通り抜けうる
    List.max (paired 4242 false) |> should greaterThan 1

  [<Test>]
  member _.``1 面 目 も進む``() =
    let e1 = envOf 4242
    let f1 = Playfield.Create e1 a
    let counts =
      [ for _ in 1 .. Frames do
          f1.Tick()
          yield f1.Count ]
    List.max counts |> should greaterThan 1

  [<Test>]
  member _.``種を変えれば 2 面 目 も変わる``() =
    // 面を分けても**種は共通**（比べるのは弾幕であって走らせ方ではない）
    paired 1 false |> should not' (equal (paired 999 false))

  [<Test>]
  member _.``面の形は弾幕ごとに決まる``() =
    // 2 つ の面が別の向きを持てる。**env を分けた話とは別の軸**
    let horizontal =
      Bulletml.readXmlString (XmlB.Replace("type=\"vertical\"", "type=\"horizontal\""))
    let f1 = Playfield.Create (envOf 1) b
    let f2 = Playfield.Create (envOf 1) horizontal
    f1.Field |> should equal Stage.portrait
    f2.Field |> should equal Stage.landscape
