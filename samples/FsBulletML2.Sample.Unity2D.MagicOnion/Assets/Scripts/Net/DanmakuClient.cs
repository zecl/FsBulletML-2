using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Net.Http;
using FsBulletML2.Sample.MagicOnion.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using R3;
using UnityEngine;

/// <summary>
/// サーバーと繋がっている 1 本。この面 が、このサンプルで唯一 網 を知る。
///
/// 外 へ出すのは <see cref="DanmakuState"/>（R3）だけ。
///
/// 2 か所 在ったのを、それで畳んだ。
/// 寿命 は UniTask で締める。 元 は <c>async void</c> だったので、
/// 落ちても誰も気づかず、component が壊れても走り続けた。
/// いまは <c>destroyCancellationToken</c> を通してあるので、
/// 場面 を抜けた時点で待ちが解ける。
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

    /// <summary>入っている部屋 の決めごと。繋がるまで null</summary>
    public RoomInfo Room => DanmakuState.Room.Value;

    public bool Connected => hub != null && Room != null;

    void Awake()
    {
        Instance = this;
        receiver = new Receiver();

        // 走行をまたいで残る値 を落とす。 static なので、Editor で
        // 再生し直すと前 の走行の数 が見えたままになる
        DanmakuState.Reset();
    }

    void Start()
    {
        // `async void` にしない。 落ちても誰も気づかず、
        // component が壊れても走り続ける。UniTask なら
        // destroyCancellationToken で解けて、例外 も Forget が拾う
        ConnectAsync(destroyCancellationToken).Forget();
    }

    /// <summary>
    /// 受けたコマ を主スレッド へ汲み直す。
    ///
    /// ここは R3 の <c>EveryUpdate</c> に置き換えない。
    /// あれは PlayerLoop の別の点 に刺さるので、ECS の system
    /// （<c>BulletSimulationSystem</c>）との前後 が動きうる ——
    /// 弾を並べるのが描く側 より後 になると 1 コマ 遅れる。
    void Update() => Pump();

    async UniTaskVoid ConnectAsync(CancellationToken token)
    {
        try
        {
            // h2c。 証明書 を要求すると動かし方が 1 段 増える。
            // YetAnotherHttpHandler が要るのは、Unity の素 の HttpClient が
            // HTTP/2 を喋れないから（プラットフォームによっては喋るが、揃わない）
            var handler = new YetAnotherHttpHandler { Http2Only = true };
            channel = GrpcChannel.ForAddress(Host, new GrpcChannelOptions { HttpHandler = handler });

            service = MagicOnionClient.Create<IDanmakuService>(channel);
            DanmakuState.Names.Value = await service.ListAsync();

            hub = await StreamingHubClient.ConnectAsync<IDanmakuHub, IDanmakuHubReceiver>(
                channel, receiver, cancellationToken: token);

            var room = await hub.JoinAsync(new JoinRequest());
            DanmakuState.Room.Value = room;
            DanmakuState.Connected.Value = true;
            PlaceEnemy(room);

            Debug.Log($"[Danmaku] {room.Name} / 進める {room.Fps} コマ毎秒・配る {room.Fps / Math.Max(1, room.SendEvery)} 回毎秒 / 弾幕 {DanmakuState.Names.Value.Length} 本");
        }
        catch (OperationCanceledException)
        {
            // 場面 を抜けた。これは失敗ではない
        }
        catch (Exception ex)
        {
            // 黙って繋がらないことにしない。 絵 が出ないだけだと、
            // 配線かエンジンか描く側 かが分からない
            Debug.LogError($"[Danmaku] {Host} に繋がらない: {ex.Message}");
        }
    }

    /// <summary>
    /// 降りてきたコマを主スレッド で汲む。ここだけが Unity の API を触る。
    /// </summary>
    void Pump()
    {
        if (!Connected)
        {
            return;
        }

        var frame = receiver.Take(out int dropped, out int playerHits, out int enemyHits);

        if (dropped > 0)
        {
            DanmakuState.Dropped.Value += dropped;
        }

        if (frame != null)
        {
            DanmakuState.Frame.Value = frame.Frame;
            BulletEntityFactory.Apply(frame.Bullets, Room);
            DanmakuState.frames.OnNext(frame);
        }

        if (playerHits > 0)
        {
            DanmakuState.PlayerHits.Value += playerHits;
        }

        if (enemyHits > 0)
        {
            DanmakuState.EnemyHits.Value += enemyHits;
        }

        ApplyHits(playerHits, enemyHits);
        SendPlayer();
    }

    /// <summary>
    /// 当たった数を場面 へ流す。数えるのはサーバー。
    /// ここは「何回 当たったか」を演出へ渡すだけで、判定 を 1 つ も持たない。
    /// </summary>
    void ApplyHits(int playerHits, int enemyHits)
    {
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
    /// 自機 の位置を送る。毎コマ 送る。
    ///
    /// <c>aim</c> が読むだけなら 3 コマ に 1 回 でも見え方は変わらなかったが、
    /// この位置は当たり判定 の入力 にもなった。
    /// 間引くと、その間に動いたぶんが判定 に映らない。
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
        // 待たない。 待つと自機 の動きが往復 の遅れに引きずられる。
        // `_ =` ではなく Forget —— 落ちたときに UniTask が拾って出す
        // `Task` には Forget が生えていない。 MagicOnion の口 は Task を返すので、
        // UniTask へ移してから投げっぱなしにする（`_ =` と違って、
        // 落ちたら UniTaskScheduler が拾って出す）
        hub.SetPlayerAsync(p.x, p.y).AsUniTask().Forget();
    }

    /// <summary>
    /// 敵 を、サーバーが言った弾の出どころ へ置く。
    ///
    /// 逆をやらない。 場面 に置いてある敵 の位置をサーバーへ教える形にすると、
    /// client が 2 人 居たときにどちらの言い分を採るかが決まらない。
    /// サーバーが空間 を決めるほうへ揃える。
    ///
    /// 置かないと、弾の出どころ と敵 の絵 がずれる（0.4 ずれていた）。
    /// </summary>
    static void PlaceEnemy(RoomInfo room)
    {
        var enemy = BulletEcsRuntime.Enemy;
        if (enemy == null || room == null)
        {
            return;
        }

        var p = enemy.transform.position;
        enemy.transform.position = new Vector3(room.OriginX, room.OriginY, p.z);
    }

    /// <summary>
    /// 自機 の弾 を撃つ。撃つのはサーバー。
    /// ここが渡すのは位置だけで、何発 出るかも どう飛ぶかも向こうが決める。
    ///
    /// 待たない。 撃てたかどうかは次のコマの並びに出る。
    /// </summary>
    public void Shoot(float x, float y)
    {
        if (hub == null)
        {
            return;
        }

        hub.ShootAsync(x, y).AsUniTask().Forget();
    }

    /// <summary>
    /// 走らせる弾幕を替える。出て、入り直す。
    /// 部屋 は「弾幕 と 種」で決まるので、入り直すと別の部屋 になる。
    /// </summary>
    public void Switch(string bulletml)
    {
        if (hub == null)
        {
            return;
        }

        SwitchAsync(bulletml, destroyCancellationToken).Forget();
    }

    async UniTaskVoid SwitchAsync(string bulletml, CancellationToken token)
    {
        try
        {
            DanmakuState.Connected.Value = false;
            BulletEntityFactory.DestroyAllEnemy();

            await hub.LeaveAsync();
            token.ThrowIfCancellationRequested();

            var room = await hub.JoinAsync(new JoinRequest { Bulletml = bulletml });
            DanmakuState.Room.Value = room;
            DanmakuState.Connected.Value = true;
            PlaceEnemy(room);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Danmaku] 弾幕 を替えられなかった: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        DanmakuState.Connected.Value = false;

        // 片付けは待たない。 OnDestroy は同期 の口 なので、
        // ここで待つと場面 の切り替えが止まる
        DisposeAsync(hub, channel).Forget();
        hub = null;
        channel = null;
    }

    static async UniTaskVoid DisposeAsync(IDanmakuHub closing, GrpcChannel closingChannel)
    {
        try
        {
            if (closing != null)
            {
                await closing.DisposeAsync();
            }

            closingChannel?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Danmaku] 切るときに: {ex.Message}");
        }
    }

    /// <summary>
    /// 降ってきたコマを 1 つ だけ持つ。溜めない。
    ///
    /// 溜めると、描く側 が遅れたぶんだけ古いコマを順に描くことになり、
    /// 遅れが返ってこない。 いちばん新しい 1 つ を残して捨てる。
    /// </summary>
    sealed class Receiver : IDanmakuHubReceiver
    {
        FrameDto latest;
        int dropped;
        int pendingPlayerHits;
        int pendingEnemyHits;

        public void OnFrame(FrameDto frame)
        {
            // Interlocked で置き換える。 ここは Unity の主スレッドではない
            var previous = Interlocked.Exchange(ref latest, frame);
            if (previous == null)
            {
                return;
            }

            Interlocked.Increment(ref dropped);

            // 当たりは捨てられない。 位置は「いまどこか」なので古いコマを
            // 落としてよいが、当たりは出来事で、落とすとその 1 発 が
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