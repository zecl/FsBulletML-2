// **このファイルは生成物。手で直すと次の焼き直しで消える。**
//
// samples/FsBulletML2.Bullets の同名ファイルから、焼いたアセンブリの値を
// 読んで CE の構文へ写している。元の .fs から拾うのは namespace / module /
// 値の名前 / doc コメントだけ。
//
// 焼き直し:
//     dotnet build samples/FsBulletML2.Bullets -c Release
//     dotnet fsi samples/FsBulletML2.Bullets.Dsl/gen.fsx
//
// **焼き直したら必ず tests/FsBulletML2.Dsl.Tests を回すこと。**
// 元と同じ木になることは、あそこが 179 個 を 1 個 ずつ突き合わせて言う。

namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Garegga
[<RequireQualifiedAccess>]
module Garegga =

  /// バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん
  /// [Garegga]_black_heart_mk2_winder.xml
  let black_heart_mk2_winder =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん" {
        top {
            fire {
                absolute "135"
                refBullet "winder" []
            }
            fire {
                absolute "225"
                refBullet "winder" []
            }
            wait "220"
        }
        defBullet "winder" {
            speed "2.3"
            doActs (body {
                wait "10"
                changeSpeed "0" "1"
                fire {
                    absolute "230"
                    ofBullet (bulletAnon {
                        doActs (body {
                            vanish
                        })
                    })
                }
                actionRef "move" ["0"; "40"]
                actionRef "move" ["0.7+$rank"; "20"]
                actionRef "move" ["-0.7-$rank"; "40"]
                actionRef "move" ["0.7+$rank"; "20"]
                vanish
            })
        }
        defAction "move" {
            repeat "$2" {
                fire {
                    sequence "$1-100"
                    speed "5"
                    plain
                }
                repeat "4" {
                    fire {
                        sequence "25"
                        speed "5"
                        plain
                    }
                }
                wait "2"
            }
        }
    }
