namespace FsBulletML2.Unity2D.Tests

open UnityEngine
open FsBulletML2
open FsBulletML2.Unity2D

/// 位置 と半径 だけ 持つ弾。オブジェクト式には member val を書けないので型にしてある
type internal StubBullet(x: float32, y: float32) =
    let mutable pos = Vector3(x, y, 0.0f)

    interface IDefaultBullet with
        member _.Update() = ()

        member _.Pos
            with get () = pos
            and set v = pos <- v

        member _.X
            with get () = pos.x
            and set v = pos <- Vector3(v, pos.y, pos.z)

        member _.Y
            with get () = pos.y
            and set v = pos <- Vector3(pos.x, v, pos.z)

        member val Radius = 1.0f with get, set
        member val Root = false with get, set
        member val Speed = 0.0f with get, set
        member val Dir = 0.0f with get, set
        member val AccelerationX = 0.0f with get, set
        member val AccelerationY = 0.0f with get, set
        member val Used = true with get, set
        member val IsBullet = false with get, set
        member val BulletRoot = false with get, set
        member val BulletType = BulletType.Enemy with get, set
        member val ShootingDirection = ShootingDirection.BulletVertical with get, set
        member _.Init() = ()
        member _.Vanish() = ()
        member _.SetScript(_) = ()
        member _.SetRun(_) = ()
        member _.Script = None
        member _.Finished = false
