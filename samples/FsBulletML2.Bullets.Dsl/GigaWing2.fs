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
/// GigaWing2
[<RequireQualifiedAccess>]
module GigaWing2 =

  /// ギガウィング2のアークリミかも。by 白い弾幕くん
  /// [GigaWing2]_akurimi.xml
  let akurimi =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ギガウィング2のアークリミかも。by 白い弾幕くん" {
        defAction "add2" {
            repeat "2" {
                fire {
                    sequence "9"
                    speedSeq "0"
                    plain
                }
            }
        }
        defAction "top1" {
            fire {
                absolute "9"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            repeat "150" {
                fire {
                    sequence "7-18"
                    speed "1.8"
                    plain
                }
                actionRef "add2" []
                wait "4-$rank*2+$rand"
            }
        }
        defAction "top2" {
            fire {
                absolute "9"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            repeat "150" {
                fire {
                    sequence "-7-18"
                    speed "1.8"
                    plain
                }
                actionRef "add2" []
                wait "4-$rank*2+$rand"
            }
        }
    }
