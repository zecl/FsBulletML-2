namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.Domain

/// Env はレコード 1 行で作れること。17 メンバの Fake を書かずに済むことが要点。
///
/// **「グローバルと弾から組んだ Env」を見ていた 2 本 は、ここから出した。**
/// あれは `BulletRunner.envOfGlobal`（旧 API）に対する門だったが、測って
/// いたのは**同梱フロント（MonoGame）の規約**だった ——
///
///     Y の符号    -(py - y)。**Unity2D は反転しない**（座標系が逆）
///     Spawn の元   原点。**Unity2D は撃った側と同じ場所**なので AimDir と同値
///
/// つまり Core の門ではなかった。式の在る場所（`FsBulletML2.MonoGame` の
/// `FrontEnv`）へ移し、門も `tests/FsBulletML2.MonoGame.Tests/EnvGate.fs` へ
/// 置いた。**あちらは InternalsVisibleTo に入っていないので、
/// 「公開だけで書けているか」も同時に測る。**
[<TestFixture>]
type EnvTests() =

  [<Test>]
  member _.``Env はレコードリテラルで作れる``() =
    let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; AimDir = 1.0f; EnemyAimDir = 2.0f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }
    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f

  [<Test>]
  member _.``Rand は呼ぶたびに読み直される``() =
    let mutable n = 0
    let env = { Rand = (fun () -> n <- n + 1; float32 n); Rank = 0.f; AimDir = 0.f; EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }
    env.Rand () |> should equal 1.0f
    env.Rand () |> should equal 2.0f
