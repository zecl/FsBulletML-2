namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Bulletsmorph
[<RequireQualifiedAccess>]
module Bulletsmorph =

  /// Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん
  /// [Bulletsmorph]_aba_2.xml
  let aba_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん" {
        top {
            repeat "8" {
                actionRef "center" ["90 * $rand"; "1"]
                wait "12"
                actionRef "center" ["90 * $rand"; "-1"]
                wait "12"
                actionRef "center" ["30 * $rand"; "1"]
                wait "12"
                actionRef "center" ["30 * $rand"; "-1"]
                wait "12"
            }
            wait "150"
        }
        defAction "center" {
            fire {
                absolute "360 * $rand"
                refBullet "circle" ["$1"; "$2"]
            }
            repeat "(4 + 8 * $rank) - 1" {
                fire {
                    sequence "360 / (4 + 8 * $rank)"
                    refBullet "circle" ["$1"; "$2"]
                }
            }
        }
        defBullet "circle" {
            speed "1.3"
            doActs (body {
                wait "20"
                changeDirectionAbs "180 + $1 * $2" "1"
                wait "125 - $1"
                fire {
                    aim "0"
                    refBullet "red" []
                }
                vanish
            })
        }
        defBullet "red" {
            speed "0.1"
            doActs (body {
                changeSpeed "4.0" "300"
            })
        }
    }

  /// Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん
  /// [Bulletsmorph]_aba_3.xml
  let aba_3 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん" {
        top {
            repeat "4 + 16 * $rank" {
                fire {
                    absolute "120 + 120 * $rand"
                    refBullet "bomb" []
                }
                wait "60 - 30 * $rank"
            }
            wait "180"
        }
        defBullet "bomb" {
            speed "0.5 + 1.9 * $rand"
            doActs (body {
                wait "50"
                fire {
                    absolute "360 * $rand"
                    refBullet "bombbit" []
                }
                repeat "(4 + 8 * $rank) - 1" {
                    fire {
                        sequence "360 / (4 + 8 * $rank)"
                        refBullet "bombbit" []
                    }
                }
                vanish
            })
        }
        defBullet "bombbit" {
            speed "0.8"
            doActs (body {
                wait "120"
                fire {
                    relative "120"
                    speed "1.3"
                    plain
                }
                fire {
                    relative "240"
                    speed "1.3"
                    plain
                }
                fire {
                    aim "0"
                    speed "1.3"
                    refBullet "changecolor" []
                }
                vanish
            })
        }
        defBullet "changecolor" {
            doActs (body {
                fire {
                    relative "0"
                    speedRel "0"
                    plain
                }
                vanish
            })
        }
    }

  /// Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん
  /// [Bulletsmorph]_aba_4.xml
  let aba_4 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん" {
        top {
            fire {
                speed "0.1"
                refBullet "cross" []
            }
            wait "5"
            repeat "40 + 60 * $rank" {
                fire {
                    speedSeq "0.04"
                    refBullet "cross" []
                }
                wait "20 - 10 * $rank"
            }
            wait "60"
        }
        defBullet "cross" {
            aim "0"
            doActs (body {
                changeSpeedRel "4.0" "300"
                wait "45"
                fire {
                    absolute "0"
                    speed "1.3"
                    plain
                }
                fire {
                    absolute "90"
                    speed "1.3"
                    plain
                }
                fire {
                    absolute "-90"
                    speed "1.3"
                    plain
                }
                fire {
                    aim "0"
                    speed "1.3"
                    plain
                }
            })
        }
    }

  /// Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん
  /// [Bulletsmorph]_aba_5.xml
  let aba_5 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん" {
        top {
            fire {
                absolute "90"
                refBullet "bit" ["1"]
            }
            fire {
                absolute "90"
                refBullet "bit" ["-1"]
            }
            fire {
                absolute "-90"
                refBullet "bit" ["1"]
            }
            fire {
                absolute "-90"
                refBullet "bit" ["-1"]
            }
            repeat "300" {
                fire {
                    absolute "-(120 + 45 * $rank) + (240 + 90 * $rank) * $rand"
                    speed "1.6"
                    plain
                }
                repeat "5" {
                    fire {
                        sequence "0"
                        speedSeq "0.2"
                        plain
                    }
                }
                wait "2"
            }
        }
        defBullet "bit" {
            speed "0.2"
            doActs (body {
                wait "60"
                changeSpeed "0" "1"
                wait "5"
                fire {
                    aim "(45 - 25 * $rank) * $1"
                    refBullet "backstab" []
                }
                wait "3"
                repeat "29" {
                    fire {
                        sequence "-0.5 * $1"
                        refBullet "backstab" []
                    }
                    wait "3"
                }
                repeat "30" {
                    fire {
                        sequence "0.5 * $1"
                        refBullet "backstab" []
                    }
                    wait "3"
                }
                repeat "30" {
                    fire {
                        sequence "-0.5 * $1"
                        refBullet "backstab" []
                    }
                    wait "3"
                }
                vanish
            })
        }
        defBullet "backstab" {
            speed "1.6"
            doActs (body {
                wait "70 + 20 * $rand"
                changeDirectionAim "0" "1"
            })
        }
    }

  /// Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん
  /// [Bulletsmorph]_aba_6.xml
  let aba_6 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん" {
        top {
            actionRef "allway" []
            actionRef "bar" []
            wait "200"
        }
        defAction "allway" {
            fire {
                aim "15"
                refBullet "allwaybit" []
            }
            repeat "11" {
                fire {
                    sequence "30"
                    refBullet "allwaybit" []
                }
            }
        }
        defBullet "allwaybit" {
            speed "6.0"
            doActs (body {
                repeat "999" {
                    fire {
                        relative "90"
                        refBullet "stopandgo" []
                    }
                    fire {
                        relative "-90"
                        refBullet "stopandgo" []
                    }
                    wait "6 - 4 * $rank"
                }
            })
        }
        defBullet "stopandgo" {
            speed "1.0"
            doActs (body {
                wait "20"
                changeSpeed "0.0001" "1"
                wait "40"
                changeSpeed "4.0" "300"
            })
        }
        defAction "bar" {
            fire {
                refBullet "barhand" ["1"]
            }
            fire {
                refBullet "barhand" ["-1"]
            }
        }
        defBullet "barhand" {
            absolute "0"
            speed "0.0001"
            doActs (body {
                fire {
                    absolute "90"
                    speed "4.0 - 2.0 * $rank"
                    refBullet "barbit" ["1"]
                }
                repeat "2 + 3 * $rank" {
                    fire {
                        sequence "0"
                        speedSeq "4.0 - 2.0 * $rank"
                        refBullet "barbit" ["1"]
                    }
                }
                fire {
                    sequence "180"
                    speed "4.0 - 2.0 * $rank"
                    refBullet "barbit" ["-1"]
                }
                repeat "2 + 3 * $rank" {
                    fire {
                        sequence "0"
                        speedSeq "4.0 - 2.0 * $rank"
                        refBullet "barbit" ["-1"]
                    }
                }
                wait "5"
                repeat "20" {
                    fire {
                        sequence "180 + 10 * $1"
                        speed "4.0 - 2.0 * $rank"
                        refBullet "barbit" ["1"]
                    }
                    repeat "2 + 3 * $rank" {
                        fire {
                            sequence "0"
                            speedSeq "4.0 - 2.0 * $rank"
                            refBullet "barbit" ["1"]
                        }
                    }
                    fire {
                        sequence "180"
                        speed "4.0 - 2.0 * $rank"
                        refBullet "barbit" ["-1"]
                    }
                    repeat "2 + 3 * $rank" {
                        fire {
                            sequence "0"
                            speedSeq "4.0 - 2.0 * $rank"
                            refBullet "barbit" ["-1"]
                        }
                    }
                    wait "5"
                }
                vanish
            })
        }
        defBullet "barbit" {
            doActs (body {
                wait "5"
                changeSpeed "0.0001" "1"
                wait "5"
                fire {
                    relative "90 * $1"
                    speed "1.3"
                    plain
                }
                repeat "2" {
                    fire {
                        sequence "0"
                        speedSeq "0.1"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん
  /// [Bulletsmorph]_aba_7.xml
  let aba_7 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん" {
        top {
            repeat "3" {
                fire {
                    absolute "90"
                    speed "1.1"
                    refBullet "dummy" []
                }
                wait "60"
                fire {
                    absolute "-90"
                    speed "1.1"
                    refBullet "dummy" []
                }
                wait "60"
            }
            wait "250 - 50 * $rank"
        }
        defBullet "dummy" {
            doActs (body {
                wait "60"
                fire {
                    aim "-32"
                    speed "1.1"
                    refBullet "bit" []
                }
                repeat "8" {
                    fire {
                        sequence "8"
                        speedSeq "0"
                        refBullet "bit" []
                    }
                }
                vanish
            })
        }
        defBullet "bit" {
            doActs (body {
                wait "20"
                fire {
                    relative "0"
                    speedRel "0.3"
                    refBullet "slowdown" []
                }
                repeat "2 + 4 * $rank" {
                    fire {
                        relative "0"
                        speedSeq "0.3"
                        refBullet "slowdown" []
                    }
                }
                wait "20"
                changeDirectionAim "(30 - 20 * $rank) * (-1.0 + 2.0 * $rand)" "1"
                changeSpeedRel "2.0 + 2.0 * $rank" "300"
            })
        }
        defBullet "slowdown" {
            doActs (body {
                wait "20"
                changeSpeed "0.3" "60"
            })
        }
    }

  /// Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん
  /// [Bulletsmorph]_convergent.xml
  let convergent =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん" {
        top {
            fire {
                absolute "360 * $rand"
                speed "1.0"
                refBullet "nwaybit" ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"]
            }
            repeat "35" {
                fire {
                    sequence "10"
                    speed "1.0"
                    refBullet "nwaybit" ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"]
                }
            }
            repeat "36" {
                fire {
                    sequence "10"
                    speed "1.0"
                    refBullet "nwaybit" ["-90"; "1.5 * (0.5 + 0.5 * $rank)"; "-3"]
                }
            }
            wait "150"
        }
        defBullet "nwaybit" {
            doActs (body {
                fire {
                    relative "$1"
                    speed "$2"
                    plain
                }
                wait "4"
                repeat "2 + 4 * $rank" {
                    fire {
                        sequence "$3"
                        speed "$2"
                        plain
                    }
                    wait "4"
                }
                vanish
            })
        }
    }

  /// Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん
  /// [Bulletsmorph]_double_seduction.xml
  let double_seduction =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん" {
        top {
            fire {
                aim "30"
                refBullet "parentbit" ["1"]
            }
            fire {
                aim "-30"
                refBullet "parentbit" ["-1"]
            }
            wait "300"
        }
        defBullet "parentbit" {
            speed "2.0"
            doActs (body {
                actionRef "cross" ["75"; "0"]
                actionRef "cross" ["70"; "0"]
                actionRef "cross" ["65"; "0"]
                actionRef "cross" ["60"; "0"]
                actionRef "cross" ["55"; "0"]
                actionRef "cross" ["50"; "0"]
                actionRef "cross" ["80"; "15 * $1"]
                actionRef "cross" ["75"; "10 * $1"]
                actionRef "cross" ["70"; "6 * $1"]
                actionRef "cross" ["65"; "3 * $1"]
                actionRef "cross" ["60"; "1 * $1"]
                actionRef "cross" ["55"; "0"]
                vanish
            })
        }
        defAction "cross" {
            fire {
                absolute "0"
                refBullet "aimbit" ["$1"; "$2"]
            }
            fire {
                absolute "90"
                refBullet "aimbit" ["$1"; "$2"]
            }
            fire {
                absolute "180"
                refBullet "aimbit" ["$1"; "$2"]
            }
            fire {
                absolute "270"
                refBullet "aimbit" ["$1"; "$2"]
            }
            wait "5"
        }
        defBullet "aimbit" {
            speed "0.6"
            doActs (body {
                wait "$1"
                fire {
                    aim "$2"
                    speed "1.6 * (0.5 + 0.5 * $rank)"
                    plain
                }
                repeat "2 + 5 * $rank" {
                    fire {
                        sequence "0"
                        speedSeq "0.1"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// Bulletsmorphで生成。落下するひも。by 白い弾幕くん
  /// [Bulletsmorph]_fallen_string.xml
  let fallen_string =
    createBulletmlInfo <|
    untyped "Bulletsmorphで生成。落下するひも。by 白い弾幕くん" {
        top {
            repeat "5" {
                actionRef "impl:48" []
                wait "50"
            }
            wait "50"
        }
        defAction "impl:48" {
            wait "20"
            fire {
                relative "-90"
                speedAbs "0.6"
                ofBullet (bulletAnon {
                    doActs (body {
                        actionRef "impl:60" []
                        actionRef "impl:38" []
                    })
                })
            }
        }
        defAction "impl:60" {
            fire {
                aim "0"
                speedAbs "1"
                plain
            }
            fire {
                absolute "180"
                speedAbs "1.8"
                plain
            }
            wait "3"
        }
        defBullet "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel" {
            doActs (body {
                accel "120" {
                    verticalAbs "2.7"
                }
            })
        }
        defAction "impl:38" {
            fireRef "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt" ["90"]
            wait "24-$rank*8"
            fireRef "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt" ["-90"]
            wait "24-$rank*8"
        }
        topFireAs "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt" {
            relative "$1-25+$rand*50"
            ofBullet (bulletAnon {
                doActs (body {
                    actionRef "impl:59" []
                })
            })
        }
        defAction "impl:59" {
            repeat "9999" {
                fire {
                    absolute "0"
                    speedAbs "1"
                    refBullet "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel" []
                }
                fire {
                    absolute "60"
                    speedAbs "1.8"
                    refBullet "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel" []
                }
                wait "3"
            }
        }
    }

  /// Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん
  /// [Bulletsmorph]_kunekune_plus_homing.xml
  let kunekune_plus_homing =
    createBulletmlInfo <|
    untyped "Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん" {
        top {
            repeat "4" {
                actionRef "impl:259" []
                wait "50"
            }
            wait "60"
        }
        defAction "impl:259" {
            fire {
                aim "15+30*$rand"
                speedAbs "1.8-$rank+$rand"
                ofBullet (bulletAnon {
                    doActs (body {
                        actionRef "impl:30" []
                    })
                })
            }
        }
        defAction "impl:30" {
            fire {
                absolute "$2"
                speedAbs "$1"
                ofBullet (bulletAnon {
                    doActs (body {
                        actionRef "impl:156" []
                        vanish
                    })
                })
            }
            repeat "10+$rank*10" {
                actionRef "impl:12" []
            }
            vanish
        }
        defAction "impl:156" {
            wait "1"
            fire {
                sequence "0"
                refBullet "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr" []
            }
        }
        defBullet "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr" {
            speedAbs "2"
            doActs (body {
                changeSpeedAbs "0.3" "30"
                wait "100"
                changeSpeedAbs "5" "100"
            })
            doActs (body {
                repeat "12" {
                    changeDirectionAim "0" "45-$rank*30"
                    wait "5"
                }
            })
        }
        defAction "impl:12" {
            repeat "9999" {
                wait "2"
                fire {
                    sequence "15"
                    plain
                }
            }
        }
    }

  /// Bulletsmorphで生成。悟君が4人。by 白い弾幕くん
  /// [Bulletsmorph]_satoru4.xml
  let satoru4 =
    createBulletmlInfo <|
    untyped "Bulletsmorphで生成。悟君が4人。by 白い弾幕くん" {
        top {
            actionRef "impl:100" []
            wait "80"
        }
        defAction "impl:100" {
            repeat "4" {
                fire {
                    aim "$rand*16-8"
                    speedAbs "($1+$rand*$1)*($rank/2+0.65)"
                    ofBullet (bulletAnon {
                        doActs (body {
                            actionRef "impl:205" []
                        })
                    })
                }
                wait "1"
            }
        }
        defAction "impl:205" {
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way" ["$rank*3+$rand"]
        }
        defAction "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way" {
            changeDirectionAim "$rand*360" "1"
            changeSpeedAbs "2" "1"
            wait "30"
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way" ["$1"]
            changeSpeedAbs "0" "1"
            vanish
        }
        defAction "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way" {
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" ["$1"; "-30"]
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" ["$1"; "-15"]
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" ["$1"; "0"]
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" ["$1"; "15"]
            actionRef "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" ["$1"; "30"]
        }
        defAction "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way" {
            fire {
                aim "$2+$1*$rand*2-$1"
                speedAbs "1"
                plain
            }
            repeat "20" {
                fire {
                    aim "$2+$1*$rand*2-$1"
                    speedSeq "0.1"
                    plain
                }
            }
        }
    }
