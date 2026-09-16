using UnityEngine;

/// <summary>
/// 弾 の prefab に付いている札。もう何もしない。
///
/// 撃たれた弾は ECS の実体 で、この MonoBehaviour は 1 つ も作られない。
/// 残してあるのは GUID のため —— <c>g_bullet_s.prefab</c> が
/// この file を指しているので、消すと prefab が壊れる。
/// prefab が要るのは絵（<c>SpriteRenderer</c>）だけ。
/// </summary>
public class EnemyBullet : MonoBehaviour
{
}
