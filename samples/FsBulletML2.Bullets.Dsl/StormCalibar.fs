namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// StormCalibar
[<RequireQualifiedAccess>]
module StormCalibar =

  /// ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん
  /// [STORM_CALIBAR]_last_boss_double_roll_bullets.xml
  let last_boss_double_roll_bullets =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん" {
        defAction "rollShots" {
            repeat "200" {
                fire {
                    sequence "11*$1"
                    speed "1"
                    plain
                }
                repeat "3+$rank*4" {
                    fire {
                        sequence "0"
                        speedSeq "0.3"
                        plain
                    }
                }
                wait "2"
            }
        }
        defAction "right" {
            changeDirectionAbs "90" "1"
            changeSpeedAbs "1.5" "1"
            wait "50"
        }
        defAction "left" {
            changeDirectionAbs "-90" "1"
            changeSpeedAbs "1.5" "1"
            wait "50"
        }
        defAction "top1" {
            repeat "2" {
                actionRef "right" []
                actionRef "left" []
                actionRef "left" []
                actionRef "right" []
            }
            changeSpeed "0" "1"
            wait "1"
        }
        defAction "top2" {
            actionRef "rollShots" ["-1"]
        }
        defAction "top3" {
            actionRef "rollShots" ["1"]
        }
    }
