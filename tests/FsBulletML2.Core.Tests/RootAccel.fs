namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.DTD
open FsBulletML2.Domain

/// final review 2: 根の accel は、木を組む段の placeholder が
/// first = false（term = 1.f）であり、根の弾は Init を一度も通らない
/// （設計文書 5.3「走らせ直す」）ので、この first = false のまま
/// 最初のフレームへ入る。changeDirection / changeSpeed は placeholder が
/// first = true なので、Init を経ないまま入っても「まだ評価前」と一致するが、
/// accel だけは「もう評価済み」を意味する逆の初期状態を持つ。
///
/// rootProgress が Accel を素通りして Progress.initial（= first = true と
/// 同じ「まだ評価前」の状態）を返すと、根の accel が実際に評価されて
/// 本物の加速度になる。旧は 2 フレームで term を使い切って何も足さずに
/// 終わる no-op だった。
[<TestFixture>]
type RootAccel() =

  let env = { Rand = (fun () -> 0.5f); Rank = 0.5f; AimDir = 0.f; EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }

  let noResolvers : Step.Resolvers =
    { Bullet = (fun _ _ -> None); Action = fun _ _ -> None }

  let stateWith tops =
    { Pos = { X = 0.f; Y = 0.f }
      Speed = 0.f
      Dir = 0.f
      Accel = { X = 0.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = tops }

  // 4077ed6 の accelCommand を first = false で読んだときの結果
  // （設計文書 5.3 に書いた手計算）。horizontal absolute 2、vertical
  // absolute 1、term 5 と書いてあっても、木を組む段の placeholder
  // term = 1.f が Init されるまで残るので、term も horizontal / vertical も
  // 一度も読まれない
  let accelScript =
    RecBulletml.Accel (
      Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, numExpr "2")),
      Some (Vertical (Some { verticalType = VerticalType.Absolute }, numExpr "1")),
      Term (numExpr "5"))

  [<Test>]
  member _.``rootProgress は accel を、first = false ＝ もう評価済みの状態で組む``() =
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    match Step.rootProgress counting accelScript with
    | PAccel (started, left, dx, dy) ->
        started |> should equal true
        left |> should (equalWithin 0.0001) 1.0f
        dx |> should equal 0.0f
        dy |> should equal 0.0f
    | other -> Assert.Fail (sprintf "PAccel のはずが %A" other)
    // 木を組む段の placeholder をそのまま置くだけで、horizontal / vertical /
    // term のどれも読まない（旧の convertRecBulletmlEx の Accel 腕と同じ）
    draws |> should equal 0

  [<Test>]
  member _.``根の accel は、1 度も評価されずに 2 コマで終わる 2 フレームの no-op``() =
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let p0 = Step.rootProgress counting accelScript
    let (r1, p1), st1, _ = Sim.run counting (stateWith []) (Step.accel accelScript p0)
    r1 |> should equal Step.Continue
    st1.Accel |> should equal { X = 0.0f; Y = 0.0f }
    let (r2, _), st2, _ = Sim.run counting st1 (Step.accel accelScript p1)
    r2 |> should equal Step.Ended
    st2.Accel |> should equal { X = 0.0f; Y = 0.0f }
    // horizontal "2" / vertical "1" / term "5" のどれかでも読んでいれば
    // ここが 0 のままでは済まない
    draws |> should equal 0

  /// Step.step（BulletRunner.run が実際に呼ぶのと同じ入口）を通して、
  /// 根の top* の中に直接書いた accel が、3 コマとも軌跡へ何も足さないことを見る
  [<Test>]
  member _.``top 直下の accel は、3 コマ動いても軌跡を動かさない``() =
    let top = RecBulletml.Action ({ actionLabel = Some (ActionLabel "top") }, [ accelScript; RecBulletml.Wait (numExpr "20") ])
    let p0 = Step.rootProgress env top
    let st0 = stateWith [ top, p0, FireContext.zero ]
    let r1 = Step.step noResolvers env st0
    r1.Delta |> should equal { X = 0.0f; Y = 0.0f }
    let r2 = Step.step noResolvers env r1.State
    r2.Delta |> should equal { X = 0.0f; Y = 0.0f }
    let r3 = Step.step noResolvers env r2.State
    r3.Delta |> should equal { X = 0.0f; Y = 0.0f }

  /// 較正: rootProgress の Accel の腕を外す（Progress.initial に戻す）と
  /// どうなるかを、同じテストファイルの中で実際に組んで見せる。
  /// 上のテストが緑のまま、こちらだけが割れた値を返すことで、
  /// 「直さなければ何が起きていたか」を数値で残す。
  ///
  /// 現物のレビューで確かめた HEAD の数（f00 x=0.400 y=0.200、
  /// f01 x=1.200 y=0.600、f02 x=2.400 y=1.200）と一致する
  [<Test>]
  member _.``較正: Progress.initial のままだと、根の accel が本物の加速度になる``() =
    let top = RecBulletml.Action ({ actionLabel = Some (ActionLabel "top") }, [ accelScript; RecBulletml.Wait (numExpr "20") ])
    // rootProgress の代わりに Progress.initial で組む。accel は
    // PAccel (false, 0, 0, 0) になり、first = true（まだ評価前）と
    // 同じ扱いで最初のフレームに本当に評価される
    let buggyProgress = Progress.initial top
    let st0 = stateWith [ top, buggyProgress, FireContext.zero ]
    let r1 = Step.step noResolvers env st0
    r1.Delta.X |> should (equalWithin 0.0001) 0.400f
    r1.Delta.Y |> should (equalWithin 0.0001) 0.200f
    let r2 = Step.step noResolvers env r1.State
    let x1 = r1.Delta.X + r2.Delta.X
    let y1 = r1.Delta.Y + r2.Delta.Y
    x1 |> should (equalWithin 0.0001) 1.200f
    y1 |> should (equalWithin 0.0001) 0.600f
    let r3 = Step.step noResolvers env r2.State
    let x2 = x1 + r3.Delta.X
    let y2 = y1 + r3.Delta.Y
    x2 |> should (equalWithin 0.0001) 2.400f
    y2 |> should (equalWithin 0.0001) 1.200f
