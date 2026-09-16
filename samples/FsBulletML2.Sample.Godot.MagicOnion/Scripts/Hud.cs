using System;
using System.Collections.Generic;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 数 を出して、弾幕 を選ばせる。網 を 1 つ も知らない。
    ///
    /// 見るのは <see cref="DanmakuState"/> だけ で、
    ///
    /// 別 の Node に分けると「出す側」と「選ぶ側」の 2 か所 に index が要る。
    /// 頼む先 だけ を外（<see cref="Client"/>）に持つ。
    public sealed partial class Hud : Control
    {
        public DanmakuClient Client { get; set; }

        Label stats;
        Label choice;
        Label help;

        string[] names = Array.Empty<string>();
        int index;
        readonly List<IDisposable> subs = new();
        bool commaHeld;
        bool periodHeld;
        bool enterHeld;

        public override void _Ready()
        {
            SetAnchorsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Ignore;

            // 端末 のフォント を借りる。 Godot の既定 フォント は
            // 日本語 のグリフ を持たないので、字 が全部 豆腐 になる ——
            // 同梱 すると sample に数 MB の font が付く。
            // 並べた名前 は上 から順 に探されて、無ければ次 へ落ちる
            var font = new SystemFont
            {
                FontNames = new[] { "Yu Gothic UI", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Noto Sans JP" },
            };

            stats = MakeLabel(font, 13);
            stats.Position = new Vector2(8, 6);

            // アンカー で下 に貼らない。 `SetAnchorsPreset` は offset を
            // 書き換えるので、後 から Position を書いても効かない（1 度 踏んだ
            // —— 2 行 とも画面 の外 に居て、撮った 1 枚 で分かった）。
            // 高さ から引いて置くほう が、読む人 にも位置 が見える
            choice = MakeLabel(font, 13);

            help = MakeLabel(font, 11);
            help.Modulate = new Color(1f, 1f, 1f, 0.55f);
            help.Text = "← → ↑ ↓ 動く   Z 撃つ   , . / Enter 次へ";

            AddChild(stats);
            AddChild(choice);
            AddChild(help);

            // 1 つ でも変われば書き直す。 別々 に書き分けると、
            // 「どれ が最後 に書いたか」で行 が入れ替わる
            // 部屋 が変わっても index は触らない。 Unity 版 と同じで、
            // 番号 はこっち の持ち物。部屋 の名前 で合わせると、既定 の
            // 「全方位弾」の位置 へ跳んで、1 番 から順 にならない
            subs.Add(DanmakuState.Room.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Frame.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Bullets.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.OnScreen.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Dropped.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Gaps.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.PlayerHits.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.EnemyHits.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Error.Subscribe(_ => Redraw()));
            subs.Add(DanmakuState.Names.Subscribe(OnNames));
        }

        static Label MakeLabel(Font font, int size)
        {
            var label = new Label { MouseFilter = MouseFilterEnum.Ignore };
            label.AddThemeFontOverride("font", font);
            label.AddThemeFontSizeOverride("font_size", size);
            label.AddThemeColorOverride("font_color", new Color(0.91f, 0.91f, 0.94f));

            // 地 が暗い とは限らない。 影 を敷いておくと、
            // 弾 が白 で埋まった上 でも字 が読める
            label.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.8f));
            label.AddThemeConstantOverride("shadow_offset_x", 1);
            label.AddThemeConstantOverride("shadow_offset_y", 1);
            return label;
        }

        void OnNames(string[] value)
        {
            names = value ?? Array.Empty<string>();
            if (names.Length == 0)
            {
                index = 0;
            }
            else if (index >= names.Length)
            {
                index = 0;
            }

            Redraw();
        }

        /// <summary>
        /// Unity 版 と同じ。番号 はこっち が持つ。Enter は即 次。
        ///
        /// `_UnhandledInput` では Enter が届かない。 Hud は画面 いっぱい の
        /// Control で、Godot の既定 では Enter が `ui_accept` として GUI が
        /// 先に取る。矢印 と Z は Player が `_Process` で直に見ているのと同じ口。
        /// </summary>
        public override void _Process(double delta)
        {
            if (names.Length == 0)
            {
                return;
            }

            bool comma = Pressed(Key.Comma);
            bool period = Pressed(Key.Period);
            bool enter = Pressed(Key.Enter) || Pressed(Key.KpEnter);

            if (comma && !commaHeld)
            {
                Prev();
            }
            else if ((period && !periodHeld) || (enter && !enterHeld))
            {
                Next();
            }

            commaHeld = comma;
            periodHeld = period;
            enterHeld = enter;
        }

        static bool Pressed(Key key) =>
            Input.IsPhysicalKeyPressed(key) || Input.IsKeyPressed(key);

        void Next()
        {
            index = (index + 1) % names.Length;
            Apply();
        }

        void Prev()
        {
            index = (index - 1 + names.Length) % names.Length;
            Apply();
        }

        /// <summary>
        /// 番号 で頼む。名前は日本語の説明文 なので、Unity 版 と同じ口。
        /// </summary>
        void Apply()
        {
            Client?.Switch(index.ToString());
            Redraw();
        }

        void Redraw()
        {
            var room = DanmakuState.Room.Value;
            var error = DanmakuState.Error.Value;

            if (room == null)
            {
                stats.Text = error != "" ? $"× {error}" : "繋いでいます…";
            }
            else
            {
                stats.Text = string.Join('\n',
                    room.Name,
                    $"進める {room.Fps} /秒・配る {room.Fps / Math.Max(1, room.SendEvery)} /秒   空間 {room.Space}",
                    // 届いた数 と 描いた数 と 画面 の中 の数。 絵 がまばら に
                    // 見えたとき、この 3 つ が揃っていれば落としてはいない
                    // （重なっているか、盤面 の外 へ出ている）
                    $"コマ {DanmakuState.Frame.Value}   弾 {DanmakuState.Bullets.Value}" +
                        $" 描 {DanmakuState.Drawn.Value} 画面 {DanmakuState.OnScreen.Value}",
                    // 「捨てた」と「来ていない」を分けて出す。
                    // 畳むと、描く側 が遅いのか 網 が落としたのかが読めない
                    $"捨てた {DanmakuState.Dropped.Value}   飛び {DanmakuState.Gaps.Value}",
                    // 当たり はサーバーが数えた数。 client は判定 を 1 つ も持たない
                    $"当たり 自機へ {DanmakuState.PlayerHits.Value}   敵へ {DanmakuState.EnemyHits.Value}");
            }

            choice.Text = names.Length == 0
                ? ""
                : $"[{index + 1}/{names.Length}] {names[index]}";

            // 重ならないように積む。 下 から help、その上 に choice。
            // 窓 の大きさ は変わりうるので毎回 引き直す
            float bottom = GetViewportRect().Size.Y;
            float helpH = help.GetMinimumSize().Y;
            help.Position = new Vector2(8, bottom - helpH - 6);
            choice.Position = new Vector2(8, bottom - helpH - choice.GetMinimumSize().Y - 10);
        }

        public override void _ExitTree()
        {
            foreach (var sub in subs)
            {
                sub.Dispose();
            }

            subs.Clear();
        }
    }
}
