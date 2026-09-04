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
/// KetuiLt
[<RequireQualifiedAccess>]
module KetuiLt =

  /// ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん
  /// [Ketui_LT]_1boss_bit.xml
  let b1boss_bit =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん" {
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defAction "XWay" {
            actionRef "XWayFan" ["$1"; "$2"; "0"]
        }
        defAction "XWayFan" {
            repeat "$1-1" {
                fire {
                    sequence "$2"
                    speedSeq "$3"
                    plain
                }
            }
        }
        defAction "3way" {
            repeat "2" {
                wait "30"
                fire {
                    aim "-3"
                    speed "1.4"
                    plain
                }
                actionRef "XWay" ["3"; "2"]
            }
        }
        defBullet "bit" {
            doActs (body {
                repeat "3" {
                    accel "60" {
                        horizontalAbs "0"
                        verticalAbs "1"
                    }
                    actionRef "3way" []
                    accel "60" {
                        horizontalAbs "-2"
                        verticalAbs "0"
                    }
                    actionRef "3way" []
                    accel "60" {
                        horizontalAbs "0"
                        verticalAbs "-1"
                    }
                    actionRef "3way" []
                    accel "60" {
                        horizontalAbs "2"
                        verticalAbs "0"
                    }
                    actionRef "3way" []
                }
            })
        }
        top {
            repeat "4+$rank*6" {
                fire {
                    absolute "90"
                    speed "2"
                    refBullet "bit" []
                }
                wait "245/(4+$rank*6)"
            }
            wait "550"
        }
    }

  /// ケツイロケテより、三ボスのくねくね by 白い弾幕くん
  /// [Ketui_LT]_3boss_kunekune.xml
  let b3boss_kunekune =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ケツイロケテより、三ボスのくねくね by 白い弾幕くん" {
        defBullet "aimSrc" {
            speed "3"
            doActs (body {
                wait "10"
                changeSpeed "0" "1"
                repeat "5+$rank*10" {
                    wait "340/(5+$rank*10)"
                    repeat "3" {
                        wait "2"
                        fire {
                            aim "0"
                            speed "2"
                            plain
                        }
                    }
                }
                vanish
            })
        }
        defBullet "circleSrc" {
            speed "4"
            doActs (body {
                wait "10"
                changeSpeed "0.5+$rank" "1"
                changeDirectionSeq "5" "9999"
                repeat "200" {
                    wait "2"
                    fire {
                        absolute "180"
                        speed "3+$rand*0.02"
                        plain
                    }
                }
                vanish
            })
        }
        top {
            fire {
                absolute "90"
                refBullet "circleSrc" []
            }
            fire {
                absolute "-90"
                refBullet "circleSrc" []
            }
            fire {
                absolute "135"
                refBullet "aimSrc" []
            }
            fire {
                absolute "-135"
                refBullet "aimSrc" []
            }
            repeat "20" {
                wait "12-$rank*8"
                repeat "4+$rank*4" {
                    wait "2"
                    fire {
                        absolute "180"
                        speed "4"
                        ofBullet (bulletAnon {
                            doActs (body {
                                wait "6"
                                changeSpeed "1" "5"
                                wait "20"
                                changeDirectionAim "0" "1"
                                changeSpeed "2.2" "1"
                            })
                        })
                    }
                }
            }
        }
    }

  /// ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん
  /// [Ketui_LT]_3boss_roll_and_aim.xml
  let b3boss_roll_and_aim =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん" {
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defAction "XWay" {
            actionRef "XWayFan" ["$1"; "$2"; "0"]
        }
        defAction "XWayFan" {
            repeat "$1-1" {
                fire {
                    sequence "$2"
                    speedSeq "$3"
                    plain
                }
            }
        }
        defBullet "curve" {
            doActs (body {
                repeat "9999" {
                    changeDirectionRel "-$1*(4+$rank*$rank*4)" "10"
                    wait "10"
                }
            })
        }
        defAction "spiral" {
            wait "$rand * 30"
            repeat "10+$rank*15" {
                repeat "2" {
                    fire {
                        sequence "$1*5"
                        speed "1.5+$rank*$rank*1.5"
                        refBullet "curve" ["$1"]
                    }
                    repeat "4" {
                        fire {
                            sequence "90"
                            speedSeq "0"
                            refBullet "curve" ["$1"]
                        }
                    }
                    wait "6 + $rand * 3"
                }
                wait "6"
                fire {
                    sequence "$1"
                    speed "1+$rank*2"
                    refBullet "Dummy" []
                }
            }
        }
        defAction "twoWay" {
            repeat "5+$rank*4" {
                repeat "3+$rank*4" {
                    fire {
                        aim "3"
                        speed "1.8"
                        plain
                    }
                    fire {
                        aim "-3"
                        speed "1.8"
                        plain
                    }
                    wait "5"
                }
                wait "20"
            }
        }
        defAction "top1" {
            actionRef "spiral" ["-2"]
        }
        defAction "top2" {
            actionRef "spiral" ["2"]
        }
        defAction "top3" {
            actionRef "twoWay" []
        }
    }

  /// ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん
  /// [Ketui_LT]_2boss_winder_crash.xml
  let b2boss_winder_crash =
    createBulletmlInfo <|
    vertical "ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん" {
        defAction "pre" {
            fire {
                aim "-20"
                speed "2"
                plain
            }
            repeat "20" {
                fire {
                    sequence "40"
                    speed "4"
                    plain
                }
                fire {
                    sequence "-40"
                    speed "4"
                    plain
                }
                wait "2"
            }
        }
        defBullet "missile" {
            doActs (body {
                repeat "9999" {
                    wait "5-$rank*2+$rand"
                    fire {
                        aim "0"
                        speed "0.0000001"
                        ofBullet (bulletAnon {
                            doActs (body {
                                wait "60"
                                changeSpeed "3" "30"
                            })
                        })
                    }
                }
            })
        }
        defAction "missiles" {
            fire {
                sequence "-($1-1)*1.5"
                speed "4"
                refBullet "missile" []
            }
            repeat "$1-1" {
                fire {
                    sequence "3"
                    speed "4"
                    refBullet "missile" []
                }
            }
            fire {
                sequence "40-($1-1)*3"
                speed "4"
                refBullet "missile" []
            }
            repeat "$1-1" {
                fire {
                    sequence "3"
                    speed "4"
                    refBullet "missile" []
                }
            }
        }
        top {
            actionRef "pre" []
            actionRef "missiles" ["3+$rank*4"]
            wait "160"
            actionRef "pre" []
            actionRef "missiles" ["4+$rank*6"]
            wait "160"
        }
    }
