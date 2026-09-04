namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Xevious
[<RequireQualifiedAccess>]
module Xevious =

  /// ゼビウス、らしい。 by 白い弾幕くん
  /// [XEVIOUS]_garu_zakato.xml
  let garu_zakato =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ゼビウス、らしい。 by 白い弾幕くん" {
        top {
            repeat "10" {
                fire {
                    absolute "180"
                    speed "3"
                    refBullet "gzc" []
                }
                wait "20-$rank*10+$rand*10"
            }
            wait "60"
        }
        defBullet "gzc" {
            doActs (body {
                wait "10+$rand*10"
                repeat "16" {
                    fire {
                        sequence "360/16"
                        refBullet "spr" []
                    }
                }
                repeat "4" {
                    fire {
                        sequence "90"
                        refBullet "hrmSpr" []
                    }
                }
                vanish
            })
        }
        defBullet "spr" {
            speed "2"
        }
        defBullet "hrmSpr" {
            speed "0"
            doActs (body {
                changeSpeed "2" "60"
            })
            doActs (body {
                repeat "9999" {
                    changeDirectionAim "0" "40"
                    wait "1"
                }
            })
        }
    }
