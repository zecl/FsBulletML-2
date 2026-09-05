namespace FsBulletML2

/// フロントが「いまの乱数・ランク・自機の位置」を Core へ渡すための
/// グローバルな口。**Core のエンジンはここを読まない。**
///
/// 読むのはフロント側で、`Env` を組むときに使う（`FsBulletML2.MonoGame` の
/// `IBullet.envOfGlobal` と `FsBulletML2.Unity2D` の同じ関数）。
/// 新 API（`Runner.load` / `Runner.stepWith`）は `Env` を引数で受け取るので、
/// Core からグローバルを引く経路は無い。
///
/// **以前は `Processable` モジュールの中に居た。** あそこは旧 API
/// （`IBulletmlObject` / `BulletmlTask`）のシムで、**その 2 つ は落とした。**
/// こちらは廃止後も残るものなので、先に外へ出して namespace 直下に置いてある
/// —— C# から見える名前も `FsBulletML2.Processable.BulletMLManager` から
/// `FsBulletML2.BulletMLManager` になり、入れ子が 1 段 減る。
type IBulletMLManager =
  abstract GetRandom : unit -> float32
  abstract GetRank : unit -> float32
  abstract GetPlayerPosX : unit -> float32
  abstract GetPlayerPosY : unit -> float32

/// 上の口を 1 つ だけ持つ static な入れ物。
///
/// **static mutable なので、これを使う試験は並列に走らせられない**
/// （Core のテストに `NonParallelizable` が残っているのはこれが理由）。
/// 新 API だけで書いた試験は `Env` を直に組めるので、その縛りが要らない。
type BulletMLManager () =
  static let mutable ib : IBulletMLManager =
    Microsoft.FSharp.Core.Operators.Unchecked.defaultof<IBulletMLManager>
  static member Init(ib1: IBulletMLManager) =
    ib <- ib1
  static member GetRandom() = ib.GetRandom()
  static member GetRank() = ib.GetRank()
  static member GetPlayerPosX() = ib.GetPlayerPosX()
  static member GetPlayerPosY() = ib.GetPlayerPosY()
