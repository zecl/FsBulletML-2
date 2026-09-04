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
/// Psyvariar
[<RequireQualifiedAccess>]
module Psyvariar =

  /// サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん
  /// [Psyvariar]_4-D_boss_MZIQ.xml
  let b4_D_boss_MZIQ =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん" {
        defAction "add11" {
            repeat "11" {
                fire {
                    sequence "30"
                    speedSeq "0"
                    plain
                }
            }
        }
        top {
            repeat "30" {
                fire {
                    sequence "-11"
                    speed "1+$rank"
                    plain
                }
                actionRef "add11" []
                repeat "3" {
                    wait "4-$rank*2+$rand"
                    fire {
                        sequence "-5+30"
                        speed "1+$rank"
                        plain
                    }
                    actionRef "add11" []
                }
                wait "4-$rank*2+$rand"
            }
            wait "30-$rank*30"
        }
    }

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_opening.xml
  let X_A_boss_opening =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "サイヴァリア、多分最終面ボス。by 白い弾幕くん" {
        top {
            repeat "600" {
                fire {
                    dir "-45+$rand*90"
                    speed "(0.3+$rand*0.5)*($rank+1)"
                    plain
                }
                wait "1"
            }
            wait "100"
        }
    }

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_winder.xml
  let X_A_boss_winder =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "サイヴァリア、多分最終面ボス。by 白い弾幕くん" {
        defBullet "winderBullet" {
            speed "3"
        }
        topFireAs "fireWinder" {
            sequence "$1"
            refBullet "winderBullet" []
        }
        defAction "roundWinder" {
            fireRef "fireWinder" ["$1"]
            repeat "11" {
                fireRef "fireWinder" ["30"]
            }
            wait "5"
        }
        defAction "winderSequence" {
            repeatRef "12" "roundWinder" ["30"]
            repeatRef "12" "roundWinder" ["$1"]
            repeatRef "12" "roundWinder" ["30"]
        }
        defAction "top1" {
            fire {
                absolute "2"
                refBullet "winderBullet" []
            }
            actionRef "winderSequence" ["30.9+0.1*$rank"]
        }
        defAction "top2" {
            fire {
                absolute "-2"
                refBullet "winderBullet" []
            }
            actionRef "winderSequence" ["29.1-0.1*$rank"]
        }
    }
