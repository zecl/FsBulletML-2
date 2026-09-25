namespace FsBulletML2.MonoGame.Tests

open FsBulletML2
open FsBulletML2.MonoGame

/// 位置 と半径 だけ 持つ弾。オブジェクト式には member val を書けないので型にしてある
type internal StubBullet(x: float32, y: float32) =
    let mutable pos = Microsoft.Xna.Framework.Vector2(x, y)

    interface IBullet with
        member _.Update() = ()

        member _.Pos
            with get () = pos
            and set v = pos <- v

        member _.X
            with get () = pos.X
            and set v = pos <- Microsoft.Xna.Framework.Vector2(v, pos.Y)

        member _.Y
            with get () = pos.Y
            and set v = pos <- Microsoft.Xna.Framework.Vector2(pos.X, v)

        member val Radius = 1.0f with get, set
        member val Speed = 0.0f with get, set
        member val Dir = 0.0f with get, set
        member val AccelerationX = 0.0f with get, set
        member val AccelerationY = 0.0f with get, set
        member val Used = true with get, set
        member val IsBullet = false with get, set
        member val BulletType = BulletType.Enemy with get, set
        member val ShootingDirection = ShootingDirection.BulletVertical with get, set
        member _.Init() = ()
        member _.Vanish() = ()
        member _.SetScript(_) = ()
        member _.SetRun(_) = ()
        member _.Script = None
        member _.Finished = false
