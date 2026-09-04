namespace FsBulletML2.Sample.Unity2D.FSharp

open UnityEngine

type BgScroll () =
  inherit MonoBehaviour ()
  [<SerializeField;DefaultValue>]val mutable public scrollSpeed : float32

  /// 背景を毎コマ 少しずつ流す。
  ///
  /// **Renderer は 1 回 だけ引く。** 旧は毎コマ `GetComponent` を呼んでいた ——
  /// あれは型で component を走査するので、毎コマ 払う理由が無い
  member this.Start () =
    let r = this.GetComponent<Renderer>()
    FrameTicker.Frames
    |> subscribeUntilDestroy this (fun _ ->
        let offset = r.material.mainTextureOffset
        r.material.mainTextureOffset <-
          Vector2(offset.x, offset.y - Time.deltaTime * this.scrollSpeed))
