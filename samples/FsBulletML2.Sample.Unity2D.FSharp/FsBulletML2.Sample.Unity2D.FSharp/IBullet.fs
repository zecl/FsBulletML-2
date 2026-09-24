namespace FsBulletML2.Sample.Unity2D.FSharp

type IBullet =
    abstract GetDefaultBullet: unit -> FsBulletML2.Unity2D.IDefaultBullet
