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
/// Daiouzyou
[<RequireQualifiedAccess>]
module Daiouzyou =

  /// 怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_1.xml
  let hibachi_1 =
    createBulletmlInfo <|
    untyped "怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん" {
        top {
            repeat "10+$rank*70" {
                fire {
                    aim "$rand*30-74+$rank*2"
                    speed "0.5+$rank*2"
                    plain
                }
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                fireRef "n" []
                wait "14-$rank*10"
            }
        }
        topFireAs "n" {
            sequence "$rand*2+7-$rank*2"
            speed "0.5+$rank*2"
            plain
        }
    }

  /// 怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_2.xml
  let hibachi_2 =
    createBulletmlInfo <|
    untyped "怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん" {
        top {
            fire {
                absolute "175"
                speed "1+$rank*4"
                plain
            }
            repeat "30" {
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                wait "1"
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                actionRef "rights" []
                wait "15-$rank*10"
                fire {
                    sequence "4"
                    speed "1+$rank*4"
                    plain
                }
            }
        }
        defAction "tops" {
            fire {
                absolute "185"
                speed "1+$rank*4"
                plain
            }
            repeat "30" {
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                fireRef "allway" []
                wait "1"
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                actionRef "lefts" []
                wait "15-$rank*10"
                fire {
                    sequence "-4"
                    speed "1+$rank*4"
                    plain
                }
            }
        }
        defAction "lefts" {
            fire {
                sequence "-0.7"
                speed "1+$rank*4"
                plain
            }
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            wait "1"
        }
        defAction "rights" {
            fire {
                sequence "0.7"
                speed "1+$rank*4"
                plain
            }
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            fireRef "allway" []
            wait "1"
        }
        topFireAs "allway" {
            sequence "45"
            speed "1+$rank*4"
            plain
        }
    }

  /// 怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_3.xml
  let hibachi_3 =
    createBulletmlInfo <|
    untyped "怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん" {
        top {
            changeSpeedAbs "2" "1"
            changeDirectionAbs "180" "1"
            wait "10"
            changeSpeedAbs "0" "1"
            fire {
                absolute "0"
                speed "1.5"
                refBullet "blue" []
            }
            repeat "60+$rank*60" {
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                fireRef "red" []
                wait "20-$rank*14"
                fire {
                    sequence "-31.5"
                    speed "1.5"
                    refBullet "blue" []
                }
            }
        }
        topFireAs "red" {
            sequence "30"
            speed "1.5"
            refBullet "blue" []
        }
        defBullet "blue" {
            doActs (body {
                wait "30"
                fire {
                    relative "-110"
                    speed "1.5"
                    plain
                }
            })
        }
    }

  /// 怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_4.xml
  let hibachi_4 =
    createBulletmlInfo <|
    untyped "怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん" {
        top {
            fire {
                absolute "45"
                speed "1+$rank*0.5"
                plain
            }
            repeat "113+900/(16-$rank*10)" {
                fireRef "four" []
                fireRef "four" []
                fireRef "four" []
                fire {
                    sequence "86"
                    speed "1+$rank*0.5"
                    plain
                }
                wait "16-$rank*10"
            }
        }
        defAction "tops" {
            wait "(16-$rank*10)*22.5"
            fire {
                absolute "45"
                speed "1+$rank*0.5"
                plain
            }
            repeat "91+900/(16-$rank*10)" {
                fireRef "four" []
                fireRef "four" []
                fireRef "four" []
                fire {
                    sequence "94"
                    speed "1+$rank*0.5"
                    plain
                }
                wait "16-$rank*10"
            }
        }
        defAction "topt" {
            wait "(16-$rank*10)*45"
            fire {
                refBullet "gurugurup" []
            }
        }
        defBullet "gurugurup" {
            doActs (body {
                changeSpeed "0" "1"
                actionRef "guru2" []
                vanish
            })
        }
        defAction "guru2" {
            fire {
                absolute "0"
                speed "1+$rank"
                refBullet "guruc" []
            }
            repeat "450/(16-$rank*10)" {
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fireRef "guru" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fireRef "guru2" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc2" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fireRef "guru3" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc3" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fireRef "guru4" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc4" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fireRef "guru5" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc5" []
                }
                wait "16-$rank*10"
            }
            repeat "4" {
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fireRef "guru6" []
                fire {
                    sequence "20.5"
                    speed "1+$rank"
                    refBullet "guruc6" []
                }
                wait "16-$rank*10"
            }
            repeat "4" {
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fireRef "guru7" []
                fire {
                    sequence "21"
                    speed "1+$rank"
                    refBullet "guruc7" []
                }
                wait "16-$rank*10"
            }
            repeat "6" {
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fireRef "guru8" []
                fire {
                    sequence "21.5"
                    speed "1+$rank"
                    refBullet "guruc8" []
                }
                wait "16-$rank*10"
            }
            repeat "7" {
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fireRef "guru9" []
                fire {
                    sequence "22"
                    speed "1+$rank"
                    refBullet "guruc9" []
                }
                wait "16-$rank*10"
            }
            repeat "2" {
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fireRef "guru10" []
                fire {
                    sequence "22.7"
                    speed "1+$rank"
                    refBullet "guruc10" []
                }
                wait "16-$rank*10"
            }
            repeat "7" {
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fireRef "guru11" []
                fire {
                    sequence "23"
                    speed "1+$rank"
                    refBullet "guruc11" []
                }
                wait "16-$rank*10"
            }
            repeat "6" {
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fireRef "guru12" []
                fire {
                    sequence "23.5"
                    speed "1+$rank"
                    refBullet "guruc12" []
                }
                wait "16-$rank*10"
            }
            repeat "4" {
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fireRef "guru13" []
                fire {
                    sequence "24"
                    speed "1+$rank"
                    refBullet "guruc13" []
                }
                wait "16-$rank*10"
            }
            repeat "4" {
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fireRef "guru14" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc14" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fireRef "guru15" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc15" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fireRef "guru16" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc16" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fireRef "guru17" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc17" []
                }
                wait "16-$rank*10"
            }
            repeat "3" {
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fireRef "guru18" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc18" []
                }
                wait "16-$rank*10"
            }
            repeat "450/(16-$rank*10)" {
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fireRef "guru19" []
                fire {
                    sequence "24.5"
                    speed "1+$rank"
                    refBullet "guruc19" []
                }
                wait "16-$rank*10"
            }
        }
        topFireAs "guru" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc" []
        }
        topFireAs "guru2" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc2" []
        }
        topFireAs "guru3" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc3" []
        }
        topFireAs "guru4" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc4" []
        }
        topFireAs "guru5" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc5" []
        }
        topFireAs "guru6" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc6" []
        }
        topFireAs "guru7" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc7" []
        }
        topFireAs "guru8" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc8" []
        }
        topFireAs "guru9" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc9" []
        }
        topFireAs "guru10" {
            sequence "22.3+$rand*0.4"
            speed "1+$rank"
            refBullet "guruc10" []
        }
        topFireAs "guru11" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc11" []
        }
        topFireAs "guru12" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc12" []
        }
        topFireAs "guru13" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc13" []
        }
        topFireAs "guru14" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc14" []
        }
        topFireAs "guru15" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc15" []
        }
        topFireAs "guru16" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc16" []
        }
        topFireAs "guru17" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc17" []
        }
        topFireAs "guru18" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc18" []
        }
        topFireAs "guru19" {
            sequence "22.5"
            speed "1+$rank"
            refBullet "guruc19" []
        }
        topFireAs "four" {
            sequence "90"
            speed "1+$rank*0.5"
            plain
        }
        defBullet "guruc" {
            doActs (body {
                changeDirectionRel "-270" "90"
            })
        }
        defBullet "guruc2" {
            doActs (body {
                changeDirectionRel "-270" "170"
            })
        }
        defBullet "guruc3" {
            doActs (body {
                changeDirectionRel "-270" "260"
            })
        }
        defBullet "guruc4" {
            doActs (body {
                changeDirectionRel "-270" "300"
            })
        }
        defBullet "guruc5" {
            doActs (body {
                changeDirectionRel "-270" "450"
            })
        }
        defBullet "guruc6" {
            doActs (body {
                changeDirectionRel "-270" "600"
            })
        }
        defBullet "guruc7" {
            doActs (body {
                changeDirectionRel "-270" "700"
            })
        }
        defBullet "guruc8" {
            doActs (body {
                changeDirectionRel "-270" "800"
            })
        }
        defBullet "guruc9" {
            doActs (body {
                changeDirectionRel "-270" "900"
            })
        }
        defBullet "guruc10" {
            doActs (body {
                changeDirectionRel "0" "90"
            })
        }
        defBullet "guruc11" {
            doActs (body {
                changeDirectionRel "270" "900"
            })
        }
        defBullet "guruc12" {
            doActs (body {
                changeDirectionRel "270" "800"
            })
        }
        defBullet "guruc13" {
            doActs (body {
                changeDirectionRel "270" "700"
            })
        }
        defBullet "guruc14" {
            doActs (body {
                changeDirectionRel "270" "600"
            })
        }
        defBullet "guruc15" {
            doActs (body {
                changeDirectionRel "270" "450"
            })
        }
        defBullet "guruc16" {
            doActs (body {
                changeDirectionRel "270" "300"
            })
        }
        defBullet "guruc17" {
            doActs (body {
                changeDirectionRel "270" "280"
            })
        }
        defBullet "guruc18" {
            doActs (body {
                changeDirectionRel "270" "230"
            })
        }
        defBullet "guruc19" {
            doActs (body {
                changeDirectionRel "270" "90"
            })
        }
    }

  /// 怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん
  /// [Daiouzyou]_hibachi_image.xml
  let hibachi_image =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん" {
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
        defAction "spiral" {
            fire {
                speed "0"
                ofBullet (bulletAnon {
                    doActs (body {
                        repeat "30+$rank*45" {
                            fire {
                                sequence "$1"
                                speed "1+$rank*2"
                                plain
                            }
                            actionRef "XWay" ["10"; "36"]
                            wait "6-$rank*3"
                        }
                        vanish
                    })
                })
            }
            wait "225"
        }
        defAction "top1" {
            actionRef "spiral" ["7"]
            actionRef "spiral" ["-7"]
        }
        defAction "fan4" {
            fire {
                absolute "-$1"
                refBullet "Dummy" []
            }
            repeat "60+$rank*90" {
                fire {
                    sequence "$1"
                    speed "1+$rank*2"
                    plain
                }
                actionRef "XWay" ["4"; "90"]
                wait "6-$rank*3"
            }
        }
        defAction "top2" {
            actionRef "fan4" ["4"]
        }
        defAction "top3" {
            actionRef "fan4" ["-4"]
        }
    }

  /// 怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん
  /// [Daiouzyou]_hibachi_maybe.xml
  let hibachi_maybe =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん" {
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
            repeat "10+$rank*15" {
                repeat "2" {
                    fire {
                        sequence "$1"
                        speed "1.5+$rank*$rank*1.5"
                        refBullet "curve" ["$1"]
                    }
                    repeat "10+$rank*10-1" {
                        fire {
                            sequence "36/($rank+1)"
                            speedSeq "0"
                            refBullet "curve" ["$1"]
                        }
                    }
                    wait "6-$rank*3"
                }
                wait "6-$rank*3"
                fire {
                    sequence "$1"
                    speed "1+$rank*2"
                    refBullet "Dummy" []
                }
            }
        }
        defAction "top1" {
            actionRef "spiral" ["-2"]
        }
        defAction "fan4" {
            fire {
                absolute "-$1"
                refBullet "Dummy" []
            }
            repeat "30+$rank*45" {
                fire {
                    sequence "$1"
                    speed "1+$rank*$rank*2"
                    plain
                }
                actionRef "XWay" ["4"; "90"]
                wait "6-$rank*3"
            }
        }
        defAction "top2" {
            actionRef "fan4" ["4"]
        }
        defAction "top3" {
            actionRef "fan4" ["-4"]
        }
    }

  /// 怒首領蜂大往生一面ボス by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss.xml
  let round_1_boss =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生一面ボス by 白い弾幕くん" {
        top {
            repeat "3" {
                fire {
                    absolute "180"
                    speed "4"
                    refBullet "seed" []
                }
                wait "500"
            }
            wait "100"
        }
        defBullet "seed" {
            doActs (body {
                wait "9"
                fire {
                    relative "0"
                    refBullet "seed2" []
                }
                fire {
                    relative "180"
                    refBullet "seed2" []
                }
                vanish
            })
        }
        defBullet "seed2" {
            speed "18"
            doActs (body {
                wait "1"
                fire {
                    relative "90"
                    refBullet "seed3" []
                }
                vanish
            })
        }
        defBullet "seed3" {
            speed "0.8"
            doActs (body {
                changeDirectionSeq "1.2" "9999"
                repeat "100+200*$rank" {
                    fire {
                        sequence "180-12"
                        plain
                    }
                    fire {
                        sequence "180"
                        plain
                    }
                    wait "3-$rank*2*$rand"
                }
            })
            doActs (body {
                repeat "6" {
                    fire {
                        ofBullet (bulletAnon {
                            dir "-8"
                        })
                    }
                    repeat "4" {
                        fire {
                            ofBullet (bulletAnon {
                                sequence "4"
                                doActs (body {
                                    ()
                                })
                            })
                        }
                    }
                    wait "80"
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss_hakkyou.xml
  let round_1_boss_hakkyou =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん" {
        defAction "top1" {
            repeat "128" {
                wait "4"
                actionRef "four" ["$rand*90+135"]
            }
        }
        defAction "four" {
            fire {
                absolute "90"
                speed "6"
                refBullet "rb" ["$1"]
            }
            repeat "3" {
                fire {
                    sequence "60"
                    speed "6"
                    refBullet "rb" ["$1"]
                }
            }
        }
        defBullet "rb" {
            refActs "red" ["$1+$rand*20-10"]
        }
        defAction "red" {
            wait "1"
            fire {
                absolute "$1"
                speed "1+$rank"
                plain
            }
            vanish
        }
        defAction "top2" {
            repeat "4" {
                wait "160"
                fire {
                    refBullet "sht" ["1.2"]
                }
                wait "80"
            }
        }
        defBullet "sht" {
            doActs (body {
                repeat "16" {
                    fire {
                        dir "$rand*16-8"
                        speed "($1+$rand*$1)*($rank/2+0.65)"
                        plain
                    }
                }
                vanish
            })
        }
        defAction "top3" {
            repeat "4" {
                fire {
                    absolute "90"
                    refBullet "rd_seed" ["-5"; "-5"]
                }
                fire {
                    absolute "270"
                    refBullet "rd_seed" ["5"; "5"]
                }
                wait "240"
            }
        }
        defBullet "rd_seed" {
            speed "3"
            doActs (body {
                wait "1"
                fire {
                    speed "0"
                    refBullet "rd_seed2" []
                }
                fire {
                    speed "0"
                    refBullet "bd_seed" ["0"; "$2"]
                }
                fire {
                    speed "0"
                    refBullet "bd_seed" ["$1"; "$2"]
                }
                vanish
            })
        }
        defBullet "rd_seed2" {
            doActs (body {
                repeat "5" {
                    repeat "3" {
                        fire {
                            absolute "180"
                            speed "1.2"
                            plain
                        }
                        wait "4"
                    }
                    wait "12"
                }
                vanish
            })
        }
        defBullet "bd_seed" {
            doActs (body {
                fire {
                    dir "$2"
                    speed "0.6"
                    plain
                }
                repeat "11" {
                    fire {
                        sequence "$1"
                        speedSeq "0.2"
                        plain
                    }
                    wait "4"
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss.xml
  let round_3_boss =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん" {
        top {
            fire {
                absolute "155"
                speed "3.3"
                refBullet "roll" ["1"]
            }
            fire {
                absolute "205"
                speed "3.3"
                refBullet "roll" ["-1"]
            }
            fire {
                absolute "135"
                speed "3.2"
                refBullet "roll" ["1"]
            }
            fire {
                absolute "225"
                speed "3.2"
                refBullet "roll" ["-1"]
            }
            fire {
                absolute "135"
                speed "2"
                refBullet "roll" ["1"]
            }
            fire {
                absolute "225"
                speed "2"
                refBullet "roll" ["-1"]
            }
            wait "400"
        }
        defBullet "roll" {
            doActs (body {
                wait "12"
                changeSpeed "0" "1"
                fire {
                    absolute "180+90*$1"
                    ofBullet (bulletAnon {
                        doActs (body {
                            vanish
                        })
                    })
                }
                repeat "200" {
                    fire {
                        sequence "9"
                        speed "1+$rank"
                        plain
                    }
                    wait "2"
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_2.xml
  let round_3_boss_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん" {
        defBullet "Red" {
            doActs (body {
                ()
            })
        }
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defAction "Stop" {
            changeSpeed "0" "1"
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
        defAction "top1" {
            fire {
                absolute "90"
                refBullet "aim2" []
            }
            fire {
                absolute "-90"
                refBullet "aim2" []
            }
            fire {
                speed "0"
                ofBullet (bulletAnon {
                    refActs "fanRoll" ["7"]
                })
            }
            fire {
                speed "0"
                ofBullet (bulletAnon {
                    refActs "fanRoll" ["-7"]
                })
            }
        }
        defAction "top2" {
            actionRef "3wayRoll" ["13"]
        }
        defAction "top3" {
            actionRef "3wayRoll" ["-13"]
        }
        defAction "3wayRoll" {
            fire {
                absolute "180"
                refBullet "Dummy" []
            }
            repeat "14" {
                fire {
                    sequence "-1.3*$1"
                    speed "1.4+$rank*0.8"
                    plain
                }
                actionRef "XWay" ["3"; "$1"]
                wait "10"
            }
            repeat "20" {
                fire {
                    sequence "1.3*$1"
                    speed "1.4+$rank*0.8"
                    plain
                }
                actionRef "XWay" ["3"; "-$1"]
                wait "10"
            }
        }
        defAction "fanRoll" {
            fire {
                absolute "$1*8"
                refBullet "Dummy" []
            }
            repeat "32" {
                fire {
                    sequence "-$1*2.1"
                    speed "1.2+$rank*0.4"
                    plain
                }
                actionRef "XWayFan" ["4"; "$1"; "0.3"]
                wait "10"
            }
            vanish
        }
        defBullet "aim2" {
            speed "1"
            doActs (body {
                wait "8"
                actionRef "Stop" []
                repeat "14+$rank*12" {
                    wait "320/(14+$rank*12)+$rand"
                    fire {
                        aim "0"
                        speed "1.4+$rank*0.8"
                        refBullet "Red" []
                    }
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_last.xml
  let round_3_boss_last =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん" {
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        top {
            fire {
                absolute "100"
                speed "3"
                refBullet "armSrc" ["1"]
            }
            fire {
                absolute "-100"
                speed "3"
                refBullet "armSrc" ["0"]
            }
            fireRef "center" []
            wait "500"
        }
        defAction "center3" {
            fire {
                aim "-10.5*$1"
                refBullet "Dummy" []
            }
            repeat "3" {
                repeat "6" {
                    fire {
                        sequence "$1"
                        speed "1+$rank"
                        plain
                    }
                    repeat "3" {
                        fire {
                            sequence "90"
                            speedSeq "0"
                            plain
                        }
                    }
                    wait "5"
                }
                fire {
                    sequence "$1"
                    ofBullet (bulletAnon {
                        doActs (body {
                            vanish
                        })
                    })
                }
                wait "5"
            }
        }
        topFireAs "center" {
            absolute "180"
            speed "5"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "10"
                    changeSpeed "0" "1"
                    repeat "2" {
                        actionRef "center3" ["-4"]
                        wait "30"
                        actionRef "center3" ["4"]
                        wait "30"
                    }
                    vanish
                })
            })
        }
        defBullet "armSrc" {
            doActs (body {
                wait "12"
                changeSpeed "0" "1"
                wait "1"
                fireRef "arm" ["8-16*$1"; "0"]
                wait "2"
                fireRef "arm" ["8-16*$1"; "90"]
                wait "2"
                fireRef "arm" ["8-16*$1"; "180"]
                wait "2"
                fireRef "arm" ["8-16*$1"; "270"]
                vanish
            })
        }
        topFireAs "arm" {
            speed "0"
            ofBullet (bulletAnon {
                doActs (body {
                    fire {
                        absolute "$2"
                        speed "1.5"
                        plain
                    }
                    repeat "80+$rank*80" {
                        wait "480/(80+$rank*80)"
                        fire {
                            sequence "$1"
                            speedSeq "0"
                            plain
                        }
                    }
                    vanish
                })
            })
        }
    }

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss.xml
  let round_4_boss =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん" {
        top {
            fire {
                absolute "110"
                speed "3"
                refBullet "armSrc" ["1"]
            }
            fire {
                absolute "-110"
                speed "3"
                refBullet "armSrc" ["0"]
            }
            wait "400"
        }
        defBullet "armSrc" {
            doActs (body {
                wait "12"
                fire {
                    absolute "180"
                    speed "1"
                    refBullet "arm" ["$1"; "1"]
                }
                fire {
                    absolute "60"
                    speed "1"
                    refBullet "arm" ["$1"; "1"]
                }
                fire {
                    absolute "-60"
                    speed "1"
                    refBullet "arm" ["$1"; "1"]
                }
                fire {
                    absolute "180"
                    speed "1"
                    refBullet "arm" ["$1"; "-1"]
                }
                fire {
                    absolute "60"
                    speed "1"
                    refBullet "arm" ["$1"; "-1"]
                }
                fire {
                    absolute "-60"
                    speed "1"
                    refBullet "arm" ["$1"; "-1"]
                }
                vanish
            })
        }
        defBullet "arm" {
            doActs (body {
                wait "12"
                fire {
                    relative "180*$1"
                    ofBullet (bulletAnon {
                        doActs (body {
                            vanish
                        })
                    })
                }
                changeSpeed "0" "1"
                repeat "400/(6-$rank*2)" {
                    wait "6-$rank*2+$rand"
                    fire {
                        sequence "11*$2"
                        speed "1.5+$rank*0.5"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_1.xml
  let round_4_boss_1 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん" {
        defAction "Stop" {
            changeSpeed "0" "1"
        }
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defBullet "seed" {
            speed "4"
            doActs (body {
                wait "10"
                actionRef "Stop" []
                repeat "20" {
                    wait "20"
                    repeat "3" {
                        fire {
                            sequence "116+$rand*6-$rank*15"
                            speed "1.5"
                            plain
                        }
                        repeat "3.5+$rank*5" {
                            fire {
                                sequence "3"
                                speed "1.5"
                                plain
                            }
                        }
                    }
                }
                vanish
            })
        }
        defAction "xway" {
            fire {
                aim "-7*$1-7"
                refBullet "Dummy" []
            }
            repeat "$1" {
                fire {
                    sequence "15"
                    speed "1.3"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
                repeat "4" {
                    fire {
                        sequence "0"
                        speedSeq "0.1"
                        ofBullet (bulletAnon {
                            doActs (body {
                                ()
                            })
                        })
                    }
                }
            }
        }
        top {
            fire {
                absolute "110"
                refBullet "seed" []
            }
            fire {
                absolute "-110"
                refBullet "seed" []
            }
            wait "400"
        }
    }

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_2.xml
  let round_4_boss_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん" {
        defAction "Stop" {
            changeSpeed "0" "1"
        }
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defBullet "blue" {
            speed "3"
            doActs (body {
                wait "10"
                actionRef "Stop" []
                repeat "16+$rank*16" {
                    wait "10-$rank*4+$rand"
                    repeat "3" {
                        fire {
                            sequence "95"
                            speed "1.4"
                            plain
                        }
                        repeat "3" {
                            fire {
                                sequence "10"
                                speed "1.4"
                                plain
                            }
                        }
                    }
                }
                vanish
            })
        }
        defAction "xway" {
            fire {
                aim "-7*$1-7"
                refBullet "Dummy" []
            }
            repeat "$1" {
                fire {
                    sequence "15"
                    speed "1.3"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
                repeat "4" {
                    fire {
                        sequence "0"
                        speedSeq "0.08+$rank*0.08"
                        ofBullet (bulletAnon {
                            doActs (body {
                                ()
                            })
                        })
                    }
                }
            }
        }
        defBullet "red" {
            speed "3"
            doActs (body {
                wait "10"
                actionRef "Stop" []
                repeat "5" {
                    actionRef "xway" ["$rand*3+$rank*2"]
                    wait "40"
                }
                vanish
            })
        }
        top {
            fire {
                absolute "120"
                refBullet "blue" []
            }
            fire {
                absolute "-120"
                refBullet "red" []
            }
            wait "200"
            fire {
                absolute "-120"
                refBullet "blue" []
            }
            fire {
                absolute "120"
                refBullet "red" []
            }
            wait "200"
        }
    }

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_4.xml
  let round_4_boss_4 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん" {
        defAction "Stop" {
            changeSpeed "0" "1"
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
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defAction "fan" {
            wait "30"
            fire {
                absolute "$1"
                speed "1.2+$rank"
                refBullet "Dummy" []
            }
            actionRef "XWay" ["$2"; "$3"]
            repeat "6" {
                wait "30"
                fire {
                    sequence "$4"
                    speedSeq "0"
                    plain
                }
                actionRef "XWay" ["$2"; "$3"]
            }
        }
        defAction "top1" {
            actionRef "fan" ["220"; "8"; "5"; "-42.5"]
            actionRef "fan" ["150"; "8"; "-5"; "42.5"]
        }
        defAction "top2" {
            actionRef "fan" ["200"; "7"; "2.5"; "-22.5"]
            actionRef "fan" ["170"; "7"; "-2.5"; "22.5"]
        }
        defAction "top3" {
            actionRef "fan" ["160"; "8"; "5"; "-42.5"]
            actionRef "fan" ["210"; "8"; "-5"; "42.5"]
        }
        defAction "top4" {
            wait "20"
            repeat "2" {
                repeat "36+$rank*20" {
                    fire {
                        dir "$rand*360"
                        speed "2"
                        ofBullet (bulletAnon {
                            doActs (body {
                                wait "10*$rand"
                                actionRef "Stop" []
                                wait "60"
                                changeDirectionAim "0" "1"
                                changeSpeed "2.4" "1"
                            })
                        })
                    }
                    wait "3"
                }
                wait "60-$rank*60"
            }
        }
    }

  /// 怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_5.xml
  let round_4_boss_5 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん" {
        defAction "Stop" {
            changeSpeed "0" "1"
        }
        defAction "XWay" {
            repeat "$1-1" {
                fire {
                    sequence "$2"
                    speedSeq "0"
                    plain
                }
            }
        }
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defBullet "blueFan" {
            speed "3"
            doActs (body {
                wait "20"
                actionRef "Stop" []
                repeat "6" {
                    fire {
                        sequence "120+$1*2"
                        speed "1.6"
                        plain
                    }
                    actionRef "XWay" ["3"; "120"]
                    repeat "6+$rank*6" {
                        wait "56/(6+$rank*6)"
                        fire {
                            sequence "120+$1"
                            speed "1.6"
                            plain
                        }
                        actionRef "XWay" ["3"; "120"]
                    }
                    wait "14"
                }
                vanish
            })
        }
        defAction "singleRedAim" {
            fire {
                aim "0"
                speed "2"
                ofBullet (bulletAnon {
                    doActs (body {
                        ()
                    })
                })
            }
            repeat "15" {
                wait "4"
                fire {
                    sequence "0"
                    speed "2"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
            }
        }
        defAction "doubleRedAim" {
            fire {
                aim "-5*$1"
                speed "2"
                ofBullet (bulletAnon {
                    doActs (body {
                        ()
                    })
                })
            }
            fire {
                sequence "20*$1"
                speed "2"
                ofBullet (bulletAnon {
                    doActs (body {
                        ()
                    })
                })
            }
            repeat "15" {
                wait "4"
                fire {
                    sequence "-20*$1"
                    speed "2"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
                fire {
                    sequence "20*$1"
                    speed "2"
                    ofBullet (bulletAnon {
                        doActs (body {
                            ()
                        })
                    })
                }
            }
        }
        defBullet "redAim2" {
            speed "1"
            doActs (body {
                wait "20"
                actionRef "Stop" []
                wait "100"
                actionRef "singleRedAim" []
                wait "60"
                actionRef "doubleRedAim" ["-1"]
                wait "20"
                actionRef "doubleRedAim" ["-1"]
                vanish
            })
        }
        defBullet "redAim1" {
            speed "1"
            doActs (body {
                wait "20"
                actionRef "Stop" []
                wait "40"
                actionRef "singleRedAim" []
                wait "60"
                actionRef "doubleRedAim" ["1"]
                wait "80"
                actionRef "doubleRedAim" ["1"]
                vanish
            })
        }
        top {
            fire {
                absolute "90"
                refBullet "blueFan" ["4"]
            }
            fire {
                absolute "-90"
                refBullet "blueFan" ["-4"]
            }
            fire {
                absolute "90"
                refBullet "redAim2" []
            }
            fire {
                absolute "-90"
                refBullet "redAim1" []
            }
            wait "400"
        }
    }

  /// 怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_1.xml
  let round_5_boss_1 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん" {
        defAction "Stop" {
            changeSpeed "0" "1"
        }
        defAction "XWay" {
            repeat "$1-1" {
                fire {
                    sequence "$2"
                    speedSeq "0"
                    plain
                }
            }
        }
        defBullet "aim3" {
            doActs (body {
                wait "10"
                fire {
                    speed "0"
                    refBullet "aim3Impl" []
                }
                vanish
            })
        }
        defBullet "aim3Impl" {
            doActs (body {
                repeat "7" {
                    fire {
                        aim "-33+$rand*6"
                        speed "1.5"
                        plain
                    }
                    actionRef "XWay" ["3"; "30"]
                    repeat "2+$rank*3" {
                        wait "3"
                        fire {
                            sequence "-60"
                            speed "1.5"
                            plain
                        }
                        actionRef "XWay" ["3"; "30"]
                    }
                    wait "54-$rank*9"
                }
                vanish
            })
        }
        defBullet "aim" {
            doActs (body {
                wait "10"
                fire {
                    speed "0"
                    refBullet "aimImpl" []
                }
                vanish
            })
        }
        defBullet "aimImpl" {
            doActs (body {
                repeat "7" {
                    fire {
                        aim "-3+$rand*6"
                        speed "1.5"
                        plain
                    }
                    repeat "2+$rank*3" {
                        wait "3"
                        fire {
                            sequence "0"
                            speed "1.5"
                            plain
                        }
                    }
                    wait "54-$rank*9"
                }
                vanish
            })
        }
        defBullet "fan" {
            doActs (body {
                wait "10"
                actionRef "Stop" []
                repeat "3+$rank*4" {
                    fire {
                        absolute "$1-$2*3"
                        speed "$3"
                        plain
                    }
                    actionRef "XWay" ["7"; "10"]
                    wait "420/(3+$rank*4)"
                }
                vanish
            })
        }
        top {
            fire {
                absolute "110"
                speed "4"
                refBullet "aim3" []
            }
            fire {
                absolute "-110"
                speed "4"
                refBullet "aim3" []
            }
            fire {
                absolute "125"
                speed "5"
                refBullet "aim" []
            }
            fire {
                absolute "-125"
                speed "5"
                refBullet "aim" []
            }
            fire {
                absolute "150"
                speed "7"
                refBullet "aim" []
            }
            fire {
                absolute "-150"
                speed "7"
                refBullet "aim" []
            }
            wait "10"
            fire {
                absolute "90"
                speed "6"
                refBullet "fan" ["-135"; "10"; "1.3"]
            }
            fire {
                absolute "-90"
                speed "6"
                refBullet "fan" ["135"; "10"; "1.3"]
            }
            fire {
                absolute "110"
                speed "4"
                refBullet "fan" ["-164"; "8"; "1.2"]
            }
            fire {
                absolute "-110"
                speed "4"
                refBullet "fan" ["156"; "8"; "1.2"]
            }
            fire {
                absolute "130"
                speed "2"
                refBullet "fan" ["180"; "8"; "1.1"]
            }
            fire {
                absolute "-130"
                speed "2"
                refBullet "fan" ["180"; "5"; "1.1"]
            }
            wait "430"
        }
    }

  /// 怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_2.xml
  let round_5_boss_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん" {
        defBullet "Red" {
            doActs (body {
                ()
            })
        }
        defAction "Stop" {
            changeSpeed "0" "1"
        }
        defBullet "Dummy" {
            doActs (body {
                vanish
            })
        }
        defBullet "seven" {
            absolute "180"
            speed "4"
            doActs (body {
                wait "10"
                actionRef "Stop" []
                repeat "5+$rank*4" {
                    fire {
                        aim "-10"
                        speed "1.5"
                        refBullet "Red" []
                    }
                    fire {
                        aim "10"
                        speed "1.5"
                        refBullet "Red" []
                    }
                    fire {
                        aim "-5"
                        speed "1.3"
                        refBullet "Red" []
                    }
                    fire {
                        aim "5"
                        speed "1.3"
                        refBullet "Red" []
                    }
                    fire {
                        aim "-5"
                        speed "1.7"
                        refBullet "Red" []
                    }
                    fire {
                        aim "5"
                        speed "1.7"
                        refBullet "Red" []
                    }
                    fire {
                        aim "0"
                        speed "1.5"
                        refBullet "Red" []
                    }
                    wait "360/(5+$rank*4)"
                }
                vanish
            })
        }
        defBullet "fan" {
            speed "4"
            doActs (body {
                wait "10"
                actionRef "Stop" []
                fire {
                    dir "$1"
                    refBullet "Dummy" []
                }
                repeat "35+$rank*35" {
                    fire {
                        sequence "$2"
                        speed "$3"
                        plain
                    }
                    wait "10/(1+$rank)+$rand"
                }
                vanish
            })
        }
        top {
            fire {
                refBullet "seven" []
            }
            fire {
                absolute "170"
                refBullet "fan" ["55"; "10"; "1.8+$rank*0.4"]
            }
            fire {
                absolute "170"
                refBullet "fan" ["60"; "10"; "1+$rank*0.2"]
            }
            fire {
                absolute "170"
                refBullet "fan" ["225"; "10"; "1.4+$rank*0.2"]
            }
            fire {
                absolute "170"
                refBullet "fan" ["250"; "10"; "1.3+$rank*0.2"]
            }
            fire {
                absolute "-170"
                refBullet "fan" ["55"; "-10"; "1.8+$rank*0.4"]
            }
            fire {
                absolute "-170"
                refBullet "fan" ["60"; "-10"; "1+$rank*0.2"]
            }
            fire {
                absolute "-170"
                refBullet "fan" ["225"; "-10"; "1.4+$rank*0.2"]
            }
            fire {
                absolute "-170"
                refBullet "fan" ["250"; "-10"; "1.3+$rank*0.2"]
            }
            wait "360"
        }
    }

  /// 怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_1.xml
  let round_6_boss_1 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん" {
        top {
            repeat "64" {
                wait "2"
                actionRef "four" ["$rand*90+135"]
            }
        }
        defAction "four" {
            fire {
                absolute "90"
                speed "6"
                refBullet "rb" ["$1"]
            }
            repeat "3" {
                fire {
                    sequence "60"
                    speed "6"
                    refBullet "rb" ["$1"]
                }
            }
        }
        defBullet "rb" {
            refActs "red" ["$1+$rand*20-10"]
        }
        defAction "red" {
            wait "1"
            fire {
                absolute "$1"
                speed "1.2+$rank"
                plain
            }
            vanish
        }
    }

  /// 怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_2.xml
  let round_6_boss_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん" {
        top {
            changeSpeed "4" "1"
            changeDirectionAbs "180" "1"
            wait "10"
            changeSpeed "0" "1"
            repeat "5" {
                fire {
                    absolute "90"
                    refBullet "bl_seed" []
                }
                fire {
                    absolute "270"
                    refBullet "bl_seed" []
                }
                wait "80"
            }
            changeSpeed "4" "1"
            changeDirectionAbs "0" "1"
            wait "10"
            changeSpeed "0" "1"
        }
        defBullet "bl_seed" {
            speed "24"
            doActs (body {
                wait "1"
                fire {
                    ofBullet (bulletAnon {
                        speed "0"
                        refActs "bl" []
                    })
                }
                vanish
            })
        }
        defAction "bl" {
            fire {
                aim "-30"
                speedAbs "1"
                plain
            }
            repeat "4" {
                fire {
                    sequence "15"
                    speedSeq "0"
                    plain
                }
            }
            wait "4"
            repeat "3+$rank*6" {
                fire {
                    aim "-30"
                    speedSeq "0.4"
                    plain
                }
                repeat "4" {
                    fire {
                        sequence "15"
                        speedSeq "0"
                        plain
                    }
                }
                wait "4"
            }
            vanish
        }
    }

  /// 怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_3.xml
  let round_6_boss_3 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん" {
        top {
            repeat "3" {
                fire {
                    absolute "270"
                    refBullet "bm_seed" ["-25"]
                }
                wait "20"
                fire {
                    absolute "90"
                    refBullet "bm_seed" ["25"]
                }
                wait "100"
            }
        }
        defBullet "bm_seed" {
            speed "24"
            doActs (body {
                wait "1"
                fire {
                    ofBullet (bulletAnon {
                        absolute "180"
                        speed "3"
                        refActs "bm" []
                    })
                }
                fire {
                    ofBullet (bulletAnon {
                        sequence "$1"
                        speed "2"
                        refActs "bm" []
                    })
                }
                vanish
            })
        }
        defAction "bm" {
            changeSpeed "0" "50"
            wait "45"
            fire {
                refBullet "round" ["1.5"; "0"]
            }
            fire {
                refBullet "round" ["1.25"; "7"]
            }
            fire {
                refBullet "round" ["1"; "14"]
            }
            vanish
        }
        defBullet "round" {
            speed "0"
            doActs (body {
                fire {
                    absolute "$2"
                    speed "$1"
                    plain
                }
                repeat "10+$rank*10" {
                    fire {
                        sequence "360/(10+$rank*10)"
                        speed "$1"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_4.xml
  let round_6_boss_4 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん" {
        top {
            fire {
                refBullet "round_seed" []
            }
            fire {
                refBullet "sht" ["0.8"]
            }
            wait "20"
            fire {
                refBullet "round_seed" []
            }
            wait "100"
            fire {
                refBullet "round_seed" []
            }
            fire {
                refBullet "sht" ["1"]
            }
            wait "20"
            fire {
                refBullet "round_seed" []
            }
            wait "100"
            fire {
                refBullet "round_seed" []
            }
            fire {
                refBullet "sht" ["1.2"]
            }
            wait "20"
            fire {
                refBullet "round_seed" []
            }
            wait "25"
        }
        defBullet "sht" {
            doActs (body {
                repeat "16" {
                    fire {
                        dir "$rand*16-8"
                        speed "($1+$rand*$1)*(1+$rank*$rank)"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "round_seed" {
            speed "0"
            doActs (body {
                fire {
                    dir "0"
                    refBullet "two" []
                }
                repeat "15" {
                    fire {
                        sequence "22.5"
                        refBullet "two" []
                    }
                }
                vanish
            })
        }
        defBullet "two" {
            doActs (body {
                fire {
                    relative "-4"
                    speed "1+$rank"
                    plain
                }
                fire {
                    relative "4"
                    speed "1+$rank"
                    plain
                }
                vanish
            })
        }
    }

  /// 怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_5.xml
  let round_6_boss_5 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん" {
        top {
            repeat "2" {
                fire {
                    absolute "180"
                    speed "4"
                    refBullet "seed" []
                }
                wait "500"
            }
            wait "200"
        }
        defBullet "seed" {
            doActs (body {
                wait "9"
                fire {
                    relative "0"
                    refBullet "seed2" []
                }
                fire {
                    relative "180"
                    refBullet "seed2" []
                }
                vanish
            })
        }
        defBullet "seed2" {
            speed "18"
            doActs (body {
                wait "1"
                fire {
                    relative "90"
                    refBullet "seed3" []
                }
                vanish
            })
        }
        defBullet "seed3" {
            speed "0.8"
            doActs (body {
                changeDirectionSeq "1.2" "9999"
            })
            doActs (body {
                repeat "62+$rank*100" {
                    fire {
                        sequence "40-10"
                        plain
                    }
                    fire {
                        sequence "140"
                        plain
                    }
                    fire {
                        sequence "40"
                        plain
                    }
                    fire {
                        sequence "140"
                        plain
                    }
                    wait "8-$rank*6"
                }
            })
            doActs (body {
                repeat "5" {
                    fire {
                        refBullet "tw" []
                    }
                    wait "138"
                }
                vanish
            })
        }
        defBullet "tw" {
            doActs (body {
                fire {
                    ofBullet (bulletAnon {
                        dir "-12"
                        doActs (body {
                            ()
                        })
                    })
                }
                repeat "3.5+$rank*5+$rand" {
                    fire {
                        ofBullet (bulletAnon {
                            sequence "4"
                            doActs (body {
                                ()
                            })
                        })
                    }
                }
                vanish
            })
        }
    }
