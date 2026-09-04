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
/// Dodonpachi
[<RequireQualifiedAccess>]
module Dodonpachi =

  /// 怒首領蜂、火蜂。by 白い弾幕くん
  /// [Dodonpachi]_hibachi.xml
  let hibachi =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂、火蜂。by 白い弾幕くん" {
        defAction "allWay" {
            fire {
                dir "-50+$rand*20"
                speed "1+$rank"
                plain
            }
            repeat "15+16*$rank*$rank" {
                fire {
                    sequence "24-$rank*12"
                    speedSeq "0"
                    plain
                }
            }
        }
        defAction "right" {
            changeDirectionAbs "90" "1"
            changeSpeedAbs "1" "1"
            repeat "25" {
                actionRef "allWay" []
                wait "3"
            }
        }
        defAction "left" {
            changeDirectionAbs "-90" "1"
            changeSpeedAbs "1" "1"
            repeat "25" {
                actionRef "allWay" []
                wait "3"
            }
        }
        top {
            repeat "2" {
                actionRef "right" []
                actionRef "left" []
                actionRef "left" []
                actionRef "right" []
            }
            changeSpeed "0" "1"
            wait "1"
        }
    }

  /// 怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_1.xml
  let kitiku_1 =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん" {
        defBullet "fast" {
            speed "10"
            doActs (body {
                wait "6"
                changeSpeed "0" "1"
                wait "20"
                repeat "10+$rank*18" {
                    fire {
                        sequence "-11-$rand*2"
                        speed "1.5"
                        plain
                    }
                    actionRef "add3" []
                    repeat "4" {
                        fire {
                            sequence "0"
                            speedSeq "0.1+$rank*0.2"
                            plain
                        }
                        actionRef "add3" []
                    }
                    wait "336/(10+$rank*18)"
                }
                vanish
            })
        }
        defAction "add3" {
            repeat "3" {
                fire {
                    sequence "90"
                    speedSeq "0"
                    plain
                }
            }
        }
        topFireAs "slowColorChange" {
            absolute "180+45*$1"
            speed "7"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "6"
                    changeSpeed "0" "1"
                    repeat "50+$rank*50" {
                        fire {
                            sequence "(8-$rank*4)*$1"
                            speed "1.2"
                            plain
                        }
                        actionRef "add3" []
                        wait "8-$rank*4+$rand"
                    }
                    vanish
                })
            })
        }
        topFireAs "slow" {
            ofBullet (bulletAnon {
                doActs (body {
                    fireRef "slowColorChange" ["$1"]
                    vanish
                })
            })
        }
        top {
            fire {
                absolute "-85"
                refBullet "fast" []
            }
            wait "1"
            fire {
                absolute "85"
                refBullet "fast" []
            }
            wait "1"
            fireRef "slow" ["1"]
            wait "1"
            fireRef "slow" ["-1"]
            wait "430"
        }
    }

  /// 怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_2.xml
  let kitiku_2 =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん" {
        defBullet "feather" {
            speed "4"
            doActs (body {
                wait "6"
                fire {
                    relative "0"
                    ofBullet (bulletAnon {
                        doActs (body {
                            vanish
                        })
                    })
                }
                changeSpeed "0" "1"
                wait "1"
                repeat "100" {
                    fire {
                        sequence "10-(3+$rank*6)*3"
                        speed "1"
                        plain
                    }
                    repeat "3+$rank*6" {
                        fire {
                            sequence "3"
                            speedSeq "0.15"
                            plain
                        }
                    }
                    wait "4"
                }
                vanish
            })
        }
        top {
            fire {
                absolute "-90"
                refBullet "feather" []
            }
            wait "1"
            fire {
                absolute "90"
                refBullet "feather" []
            }
            wait "430"
        }
    }

  /// 怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_3.xml
  let kitiku_3 =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん" {
        defAction "top1" {
            repeat "200+$rank*200" {
                fire {
                    aim "-50+$rand*100"
                    speed "1.6"
                    plain
                }
                wait "2-$rank+$rand"
            }
        }
        defBullet "kobati" {
            doActs (body {
                wait "$1"
                repeat "20" {
                    fire {
                        aim "0"
                        speed "1.6"
                        plain
                    }
                    wait "(16-$rank*8)*3"
                }
                vanish
            })
        }
        defAction "top2" {
            repeat "8+$rank*8" {
                fire {
                    aim "80"
                    speed "1.5"
                    refBullet "kobati" ["(16-$rank*8)*3"]
                }
                fire {
                    aim "-80"
                    speed "1.5"
                    refBullet "kobati" ["(16+$rank*8)*3"]
                }
                wait "16-$rank*8"
                fire {
                    aim "80"
                    speed "1.5"
                    refBullet "kobati" ["(16-$rank*8)*2"]
                }
                fire {
                    aim "-80"
                    speed "1.5"
                    refBullet "kobati" ["(16-$rank*8)*2"]
                }
                wait "16-$rank*8"
                fire {
                    aim "80"
                    speed "1.5"
                    refBullet "kobati" ["16-$rank*8"]
                }
                fire {
                    aim "-80"
                    speed "1.5"
                    refBullet "kobati" ["16-$rank*8"]
                }
                wait "16-$rank*8"
            }
            wait "120"
        }
    }

  /// 怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん
  /// [Dodonpachi]_kitiku_5.xml
  let kitiku_5 =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん" {
        defAction "top1" {
            repeat "30+$rank*30" {
                fire {
                    sequence "220+$rand*2"
                    speed "1.2"
                    plain
                }
                fire {
                    sequence "15"
                    speed "1.2"
                    plain
                }
                fire {
                    sequence "120"
                    speed "1.2"
                    plain
                }
                fire {
                    sequence "15"
                    speed "1.2"
                    plain
                }
                wait "20-$rank*10"
            }
        }
        defAction "top2" {
            repeat "30+$rank*30" {
                fire {
                    aim "-30+$rand*60"
                    speed "1.3"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
                wait "20-$rank*10"
            }
        }
        defBullet "kobati" {
            doActs (body {
                wait "5"
                changeDirectionAbs "180" "1"
                changeSpeedAbs "1.2" "1"
                wait "1"
                repeat "9999" {
                    wait "30-$rank*10"
                    fire {
                        sequence "45"
                        speed "0.4+$rank*0.2"
                        plain
                    }
                    repeat "3" {
                        fire {
                            sequence "90"
                            speed "0.4+$rank*0.2"
                            plain
                        }
                    }
                }
            })
        }
        defAction "top3" {
            repeat "5+$rank*5" {
                fire {
                    absolute "90"
                    speed "10"
                    refBullet "kobati" []
                }
                fire {
                    absolute "90"
                    speed "5"
                    refBullet "kobati" []
                }
                fire {
                    absolute "-90"
                    speed "10"
                    refBullet "kobati" []
                }
                fire {
                    absolute "-90"
                    speed "5"
                    refBullet "kobati" []
                }
                wait "120-$rank*60"
            }
            wait "120"
        }
    }
