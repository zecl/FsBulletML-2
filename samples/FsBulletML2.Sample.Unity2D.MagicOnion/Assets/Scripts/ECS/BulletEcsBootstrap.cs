using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class BulletEcsRuntime
{
    public static Player Player;
    public static Enemy Enemy;
}

public class BulletEcsBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindAnyObjectByType<BulletEcsBootstrap>() != null)
        {
            return;
        }

        var go = new GameObject("BulletEcsBootstrap");
        DontDestroyOnLoad(go);
        go.AddComponent<BulletEcsBootstrap>();

        // 場面 に置かずに建てる。 写した場面（.unity）を触らずに済ませたい ——
        // YAML を手で足すと、Unity を開くまで壊れたことに気づけない
        if (FindAnyObjectByType<DanmakuClient>() == null)
        {
            var net = new GameObject("DanmakuClient");
            DontDestroyOnLoad(net);
            net.AddComponent<DanmakuClient>();
        }
    }

    void Awake()
    {
        Configure();
    }

    public void Configure()
    {
        UrpPlayModeCompat.Apply();

        var player = FindAnyObjectByType<Player>();
        var enemy = FindAnyObjectByType<Enemy>();
        BulletEcsRuntime.Player = player;
        BulletEcsRuntime.Enemy = enemy;

        // 当たり判定 の大きさ はここで採らない。 判定 はサーバーが持つので、
        // 半径 もサーバーの定数（Field）。両方 に置くと、片方 だけ動く
        SpriteRenderer enemySr = null;
        SpriteRenderer playerSr = null;
        if (enemy != null && enemy.BulletPrefab != null)
        {
            enemySr = enemy.BulletPrefab.GetComponent<SpriteRenderer>();
        }

        if (player != null && player.Bullet != null)
        {
            playerSr = player.Bullet.GetComponent<SpriteRenderer>();
        }

        BulletEntityFactory.Configure(enemySr, playerSr);

        var cam = Camera.main;
        if (cam == null)
        {
            cam = FindAnyObjectByType<Camera>();
        }

        if (cam != null)
        {
            cam.GetUniversalAdditionalCameraData();
        }

        if (GraphicsSettings.defaultRenderPipeline == null)
        {
            Debug.LogWarning("BulletEcsBootstrap: no URP asset assigned. Open the project once in the Editor so EnsureUrpPipeline can create Assets/Settings/URP_Pipeline.asset.");
        }
    }
}
