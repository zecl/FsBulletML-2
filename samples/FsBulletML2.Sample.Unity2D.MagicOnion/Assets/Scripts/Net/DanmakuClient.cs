using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Net.Http;
using FsBulletML2.Sample.MagicOnion.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using UnityEngine;

/// <summary>
/// サーバーと繋がっている 1 本。<b>この面 が、このサンプルで唯一 網 を知る。</b>
///
/// <b>受けるのは別のスレッド。</b> MagicOnion の受け口（<c>OnFrame</c>）は
/// Unity の主スレッドでは呼ばれないので、**そこで Unity の API を触ると落ちる。**
/// いちばん新しいコマを置いておいて、<c>Update</c> が取りに来る形にしてある。
///
/// <b>置き換えでよいのは、形 A（毎コマ の並び）だから。</b>
/// 1 コマ が丸ごと「いま在る弾 全部」なので、遅れて 2 コマ 溜まったら
/// 古いほうは捨ててよい。**生まれた合図だけを送る形（B）だと捨てられない**
/// —— 捨てた合図の弾が、以後 ずっと出てこないことになる。
/// </summary>
public sealed class DanmakuClient : MonoBehaviour
{
    public static DanmakuClient Instance { get; private set; }

    [Tooltip("h2c。証明書 は要らない")]
    public string Host = "http://127.0.0.1:5170";

    [Tooltip("自機 の位置を、何コマ に 1 回 送るか")]
    public int PlayerSendInterval = 1;

    GrpcChannel channel;
    IDanmakuHub hub;
    IDanmakuService service;
    Receiver receiver;
    int sendCountdown;

    /// <summary>入っている部屋 の決めごと。<b>繋がるまで null</b></summary>
    public RoomInfo Room { get; private set; }

    /// <summary>走らせられる弾幕の名前。<b>サーバーが持っている一覧</b></summary>
    public string[] Names { get; private set; } = Array.Empty<string>();

    public bool Connected => hub != null && Room != null;

    /// <summary>いちばん新しいコマの番号。<b>止まっていないことの印</b></summary>
    public int Frame { get; private set; }

    /// <summary>捨てたコマの数。<b>描く側 が追いついていないことの印</b></summary>
    public int Dropped { get; private set; }

    /// <summary>
    /// サーバーが数えた当たりの総数。<b>client は判定 を 1 つ も持たない。</b>
    ///
    /// 画面 に出しているのは、**判定 が届いていることを目 で確かめられる
    /// ようにするため** —— 当たっても弾が消えるだけなので、
    /// 出さないと「判定 が無い」と「当たっていない」が見分けられない。
    /// </summary>
    public int PlayerHits { get; private set; }

    public int EnemyHits { get; private set; }

    void Awake()
    {
        Instance = this;
        receiver = new Receiver();
    }

    async void Start()
    {
        try
        {
            // **h2c。** 証明書 を要求すると動かし方が 1 段 増える。
            // YetAnotherHttpHandler が要るのは、Unity の素 の HttpClient が
            // HTTP/2 を喋れないから（プラットフォームによっては喋るが、揃わない）
            var handler = new YetAnotherHttpHandler { Http2Only = true };
            channel = GrpcChannel.ForAddress(Host, new GrpcChannelOptions { HttpHandler = handler });

            service = MagicOnionClient.Create<IDanmakuService>(channel);
            Names = await service.ListAsync();

            hub = await StreamingHubClient.ConnectAsync<IDanmakuHub, IDanmakuHubReceiver>(channel, receiver);
            Room = await hub.JoinAsync(new JoinRequest());
            PlaceEnemy();

            Debug.Log($"[Danmaku] {Room.Name} / {Room.Fps} コマ毎秒 / 弾幕 {Names.Length} 本");
        }
        catch (Exception ex)
        {
            // **黙って繋がらないことにしない。** 絵 が出ないだけだと、
            // 配線かエンジンか描く側 かが分からない
            Debug.LogError($"[Danmaku] {Host} に繋がらない: {ex.Message}");
        }
    }

    void Update()
    {
        if (!Connected)
        {
            return;
        }

        var frame = receiver.Take(out int dropped, out int playerHits, out int enemyHits);
        Dropped += dropped;

        if (frame != null)
        {
            Frame = frame.Frame;
            BulletEntityFactory.Apply(frame.Bullets, Room);
        }

        ApplyHits(playerHits, enemyHits);

        SendPlayer();
    }

    /// <summary>
    /// 当たった数を場面 へ流す。<b>数えるのはサーバー。</b>
    /// ここは「何回 当たったか」を演出へ渡すだけで、判定 を 1 つ も持たない。
    /// </summary>
    void ApplyHits(int playerHits, int enemyHits)
    {
        PlayerHits += playerHits;
        EnemyHits += enemyHits;

        var player = BulletEcsRuntime.Player;
        if (player != null)
        {
            for (int i = 0; i < playerHits; i++)
            {
                player.HitByEnemyBullet();
            }
        }

        var enemy = BulletEcsRuntime.Enemy;
        if (enemy != null)
        {
            for (int i = 0; i < enemyHits; i++)
            {
                enemy.HitByPlayerBullet();
            }
        }
    }

    /// <summary>
    /// 自機 の位置を送る。<b>毎コマ 送る。</b>
    ///
    /// <c>aim</c> が読むだけなら 3 コマ に 1 回 でも見え方は変わらなかったが、
    /// **この位置は当たり判定 の入力 にもなった。**
    /// 間引くと、その間に動いたぶんが判定 に映らない。
    ///
    /// 上り は 1 コマ 8 バイト ＝ 60 コマ毎秒 で 4 kbps。
    /// 下り の 2.8 Mbps に対して**測るまでもなく無視できる。**
    /// </summary>
    void SendPlayer()
    {
        var player = BulletEcsRuntime.Player;
        if (player == null)
        {
            return;
        }

        if (--sendCountdown > 0)
        {
            return;
        }

        sendCountdown = Mathf.Max(1, PlayerSendInterval);

        var p = player.transform.position;
        // **待たない。** 待つと自機 の動きが往復 の遅れに引きずられる
        _ = hub.SetPlayerAsync(p.x, p.y);
    }

    /// <summary>
    /// 敵 を、サーバーが言った弾の出どころ へ置く。
    ///
    /// <b>逆をやらない。</b> 場面 に置いてある敵 の位置をサーバーへ教える形にすると、
    /// client が 2 人 居たときにどちらの言い分を採るかが決まらない。
    /// **サーバーが空間 を決める**ほうへ揃える。
    ///
    /// 置かないと、弾の出どころ と敵 の絵 がずれる（0.4 ずれていた）。
    /// </summary>
    void PlaceEnemy()
    {
        var enemy = BulletEcsRuntime.Enemy;
        if (enemy == null || Room == null)
        {
            return;
        }

        var p = enemy.transform.position;
        enemy.transform.position = new Vector3(Room.OriginX, Room.OriginY, p.z);
    }

    /// <summary>
    /// 自機 の弾 を撃つ。<b>撃つのはサーバー。</b>
    /// ここが渡すのは位置だけで、何発 出るかも どう飛ぶかも向こうが決める。
    ///
    /// <b>待たない。</b> 待つと自機 の動きが往復 の遅れに引きずられる。
    /// 撃てたかどうかは次のコマの並びに出る。
    /// </summary>
    public void Shoot(float x, float y)
    {
        if (hub == null)
        {
            return;
        }

        _ = hub.ShootAsync(x, y);
    }

    /// <summary>
    /// 走らせる弾幕を替える。<b>出て、入り直す。</b>
    /// 部屋 は「弾幕 と 種」で決まるので、入り直すと別の部屋 になる。
    /// </summary>
    public async void Switch(string bulletml)
    {
        if (hub == null)
        {
            return;
        }

        try
        {
            BulletEntityFactory.DestroyAllEnemy();
            await hub.LeaveAsync();
            Room = await hub.JoinAsync(new JoinRequest { Bulletml = bulletml });
            PlaceEnemy();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Danmaku] 弾幕 を替えられなかった: {ex.Message}");
        }
    }

    async void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        try
        {
            if (hub != null)
            {
                await hub.DisposeAsync();
                hub = null;
            }

            channel?.Dispose();
            channel = null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Danmaku] 切るときに: {ex.Message}");
        }
    }

    /// <summary>
    /// 降ってきたコマを 1 つ だけ持つ。<b>溜めない。</b>
    ///
    /// 溜めると、描く側 が遅れたぶんだけ古いコマを順に描くことになり、
    /// **遅れが返ってこない。** いちばん新しい 1 つ を残して捨てる。
    /// </summary>
    sealed class Receiver : IDanmakuHubReceiver
    {
        FrameDto latest;
        int dropped;
        int pendingPlayerHits;
        int pendingEnemyHits;

        public void OnFrame(FrameDto frame)
        {
            // **Interlocked で置き換える。** ここは Unity の主スレッドではない
            var previous = Interlocked.Exchange(ref latest, frame);
            if (previous == null)
            {
                return;
            }

            Interlocked.Increment(ref dropped);

            // **当たりは捨てられない。** 位置は「いまどこか」なので古いコマを
            // 落としてよいが、**当たりは出来事**で、落とすとその 1 発 が
            // 無かったことになる（サーバー側 で「撃ち」と「位置」を
            // 別 に扱ったのと同じ分かれ目）。
            Interlocked.Add(ref pendingPlayerHits, previous.PlayerHits);
            Interlocked.Add(ref pendingEnemyHits, previous.EnemyHits);
        }

        public FrameDto Take(out int droppedSinceLast, out int playerHits, out int enemyHits)
        {
            droppedSinceLast = Interlocked.Exchange(ref dropped, 0);
            playerHits = Interlocked.Exchange(ref pendingPlayerHits, 0);
            enemyHits = Interlocked.Exchange(ref pendingEnemyHits, 0);

            var frame = Interlocked.Exchange(ref latest, null);
            if (frame != null)
            {
                playerHits += frame.PlayerHits;
                enemyHits += frame.EnemyHits;
            }

            return frame;
        }
    }
}
