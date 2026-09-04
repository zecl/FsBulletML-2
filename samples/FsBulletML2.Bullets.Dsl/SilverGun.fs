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
/// SilverGun
[<RequireQualifiedAccess>]
module SilverGun =

  /// レイディアントシルバーガン4Dボス、PENTA。by 白い弾幕くん
  /// [SilverGun]_4D_boss_PENTA.xml
  let b4D_boss_PENTA =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "レイディアントシルバーガン4Dボス、PENTA。by 白い弾幕くん" {
        top {
            fire {
                absolute "100"
                speed "4"
                refBullet "arm" []
            }
            fire {
                absolute "-100"
                speed "4"
                refBullet "arm" []
            }
            repeat "400" {
                fire {
                    sequence "7"
                    speed "1.5"
                    plain
                }
                wait "1"
            }
            wait "60"
        }
        defBullet "arm" {
            doActs (body {
                wait "12"
                changeSpeed "0" "1"
                repeat "7" {
                    wait "60"
                    fire {
                        aim "-15"
                        speed "1.8"
                        refBullet "homing" []
                    }
                    fire {
                        sequence "30"
                        speed "1.8"
                        refBullet "homing" []
                    }
                    wait "2"
                }
                vanish
            })
        }
        defBullet "homing" {
            doActs (body {
                wait "60"
                changeDirectionAim "0" "15-$rank*10"
                wait "15-$rank*10"
                changeDirectionAim "0" "15-$rank*10"
            })
        }
    }
