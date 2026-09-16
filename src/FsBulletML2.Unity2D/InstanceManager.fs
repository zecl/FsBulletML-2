namespace FsBulletML2.Unity2D
open System
open System.Collections.Generic
open UnityEngine

[<Serializable>]
type ObjectData () =
  [<DefaultValue>]val mutable public prefab : GameObject
  [<DefaultValue>]val mutable public cacheSize : int
  [<DefaultValue>]val mutable private objects : GameObject[]

  /// この prefab が弾か。弾を ECS へ移したので、先に作らない。
  ///
  /// シーンには弾のプールが 3,000 個 単位 で設定されたまま残っていて、
  /// そのままだと `g_bullet_s0` から数千 個 の GameObject が起動時にできる
  /// —— 1 つ も使われない（弾は Entity になった）。
  member this.IsBulletPrefab () =
    if isNull (box this.prefab) then true
    else
      let tag = this.prefab.tag
      tag = "EnemyBullet" || tag = "PlayerBullet" || tag = "Bomb"

  member this.Initialize () =
    if this.IsBulletPrefab () then
      // 数を 0 にしておく。 残すと GetNextObjectInCache が空の配列を
      // 探して落ちる
      this.objects <- Array.empty
      this.cacheSize <- 0
    else

    this.objects <- Array.zeroCreate this.cacheSize

    for i in 0..this.cacheSize-1 do
      let prefub = UnityEngine.Object.Instantiate<GameObject>(this.prefab)
      this.objects.[i] <- prefub
      this.objects.[i].SetActive(false)
      this.objects.[i].name <- this.objects.[i].name.Replace("(Clone)", "") + i.ToString()

  /// 空いているものを 1 つ 返す。無ければ None。
  ///
  /// 元は `Array.find` で、空きが無いと KeyNotFoundException で落ちていた
  /// （プールを 0 にしたとき、敵に弾が当たった瞬間に踏んだ）。
  /// 使い切ったときも同じ形で落ちるので、呼ぶ側が選べるように option で返す。
  member this.TryGetNextObjectInCache () =
    if this.cacheSize <= 0 then None
    else this.objects |> Array.tryFind (fun x -> x.activeSelf |> not)

type InstanceManager () =
  inherit MonoBehaviour () 
  [<DefaultValue>]static val mutable private self : InstanceManager
  [<DefaultValue>]val mutable public caches : ObjectData[]
  [<DefaultValue>]val mutable private activeCachedObjects : Dictionary<string,bool>
    
  member this.Awake () =
    InstanceManager.self <- this
    let mutable amount = 0

    for cache in this.caches do
      cache.Initialize()
      amount <- amount + cache.cacheSize
    this.activeCachedObjects <- new Dictionary<string,bool>(amount)

  static member InstantiatePrefab(prefab:GameObject, position:Vector3, rotation:Quaternion) =
    let cache = InstanceManager.self.caches |> Seq.tryFind (fun x -> x.prefab.tag = prefab.tag)
    // プールが無いときも、空きが無いときも、その場で作る。
    // 落ちるより作るほうがまし —— 使い切ったのは呼ぶ側の都合で、
    // ここで例外にしても誰も回復できない
    let fresh () = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation)
    match cache with
    | None -> fresh ()
    | Some cache ->
      match cache.TryGetNextObjectInCache() with
      | None -> fresh ()
      | Some obj ->
        obj.transform.position <- position
        obj.transform.rotation <- rotation
        obj.SetActive(true)
        InstanceManager.self.activeCachedObjects.[obj.name] <- true
        obj
    
  static member Destroy(objectToDestroy:GameObject) = 
    if (InstanceManager.self.activeCachedObjects.ContainsKey(objectToDestroy.name)) then
      objectToDestroy.SetActive(false);
      InstanceManager.self.activeCachedObjects.[objectToDestroy.name] <- false
    else
      UnityEngine.Object.Destroy(objectToDestroy)