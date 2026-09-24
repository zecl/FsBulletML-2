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
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ギガウィング2のアークリミかも。by 白い弾幕くん" {
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
                    ofBullet (bulletAnon { doActs (body { vanish }) })
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
                    ofBullet (bulletAnon { doActs (body { vanish }) })
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
