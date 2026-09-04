namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Text 
open System.Collections.Generic
open System.Runtime.Serialization
open UnityEngine
open FsBulletML2
 
[<ExecuteInEditMode()>]
type Informations () =
  inherit MonoBehaviour()

  [<DefaultValue>]val mutable public show : bool
  [<DefaultValue>]val mutable public showInEditor : bool
  [<DefaultValue>]val mutable public intervalTime : float32

  /// フレームレートの上限。**0 以下 なら既定（60）を使う。**
  ///
  /// **以前は 40 を直に書いていた。** 意図した設計ではなく、そのまま残って
  /// いただけだった。inspector から変えられるように欄にしてあるが、
  /// シーンに保存されていなければ 0 になるので、そのときは既定へ落とす。
  [<DefaultValue>]val mutable public targetFps : int

  [<DefaultValue>]val mutable private enemy : Enemy
  [<DefaultValue>]val mutable private player : Player
  [<DefaultValue>]val mutable private oldTime : float32
  [<DefaultValue>]val mutable private frame : float32
  [<DefaultValue>]val mutable private frameRate : float32
  [<DefaultValue>]val mutable private info : string
  [<DefaultValue>]val mutable private enemyBullets : int
  [<DefaultValue>]val mutable private playerBullets : int
  /// 直近の窓でいちばん長かったコマ（ms）。**平均だけだと引っかかりが消える**
  [<DefaultValue>]val mutable private worstMs : float32
  [<DefaultValue>]val mutable private worstInWindow : float32

  /// 既定の上限。ApplyCapOnPlay が Play の頭で使う
  static member DefaultTargetFps = 60

  /// 上限の掛け方。**vSync が先。**
  ///
  /// vSyncCount が 1 以上 だと Unity は targetFrameRate を無視して画面の
  /// リフレッシュレートに従う。品質設定によっては 1 なので、
  /// **切らないと押さえられない**（同梱の C# サンプルで実際に押さえられなかった）。
  static member ApplyCap (fps: int) =
    QualitySettings.vSyncCount <- 0
    Application.targetFrameRate <- fps

  /// **Play に入った時点で上限を掛ける。**
  ///
  /// Awake だけに任せると、**シーンに Informations が居ること**と
  /// **その Awake が先に走ること**に依存する。どちらも外から見て分からないので、
  /// シーンに何が居ようが効く場所へ出した。
  [<RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)>]
  static member ApplyCapOnPlay () =
    Informations.ApplyCap Informations.DefaultTargetFps

  member this.Awake () =
    // 上限そのものは ApplyCapOnPlay が Play の頭で掛けている。
    // ここは inspector で変えた値を反映するための上書き
    let fps = if this.targetFps > 0 then this.targetFps else Informations.DefaultTargetFps
    Informations.ApplyCap fps
    this.enemy <- UnityEngine.Object.FindAnyObjectByType<Enemy>()
    this.player <- UnityEngine.Object.FindAnyObjectByType<Player>()
    this.useGUILayout <- false

  member this.Start () =
    this.oldTime <- Time.realtimeSinceStartup

  member this.Update () =
    if (not this.show) then
      ()
    else
      this.frame <- this.frame + 1.f
      // いちばん長かったコマも控える。**平均だけだと、たまに 1 コマ
      // 引っかかる形（GC やアセットの読み込み）が消える**
      let dtMs = Time.unscaledDeltaTime * 1000.f
      if dtMs > this.worstInWindow then this.worstInWindow <- dtMs
      let time = Time.realtimeSinceStartup - this.oldTime
      if time >= this.intervalTime then
        this.frameRate <- this.frame / time
        this.info <- "FPS:" + this.frameRate.ToString()
        this.worstMs <- this.worstInWindow
        this.worstInWindow <- 0.f
        this.oldTime <- Time.realtimeSinceStartup
        this.frame <- 0.f
        this.enemyBullets <- GameObject.FindGameObjectsWithTag("EnemyBullet").Length
        this.playerBullets <- GameObject.FindGameObjectsWithTag("PlayerBullet").Length

  member this.OnGUI () =
    if ((Application.isPlaying && this.show) || (not Application.isPlaying && this.showInEditor)) then
      GUI.Box(new Rect(5.f, 30.f, 475.f, 96.f), "")
      GUI.Label(new Rect(10.f, 30.f, 1000.f, 200.f), this.GetShowText())

    if (GUI.Button(new Rect(445.f, 35.f, 25.f, 22.f), if this.show then  "▲" else "▼")) then
        this.show <- not this.show

    // 上限を外せるボタン。**既定は掛かっている。**
    // 上限に張り付いているのか届いていないのかは、外してみないと割れない
    if Application.isPlaying then
      let capped = Application.targetFrameRate > 0
      let fps = if this.targetFps > 0 then this.targetFps else Informations.DefaultTargetFps
      let label = if capped then "上限を外す" else "上限 " + string fps
      if (GUI.Button(new Rect(360.f, 5.f, 80.f, 22.f), label)) then
        Informations.ApplyCap (if capped then -1 else fps)

    // Prev
    if (GUI.Button(new Rect(5.f, 5.f, 25.f, 22.f), "<")) then
        this.enemy.Prev()

    // Next
    if (GUI.Button(new Rect(445.f, 5.f, 25.f, 22.f), ">")) then
        this.enemy.Next()

  member this.GetShowText () : string =
    let sb = new StringBuilder();
    // FPS。**上限と vSync、いちばん長かったコマも並べて出す。**
    //
    // 数だけだと「これしか出ない」と読める —— 上限に張り付いているのか、
    // 届いていないのかが分からない（同梱の C# サンプルで実際に読み違えた）。
    let cap = Application.targetFrameRate
    let vsync = QualitySettings.vSyncCount
    sb.Append(String.Format("FPS:{0:F1}（上限 {1} / vSync {2}）最悪 {3:F1}ms\n",
                            this.frameRate,
                            (if cap > 0 then string cap else "無制限"),
                            (if vsync > 0 then string vsync + "（画面に同期）" else "切"),
                            this.worstMs)) |> ignore
    // 弾名
    sb.Append(String.Format("Name:{0}\n", this.enemy.BulletName)) |> ignore
    // ボスライフ
    sb.Append(String.Format("Boss Life:{0}\n", this.enemy.Life)) |> ignore
    // プレイヤーダメージ
    sb.Append(String.Format("Player Damages:{0}\n", this.player.Damage)) |> ignore
    // 敵弾数
    sb.Append(String.Format("EnemyBullets:{0}\n", this.enemyBullets)) |> ignore
    // 自機弾数
    sb.Append(String.Format("PlayerBullets:{0}\n", this.playerBullets)) |> ignore
    sb.ToString()