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
/// GWange
[<RequireQualifiedAccess>]
module GWange =

  /// G-わんげスレの957氏、回転ガラ by 白い弾幕くん
  /// [G-Wange]_roll_gara.xml
  let _roll_gara =
    createBulletmlInfo <|
    untyped "G-わんげスレの957氏、回転ガラ by 白い弾幕くん" {
        defAction "top1" {
            repeat "600/(3-$rank*2)" {
                actionRef "line" []
                wait "3-$rank*2+$rand"
            }
        }
        defAction "line" {
            fire {
                sequence "-7"
                speed "0.6"
                plain
            }
            repeat "5+$rank*5" {
                fire {
                    sequence "0"
                    speedSeq "0.3"
                    plain
                }
            }
        }
        defAction "top2" {
            repeat "20" {
                changeDirectionSeq "-1+$rand*2" "30"
                changeSpeedAbs "(-1+$rand*2)*($rank*2+1)" "30"
                wait "30"
            }
        }
    }

  /// G-わんげスレの966氏考案、往復ビット by 白い弾幕くん
  /// [G-Wange]_round_trip_bit.xml
  let round_trip_bit =
    createBulletmlInfo <|
    untyped "G-わんげスレの966氏考案、往復ビット by 白い弾幕くん" {
        top {
            fire {
                refBullet "src" ["5"; "91"]
            }
            fire {
                refBullet "src" ["4"; "-91"]
            }
            wait "600"
        }
        defAction "Xway" {
            fire {
                aim "-(5+$rank*5)*($1-1)-4+$rand*8"
                speed "1.6"
                plain
            }
            repeat "$1-1" {
                fire {
                    sequence "5+$rank*5"
                    speed "1.6"
                    plain
                }
            }
        }
        defAction "fire" {
            actionRef "Xway" ["3"]
            wait "15"
            actionRef "Xway" ["5"]
            wait "15"
        }
        defBullet "src" {
            absolute "$2"
            speed "$1"
            doActs (body {
                repeat "5" {
                    changeSpeed "0.01" "30"
                    actionRef "fire" []
                    changeDirectionAbs "-$2" "1"
                    changeSpeed "$1" "30"
                    actionRef "fire" []
                    changeSpeed "0.01" "30"
                    actionRef "fire" []
                    changeDirectionAbs "$2" "1"
                    changeSpeed "$1" "30"
                    actionRef "fire" []
                }
            })
        }
    }
