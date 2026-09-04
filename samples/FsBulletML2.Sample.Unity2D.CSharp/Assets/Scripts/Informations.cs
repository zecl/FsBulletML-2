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
    /// フレームレートの上限。<b>これは設計値であって、出せる上限ではない。</b>
    ///
    /// 「40 fps しか出ない」と読み違えやすいので、下の表示に上限も並べてある
    /// （実際に読み違えた）。<b>重さを見たいときは上限を外すこと</b> ——
    /// 画面の「無制限」ボタンか、ここを -1 にする。
    ///
    /// 参考: エンジン（Runner.StepWith）が 1 コマ に使うのは弾 121 本 で
    /// 0.12 ms、ホーミング 25 本 で 0.43 ms。<b>40 fps の予算 25 ms に対して
    /// 2% 未満</b>なので、fps が落ちるならエンジンの外を疑うこと
    /// （実測は Assets/Editor/BulletSmokeCheck.cs で出せる）。
    /// </summary>
    public int targetFps = 40;

    private Enemy enemy;
    private Player player;
    private readonly ReactiveProperty<string> StatusTextRp = new("");

    void Awake()
    {
        Application.targetFrameRate = targetFps;
        enemy = FindAnyObjectByType<Enemy>();
        player = FindAnyObjectByType<Player>();
        useGUILayout = false;
    }

    public void Start()
    {
        if (!Application.isPlaying || enemy == null || player == null)
        {
            return;
        }

        var token = destroyCancellationToken;
        var stats = Observable.Return((enemy: 0, player: 0, fps: 0f))
            .Concat(
                Observable.Interval(TimeSpan.FromSeconds(0.5), token)
                    .Select(_ => (
                        enemy: BulletEntityFactory.EnemyCount,
                        player: BulletEntityFactory.PlayerCount,
                        fps: 1f / Time.unscaledDeltaTime
                    )));

        enemy.BulletNameRp
            .CombineLatest(enemy.LifeRp, (name, life) => (name, life))
            .CombineLatest(player.DamageRp, (x, dmg) => (name: x.name, life: x.life, dmg: dmg))
            .CombineLatest(stats, (x, s) => FormatStatus(x.name, x.life, x.dmg, s.fps, s.enemy, s.player))
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

        // **上限を外して測るためのボタン。** 「40 しか出ない」が上限のせいか
        // 本当に重いのかは、外してみないと割れない
        if (Application.isPlaying)
        {
            var capped = Application.targetFrameRate > 0;
            if (GUI.Button(new Rect(360, 5, 80, 22), capped ? "上限を外す" : "上限 " + targetFps))
            {
                Application.targetFrameRate = capped ? -1 : targetFps;
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
            return FormatStatus(enemy.BulletName, enemy.Life, player.Damage, 0f, 0, 0);
        }
        return StatusTextRp.Value;
    }

    private static string FormatStatus(string name, int life, int damage, float fps, int enemyBullets, int playerBullets)
    {
        var sb = new StringBuilder();
        // **上限も並べて出す。** 数だけだと「これしか出ない」と読める ——
        // 上限に張り付いているのか、届いていないのかが 1 行 で分かるように
        var cap = Application.targetFrameRate;
        sb.Append(string.Format("FPS:{0:F2}fps（上限 {1}）\n",
                                fps, cap > 0 ? cap.ToString() : "無制限"));
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
