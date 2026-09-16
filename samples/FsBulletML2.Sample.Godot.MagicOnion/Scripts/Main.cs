using System;
using System.Collections.Generic;
using System.Globalization;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 場面 を組んで、引数 を配る。
    ///
    /// 場面（`.tscn`）に並べない。 Unity 版 も同じ形 で、あちら は
    /// 「場面 を 1 行 も触っていない」と書いてある —— 組み方 がコード に在ると、
    /// 何 が何 を見ているか を読む場所 が 1 か所 になる。
    /// ## 測る口 を持っている
    /// <code>
    /// --frames &lt;n&gt;   n コマ 受けたら数 を出して終わる（headless で回せる）
    public sealed partial class Main : Node2D
    {
        DanmakuClient client;
        Hud hud;

        readonly List<int> counts = new();
        int wantFrames;
        string shotPath = "";
        bool shotPending;
        IDisposable frameSub;

        public override void _Ready()
        {
            var args = OS.GetCmdlineUserArgs();

            string host = Arg(args, "--host") ?? "http://127.0.0.1:5170";
            string bulletml = Arg(args, "--bulletml") ?? "";
            int seed = int.TryParse(Arg(args, "--seed"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : 0;
            wantFrames = int.TryParse(Arg(args, "--frames"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var f) ? f : 0;
            shotPath = Arg(args, "--shot") ?? "";

            // 撮るには待つ コマ数 が要る。 言われていなければ 120 コマ
            // （繋いで、弾 が画面 に散るまで）
            if (shotPath != "" && wantFrames <= 0)
            {
                wantFrames = 120;
            }

            client = new DanmakuClient
            {
                Name = "DanmakuClient",
                Host = host,
                Bulletml = bulletml,
                Seed = seed,
            };

            var field = new BulletField { Name = "BulletField" };
            var enemy = new Enemy { Name = "Enemy" };
            var player = new Player { Name = "Player", Client = client, Start = ParsePoint(Arg(args, "--player")) };
            hud = new Hud { Name = "Hud", Client = client };

            // 自機 の位置 は「引きに来てもらう」。 client が場面 を探しに行くと、
            // 建つ順 に依る形 が戻ってくる
            client.PlayerPosition = () => player.Field;

            AddChild(client);
            AddChild(field);
            AddChild(enemy);
            AddChild(player);
            AddChild(hud);

            if (wantFrames > 0)
            {
                frameSub = DanmakuState.Frames.Subscribe(OnFrame);
            }
        }

        void OnFrame(FrameDto frame) => counts.Add(frame.Bullets?.Length ?? 0);

        public override void _Process(double delta)
        {
            if (wantFrames <= 0)
            {
                return;
            }

            // 1 コマ 置いてから撮る。 `_Process` の中 で引くと、
            // いま のコマ はまだ描かれていない（前 のコマ が返る）
            if (shotPending)
            {
                Save(shotPath);
                Finish();
                return;
            }

            if (counts.Count < wantFrames)
            {
                return;
            }

            if (shotPath != "")
            {
                shotPending = true;
                return;
            }

            Finish();
        }

        void Save(string path)
        {
            var image = GetViewport().GetTexture().GetImage();
            var error = image.SavePng(path);
            if (error != Error.Ok)
            {
                GD.PushError($"[Danmaku] {path} に書けない: {error}");
                return;
            }

            GD.Print($"[Danmaku] {path} に {image.GetWidth()}x{image.GetHeight()} を撮った");
        }

        /// <summary>
        /// 数 を出して終わる。console client と同じ 2 行。
        ///
        /// 「何コマ 来たか」だけ では足りない。 番号 の飛び と、
        /// 弾数 の山 と中央値 まで出す —— 揃えてあるので、
        /// 同じ種・同じ弾幕 なら console client と突き合わせられる。
        /// </summary>
        void Finish()
        {
            frameSub?.Dispose();

            if (counts.Count == 0)
            {
                GD.PrintErr("[Danmaku] 1 コマ も来なかった");
                GetTree().Quit(1);
                return;
            }

            var sorted = counts.ToArray();
            Array.Sort(sorted);

            long sum = 0;
            foreach (var c in counts)
            {
                sum += c;
            }

            GD.Print(string.Format(
                CultureInfo.InvariantCulture,
                "[Danmaku] {0} コマ 受けた（飛び {1}）。弾数 中央値 {2} / 山 {3} / 平均 {4:0.0}",
                counts.Count, DanmakuState.Gaps.Value,
                sorted[sorted.Length / 2], sorted[sorted.Length - 1],
                (double)sum / counts.Count));

            GD.Print($"[Danmaku] 当たり 敵へ {DanmakuState.EnemyHits.Value} 発 / 自機へ {DanmakuState.PlayerHits.Value} 発");

            GetTree().Quit(counts.Count < wantFrames ? 1 : 0);
        }

        /// <summary>
        /// <c>x,y</c> を読む。読めなければ null（既定 の置き場 へ落ちる）。
        ///
        /// <c>InvariantCulture</c> で読む。 端末 の地方設定 が
        /// 小数点 に <c>,</c> を使う国 だと、区切り と小数点 が同じ字 になる
        /// —— そこ で割れると「なぜか 2 点目 が取れない」になる。
        /// </summary>
        static Vector2? ParsePoint(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var xy = text.Split(',');
            if (xy.Length == 2
                && float.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && float.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                return new Vector2(x, y);
            }

            GD.PushWarning($"[Danmaku] --player {text} を読めない（x,y）");
            return null;
        }

        static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.Ordinal))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        public override void _ExitTree() => frameSub?.Dispose();
    }
}
