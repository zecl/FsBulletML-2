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
/// ChaosSeed
[<RequireQualifiedAccess>]
module ChaosSeed =

  /// カオスシード、大猿ボス。by 白い弾幕くん
  /// [ChaosSeed]_big_monkey_boss.xml
  let big_monkey_boss =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "カオスシード、大猿ボス。by 白い弾幕くん" {
        defBullet "roll" {
            doActs (body {
                changeDirectionSeq "3" "10000"
                changeSpeed "2" "60"
                wait "60"
                changeSpeed "1.8" "40"
                wait "40"
                changeSpeed "2" "30"
                wait "30"
                changeDirectionSeq "2" "10000"
                changeSpeedSeq "0.01" "100000"
            })
        }
        defBullet "explosionBullet" {
            doActs (body {
                wait "30"
                repeat "12" {
                    fire {
                        sequence "30"
                        speed "1.2"
                        refBullet "roll" []
                    }
                }
                vanish
            })
        }
        top {
            repeat "3+$rank*6" {
                fire {
                    aim "-90+180*$rand"
                    speed "$rand*3+1"
                    refBullet "explosionBullet" []
                }
                wait "90-$rank*60"
            }
        }
    }
