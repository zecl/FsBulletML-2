namespace FsBulletML2.Sample.Unity2D.FSharp

open UnityEngine

/// 爆発の見た目。
///
/// GameObject を作らない。 旧はここで `InstanceManager.InstantiatePrefab`
/// を呼んでプールから取っていた。そのプールは起動時に `cacheSize` ぶんの
/// `bomb0` `bomb1` ... を作るので、1 発 も撃たないうちから場に並ぶ。
/// `Bomb` をこの形にして初めて、プールから外せる
/// （`ObjectData.IsBulletPrefab` の但し書き）。
[<AbstractClass; Sealed>]
type Bomb private () =

  /// 1 回 の爆発で出す粒の数
  static let emitCount = 12

  /// 音を鳴らす間隔。これより短い連打は鳴らさない
  static let seCooldown = 0.12f

  /// 起こした入れ物。シーンをまたいで 1 個。
  static let mutable ps : ParticleSystem = null
  static let mutable sound : AudioSource = null
  static let mutable lastSe = -999.0f

  /// prefab から 1 個 だけ起こす。2 回目 以降は何もしない
  static member private Ensure (bombType: GameObject) =
    if isNull (box ps) && not (isNull (box bombType)) then
      let go = Object.Instantiate<GameObject> bombType
      go.name <- "BombVfxPool"
      Object.DontDestroyOnLoad go

      // 並べ替えは切る。ここは撃たれるたびに Emit する入れ物で、
      // 自分では並べ替えない
      let sorting = go.GetComponent<ParticleSortingLayer>()
      if not (isNull (box sorting)) then sorting.enabled <- false

      // 音は残す。 ただし起きた瞬間に鳴らないよう playOnAwake は切る
      sound <- go.GetComponent<AudioSource>()
      if not (isNull (box sound)) then
        sound.playOnAwake <- false

      ps <- go.GetComponent<ParticleSystem>()
      if not (isNull (box ps)) then
        // World にする。 入れ物は動かないので、Local だと粒が原点から出る
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
      if not (isNull (box sound)) && Time.unscaledTime - lastSe >= seCooldown then
        lastSe <- Time.unscaledTime
        sound.Play()
