namespace FsBulletML2.Sample.Unity2D.FSharp

open UnityEngine
open FsBulletML2.Unity2D 

type Bomb =
  static member GenerateBomb(bombType:GameObject, position:Vector3) = 
    InstanceManager.InstantiatePrefab (bombType, position, Quaternion.identity) |> ignore
