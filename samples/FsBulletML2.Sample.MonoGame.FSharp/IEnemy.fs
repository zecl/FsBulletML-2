namespace FsBulletML2.Sample.MonoGame.FSharp

open FsBulletML2.MonoGame

type IEnemy =
    inherit IBullet
    abstract Life: int32 with get, set
    abstract Shoot: unit -> unit
