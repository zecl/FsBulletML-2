namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Text
open R3
// R3 のあとに開くこと（`Observable` が両方にある。FrameTicker の但し書き）
open FSharp.Control.R3
open UnityEngine

/// 窓 1 つ ぶんの測り。
[<Struct>]
type FrameStats =
    {
        /// 窓の平均 fps
        Fps: float32
        /// 窓でいちばん長かったコマ（ms）。平均だけだと引っかかりが消える
        WorstMs: float32
        EnemyBullets: int
        PlayerBullets: int
    }

    static member Zero =
        {
            Fps = 0.0f
            WorstMs = 0.0f
            EnemyBullets = 0
            PlayerBullets = 0
        }

[<ExecuteInEditMode>]
type Informations() =
    inherit MonoBehaviour()

    /// 画面に出す文。組むのは購読 1 か所（`Start`）で、`OnGUI` は読むだけ。
    /// F# は let 束縛を val より前に置くので、ここに居る
    let statusTextRp = new ReactiveProperty<string>("")

    /// 窓の中で数えているもの。窓が閉じるたびに 0 へ戻す
    let mutable frames = 0
    let mutable elapsed = 0.0f
    let mutable worstInWindow = 0.0f

    /// 直近の窓の測り。
    let statsRp = new ReactiveProperty<FrameStats>(FrameStats.Zero)

    [<DefaultValue>]
    val mutable public show: bool

    [<DefaultValue>]
    val mutable public showInEditor: bool

    /// 窓の長さ（秒）。0 以下 なら 0.5
    [<DefaultValue>]
    val mutable public intervalTime: float32

    /// フレームレートの上限。
    [<DefaultValue>]
    val mutable public targetFps: int

    [<DefaultValue>]
    val mutable private enemy: Enemy

    [<DefaultValue>]
    val mutable private player: Player

    /// 既定の上限。ApplyCapOnPlay が Play の頭で使う
    static member DefaultTargetFps = 60

    /// 上限の掛け方。
    static member ApplyCap(fps: int) =
        QualitySettings.vSyncCount <- 0
        Application.targetFrameRate <- fps

    /// Play に入った時点で上限を掛ける。
    [<RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)>]
    static member ApplyCapOnPlay() =
        Informations.ApplyCap Informations.DefaultTargetFps

    member this.Awake() =
        // 上限そのものは ApplyCapOnPlay が Play の頭で掛けている。
        // ここは inspector で変えた値を反映するための上書き
        let fps =
            if this.targetFps > 0 then
                this.targetFps
            else
                Informations.DefaultTargetFps

        Informations.ApplyCap fps

        // ECS の下ごしらえを、シーンに確実に居るここからも起こす。
        if Application.isPlaying then
            BulletEcsBootstrap.AutoCreate()

        this.enemy <- UnityEngine.Object.FindAnyObjectByType<Enemy>()
        this.player <- UnityEngine.Object.FindAnyObjectByType<Player>()
        this.useGUILayout <- false

    /// 表示の文を組む流れを 1 本 に畳む。
    member this.Start() =
        if not Application.isPlaying then
            ()
        else if isNull (box this.enemy) || isNull (box this.player) then
            ()
        else

            // 毎コマ 数える。窓が閉じたコマだけ `statsRp` が動く
            FrameTicker.Frames |> subscribeUntilDestroy this (fun _ -> this.Tick())

            // 名前・ライフ・ダメージ・測り。どれが変わっても組み直す
            R3.Observable.CombineLatest(
                (this.enemy.BulletNameRp :> Observable<string>),
                (this.enemy.LifeRp :> Observable<int>),
                (this.player.DamageRp :> Observable<int>),
                (statsRp :> Observable<FrameStats>),
                Func<string, int, int, FrameStats, string>(fun name life damage s ->
                    Informations.FormatStatus(name, life, damage, s))
            )
            |> subscribeUntilDestroy this (fun text -> statusTextRp.Value <- text)

    /// 1 コマ 数える。窓が閉じたコマだけ `statsRp` を差し替える
    member private this.Tick() =
        let dt = Time.unscaledDeltaTime
        frames <- frames + 1
        elapsed <- elapsed + dt
        let dtMs = dt * 1000.0f

        if dtMs > worstInWindow then
            worstInWindow <- dtMs

        let window = if this.intervalTime > 0.0f then this.intervalTime else 0.5f

        if elapsed >= window then
            statsRp.Value <-
                {
                    Fps = float32 frames / elapsed
                    WorstMs = worstInWindow
                    // 弾は Entity。 旧は `FindGameObjectsWithTag` で数えていて、
                    // ECS へ移したあとはいつも 0 だった（GameObject の弾は出ない）
                    EnemyBullets = BulletEntityFactory.EnemyCount
                    PlayerBullets = BulletEntityFactory.PlayerCount
                }

            frames <- 0
            elapsed <- 0.0f
            worstInWindow <- 0.0f

    member this.OnGUI() =
        if
            ((Application.isPlaying && this.show)
             || (not Application.isPlaying && this.showInEditor))
        then
            GUI.Box(new Rect(5.f, 30.f, 475.f, 96.f), "")
            GUI.Label(new Rect(10.f, 30.f, 1000.f, 200.f), this.GetShowText())

        if (GUI.Button(new Rect(445.f, 35.f, 25.f, 22.f), if this.show then "▲" else "▼")) then
            this.show <- not this.show

        // 上限を外せるボタン。既定は掛かっている。
        // 上限に張り付いているのか届いていないのかは、外してみないと割れない
        if Application.isPlaying then
            let capped = Application.targetFrameRate > 0

            let fps =
                if this.targetFps > 0 then
                    this.targetFps
                else
                    Informations.DefaultTargetFps

            let label = if capped then "上限を外す" else "上限 " + string fps

            if (GUI.Button(new Rect(360.f, 5.f, 80.f, 22.f), label)) then
                Informations.ApplyCap(if capped then -1 else fps)

        // 敵が居ないときは押させない。 旧は無防備に呼んでいて、
        // Awake が途中で落ちた回に OnGUI ごと止まった
        if not (isNull (box this.enemy)) then
            if (GUI.Button(new Rect(5.f, 5.f, 25.f, 22.f), "<")) then
                this.enemy.Prev()

            if (GUI.Button(new Rect(445.f, 5.f, 25.f, 22.f), ">")) then
                this.enemy.Next()

    member private this.GetShowText() : string =
        if isNull (box this.enemy) || isNull (box this.player) then
            ""
        elif not Application.isPlaying then
            // Play していないと流れが動かないので、その場で組む
            Informations.FormatStatus(this.enemy.BulletName, this.enemy.Life, this.player.Damage, FrameStats.Zero)
        else
            statusTextRp.Value

    static member private FormatStatus(name: string, life: int, damage: int, s: FrameStats) : string =
        let sb = new StringBuilder()
        // FPS。
        // 数だけだと「これしか出ない」と読める —— 上限に張り付いているのか、 届いていないのかが分からない（同梱の C# サンプルで実際に読み違えた）。
        let cap = Application.targetFrameRate
        let vsync = QualitySettings.vSyncCount

        sb.Append(
            String.Format(
                "FPS:{0:F1}（上限 {1} / vSync {2}）最悪 {3:F1}ms\n",
                s.Fps,
                (if cap > 0 then string cap else "無制限"),
                (if vsync > 0 then string vsync + "（画面に同期）" else "切"),
                s.WorstMs
            )
        )
        |> ignore

        sb.Append(String.Format("Name:{0}\n", name)) |> ignore
        sb.Append(String.Format("Boss Life:{0}\n", life)) |> ignore
        sb.Append(String.Format("Player Damages:{0}\n", damage)) |> ignore
        sb.Append(String.Format("EnemyBullets:{0}\n", s.EnemyBullets)) |> ignore
        sb.Append(String.Format("PlayerBullets:{0}\n", s.PlayerBullets)) |> ignore
        sb.ToString()

    member this.OnDestroy() =
        statusTextRp.Dispose()
        statsRp.Dispose()
