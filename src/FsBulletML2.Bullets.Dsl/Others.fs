namespace FsBulletML2.Bullets.Dsl.EnemyBullet

open FsBulletML2
open FsBulletML2.Dsl

/// その他
[<RequireQualifiedAccess>]
module Others =

    /// 全方位弾
    let AllWay =
        createBulletmlInfo
        <| vertical "全方位弾" {
            defAction "circle" {
                repeat "$1" {
                    fire {
                        sequence "360/$1"
                        plain
                    }
                }
            }

            top {
                repeat "30" {
                    actionRef "circle" [ "20" ]
                    wait "20"
                }
            }
        }

    /// 前方5way弾
    let b5way =
        createBulletmlInfo
        <| vertical "前方5way弾" {
            top {
                fire {
                    relative "-20+180"
                    plain
                }

                repeat "4" {
                    fire {
                        sequence "10"
                        plain
                    }
                }
            }
        }

    /// 初期方向Aim弾１発
    let homingOne =
        createBulletmlInfo <| untyped "初期方向Aim弾１発" { top { fire { plain } } }
