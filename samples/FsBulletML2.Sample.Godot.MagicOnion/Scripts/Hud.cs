using System;
using System.Collections.Generic;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 数 を出して、弾幕 を選ばせる。
    /// </summary>
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

            // 端末 のフォント を借りる。
            // 並べた名前 は上 から順 に探されて、無ければ次 へ落ちる
            var font = new SystemFont
            {
                FontNames = new[] { "Yu Gothic UI", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Noto Sans JP" },
            };

            stats = MakeLabel(font, 13);
            stats.Position = new Vector2(8, 6);

            // アンカー で下 に貼らない。
            choice = MakeLabel(font, 13);

            help = MakeLabel(font, 11);
            help.Modulate = new Color(1f, 1f, 1f, 0.55f);
            help.Text = "← → ↑ ↓ 動く   Z 撃つ   , . / Enter 次へ";

            AddChild(stats);
            AddChild(choice);
            AddChild(help);

            // 別々に書き分けると行が入れ替わる。部屋が変わっても index は触らない。
            // 名前で合わせると、既定の位置へ跳ぶ。
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
        /// Unity 版 と同じ。
        /// `_UnhandledInput` では Enter が届かない。
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
                    // 届いた数 と 描いた数 と 画面 の中 の数。
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
