using System;
using FsBulletML2.Sample.MagicOnion.Shared;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// ゲームが見る面。網は知らない。2 本目が要るようになったら、ここが最初に壊れる。
    /// </summary>
    public static class DanmakuState
    {
        /// <summary>入っている部屋 の決めごと。繋がるまで null</summary>
        public static readonly Live<RoomInfo> Room = new(null);

        /// <summary>走らせられる弾幕 の名前。サーバーが持っている一覧</summary>
        public static readonly Live<string[]> Names = new(Array.Empty<string>());

        /// <summary>いちばん新しいコマ の番号。止まっていないことの印</summary>
        public static readonly Live<int> Frame = new(0);

        /// <summary>いま在る弾 の数。描く側 が持っている数 ではなく、届いた数</summary>
        public static readonly Live<int> Bullets = new(0);

        /// <summary>
        /// まばらに見えたとき、落としたのか重なっているのか盤面の外なのか、目では割れない。
        /// </summary>
        public static readonly Live<int> Drawn = new(0);

        public static readonly Live<int> OnScreen = new(0);

        /// <summary>捨てたコマ の数。描く側 が追いついていないことの印</summary>
        public static readonly Live<int> Dropped = new(0);

        /// <summary>番号 が飛んだ数。捨てた（追いつけない）とは別 —— 来ていない</summary>
        public static readonly Live<int> Gaps = new(0);

        /// <summary>サーバーが数えた当たり の総数。client は判定 を 1 つ も持たない</summary>
        public static readonly Live<int> PlayerHits = new(0);

        public static readonly Live<int> EnemyHits = new(0);

        /// <summary>繋がっているか</summary>
        public static readonly Live<bool> Connected = new(false);

        /// <summary>繋がらなかった理由。空 なら まだ何も起きていない</summary>
        public static readonly Live<string> Error = new("");

        /// <summary>
        /// 主スレッドで流す。受け口は別スレッドなので、直に流すと Godot の API を触れない。
        /// </summary>
        public static readonly Stream<FrameDto> Frames = new();

        /// <summary>
        /// 建て直したときに戻す。static なので走行 をまたいで残る ——
        /// エディタ で再生 し直すと、前 の走行 の数 が見えたままになる。
        /// </summary>
        public static void Reset()
        {
            Room.Value = null;
            Names.Value = Array.Empty<string>();
            Frame.Value = 0;
            Bullets.Value = 0;
            Dropped.Value = 0;
            Gaps.Value = 0;
            PlayerHits.Value = 0;
            EnemyHits.Value = 0;
            Connected.Value = false;
            Error.Value = "";
        }
    }
}
