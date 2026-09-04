namespace FsBulletML2.Sample.Unity2D.FSharp

open R3
// **R3 のあとに開くこと。** どちらにも `Observable` があり、あとに開いたほうが
// 勝つ。ここで欲しいのは F# 側（`Observable.filter` などのパイプライン関数）
open FSharp.Control.R3
open UnityEngine
open FsBulletML2.Unity2D

type ParticleSortingLayer () =
  inherit MonoBehaviour ()

  /// 爆風が消え終わったらプールへ返す。
  ///
  /// **`take 1` を付けてある。** 旧は毎コマ `IsAlive` を見て、消えていたら
  /// `Destroy` を呼んでいた —— **返したあとも毎コマ 呼び続ける形**だった。
  /// 落ちはしない（プールへ返すだけ）が、いちど 流れたら購読ごと切れるのが正しい
  member this.Start () =
    let r = this.GetComponent<ParticleSystemRenderer>()
    r.sortingLayerName <- "Bomb"
    r.sortingOrder <- 2

    let ps = this.GetComponent<ParticleSystem>()
    FrameTicker.Frames
    |> Observable.filter (fun _ -> ps.IsAlive() |> not)
    |> Observable.take 1
    |> subscribeUntilDestroy this (fun _ -> InstanceManager.Destroy this.gameObject)
