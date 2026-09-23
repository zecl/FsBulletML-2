namespace FsBulletML2.Sample.Unity2D.FSharp

open UnityEngine

type BgScroll () =
  inherit MonoBehaviour ()
  [<SerializeField;DefaultValue>]val mutable public scrollSpeed : float32

  /// `Renderer` は 1 回 だけ引く。毎コマ `GetComponent` する理由は無い。
  member this.Start () =
    let r = this.GetComponent<Renderer>()
    FrameTicker.Frames
    |> subscribeUntilDestroy this (fun _ ->
        let offset = r.material.mainTextureOffset
        r.material.mainTextureOffset <-
          Vector2(offset.x, offset.y - Time.deltaTime * this.scrollSpeed))
