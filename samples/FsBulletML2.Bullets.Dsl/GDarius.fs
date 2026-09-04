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
/// GDarius
[<RequireQualifiedAccess>]
module GDarius =

  /// Gダライアス中のホーミングレーザー by 白い弾幕くん
  /// [G_DARIUS]_homing_laser.xml
  let homing_laser =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Gダライアス中のホーミングレーザー by 白い弾幕くん" {
        top {
            repeat "20" {
                fire {
                    dir "-60+$rand*120"
                    refBullet "hmgLsr" []
                }
                repeat "8" {
                    wait "1"
                    fire {
                        sequence "0"
                        refBullet "hmgLsr" []
                    }
                }
                wait "10"
            }
            wait "60"
        }
        defBullet "hmgLsr" {
            speed "2"
            doActs (body {
                changeSpeed "0.3" "30"
                wait "100"
                changeSpeed "5" "100"
            })
            doActs (body {
                repeat "12" {
                    changeDirectionAim "0" "45-$rank*30"
                    wait "5"
                }
            })
        }
    }
