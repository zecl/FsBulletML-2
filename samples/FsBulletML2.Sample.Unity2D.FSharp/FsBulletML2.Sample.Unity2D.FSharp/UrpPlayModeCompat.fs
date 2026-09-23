namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open UnityEngine
open UnityEngine.Rendering
open UnityEngine.Rendering.Universal

/// URP に移したあと、GameObject の見た目が出るようにする安全網。
[<AbstractClass; Sealed>]
type UrpPlayModeCompat private () =

  static let mutable applied = false

  /// URP の Unlit。名前が版で変わるので 2 つ 試す
  static member FindUrpUnlit () =
    match Shader.Find "Universal Render Pipeline/Unlit" with
    | null -> Shader.Find "Universal Render Pipeline/Unlit/Unlit"
    | s -> s

  static member FindUrpParticleUnlit () =
    match Shader.Find "Universal Render Pipeline/Particles/Unlit" with
    | null -> UrpPlayModeCompat.FindUrpUnlit ()
    | s -> s

  /// そのシェーダが既に URP のものか。差し替えを 2 度 やらないため
  static member IsUrpShader (shader: Shader) =
    if isNull (box shader) then false
    else
      let n = if isNull shader.name then "" else shader.name
      n.IndexOf("Universal Render Pipeline", StringComparison.OrdinalIgnoreCase) >= 0
      || n.IndexOf("Shader Graphs", StringComparison.OrdinalIgnoreCase) >= 0

  /// URP の Unlit として描けるように材質を整える
  static member ConfigureUrpUnlit (mat: Material, texture: Texture, color: Color,
                                   transparent: bool, additive: bool) =
    if isNull (box mat) then () else

    if mat.HasProperty "_BaseMap" then mat.SetTexture("_BaseMap", texture)
    if mat.HasProperty "_MainTex" then mat.SetTexture("_MainTex", texture)
    mat.mainTexture <- texture
    if mat.HasProperty "_BaseColor" then mat.SetColor("_BaseColor", color)
    if mat.HasProperty "_Color" then mat.SetColor("_Color", color)
    if mat.HasProperty "_Cull" then mat.SetFloat("_Cull", 0.0f)
    mat.doubleSidedGI <- true

    if transparent then
      if mat.HasProperty "_Surface" then mat.SetFloat("_Surface", 1.0f)
      let dst = if additive then BlendMode.One else BlendMode.OneMinusSrcAlpha
      if mat.HasProperty "_Blend" then mat.SetFloat("_Blend", (if additive then 2.0f else 0.0f))
      if mat.HasProperty "_SrcBlend" then mat.SetFloat("_SrcBlend", float32 BlendMode.SrcAlpha)
      if mat.HasProperty "_DstBlend" then mat.SetFloat("_DstBlend", float32 dst)
      if mat.HasProperty "_ZWrite" then mat.SetFloat("_ZWrite", 0.0f)
      mat.SetOverrideTag("RenderType", "Transparent")
      mat.EnableKeyword "_SURFACE_TYPE_TRANSPARENT"
      mat.DisableKeyword "_ALPHAPREMULTIPLY_ON"
      if additive then mat.EnableKeyword "_ALPHAMODULATE_ON"
      mat.renderQueue <- int RenderQueue.Transparent
    else
      if mat.HasProperty "_Surface" then mat.SetFloat("_Surface", 0.0f)
      if mat.HasProperty "_ZWrite" then mat.SetFloat("_ZWrite", 1.0f)
      mat.SetOverrideTag("RenderType", "Opaque")
      mat.DisableKeyword "_SURFACE_TYPE_TRANSPARENT"
      mat.renderQueue <- int RenderQueue.Geometry

    mat.enableInstancing <- true

  /// カメラを URP が描ける形にする。
  static member private EnsureCameras () =
    let cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)
    let main =
      match Camera.main with
      | null ->
        let tagged = cameras |> Array.tryFind (fun c -> not (isNull (box c)) && c.CompareTag "MainCamera")
        match tagged with
        | Some c -> c
        | None -> if cameras.Length > 0 then cameras.[0] else null
      | c -> c

    if isNull (box main) then
      Debug.LogError "UrpPlayModeCompat: カメラが 1 台 も無い"
    else
      main.enabled <- true
      main.gameObject.tag <- "MainCamera"
      main.gameObject.SetActive true
      main.clearFlags <- CameraClearFlags.SolidColor
      // 元のサンプルの背景は黒。透明のままだと何も塗られないので不透明にする
      if main.backgroundColor.a < 0.01f then
        let mutable bg = main.backgroundColor
        bg.a <- 1.0f
        main.backgroundColor <- bg

      let mainData = main.GetUniversalAdditionalCameraData()
      mainData.renderType <- CameraRenderType.Base
      mainData.SetRenderer 0
      mainData.renderPostProcessing <- false
      mainData.cameraStack.Clear()

      for cam in cameras do
        if not (isNull (box cam)) && not (Object.ReferenceEquals(cam, main)) then
          cam.enabled <- true
          let data = cam.GetUniversalAdditionalCameraData()
          data.renderType <- CameraRenderType.Overlay
          data.SetRenderer 0
          data.renderPostProcessing <- false
          if not (mainData.cameraStack.Contains cam) then
            mainData.cameraStack.Add cam

      Debug.Log(
        sprintf "UrpPlayModeCompat cameras: main=%s ortho=%b size=%f stack=%d pipeline=%s"
          main.name main.orthographic main.orthographicSize mainData.cameraStack.Count
          (if isNull (box GraphicsSettings.defaultRenderPipeline) then "NULL"
           else GraphicsSettings.defaultRenderPipeline.name))

  /// 平行光源が無ければ足す。URP の Lit は光源が無いと真っ黒になる
  static member private EnsureDirectionalLight () =
    let lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include)
    // `type` は F# の予約語なので、メンバ名はバッククォートで囲む
    let hasDirectional =
      lights |> Array.exists (fun l -> not (isNull (box l)) && l.``type`` = LightType.Directional)
    if not hasDirectional then
      let go = new GameObject("Directional Light")
      let light = go.AddComponent<Light>()
      light.``type`` <- LightType.Directional
      light.intensity <- 1.0f
      light.color <- Color.white
      go.transform.rotation <- Quaternion.Euler(50.0f, -30.0f, 0.0f)

  /// Built-in のシェーダを使っている材質を URP の Unlit へ差し替える。
  static member private UpgradeSceneRenderers () =
    let unlit = UrpPlayModeCompat.FindUrpUnlit ()
    let particle = UrpPlayModeCompat.FindUrpParticleUnlit ()
    if isNull (box unlit) then
      Debug.LogError "UrpPlayModeCompat: URP の Unlit シェーダが見つからない"
    else
      let renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)
      for r in renderers do
        if not (isNull (box r)) then
          let shared = r.sharedMaterials
          if not (isNull shared) then
            let mutable instances : Material[] = null
            for m in 0 .. shared.Length - 1 do
              let src = shared.[m]
              if not (isNull (box src)) && not (UrpPlayModeCompat.IsUrpShader src.shader) then
                if isNull instances then instances <- r.materials
                let shaderName = if isNull (box src.shader) then "" else src.shader.name
                let additive =
                  shaderName.IndexOf("Particle", StringComparison.OrdinalIgnoreCase) >= 0
                  || shaderName.IndexOf("Additive", StringComparison.OrdinalIgnoreCase) >= 0
                let shader = if additive && not (isNull (box particle)) then particle else unlit
                let copy = new Material(shader, name = src.name + " (URP)")
                let tex = if src.HasProperty "_MainTex" then src.GetTexture "_MainTex" else src.mainTexture
                let color = if src.HasProperty "_Color" then src.GetColor "_Color" else Color.white
                // 背景の板だけ不透明。ほかは透過で重ねる
                let transparent =
                  not (r :? MeshRenderer
                       && r.gameObject.name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0)
                let tex = if isNull (box tex) then Texture2D.whiteTexture :> Texture else tex
                UrpPlayModeCompat.ConfigureUrpUnlit(copy, tex, color, transparent, additive)
                instances.[m] <- copy
            if not (isNull instances) then r.materials <- instances

  /// SpriteRenderer のテクスチャを property block で束ね直す。
  /// URP の sprite 経路はこれが無いと白（またはピンク）になる
  static member BindSprite (sr: SpriteRenderer) =
    if not (isNull (box sr)) && not (isNull (box sr.sprite)) then
      let tex = sr.sprite.texture
      if not (isNull (box tex)) then
        let block = new MaterialPropertyBlock()
        sr.GetPropertyBlock block
        block.SetTexture("_MainTex", tex)
        block.SetTexture("_BaseMap", tex)
        block.SetColor("_BaseColor", sr.color)
        block.SetColor("_Color", sr.color)
        sr.SetPropertyBlock block

  static member private BindSpriteTextures () =
    let sprites = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include)
    for sr in sprites do UrpPlayModeCompat.BindSprite sr

  /// Play の頭で 1 回 だけ。 2 度 呼んでも 2 度目 は何もしない
  static member Apply () =
    if not applied then
      applied <- true
      UrpPlayModeCompat.EnsureCameras ()
      UrpPlayModeCompat.EnsureDirectionalLight ()
      UrpPlayModeCompat.UpgradeSceneRenderers ()
      UrpPlayModeCompat.BindSpriteTextures ()
