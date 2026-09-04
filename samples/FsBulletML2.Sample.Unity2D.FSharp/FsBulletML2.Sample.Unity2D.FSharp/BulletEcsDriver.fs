namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Collections.Generic
open Unity.Collections
open Unity.Entities
open Unity.Mathematics
open Unity.Transforms
open UnityEngine
open FsBulletML2

/// 弾の Entity を毎コマ 回す。
///
/// **`SystemBase` を使っていないのは意図。** ECS の System は Roslyn の
/// source generator（`SystemGenerator.dll`）が partial クラスを生成する
/// 仕組みで、**F# コンパイラでは走らない**。`EntityManager` を直に触るぶんには
/// F# から普通に呼べるので、MonoBehaviour の Update から回している。
///
/// **性能はどちらでも同じ。** `BulletSim` は managed component（class）なので、
/// System で回しても Burst もジョブ化も効かない。C# サンプルは
/// `SystemBase` を使っているが、その `OnUpdate` も `SystemAPI` を使わずに
/// `ToEntityArray` して手で回す形で、中身はここと同じ。
///
/// 実行順は早めに寄せてある —— 弾を動かしてから当たり判定と描画に渡したいので。
[<DefaultExecutionOrder(-100)>]
type BulletEcsDriver () =
  inherit MonoBehaviour ()

  /// このコマで消す Entity。走査の途中で消すと列挙が壊れるので、いったん貯める
  let doomed = List<Entity>()
  /// 自機に触れている弾。**入った瞬間だけ**ダメージにするための控え
  let overlappingPlayer = HashSet<Entity>()
  let overlapScratch = List<Entity>()

  /// 撃たれた弾を実体にする関数。**毎コマ 作らない**（弾の数だけ確保が増える）。
  /// **F# は let 束縛を val より前に置く**ので、ここに居る
  let spawn = System.Action<BulletSim, BulletRun>(fun parent child ->
    BulletEntityFactory.SpawnChild(parent, child) |> ignore)

  [<DefaultValue>]val mutable private query : EntityQuery
  [<DefaultValue>]val mutable private hasQuery : bool
  /// ダメージの通知先。**Bootstrap が入れる。**
  /// BulletEcsRuntime は Transform しか持たない（型の輪を避けるため）ので、
  /// 型を知っているこちらで持つ
  [<DefaultValue>]val mutable public player : Player
  [<DefaultValue>]val mutable public enemy : Enemy

  static member private Overlaps (x: float32) (y: float32) (radius: float32)
                                 (target: Vector3) (targetRadius: float32) =
    let dx = x - target.x
    let dy = y - target.y
    let rr = radius + targetRadius
    (dx * dx) + (dy * dy) <= rr * rr

  member this.Update () =
    let world = World.DefaultGameObjectInjectionWorld
    if isNull (box world) || not BulletEntityFactory.IsReady then () else

    let em = world.EntityManager
    if not this.hasQuery then
      this.query <-
        em.CreateEntityQuery(
          [| ComponentType.ReadWrite typeof<BulletSim>
             ComponentType.ReadWrite typeof<LocalTransform>
             ComponentType.ReadOnly typeof<BulletTag> |])
      this.hasQuery <- true

    let playerT = BulletEcsRuntime.PlayerTransform
    let enemyT = BulletEcsRuntime.EnemyTransform
    let hasPlayer = not (isNull (box playerT))
    let hasEnemy = not (isNull (box enemyT))
    let playerPos = if hasPlayer then playerT.position else Vector3.zero
    let enemyPos = if hasEnemy then enemyT.position else Vector3.zero

    doomed.Clear()
    let entities = this.query.ToEntityArray(Allocator.Temp)
    try
      for i in 0 .. entities.Length - 1 do
        let entity = entities.[i]
        if em.Exists entity then
          let sim = em.GetComponentObject<BulletSim> entity
          let tag = em.GetComponentData<BulletTag> entity

          // 1 コマ 進める。撃たれた弾はその場で実体になる
          if sim.Used then sim.Step spawn

          // 描画へ位置を渡す。**向きは -Dir**（エンジンの角度は時計回り）
          let mutable tr = em.GetComponentData<LocalTransform> entity
          tr.Position <- float3(sim.X, sim.Y, 0.0f)
          tr.Rotation <- quaternion.AxisAngle(float3(0.0f, 0.0f, 1.0f), -sim.Dir)
          em.SetComponentData(entity, tr)

          let dead = not sim.Used || BulletEntityFactory.IsOffScreen sim.X sim.Y
          if dead then
            overlappingPlayer.Remove entity |> ignore
            doomed.Add entity
          elif tag.Kind = BulletKind.Enemy && hasPlayer then
            if BulletEcsDriver.Overlaps sim.X sim.Y tag.Radius playerPos BulletEcsRuntime.PlayerRadius then
              // **入った瞬間だけ**ダメージにする。触れ続けている間 減らさない
              if overlappingPlayer.Add entity && not (isNull (box this.player)) then
                this.player.HitByEnemyBullet()
              // 根の弾（弾幕の元）は当たっても消さない
              if not sim.Root then
                overlappingPlayer.Remove entity |> ignore
                doomed.Add entity
            else
              overlappingPlayer.Remove entity |> ignore
          elif tag.Kind = BulletKind.Player && hasEnemy then
            if BulletEcsDriver.Overlaps sim.X sim.Y tag.Radius enemyPos BulletEcsRuntime.EnemyRadius then
              if not (isNull (box this.enemy)) then this.enemy.HitByPlayerBullet()
              doomed.Add entity
    finally
      entities.Dispose()

    for e in doomed do BulletEntityFactory.Destroy e

    // 消えた Entity を控えから外す。**放っておくと際限なく溜まる**
    if overlappingPlayer.Count > 0 then
      overlapScratch.Clear()
      for e in overlappingPlayer do
        if not (em.Exists e) then overlapScratch.Add e
      for e in overlapScratch do overlappingPlayer.Remove e |> ignore

/// Play の頭で場面を組む。**シーンに置く必要はない** —— 居なければ自分で作る。
///
/// やることは 3 つ。
///   1. 自機と敵の Transform を BulletEcsRuntime へ（毎コマ 探さないため）
///   2. ダメージの通知先を Driver へ
///   3. 弾の見た目を BulletEntityFactory へ（prefab の SpriteRenderer から）
type BulletEcsBootstrap () =
  inherit MonoBehaviour ()

  [<RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)>]
  static member AutoCreate () =
    let found = UnityEngine.Object.FindAnyObjectByType<BulletEcsBootstrap>()
    if isNull (box found) then
      let go = new GameObject("BulletEcsBootstrap")
      UnityEngine.Object.DontDestroyOnLoad go
      go.AddComponent<BulletEcsBootstrap>() |> ignore

  member this.Awake () = this.Configure ()

  member this.Configure () =
    let player = UnityEngine.Object.FindAnyObjectByType<Player>()
    let enemy = UnityEngine.Object.FindAnyObjectByType<Enemy>()
    let hasPlayer = not (isNull (box player))
    let hasEnemy = not (isNull (box enemy))

    BulletEcsRuntime.PlayerTransform <- if hasPlayer then player.transform else null
    BulletEcsRuntime.EnemyTransform <- if hasEnemy then enemy.transform else null

    // 当たり判定の半径は Collider から取る。無ければ既定のまま
    if hasPlayer then
      let col = player.GetComponent<Collider2D>()
      if not (isNull (box col)) then
        let e = col.bounds.extents
        BulletEcsRuntime.PlayerRadius <- max 0.05f (max e.x e.y)
    if hasEnemy then
      let col = enemy.GetComponent<Collider2D>()
      if not (isNull (box col)) then
        let e = col.bounds.extents
        BulletEcsRuntime.EnemyRadius <- max 0.08f (max e.x e.y)

    // 弾の見た目は prefab の SpriteRenderer から取る。
    // **prefab はもう実体化しない**（弾は Entity）が、見本としては残っている
    let spriteOf (go: GameObject) =
      if isNull (box go) then null else go.GetComponent<SpriteRenderer>()
    let enemySr = if hasEnemy then spriteOf enemy.bulletObject else null
    let playerSr = if hasPlayer then spriteOf player.bulletObject else null
    BulletEntityFactory.Configure(enemySr, playerSr)

    // 弾を回す本体。**この GameObject に付ける**（自分で作った場合も含む）。
    // F# の型に null は入れられないので、パターン照合ではなく isNull で見る
    let existing = this.GetComponent<BulletEcsDriver>()
    let driver =
      if isNull (box existing) then this.gameObject.AddComponent<BulletEcsDriver>()
      else existing
    driver.player <- player
    driver.enemy <- enemy
