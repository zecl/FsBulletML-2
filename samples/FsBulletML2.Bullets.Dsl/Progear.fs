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
/// Progear
[<RequireQualifiedAccess>]
module Progear =

  /// CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん
  /// [Progear]_round_1_boss_grow_bullets.xml
  let round_1_boss_grow_bullets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん" {
        defAction "oogi" {
            fire {
                absolute "270-(4+$rank*6)*15/2"
                refBullet "seed" []
            }
            repeat "4+$rank*6" {
                fire {
                    sequence "15"
                    refBullet "seed" []
                }
            }
        }
        top {
            repeat "4" {
                actionRef "oogi" []
                wait "40"
            }
            wait "40"
            repeat "8" {
                actionRef "oogi" []
                wait "20"
            }
            wait "30"
        }
        defBullet "seed" {
            speed "1.5"
            doActs (body {
                changeSpeed "0" "60"
                wait "60"
                fire {
                    speed "0.75"
                    plain
                }
                repeat "4+$rank*4" {
                    fire {
                        speedSeq "0.3"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん
  /// [Progear]_round_2_boss_struggling.xml
  let round_2_boss_struggling =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん" {
        top {
            repeat "1000" {
                fire {
                    sequence "180"
                    refBullet "changeStraight" []
                }
                fire {
                    sequence "159"
                    refBullet "changeStraight" []
                }
                wait "1+(1-$rank)*3*$rand"
            }
            wait "180"
        }
        defBullet "changeStraight" {
            speed "0.8"
            doActs (body {
                wait "20+$rand*100"
                changeDirectionAbs "270" "60"
                changeSpeed "0" "40"
                wait "40"
                changeSpeed "0.5+$rand*0.7" "20"
            })
        }
    }

  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_back_burst.xml
  let round_3_boss_back_burst =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん" {
        top {
            repeat "200" {
                fire {
                    absolute "220+$rand*100"
                    refBullet "backBurst" []
                }
                wait "4-$rank*2"
            }
            wait "60"
        }
        defBullet "backBurst" {
            speed "1.2"
            doActs (body {
                changeSpeed "0" "80"
                wait "60+$rand*20"
                repeat "2" {
                    fire {
                        absolute "60+$rand*60"
                        refBullet "downAccel" []
                    }
                }
                vanish
            })
        }
        defBullet "downAccel" {
            speed "1.8"
            doActs (body {
                accel "250" {
                    horizontalRel "-7"
                }
            })
        }
    }

  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_wave_bullets.xml
  let round_3_boss_wave_bullets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん" {
        top {
            repeat "10" {
                fire {
                    absolute "310"
                    refBullet "wave" ["-3"]
                }
                wait "30"
                fire {
                    absolute "230"
                    refBullet "wave" ["3"]
                }
                wait "30"
            }
            wait "60"
        }
        defBullet "wave" {
            speed "1.5"
            doActs (body {
                fire {
                    dir "0"
                    refBullet "nrm" []
                }
                repeat "12+$rank*12" {
                    fire {
                        sequence "$1"
                        refBullet "nrm" []
                    }
                    wait "3"
                }
                vanish
            })
        }
        defBullet "nrm" {
            speed "1"
        }
    }

  /// CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん
  /// [Progear]_round_4_boss_fast_rocket.xml
  let round_4_boss_fast_rocket =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん" {
        defAction "fireRoot" {
            fire {
                absolute "$1"
                speed "0.2"
                refBullet "rootBl" []
            }
            repeat "3" {
                fire {
                    absolute "$1"
                    speedSeq "0.5"
                    refBullet "rootBl" []
                }
            }
        }
        top {
            actionRef "fireRoot" ["$rand*16"]
            actionRef "fireRoot" ["180+$rand*16"]
            wait "120"
        }
        defBullet "rootBl" {
            doActs (body {
                wait "40"
                fire {
                    absolute "274+$rand*4"
                    refBullet "rocket" []
                }
                vanish
            })
        }
        defBullet "rocket" {
            speed "5+$rand"
            doActs (body {
                repeat "9999" {
                    fire {
                        absolute "0"
                        speed "1"
                        refBullet "downAccel" []
                    }
                    fire {
                        absolute "60"
                        speed "1.8"
                        refBullet "downAccel" []
                    }
                    wait "5-$rank*4"
                }
            })
        }
        defBullet "downAccel" {
            doActs (body {
                accel "120" {
                    vertical "2.7"
                }
            })
        }
    }

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_boss_last_round_wave.xml
  let round_5_boss_last_round_wave =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん" {
        top {
            repeat "4" {
                repeat "2+$rank*1.5" {
                    fire {
                        refBullet "rfRkt" []
                    }
                    wait "45"
                }
                wait "100"
            }
        }
        defBullet "rfRkt" {
            doActs (body {
                repeat "9999" {
                    wait "2"
                    fire {
                        sequence "15"
                        plain
                    }
                }
            })
        }
    }

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_middle_boss_rockets.xml
  let round_5_middle_boss_rockets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん" {
        top {
            repeat "50" {
                fire {
                    absolute "270"
                    refBullet "rocket" []
                }
                wait "10"
            }
            wait "120"
        }
        defBullet "rocket" {
            doActs (body {
                repeat "9999" {
                    fireRef "udBlt" ["90"]
                    wait "20-$rank*8"
                    fireRef "udBlt" ["-90"]
                    wait "$rand*10+15-$rank*8"
                }
            })
        }
        topFireAs "udBlt" {
            relative "$1-25+$rand*50"
            plain
        }
    }

  /// CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん
  /// [Progear]_round_6_boss_parabola_shot.xml
  let round_6_boss_parabola_shot =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん" {
        top {
            repeat "25" {
                fire {
                    absolute "190+$rand*30"
                    refBullet "seed" ["1"]
                }
                wait "15-$rank*5"
                fire {
                    absolute "350-$rand*30"
                    refBullet "seed" ["-1"]
                }
                wait "15-$rank*5"
            }
            wait "60"
        }
        defBullet "seed" {
            speed "1"
            doActs (body {
                changeSpeed "0" "60"
                wait "60"
                fire {
                    plain
                }
                fire {
                    absolute "270+30*$1+$rand*50*$1"
                    refBullet "downAccel" ["$1"]
                }
                repeat "3" {
                    fire {
                        sequence "0"
                        speedSeq "-0.4"
                        refBullet "downAccel" ["$1"]
                    }
                }
                vanish
            })
        }
        defBullet "downAccel" {
            speed "2.5"
            doActs (body {
                accel "120" {
                    vertical "4*$1"
                }
            })
        }
    }

  /// CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん
  /// [Progear]_round_9_boss.xml
  let round_9_boss =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん" {
        defBullet "accel" {
            doActs (body {
                changeSpeedSeq "0.03" "9999"
                wait "9999"
            })
        }
        top {
            fire {
                absolute "80"
                ofBullet (bulletAnon {
                    doActs (body {
                        wait "20"
                        vanish
                    })
                })
            }
            repeat "4" {
                fire {
                    sequence "40"
                    speed "5"
                    ofBullet (bulletAnon {
                        doActs (body {
                            repeat "9999" {
                                fire {
                                    absolute "0"
                                    speed "0.5"
                                    refBullet "accel" []
                                }
                                fire {
                                    absolute "180"
                                    speed "0.5"
                                    refBullet "accel" []
                                }
                                wait "4-$rank*2+$rand"
                            }
                        })
                    })
                }
            }
            wait "120"
        }
    }

  /// CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん
  /// [Progear]_round_10_boss_before_final.xml
  let round_10_boss_before_final =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん" {
        topFireAs "rollOut" {
            relative "90"
            speed "0.0001"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "350"
                    changeSpeed "1" "100"
                    changeDirectionRel "50-$rank*40" "100"
                    wait "1000"
                })
            })
        }
        defBullet "setter" {
            speed "3"
            doActs (body {
                repeat "999" {
                    wait "5"
                    fireRef "rollOut" []
                }
            })
        }
        defAction "top1" {
            fire {
                absolute "$rand*10"
                refBullet "setter" []
            }
            repeat "45/(2-$rank)" {
                fire {
                    sequence "16-$rank*8"
                    refBullet "setter" []
                }
                wait "1"
            }
            wait "40"
            repeat "125+$rank*125" {
                wait "1.5-$rank/2+$rand"
                fire {
                    aim "45-$rand*90"
                    speed "1.2"
                    plain
                }
            }
        }
        defAction "top2" {
            wait "80"
            changeSpeed "0.7" "1"
            changeDirectionAim "0" "1"
            wait "1"
            changeDirectionSeq "1.44444" "250"
            wait "250"
            changeSpeed "0" "1"
            wait "20"
            changeSpeed "0.7" "1"
            changeDirectionSeq "30" "12"
            wait "12"
            changeSpeed "0" "1"
            wait "200-$rank*60"
        }
    }
