namespace FsBulletML2.Sample.Unity2D.FSharp

open Unity.Collections
open Unity.Entities
open Unity.Mathematics
open Unity.Rendering
open Unity.Transforms
open UnityEngine
open UnityEngine.Rendering
open FsBulletML2

/// 弾の Entity を作る・消す。
///
/// **描画は Entities Graphics。** 弾ごとに mesh と material を持たせず、
/// `RenderMeshArray` に 2 組（敵・自機）だけ入れて index で指す。
/// これで同じ material の弾がインスタンシングでまとめて描かれる。
[<AbstractClass; Sealed>]
type BulletEntityFactory private () =

  static let mutable ready = false
  static let mutable renderMeshArray = Unchecked.defaultof<RenderMeshArray>
  static let mutable renderDesc = Unchecked.defaultof<RenderMeshDescription>
  static let mutable enemyRadius = 0.1f
  static let mutable playerRadius = 0.1f
  static let mutable enemyCount = 0
  static let mutable playerCount = 0

  /// 画面の外へ出たら消す範囲。シーンのカメラに合わせてある
  static member val ScreenMinX = 0.0f with get
  static member val ScreenMaxX = 4.8f with get
  static member val ScreenMinY = -6.4f with get
  static member val ScreenMaxY = 0.0f with get

  static member IsReady = ready
  static member EnemyCount = enemyCount
  static member PlayerCount = playerCount

  /// sprite から mesh を作る。sprite が無ければ小さい四角で代用する
  static member private MeshOfSprite (sprite: Sprite) (name: string) =
    let mesh = new Mesh(name = name)
    if not (isNull (box sprite)) && not (isNull sprite.vertices) && sprite.vertices.Length > 0 then
      let src = sprite.vertices
      let verts = Array.init src.Length (fun i -> Vector3(src.[i].x, src.[i].y, 0.0f))
      mesh.SetVertices verts
      mesh.SetUVs(0, sprite.uv)
      mesh.SetTriangles(Array.map int sprite.triangles, 0)
    else
      let h = 0.08f
      mesh.SetVertices [| Vector3(-h, -h, 0.0f); Vector3(h, -h, 0.0f)
                          Vector3(-h, h, 0.0f); Vector3(h, h, 0.0f) |]
      mesh.SetUVs(0, [| Vector2(0.0f, 0.0f); Vector2(1.0f, 0.0f)
                        Vector2(0.0f, 1.0f); Vector2(1.0f, 1.0f) |])
      // カメラは z=-10 から +Z を見るので、-Z 向きに巻く
      mesh.SetTriangles([| 0; 1; 2; 1; 3; 2 |], 0)
    mesh.RecalculateBounds()
    mesh.RecalculateNormals()
    mesh

  /// 透過で描く設定。URP の Unlit を前提にする
  static member private ConfigureTransparent (mat: Material) =
    if mat.HasProperty "_Surface" then mat.SetFloat("_Surface", 1.0f)
    if mat.HasProperty "_Blend" then mat.SetFloat("_Blend", 0.0f)
    if mat.HasProperty "_SrcBlend" then mat.SetFloat("_SrcBlend", float32 BlendMode.SrcAlpha)
    if mat.HasProperty "_DstBlend" then mat.SetFloat("_DstBlend", float32 BlendMode.OneMinusSrcAlpha)
    if mat.HasProperty "_ZWrite" then mat.SetFloat("_ZWrite", 0.0f)
    if mat.HasProperty "_Cull" then mat.SetFloat("_Cull", 0.0f)
    mat.SetOverrideTag("RenderType", "Transparent")
    mat.EnableKeyword "_SURFACE_TYPE_TRANSPARENT"
    mat.DisableKeyword "_ALPHAPREMULTIPLY_ON"
    mat.renderQueue <- int RenderQueue.Transparent
    mat.enableInstancing <- true

  /// sprite から material を作る。**シェーダは URP を順に探す** ——
  /// 見つからないと弾がピンクになるので、代わりを何段か置いてある
  static member private MaterialOfSprite (sr: SpriteRenderer) (name: string) =
    let texture =
      if not (isNull (box sr)) && not (isNull (box sr.sprite)) then sr.sprite.texture
      else Texture2D.whiteTexture
    let color = if isNull (box sr) then Color.white else sr.color
    let shader =
      [ "Universal Render Pipeline/Unlit"
        "Universal Render Pipeline/Unlit/Unlit"
        "Unlit/Transparent"
        "Sprites/Default"
        "Hidden/InternalErrorShader" ]
      |> List.tryPick (fun n -> match Shader.Find n with | null -> None | s -> Some s)
    let mat = new Material(Option.toObj shader, name = name)
    if mat.HasProperty "_BaseMap" then mat.SetTexture("_BaseMap", texture)
    else mat.mainTexture <- texture
    if mat.HasProperty "_BaseColor" then mat.SetColor("_BaseColor", color)
    elif mat.HasProperty "_Color" then mat.SetColor("_Color", color)
    BulletEntityFactory.ConfigureTransparent mat
    mat

  /// 弾の見た目を組む。**Play の頭で 1 回 だけ。**
  /// 呼ばずに Spawn すると描画コンポーネントが付かず、弾が見えない
  static member Configure (enemySprite: SpriteRenderer, playerSprite: SpriteRenderer) =
    let enemyMesh = BulletEntityFactory.MeshOfSprite (if isNull (box enemySprite) then null else enemySprite.sprite) "g_bullet_s_ecs"
    let playerMesh = BulletEntityFactory.MeshOfSprite (if isNull (box playerSprite) then null else playerSprite.sprite) "p_bullet_s_ecs"
    let enemyMat = BulletEntityFactory.MaterialOfSprite enemySprite "g_bullet_s_ecs"
    let playerMat = BulletEntityFactory.MaterialOfSprite playerSprite "p_bullet_s_ecs"

    renderMeshArray <- new RenderMeshArray([| enemyMat; playerMat |], [| enemyMesh; playerMesh |])
    renderDesc <-
      RenderMeshDescription(
        shadowCastingMode = ShadowCastingMode.Off,
        receiveShadows = false,
        motionVectorGenerationMode = MotionVectorGenerationMode.Camera,
        layer = 0)

    if not (isNull (box enemySprite)) && not (isNull (box enemySprite.sprite)) then
      let b = enemySprite.sprite.bounds.extents
      enemyRadius <- max 0.04f (max b.x b.y)
    if not (isNull (box playerSprite)) && not (isNull (box playerSprite.sprite)) then
      let b = playerSprite.sprite.bounds.extents
      playerRadius <- max 0.04f (max b.x b.y)

    ready <- true

  static member private World () =
    World.DefaultGameObjectInjectionWorld

  /// Entity を 1 個 作って、位置と印を入れる
  static member Spawn (kind: BulletKind, x: float32, y: float32, root: bool) : BulletSim =
    let world = BulletEntityFactory.World ()
    if isNull (box world) then
      Debug.LogError "BulletEntityFactory: DefaultGameObjectInjectionWorld が無い"
      BulletSim ()
    else
      if not ready then
        Debug.LogError "BulletEntityFactory: Spawn の前に Configure が呼ばれていない"

      let em = world.EntityManager
      let entity = em.CreateEntity()

      let sim = BulletSim ()
      sim.Entity <- entity
      sim.Kind <- kind
      sim.BulletType <- (if kind = BulletKind.Player then BulletType.Player else BulletType.Enemy)
      sim.Init()
      sim.Root <- root
      sim.X <- x
      sim.Y <- y

      let radius = if kind = BulletKind.Player then playerRadius else enemyRadius
      em.AddComponentData(entity,
        LocalTransform.FromPositionRotationScale(float3(x, y, 0.0f), quaternion.identity, 1.0f)) |> ignore
      let mutable tag = BulletTag()
      tag.Kind <- kind
      tag.Radius <- radius
      em.AddComponentData(entity, tag) |> ignore
      em.AddComponentObject(entity, sim)

      if ready then
        let idx = if kind = BulletKind.Player then 1 else 0
        // AddComponents は description を inref で取る。
        // **F# は let mutable の束縛でないと & を渡せない**
        let mutable desc = renderDesc
        RenderMeshUtility.AddComponents(
          entity, em, &desc, renderMeshArray,
          MaterialMeshInfo.FromRenderMeshArrayIndices(idx, idx))

      if kind = BulletKind.Player then playerCount <- playerCount + 1
      else enemyCount <- enemyCount + 1
      sim

  /// 敵が弾幕を撃つ。根から始めるので実行位置は Core に作らせる
  static member SpawnEnemy (position: Vector3, script: BulletmlScript, root: bool) =
    let sim = BulletEntityFactory.Spawn(BulletKind.Enemy, position.x, position.y, root)
    sim.SetScript(script, None)
    sim

  /// 自機が弾を撃つ
  static member SpawnPlayer (position: Vector3, script: BulletmlScript) =
    let sim = BulletEntityFactory.Spawn(BulletKind.Player, position.x, position.y, false)
    sim.SetScript(script, None)
    sim

  /// 撃たれた弾を実体にする。旧 GetBulletPrefubInstance ＋ Spawn の合わせ。
  ///
  /// **弾幕は親と同じものを引き継ぐ。** 引き継がないと、弾の中に残った
  /// bulletRef / actionRef を誰も解けない。実行位置はエンジンが
  /// `Frame.Spawned` で渡してきたものをそのまま使う。
  ///
  /// **産まれる位置は撃った側と同じ。** FrontEnv が SpawnAimDir に AimDir と
  /// 同じ値を入れているのはこのため。**片方だけ直すと軌跡が割れる。**
  static member SpawnChild (parent: BulletSim, child: BulletRun) =
    let sim = BulletEntityFactory.Spawn(parent.Kind, parent.X, parent.Y, false)
    sim.SetScript(parent.Script, Some child)
    let body = child.Body
    sim.X <- body.Pos.X
    sim.Y <- body.Pos.Y
    sim.Dir <- body.Dir
    sim.Speed <- body.Speed
    sim

  static member Destroy (entity: Entity) =
    let world = BulletEntityFactory.World ()
    if not (isNull (box world)) && entity <> Entity.Null then
      let em = world.EntityManager
      if em.Exists entity then
        if em.HasComponent<BulletTag> entity then
          let kind = em.GetComponentData<BulletTag>(entity).Kind
          if kind = BulletKind.Player then playerCount <- max 0 (playerCount - 1)
          else enemyCount <- max 0 (enemyCount - 1)
        em.DestroyEntity entity

  static member DestroySim (sim: BulletSim) =
    if not (isNull (box sim)) then BulletEntityFactory.Destroy sim.Entity

  /// 種類ごとに全部 消す。弾幕を切り替えるときに使う
  static member DestroyAll (kind: BulletKind) =
    let world = BulletEntityFactory.World ()
    if not (isNull (box world)) then
      let em = world.EntityManager
      use query = em.CreateEntityQuery(typeof<BulletTag>, typeof<BulletSim>)
      use entities = query.ToEntityArray(Allocator.Temp)
      for i in 0 .. entities.Length - 1 do
        let e = entities.[i]
        if em.Exists e && em.GetComponentData<BulletTag>(e).Kind = kind then
          em.DestroyEntity e
      if kind = BulletKind.Player then playerCount <- 0 else enemyCount <- 0

  static member DestroyAllEnemy () = BulletEntityFactory.DestroyAll BulletKind.Enemy

  static member IsOffScreen (x: float32) (y: float32) =
    x < BulletEntityFactory.ScreenMinX || x > BulletEntityFactory.ScreenMaxX
    || y < BulletEntityFactory.ScreenMinY || y > BulletEntityFactory.ScreenMaxY
