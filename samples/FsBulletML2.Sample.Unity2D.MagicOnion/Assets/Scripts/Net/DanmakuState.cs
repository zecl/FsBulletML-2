using System;
using FsBulletML2.Sample.MagicOnion.Shared;
using R3;

/// <summary>
/// ゲームが見る面。
/// 建つ順 が外から見えないのが理由 だったので、 2 本 目 が要るようになったら、ここが最初に壊れる。
/// </summary>
public static class DanmakuState
{
    /// <summary>入っている部屋 の決めごと。繋がるまで null</summary>
    public static readonly ReactiveProperty<RoomInfo> Room = new(null);

    /// <summary>走らせられる弾幕の名前。サーバーが持っている一覧</summary>
    public static readonly ReactiveProperty<string[]> Names = new(Array.Empty<string>());

    /// <summary>いちばん新しいコマの番号。止まっていないことの印</summary>
    public static readonly ReactiveProperty<int> Frame = new(0);

    /// <summary>捨てたコマの数。描く側 が追いついていないことの印</summary>
    public static readonly ReactiveProperty<int> Dropped = new(0);

    /// <summary>サーバーが数えた当たりの総数。client は判定 を 1 つ も持たない</summary>
    public static readonly ReactiveProperty<int> PlayerHits = new(0);

    public static readonly ReactiveProperty<int> EnemyHits = new(0);

    /// <summary>繋がっているか</summary>
    public static readonly ReactiveProperty<bool> Connected = new(false);

    /// <summary>
    /// 降りてきた 1 コマ。
    /// </summary>
    public static Observable<FrameDto> Frames => frames;

    internal static readonly Subject<FrameDto> frames = new();

    /// <summary>
    /// 場面 を建て直したときに戻す。static なので走行をまたいで残る ——
    /// Editor で再生し直すと、前 の走行の数 が見えたままになる。
    /// </summary>
    internal static void Reset()
    {
        Room.Value = null;
        Names.Value = Array.Empty<string>();
        Frame.Value = 0;
        Dropped.Value = 0;
        PlayerHits.Value = 0;
        EnemyHits.Value = 0;
        Connected.Value = false;
    }
}