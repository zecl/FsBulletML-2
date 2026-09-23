namespace FsBulletML2.Sample.Unity2D.FSharp

open R3
// R3 のあとに開く。`Observable` はあとに開いたほうが勝つ。欲しいのは F# 側。
open FSharp.Control.R3
open UnityEngine
open FsBulletML2.Unity2D

type ParticleSortingLayer () =
  inherit MonoBehaviour ()

  /// `take 1` で切る。返したあとも毎コマ `Destroy` を呼び続けない。
  member this.Start () =
    let r = this.GetComponent<ParticleSystemRenderer>()
    r.sortingLayerName <- "Bomb"
    r.sortingOrder <- 2

    let ps = this.GetComponent<ParticleSystem>()
    FrameTicker.Frames
    |> Observable.filter (fun _ -> ps.IsAlive() |> not)
    |> Observable.take 1
    |> subscribeUntilDestroy this (fun _ -> InstanceManager.Destroy this.gameObject)
