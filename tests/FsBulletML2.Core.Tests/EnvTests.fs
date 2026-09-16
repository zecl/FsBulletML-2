namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.Domain

/// Env はレコード 1 行で作れること。17 メンバの Fake を書かずに済むことが要点。
///
/// 「グローバルと弾から組んだ Env」を見ていた 2 本 は、ここから出した。
/// あれは `BulletRunner.envOfGlobal`（旧 API）に対する門だったが、測って
/// いたのは同梱フロント（MonoGame）の規約だった ——
///     Y の符号    -(py - y)。Unity2D は反転しない（座標系が逆）
[<TestFixture>]
type EnvTests() =

  [<Test>]
  member _.``Env はレコードリテラルで作れる``() =
    let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; Aim = { ToPlayer = 1.0f; ToEnemy = 2.0f }; Spawn = { ToPlayer = 0.f; ToEnemy = 0.f } }
    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f

  [<Test>]
  member _.``Rand は呼ぶたびに読み直される``() =
    let mutable n = 0
    let env = { Rand = (fun () -> n <- n + 1; float32 n); Rank = 0.f; Aim = { ToPlayer = 0.f; ToEnemy = 0.f }; Spawn = { ToPlayer = 0.f; ToEnemy = 0.f } }
    env.Rand () |> should equal 1.0f
    env.Rand () |> should equal 2.0f
