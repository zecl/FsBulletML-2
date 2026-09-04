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
    /// フレームレートの上限。<b>-1 で無制限。</b>
    ///
    /// <b>以前は 40 に固定していた。</b> 意図した設計ではなく、
    /// 「40 fps しか出ない」と読める状態がそのまま残っていただけだった。
    /// 外したので、いま出ている数はそのまま実力。
    ///
    /// 表示には上限も並べてある —— 数だけだと「これしか出ない」と
    /// 「上限に張り付いている」を見分けられない（実際に読み違えた）。
    ///
    /// 参考: エンジン（Runner.StepWith）が 1 コマ に使うのは弾 121 本 で
    /// 0.12 ms、ホーミング 25 本 で 0.43 ms。<b>60 fps の予算 16.7 ms に対して
    /// 3% 未満</b>なので、fps が落ちるならエンジンの外を疑うこと
    /// （実測は Assets/Editor/BulletSmokeCheck.cs で出せる）。
    /// </summary>
    public int targetFps = -1;

    private Enemy enemy;
    private Player player;
    private readonly ReactiveProperty<string> StatusTextRp = new("");

    // fps を数えるための控え。**Update で毎コマ 数える。**
    //
    // 前は 0.5 秒 に 1 回 `1f / Time.unscaledDeltaTime` を読んでいた。あれは
    // **その瞬間の 1 コマ の長さ**であって平均ではない。1 コマ でも長いものが
    // サンプリング点に当たれば、そのまま低い数字が出る ——
    // **「40 前後しか出ない」の「前後」は、その振れ幅を見ていた可能性がある。**
    // ここは期間内のコマ数を数えて割る（本当の平均）。
    int frames;
    float elapsed;
    /// 直近 0.5 秒 の平均 fps
    float fps;
    /// 直近 0.5 秒 でいちばん長かったコマ（ms）。**平均だけだと引っかかりが消える**
    float worstMs;
    float worstInWindow;

    void Awake()
    {
        Application.targetFrameRate = targetFps;
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
                        // Update が数えた平均。**ここで割り算しない**
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

        // 上限を掛け直せるボタン。**既定は無制限。**
        // 掛けたときと外したときで数が変わるかを、その場で見比べるため
        if (Application.isPlaying)
        {
            var capped = Application.targetFrameRate > 0;
            if (GUI.Button(new Rect(360, 5, 80, 22), capped ? "上限を外す" : "上限 60"))
            {
                Application.targetFrameRate = capped ? -1 : 60;
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
        // **上限と、いちばん長かったコマも並べて出す。**
        //
        // 数だけだと「これしか出ない」と読める —— 上限に張り付いているのか、
        // 届いていないのかが分からない。さらに平均だけだと、たまに 1 コマ
        // 引っかかる形（GC やアセットの読み込み）が消える。
        //
        // **読み方**: 平均が上限どおりで最悪も予算内なら、出るべきものは出ている。
        // 平均が低いなら継続的に重い。平均は出ていて最悪だけ大きいなら、
        // どこかで 1 コマ だけ止まっている
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
