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
[<DefaultExecutionOrder(-100)>]
type BulletEcsDriver() =
    inherit MonoBehaviour()

    /// このコマで消す Entity。走査の途中で消すと列挙が壊れるので、いったん貯める
    let doomed = List<Entity>()
    /// 自機に触れている弾。入った瞬間だけダメージにするための控え
    let overlappingPlayer = HashSet<Entity>()
    let overlapScratch = List<Entity>()

    /// 撃たれた弾を実体にする関数。毎コマ 作らない（弾の数だけ確保が増える）。
    /// F# は let 束縛を val より前に置くので、ここに居る
    let spawn =
        System.Action<BulletSim, BulletRun>(fun parent child -> BulletEntityFactory.SpawnChild(parent, child) |> ignore)

    [<DefaultValue>]
    val mutable private query: EntityQuery

    [<DefaultValue>]
    val mutable private hasQuery: bool

    /// 初回の 1 コマ だけログを出すための印。
    /// 毎コマ 出すと弾の数だけ行が流れて Console が使えなくなる
    [<DefaultValue>]
    val mutable private logged: bool

    /// ダメージの通知先。
    [<DefaultValue>]
    val mutable public player: Player

    [<DefaultValue>]
    val mutable public enemy: Enemy

    static member private Overlaps
        (x: float32)
        (y: float32)
        (radius: float32)
        (target: Vector3)
        (targetRadius: float32)
        =
        let dx = x - target.x
        let dy = y - target.y
        let rr = radius + targetRadius
        (dx * dx) + (dy * dy) <= rr * rr

    member this.Update() =
        let world = World.DefaultGameObjectInjectionWorld

        if isNull (box world) || not BulletEntityFactory.IsReady then
            ()
        else

            let em = world.EntityManager

            if not this.hasQuery then
                this.query <-
                    em.CreateEntityQuery(
                        [|
                            ComponentType.ReadWrite typeof<BulletSim>
                            ComponentType.ReadWrite typeof<LocalTransform>
                            ComponentType.ReadOnly typeof<BulletTag>
                        |]
                    )

                this.hasQuery <- true

            let playerT = BulletEcsRuntime.PlayerTransform
            let enemyT = BulletEcsRuntime.EnemyTransform
            let hasPlayer = not (isNull (box playerT))
            let hasEnemy = not (isNull (box enemyT))
            let playerPos = if hasPlayer then playerT.position else Vector3.zero
            let enemyPos = if hasEnemy then enemyT.position else Vector3.zero

            doomed.Clear()
            let entities = this.query.ToEntityArray(Allocator.Temp)

            // 初回だけ、回っていることと画面の範囲を出す。
            // 「弾が変な位置に残る」を追うとき、Driver が回っていないのか 消す範囲がずれているのかを、画面からは区別できない
            if not this.logged then
                this.logged <- true

                Debug.Log(
                    sprintf
                        "BulletEcsDriver: 回り始めた。弾 %d 個。消す範囲 x=%f..%f y=%f..%f。自機=%b 敵=%b"
                        entities.Length
                        BulletEntityFactory.ScreenMinX
                        BulletEntityFactory.ScreenMaxX
                        BulletEntityFactory.ScreenMinY
                        BulletEntityFactory.ScreenMaxY
                        hasPlayer
                        hasEnemy
                )

            try
                for i in 0 .. entities.Length - 1 do
                    let entity = entities.[i]

                    if em.Exists entity then
                        let sim = em.GetComponentObject<BulletSim> entity
                        let tag = em.GetComponentData<BulletTag> entity

                        // 1 コマ 進める。撃たれた弾はその場で実体になる
                        if sim.Used then
                            sim.Step spawn

                        // 描画へ位置を渡す。向きは -Dir（エンジンの角度は時計回り）
                        let pos = float3 (sim.X, sim.Y, 0.0f)
                        let rot = quaternion.AxisAngle(float3 (0.0f, 0.0f, 1.0f), -sim.Dir)
                        let mutable tr = em.GetComponentData<LocalTransform> entity
                        tr.Position <- pos
                        tr.Rotation <- rot
                        em.SetComponentData(entity, tr)

                        // LocalToWorld も自分で書く。
                        if em.HasComponent<LocalToWorld> entity then
                            let mutable ltw = LocalToWorld()
                            ltw.Value <- float4x4.TRS(pos, rot, float3 (1.0f, 1.0f, 1.0f))
                            em.SetComponentData(entity, ltw)

                        let dead = not sim.Used || BulletEntityFactory.IsOffScreen sim.X sim.Y

                        if dead then
                            overlappingPlayer.Remove entity |> ignore
                            doomed.Add entity
                        elif tag.Kind = BulletKind.Enemy && hasPlayer then
                            if
                                BulletEcsDriver.Overlaps sim.X sim.Y tag.Radius playerPos BulletEcsRuntime.PlayerRadius
                            then
                                // 入った瞬間だけダメージにする。触れ続けている間 減らさない
                                if overlappingPlayer.Add entity && not (isNull (box this.player)) then
                                    this.player.HitByEnemyBullet()
                                // 根の弾（弾幕の元）は当たっても消さない
                                if not sim.Root then
                                    overlappingPlayer.Remove entity |> ignore
                                    doomed.Add entity
                            else
                                overlappingPlayer.Remove entity |> ignore
                        elif tag.Kind = BulletKind.Player && hasEnemy then
                            if
                                BulletEcsDriver.Overlaps sim.X sim.Y tag.Radius enemyPos BulletEcsRuntime.EnemyRadius
                            then
                                if not (isNull (box this.enemy)) then
                                    this.enemy.HitByPlayerBullet()

                                doomed.Add entity
            finally
                entities.Dispose()

            for e in doomed do
                BulletEntityFactory.Destroy e

            // 消えた Entity を控えから外す。放っておくと際限なく溜まる
            if overlappingPlayer.Count > 0 then
                overlapScratch.Clear()

                for e in overlappingPlayer do
                    if not (em.Exists e) then
                        overlapScratch.Add e

                for e in overlapScratch do
                    overlappingPlayer.Remove e |> ignore

/// Play の頭で場面を組む。
type BulletEcsBootstrap() =
    inherit MonoBehaviour()

    [<RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)>]
    static member AutoCreate() =
        // Play 中でなければ何もしない。
        if Application.isPlaying then
            let found = UnityEngine.Object.FindAnyObjectByType<BulletEcsBootstrap>()

            if isNull (box found) then
                let go = new GameObject("BulletEcsBootstrap")
                UnityEngine.Object.DontDestroyOnLoad go
                go.AddComponent<BulletEcsBootstrap>() |> ignore
                // ここが出ないなら、この属性が Unity に拾われていない。
                Debug.Log "BulletEcsBootstrap: AutoCreate で作った"

    member this.Awake() = this.Configure()

    /// F# を外でビルドした dll には codegen が掛からず、TypeManager が型を知らない。
    /// 例外文の `GetOrCreateTypeIndex` は 6.5.0 には無い。
    member this.Configure() =
        // シーンは Built-in の前提のままなので、URP で描ける形に
        // 直さないと自機も敵も背景も出ない（実際に真っ暗になった）
        UrpPlayModeCompat.Apply()

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
        // prefab はもう実体化しない（弾は Entity）が、見本としては残っている
        let spriteOf (go: GameObject) =
            if isNull (box go) then
                null
            else
                go.GetComponent<SpriteRenderer>()

        let enemySr = if hasEnemy then spriteOf enemy.bulletObject else null
        let playerSr = if hasPlayer then spriteOf player.bulletObject else null
        BulletEntityFactory.Configure(enemySr, playerSr)

        // 弾を回す本体。この GameObject に付ける（自分で作った場合も含む）。
        // F# の型に null は入れられないので、パターン照合ではなく isNull で見る
        let existing = this.GetComponent<BulletEcsDriver>()

        let driver =
            if isNull (box existing) then
                this.gameObject.AddComponent<BulletEcsDriver>()
            else
                existing

        driver.player <- player
        driver.enemy <- enemy

        // どこで切れているかを 1 行 で読めるようにする。
        // 弾が出ないとき、原因は「World が無い」「Configure が届いていない」 「自機か敵が見つからない」のどれか。
        let world = World.DefaultGameObjectInjectionWorld

        Debug.Log(
            sprintf
                "BulletEcsBootstrap: world=%s ready=%b player=%b enemy=%b"
                (if isNull (box world) then "無し" else world.Name)
                BulletEntityFactory.IsReady
                hasPlayer
                hasEnemy
        )
