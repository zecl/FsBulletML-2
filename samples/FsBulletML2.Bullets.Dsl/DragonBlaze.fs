namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// DragonBlaze
[<RequireQualifiedAccess>]
module DragonBlaze =

  /// ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん
  /// [DragonBlaze]_nebyurosu_2.xml
  let nebyurosu_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん" {
        defAction "add3" {
            repeat "3" {
                fire {
                    sequence "90"
                    speedSeq "0"
                    plain
                }
            }
        }
        defAction "top1" {
            repeat "150" {
                fire {
                    sequence "4"
                    speed "1+$rank"
                    plain
                }
                actionRef "add3" []
                wait "2"
            }
            wait "60-$rank*30"
        }
        defAction "top2" {
            repeat "150" {
                fire {
                    sequence "-5"
                    speed "1+$rank"
                    plain
                }
                actionRef "add3" []
                wait "2"
            }
            wait "60-$rank*30"
        }
    }
