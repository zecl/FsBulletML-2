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
    /// <b>以前は 40 だった。</b> 意図した設計ではなく、そのまま残っていた
    /// だけだった。いちど 外して実力を見たら 60 では収まらない量が出たので、
    /// <b>60 で頭を押さえる</b>ことにした（弾幕の見た目を一定にするため）。
    ///
    /// 表示には上限も並べてある —— 数だけだと「これしか出ない」と
    /// 「上限に張り付いている」を見分けられない（実際に読み違えた）。
    /// <b>重さを見たいときは画面のボタンで外すこと。</b>
    ///
    /// 参考: エンジン（Runner.StepWith）が 1 コマ に使うのは弾 121 本 で
    /// 0.12 ms、ホーミング 25 本 で 0.43 ms。<b>60 fps の予算 16.7 ms に対して
    /// 3% 未満</b>なので、fps が落ちるならエンジンの外を疑うこと
    /// （実測は Assets/Editor/BulletSmokeCheck.cs で出せる）。
    /// </summary>
    public int targetFps = DefaultTargetFps;

    /// <summary>既定の上限。<see cref="ApplyCapOnPlay"/> が Play の頭で使う</summary>
    public const int DefaultTargetFps = 60;

    /// <summary>
    /// <b>Play に入った時点で上限を掛ける。</b>
    ///
    /// これを <c>Informations.Awake</c> だけに任せると、<b>シーンに
    /// Informations が居ること</b>と<b>その Awake が先に走ること</b>に依存する。
    /// どちらも外から見て分からないので、シーンに何が居ようが効く場所へ出した。
    ///
    /// Informations がシーンに居れば、そのあと Awake が inspector の値で
    /// 上書きする（既定は同じ 60）。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyCapOnPlay()
    {
        ApplyCap(DefaultTargetFps);
    }

    /// <summary>
    /// 上限の掛け方。<b>vSync が先。</b>
    ///
    /// vSyncCount が 1 以上 だと Unity は targetFrameRate を無視して画面の
    /// リフレッシュレートに従う。品質設定「Good」では 1 なので、
    /// <b>切らないと 60 に押さえられない</b>（実際に押さえられなかった）。
    /// </summary>
    static void ApplyCap(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

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
        var stats = Observable.Return((enemy: 0, player: 0, fps: 0f, worst: 0f, hitEnemy: 0, hitPlayer: 0))
            .Concat(
                Observable.Interval(TimeSpan.FromSeconds(0.5), token)
                    .Select(_ => (
                        enemy: BulletEntityFactory.EnemyCount,
                        player: BulletEntityFactory.PlayerCount,
                        // Update が数えた平均。**ここで割り算しない**
                        fps: this.fps,
                        worst: this.worstMs,
                        // **当たりはサーバーが数えている。** ここに出しているのは、
                        // 判定 が届いていることを目 で確かめるため
                        hitEnemy: DanmakuClient.Instance != null ? DanmakuClient.Instance.EnemyHits : 0,
                        hitPlayer: DanmakuClient.Instance != null ? DanmakuClient.Instance.PlayerHits : 0
                    )));

        enemy.BulletNameRp
            .CombineLatest(enemy.LifeRp, (name, life) => (name, life))
            .CombineLatest(player.DamageRp, (x, dmg) => (name: x.name, life: x.life, dmg: dmg))
            .CombineLatest(stats, (x, s) => FormatStatus(x.name, x.life, x.dmg, s.fps, s.worst, s.enemy, s.player, s.hitEnemy, s.hitPlayer))
            .Subscribe(text => StatusTextRp.Value = text)
            .AddTo(this);
    }

    public void OnGUI()
    {
        if ((Application.isPlaying && show) || (!Application.isPlaying && showInEditor))
        {
            GUI.Box(new Rect(5, 30, 475, 112), "");
            GUI.Label(new Rect(10, 30, 1000, 200), GetShowText());
        }

        if (GUI.Button(new Rect(445, 35, 25, 22), show ? "-" : "+"))
        {
            this.show = !this.show;
        }

        // 上限を外せるボタン。**既定は 60 で掛かっている。**
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
            return FormatStatus(enemy.BulletName, enemy.Life, player.Damage, 0f, 0f, 0, 0, 0, 0);
        }
        return StatusTextRp.Value;
    }

    private static string FormatStatus(string name, int life, int damage, float fps, float worstMs,
                                       int enemyBullets, int playerBullets,
                                       int hitEnemy, int hitPlayer)
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
        // **当たりはサーバーが判定して数えている。** client は 1 つ も持たない ——
        // 当たった弾はその場で並びから消えるので、出さないと
        // 「判定 が効いていない」と「当たっていない」が見分けられない
        sb.Append(string.Format("Hits（server）:敵へ {0} / 自機へ {1}\n", hitEnemy, hitPlayer));
        return sb.ToString();
    }

    void OnDestroy()
    {
        StatusTextRp.Dispose();
    }
}
