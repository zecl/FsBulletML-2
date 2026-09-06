namespace FsBulletML2.Sample.Unity2D.FSharp

open UnityEngine

/// 爆発の見た目。
///
/// **GameObject を毎回 作らない。** 旧はここで `InstanceManager.InstantiatePrefab`
/// を呼んでいて、敵と自機が落ちるたびに GameObject が 1 個 ずつ場に残っていた
/// （プールの tag が合わないので毎回 `Instantiate` に落ちる）。
/// **C# のサンプルはすでにこの形**で、あちらでは起きていなかった。
///
/// いま起こすのは `ParticleSystem` 1 個 だけ。あとはそこから粒子を出す。
///
/// **音は鳴らさない。** このサンプルには音を出す仕組みがそもそも無い
/// （C# 側は `AudioManager.PlaySE` を持っているので、あちらは鳴る）。
[<AbstractClass; Sealed>]
type Bomb private () =

  /// 1 回 の爆発で出す粒の数
  static let emitCount = 12

  /// 起こした入れ物。**シーンをまたいで 1 個。**
  static let mutable ps : ParticleSystem = null

  /// prefab から 1 個 だけ起こす。2 回目 以降は何もしない
  static member private Ensure (bombType: GameObject) =
    if isNull (box ps) && not (isNull (box bombType)) then
      let go = Object.Instantiate<GameObject> bombType
      go.name <- "BombVfxPool"
      Object.DontDestroyOnLoad go

      // prefab に付いている「1 発 ぶんの見せ方」は切る。
      // ここは撃たれるたびに Emit する入れ物で、自分では並べ替えないし鳴らない
      let sorting = go.GetComponent<ParticleSortingLayer>()
      if not (isNull (box sorting)) then sorting.enabled <- false
      let sound = go.GetComponent<AudioSource>()
      if not (isNull (box sound)) then
        sound.enabled <- false
        sound.playOnAwake <- false

      ps <- go.GetComponent<ParticleSystem>()
      if not (isNull (box ps)) then
        // **World にする。** 入れ物は動かないので、Local だと粒が原点から出る
        let mutable main = ps.main
        main.playOnAwake <- false
        main.loop <- false
        main.simulationSpace <- ParticleSystemSimulationSpace.World
        main.stopAction <- ParticleSystemStopAction.None
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear)
        go.SetActive true

  static member GenerateBomb (bombType: GameObject, position: Vector3) =
    Bomb.Ensure bombType
    if not (isNull (box ps)) then
      let mutable emit = ParticleSystem.EmitParams()
      emit.position <- position
      emit.applyShapeToPosition <- true
      ps.Emit(emit, emitCount)
