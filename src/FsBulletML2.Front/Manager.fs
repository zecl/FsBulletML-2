namespace FsBulletML2

/// フロントが乱数・ランク・自機の位置を Core へ渡すためのグローバルな口。
/// Core のエンジンはここを読まない。読むのは `Env` を組むフロント側。
type IBulletMLManager =
  abstract GetRandom : unit -> float32
  abstract GetRank : unit -> float32
  abstract GetPlayerPosX : unit -> float32
  abstract GetPlayerPosY : unit -> float32

/// 上の口を 1 つ だけ持つ static な入れ物。
/// static mutable なので、これを使う試験は並列に走らせられない。
type BulletMLManager () =
  static let mutable ib : IBulletMLManager =
    Microsoft.FSharp.Core.Operators.Unchecked.defaultof<IBulletMLManager>
  static member Init(ib1: IBulletMLManager) =
    ib <- ib1
  static member GetRandom() = ib.GetRandom()
  static member GetRank() = ib.GetRank()
  static member GetPlayerPosX() = ib.GetPlayerPosX()
  static member GetPlayerPosY() = ib.GetPlayerPosY()
