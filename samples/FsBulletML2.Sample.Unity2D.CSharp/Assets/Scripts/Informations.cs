using UnityEngine;
using System;
using System.Text;
using R3;

[ExecuteInEditMode()]
public class Informations : MonoBehaviour
{
    public bool show = true;
    public bool showInEditor = false;

    /// <summary>
    /// フレームレートの上限。
    /// </summary>
    public int targetFps = DefaultTargetFps;

    /// <summary>既定の上限。<see cref="ApplyCapOnPlay"/> が Play の頭で使う</summary>
    public const int DefaultTargetFps = 60;

    /// <summary>
    /// Play に入った時点で上限を掛ける。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyCapOnPlay()
    {
        ApplyCap(DefaultTargetFps);
    }

    /// <summary>
    /// 上限の掛け方。
    /// </summary>
    static void ApplyCap(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    private Enemy enemy;
    private Player player;
    private readonly ReactiveProperty<string> StatusTextRp = new("");

    // fps を数えるための控え。
    int frames;
    float elapsed;
    /// 直近 0.5 秒 の平均 fps
    float fps;
    /// 直近 0.5 秒 でいちばん長かったコマ（ms）。平均だけだと引っかかりが消える
    float worstMs;
    float worstInWindow;

    void Awake()
    {
        // 上限そのものは ApplyCapOnPlay が Play の頭で掛けている。
        // ここは inspector で変えた値を反映するための上書き
        ApplyCap(targetFps);
        enemy = FindAnyObjectByType<Enemy>();
        player = FindAnyObjectByType<Player>();
        useGUILayout = false;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        var dt = Time.unscaledDeltaTime;
        frames++;
        elapsed += dt;
        if (dt * 1000f > worstInWindow)
        {
            worstInWindow = dt * 1000f;
        }

        if (elapsed >= 0.5f)
        {
            fps = frames / elapsed;
            worstMs = worstInWindow;
            frames = 0;
            elapsed = 0f;
            worstInWindow = 0f;
        }
    }

    public void Start()
    {
        if (!Application.isPlaying || enemy == null || player == null)
        {
            return;
        }

        var token = destroyCancellationToken;
        var stats = Observable.Return((enemy: 0, player: 0, fps: 0f, worst: 0f))
            .Concat(
                Observable.Interval(TimeSpan.FromSeconds(0.5), token)
                    .Select(_ => (
                        enemy: BulletEntityFactory.EnemyCount,
                        player: BulletEntityFactory.PlayerCount,
                        // Update が数えた平均。ここで割り算しない
                        fps: this.fps,
                        worst: this.worstMs
                    )));

        enemy.BulletNameRp
            .CombineLatest(enemy.LifeRp, (name, life) => (name, life))
            .CombineLatest(player.DamageRp, (x, dmg) => (name: x.name, life: x.life, dmg: dmg))
            .CombineLatest(stats, (x, s) => FormatStatus(x.name, x.life, x.dmg, s.fps, s.worst, s.enemy, s.player))
            .Subscribe(text => StatusTextRp.Value = text)
            .AddTo(this);
    }

    public void OnGUI()
    {
        if ((Application.isPlaying && show) || (!Application.isPlaying && showInEditor))
        {
            GUI.Box(new Rect(5, 30, 475, 96), "");
            GUI.Label(new Rect(10, 30, 1000, 200), GetShowText());
        }

        if (GUI.Button(new Rect(445, 35, 25, 22), show ? "-" : "+"))
        {
            this.show = !this.show;
        }

        // 上限を外せるボタン。既定は 60 で掛かっている。
        // 60 に張り付いているのか届いていないのかは、外してみないと割れない
        if (Application.isPlaying)
        {
            var capped = Application.targetFrameRate > 0;
            if (GUI.Button(new Rect(360, 5, 80, 22), capped ? "上限を外す" : "上限 " + targetFps))
            {
                ApplyCap(capped ? -1 : targetFps);
            }
        }

        if (GUI.Button(new Rect(5, 5, 25, 22), "<") && enemy != null)
        {
            enemy.Prev();
        }

        if (GUI.Button(new Rect(445, 5, 25, 22), ">") && enemy != null)
        {
            enemy.Next();
        }
    }

    private string GetShowText()
    {
        if (enemy == null || player == null)
        {
            return "";
        }
        if (!Application.isPlaying)
        {
            return FormatStatus(enemy.BulletName, enemy.Life, player.Damage, 0f, 0f, 0, 0);
        }
        return StatusTextRp.Value;
    }

    private static string FormatStatus(string name, int life, int damage, float fps, float worstMs,
                                       int enemyBullets, int playerBullets)
    {
        var sb = new StringBuilder();
        // 上限と、いちばん長かったコマも並べて出す。
        var cap = Application.targetFrameRate;
        var vsync = QualitySettings.vSyncCount;
        sb.Append(string.Format("FPS:{0:F1}（上限 {1} / vSync {2}）最悪 {3:F1}ms\n",
                                fps,
                                cap > 0 ? cap.ToString() : "無制限",
                                vsync > 0 ? vsync + "（画面に同期）" : "切",
                                worstMs));
        sb.Append(string.Format("Name:{0}\n", name));
        sb.Append(string.Format("Boss Life:{0}\n", life));
        sb.Append(string.Format("Player Damages:{0}\n", damage));
        sb.Append(string.Format("EnemyBullets:{0}\n", enemyBullets));
        sb.Append(string.Format("PlayerBullets:{0}\n", playerBullets));
        return sb.ToString();
    }

    void OnDestroy()
    {
        StatusTextRp.Dispose();
    }
}
