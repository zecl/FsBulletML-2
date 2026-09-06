namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Original
[<RequireQualifiedAccess>]
module Original =

  /// 大原さんのオリジナル、断罪 by 白い弾幕くん
  /// [Original]_accusation.xml
  let accusation =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、断罪 by 白い弾幕くん" {
        top {
            fire {
                absolute "360 * $rand"
                refBullet "centerbit" []
            }
            wait "800"
        }
        defBullet "centerbit" {
            absolute "180"
            speed "0.9"
            doActs (body {
                wait "40"
                changeSpeed "0.0" "1"
                wait "5"
                fire {
                    absolute "360 * $rand"
                    refBullet "pillarbit" []
                }
                repeat "17" {
                    fire {
                        sequence "5"
                        refBullet "dummybit" []
                    }
                }
                repeat "3" {
                    fire {
                        sequence "5"
                        refBullet "pillarbit" []
                    }
                    repeat "17" {
                        fire {
                            sequence "5"
                            refBullet "dummybit" []
                        }
                    }
                }
                wait "120"
                repeat "140" {
                    fire {
                        absolute "360 * $rand"
                        speed "0.2"
                        refBullet "weak" ["240"]
                    }
                    wait "2"
                }
                repeat "70" {
                    fire {
                        absolute "360 * $rand"
                        speed "2.0"
                        refBullet "weak" ["24"]
                    }
                    repeat "4" {
                        fire {
                            sequence "0"
                            speedSeq "-0.2"
                            refBullet "weak" ["24"]
                        }
                    }
                    wait "2"
                }
                vanish
            })
        }
        defBullet "pillarbit" {
            speed "0.6"
            doActs (body {
                wait "120"
                changeSpeed "0.001" "1"
                wait "120"
                repeat "300 / (35 - 33 * $rank)" {
                    repeat "10" {
                        fire {
                            absolute "360 * $rand"
                            speed "2.0"
                            refBullet "weak" ["15"]
                        }
                    }
                    fire {
                        relative "-45 + 90 * $rand"
                        speed "(2.5 + 1.0 * $rand) * (0.25 + 0.75 * $rank)"
                        plain
                    }
                    wait "35 - 33 * $rank"
                }
                vanish
            })
        }
        defBullet "dummybit" {
            speed "0.6"
            doActs (body {
                wait "120"
                changeSpeed "0.001" "1"
                wait "120"
                repeat "300 / (35 - 33 * $rank)" {
                    fire {
                        relative "-45 + 90 * $rand"
                        speed "(2.5 + 1.0 * $rand) * (0.25 + 0.75 * $rank)"
                        plain
                    }
                    wait "35 - 33 * $rank"
                }
                vanish
            })
        }
        defBullet "weak" {
            doActs (body {
                wait "$1"
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、風の精 by 白い弾幕くん
  /// [Original]_air_elemental.xml
  let air_elemental =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、風の精 by 白い弾幕くん" {
        top {
            fire {
                refBullet "parentbit" []
            }
            wait "650"
        }
        defAction "slash" {
            fire {
                absolute "360 * $rand"
                speed "0.22"
                refBullet "spiralbit" ["150"]
            }
            fire {
                sequence "120"
                speed "0.22"
                refBullet "spiralbit" ["150"]
            }
            fire {
                sequence "120"
                speed "0.22"
                refBullet "spiralbit" ["150"]
            }
            fire {
                sequence "180"
                speed "0.2"
                refBullet "spiralbit" ["120"]
            }
            fire {
                sequence "120"
                speed "0.2"
                refBullet "spiralbit" ["120"]
            }
            fire {
                sequence "120"
                speed "0.2"
                refBullet "spiralbit" ["120"]
            }
            fire {
                sequence "150"
                speed "0.17"
                refBullet "spiralbit" ["-150"]
            }
            fire {
                sequence "120"
                speed "0.17"
                refBullet "spiralbit" ["-150"]
            }
            fire {
                sequence "120"
                speed "0.17"
                refBullet "spiralbit" ["-150"]
            }
            fire {
                sequence "180"
                speed "0.25"
                refBullet "spiralbit" ["-120"]
            }
            fire {
                sequence "120"
                speed "0.25"
                refBullet "spiralbit" ["-120"]
            }
            fire {
                sequence "120"
                speed "0.25"
                refBullet "spiralbit" ["-120"]
            }
            repeat "18" {
                fire {
                    sequence "10"
                    speedAbs "1.0"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
                fire {
                    sequence "10"
                    speedAbs "1.0"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.3"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
                fire {
                    sequence "0"
                    speedSeq "-0.1"
                    plain
                }
            }
        }
        defBullet "parentbit" {
            absolute "170 + 20 * $rand"
            speed "1.8"
            doActs (body {
                wait "40"
                changeSpeed "0.0001" "1"
                wait "5"
                actionRef "arrow" []
                repeat "3" {
                    wait "5"
                    changeSpeed "1.8" "1"
                    changeDirectionRel "170 + 20 * $rand" "1"
                    wait "40"
                    changeSpeed "0.0001" "1"
                    wait "5"
                    actionRef "arrow" []
                }
                wait "80"
                changeSpeed "1.8" "1"
                changeDirectionRel "170 + 20 * $rand" "1"
                wait "40"
                changeSpeed "0.0001" "1"
                wait "5"
                actionRef "slash" []
                wait "150"
                changeSpeed "1.8" "1"
                changeDirectionAim "-30 + 60 * $rand" "1"
                wait "15"
                changeSpeed "0.0001" "1"
                wait "5"
                actionRef "slash" []
                vanish
            })
        }
        defAction "arrow" {
            fire {
                aim "0"
                speedAbs "1.3"
                plain
            }
            fire {
                sequence "0"
                speedSeq "-0.1"
                plain
            }
            fire {
                sequence "-3"
                speedSeq "0.0"
                plain
            }
            fire {
                sequence "6"
                speedSeq "0.0"
                plain
            }
            fire {
                sequence "-3"
                speedSeq "-0.1"
                plain
            }
            fire {
                sequence "0"
                speedSeq "-0.1"
                plain
            }
            fire {
                sequence "0"
                speedSeq "-0.1"
                plain
            }
        }
        defBullet "spiralbit" {
            doActs (body {
                changeDirectionRel "$1" "90"
                repeat "2 + 6 * $rank" {
                    fire {
                        relative "0"
                        refBullet "spiral" ["-$1"]
                    }
                    fire {
                        relative "90"
                        speedRel "0.6"
                        plain
                    }
                    wait "10 - 7 * $rank"
                    fire {
                        relative "0"
                        refBullet "spiral" ["$1"]
                    }
                    fire {
                        relative "-90"
                        speedRel "0.6"
                        plain
                    }
                    wait "10 - 7 * $rank"
                }
                vanish
            })
        }
        defBullet "spiral" {
            speedRel "0.8"
            doActs (body {
                wait "20"
                changeDirectionRel "$1" "90"
            })
        }
    }

  /// オリジナル。後ろに弾を撃つ人々。 by 白い弾幕くん
  /// [Original]_backfire.xml
  let backfire =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。後ろに弾を撃つ人々。 by 白い弾幕くん" {
        top {
            repeat "5+$rank*10" {
                fireRef "backFire" []
            }
            wait "300"
        }
        topFireAs "backFire" {
            dir "50-$rand*100"
            speed "1.2"
            ofBullet (bulletAnon {
                doActs (body {
                    repeat "10" {
                        changeDirection "150-$rand*300" "30"
                        repeat "5" {
                            wait "6"
                            fire {
                                relative "180"
                                speed "1.2"
                                plain
                            }
                        }
                    }
                    repeat "999" {
                        wait "6"
                        fire {
                            relative "180"
                            speed "1.2"
                            plain
                        }
                    }
                })
            })
        }
    }

  /// 大原さんのオリジナル、ふきだしボム by 白い弾幕くん
  /// [Original]_balloon_bomb.xml
  let balloon_bomb =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、ふきだしボム by 白い弾幕くん" {
        top {
            repeat "3 + 17 * $rank" {
                fire {
                    absolute "120 + 120 * $rand"
                    speed "1.0 + 0.3 * $rand"
                    refBullet "balloon" ["0.6 + 1.2 * $rank"]
                }
                wait "43 - 30 * $rank"
            }
            wait "200 - 100 * $rank"
        }
        defBullet "balloon" {
            doActs (body {
                wait "20"
                changeSpeed "0" "1"
                wait "5"
                fire {
                    absolute "360 * $rand"
                    speed "$1 * 0.88"
                    refBullet "balloonbit" ["$1 * 0.88"]
                }
                repeat "2" {
                    fire {
                        sequence "120"
                        speed "$1 * 0.88"
                        refBullet "balloonbit" ["$1 * 0.88"]
                    }
                }
                repeat "24" {
                    fire {
                        sequence "15"
                        speed "$1"
                        refBullet "curvebit" ["10"; "40"; "$1"]
                    }
                }
                wait "5"
                fire {
                    sequence "10"
                    speed "$1"
                    refBullet "curvebit" ["10"; "40"; "$1"]
                }
                repeat "23" {
                    fire {
                        sequence "15"
                        speed "$1"
                        refBullet "curvebit" ["10"; "40"; "$1"]
                    }
                }
                fire {
                    sequence "-3"
                    speed "$1 * 0.88"
                    refBullet "balloonbit" ["$1 * 0.88"]
                }
                repeat "2" {
                    fire {
                        sequence "120"
                        speed "$1 * 0.88"
                        refBullet "balloonbit" ["$1 * 0.88"]
                    }
                }
                vanish
            })
        }
        defBullet "balloonbit" {
            doActs (body {
                wait "4"
                fire {
                    relative "-60"
                    speed "$1"
                    refBullet "curvebit" ["5"; "-50"; "$1"]
                }
                repeat "9" {
                    fire {
                        sequence "13"
                        speed "$1"
                        refBullet "curvebit" ["5"; "-50"; "$1"]
                    }
                }
                vanish
            })
        }
        defBullet "curvebit" {
            doActs (body {
                wait "$1"
                fire {
                    relative "$2"
                    speed "$3"
                    plain
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その一 by 白い弾幕くん
  /// [Original]_btb_1.xml
  let btb_1 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その一 by 白い弾幕くん" {
        top {
            repeat "2" {
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "0"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (3/4)"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/2)"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (3/4)"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank))"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/4)"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/2)"]
                }
                wait "20"
                fire {
                    refBullet "nway" ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/4)"]
                }
                wait "20"
            }
            wait "80"
            repeat "10" {
                fire {
                    refBullet "nwayaim" ["(8 + 28 * $rank * $rank)"; "0.8 + 0.6 * $rank"; "(360 / (8 + 28 * $rank * $rank)) / 2"]
                }
                fire {
                    refBullet "nwayaim" ["(8 + 28 * $rank * $rank)"; "1.0 + 0.6 * $rank"; "(360 / (8 + 28 * $rank * $rank)) / 4"]
                }
                fire {
                    refBullet "nwayaim" ["(8 + 28 * $rank * $rank)"; "1.2 + 0.6 * $rank"; "0"]
                }
                wait "30"
            }
            wait "60"
        }
        defBullet "nway" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$3"
                    speed "$2"
                    plain
                }
                repeat "$1" {
                    fire {
                        sequence "360 / $1"
                        speed "$2"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "nwayaim" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    aim "$3"
                    speed "$2"
                    plain
                }
                repeat "$1" {
                    fire {
                        sequence "360 / $1"
                        speed "$2"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その二 by 白い弾幕くん
  /// [Original]_btb_2.xml
  let btb_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その二 by 白い弾幕くん" {
        top {
            repeat "280 / ((50 - 43 * $rank) * 4)" {
                fire {
                    refBullet "vaim" ["0"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vaim" ["18"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vaim" ["-18"; "0.7 + 0.9 * $rank"]
                }
                wait "(50 - 43 * $rank)"
                fire {
                    refBullet "vabsolute" ["0"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["60"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["120"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["180"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["240"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["300"; "0.7 + 0.9 * $rank"]
                }
                wait "(50 - 43 * $rank)"
                fire {
                    refBullet "vaimrev" ["0"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vaimrev" ["18"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vaimrev" ["-18"; "0.7 + 0.9 * $rank"]
                }
                wait "(50 - 43 * $rank)"
                fire {
                    refBullet "vabsolute" ["30"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["90"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["150"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["210"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["270"; "0.7 + 0.9 * $rank"]
                }
                fire {
                    refBullet "vabsolute" ["330"; "0.7 + 0.9 * $rank"]
                }
                wait "(50 - 43 * $rank)"
            }
            wait "60"
        }
        defBullet "vaim" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    aim "$1"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "3"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-6"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "9"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-12"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "15"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-18"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                vanish
            })
        }
        defBullet "vaimrev" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    aim "$1"
                    speed "$2"
                    refBullet "red" []
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "3"
                    speed "$2 * 1.1"
                    refBullet "red" []
                }
                fire {
                    sequence "-6"
                    speed "$2 * 1.1"
                    refBullet "red" []
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "9"
                    speed "$2 * 1.21"
                    refBullet "red" []
                }
                fire {
                    sequence "-12"
                    speed "$2 * 1.21"
                    refBullet "red" []
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "15"
                    speed "$2 * 1.331"
                    refBullet "red" []
                }
                fire {
                    sequence "-18"
                    speed "$2 * 1.331"
                    refBullet "red" []
                }
                wait "7 - 4 * $rank"
                vanish
            })
        }
        defBullet "vabsolute" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$1"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "3"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-6"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "9"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-12"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                fire {
                    sequence "15"
                    speed "$2"
                    plain
                }
                fire {
                    sequence "-18"
                    speed "$2"
                    plain
                }
                wait "7 - 4 * $rank"
                vanish
            })
        }
        defBullet "red" {
            doActs (body {
                wait "1000"
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その三 by 白い弾幕くん
  /// [Original]_btb_3.xml
  let btb_3 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その三 by 白い弾幕くん" {
        top {
            fire {
                refBullet "half" ["1"]
            }
            fire {
                refBullet "half" ["-1"]
            }
            wait "90 + 100 / (5 - 4 * $rank)"
        }
        defBullet "half" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "4 * (5 - 4 * $rank) * $1"
                    refBullet "laser" ["0.8"]
                }
                wait "1"
                repeat "30 / (5 - 4 * $rank)" {
                    repeat "2" {
                        fire {
                            sequence "6 * (5 - 4 * $rank) * $1"
                            refBullet "laser" ["0.8"]
                        }
                    }
                    wait "1"
                }
                fire {
                    aim "0"
                    refBullet "laser" ["0.9"]
                }
                wait "1"
                repeat "30 / (5 - 4 * $rank)" {
                    repeat "2" {
                        fire {
                            sequence "6 * (5 - 4 * $rank) * $1"
                            refBullet "laser" ["0.9"]
                        }
                    }
                    wait "1"
                }
                fire {
                    absolute "11 * $1"
                    refBullet "laser" ["1.2"]
                }
                wait "1"
                repeat "30 / (5 - 4 * $rank)" {
                    repeat "2" {
                        fire {
                            sequence "6 * (5 - 4 * $rank) * $1"
                            refBullet "laser" ["1.2"]
                        }
                    }
                    wait "1"
                }
                vanish
            })
        }
        defBullet "laser" {
            speed "0.01"
            doActs (body {
                fire {
                    relative "0"
                    speed "$1"
                    plain
                }
                wait "3"
                repeat "4" {
                    fire {
                        sequence "2.5 - 2.0 * $rank"
                        speed "$1"
                        plain
                    }
                    wait "3"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その四 by 白い弾幕くん
  /// [Original]_btb_4.xml
  let btb_4 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その四 by 白い弾幕くん" {
        top {
            fire {
                refBullet "dummy" ["8 + 28 * $rank"; "0.6 + 0.9 * $rank"]
            }
            wait "440 - 90 * $rank"
        }
        defBullet "dummy" {
            absolute "0"
            speed "0"
            doActs (body {
                repeat "12" {
                    fire {
                        refBullet "nwayabsolute" ["$1"; "$2"; "0"]
                    }
                    wait "3"
                    repeat "2 + 3 * $rank" {
                        fire {
                            refBullet "nwayabsolute" ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12)"]
                        }
                    }
                    wait "3"
                    fire {
                        refBullet "nwayabsolute" ["$1"; "$2"; "(360 / 36) * (1/4)"]
                    }
                    wait "3"
                    repeat "2 + 3 * $rank" {
                        fire {
                            refBullet "nwayabsolute" ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (3/4)"]
                        }
                    }
                    wait "3"
                    fire {
                        refBullet "nwayabsolute" ["$1"; "$2"; "(360 / 36) * (1/2)"]
                    }
                    wait "3"
                    repeat "2 + 3 * $rank" {
                        fire {
                            refBullet "nwayabsolute" ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (1/2)"]
                        }
                    }
                    wait "3"
                    fire {
                        refBullet "nwayabsolute" ["$1"; "$2"; "(360 / 36) * (3/4)"]
                    }
                    wait "3"
                    repeat "2 + 3 * $rank" {
                        fire {
                            refBullet "nwayabsolute" ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (1/4)"]
                        }
                    }
                    wait "3"
                }
                vanish
            })
        }
        defBullet "nwayabsolute" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$3"
                    speed "$2"
                    plain
                }
                repeat "$1" {
                    fire {
                        sequence "(360 / $1) + (3 + 12 * (1 - $rank) * (1 - $rank)) * (-1 + 2 * $rand)"
                        speed "$2"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その五 by 白い弾幕くん
  /// [Original]_btb_5.xml
  let btb_5 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その五 by 白い弾幕くん" {
        top {
            fire {
                refBullet "dummy" ["72 * $rand"]
            }
            wait "880"
        }
        defBullet "dummy" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$1"
                    refBullet "winder" ["2.0"]
                }
                repeat "4" {
                    fire {
                        sequence "72"
                        refBullet "winder" ["2.0"]
                    }
                }
                wait "420"
                fire {
                    absolute "$1 + 96"
                    refBullet "shotgun" ["1.0 + 1.0 * $rank"]
                }
                repeat "4" {
                    fire {
                        sequence "72"
                        refBullet "shotgun" ["1.0 + 1.0 * $rank"]
                    }
                }
                wait "350"
                fire {
                    absolute "$1 - 24"
                    refBullet "shotgun" ["1.0 + 1.0 * $rank"]
                }
                repeat "4" {
                    fire {
                        sequence "72"
                        refBullet "shotgun" ["1.0 + 1.0 * $rank"]
                    }
                }
                vanish
            })
        }
        defBullet "shotgun" {
            speed "0.001"
            doActs (body {
                fire {
                    relative "-34"
                    speed "$1 * 0.93"
                    plain
                }
                repeat "68 / (12 - 10 * $rank * $rank)" {
                    fire {
                        sequence "12 - 10 * $rank * $rank"
                        speed "$1 * 0.93"
                        plain
                    }
                }
                wait "5"
                fire {
                    sequence "-1"
                    speed "$1"
                    plain
                }
                repeat "66 / (12 - 10 * $rank * $rank)" {
                    fire {
                        sequence "-(12 - 10 * $rank * $rank)"
                        speed "$1"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "winder" {
            speed "0.001"
            doActs (body {
                repeat "10" {
                    fire {
                        relative "0"
                        refBullet "laser" ["$1"]
                    }
                    wait "14"
                }
                repeat "15" {
                    fire {
                        sequence "4"
                        refBullet "laser" ["$1"]
                    }
                    wait "14"
                }
                repeat "10" {
                    fire {
                        sequence "0"
                        refBullet "laser" ["$1"]
                    }
                    wait "14"
                }
                repeat "15" {
                    fire {
                        sequence "-8"
                        refBullet "laser" ["$1"]
                    }
                    wait "14"
                }
                repeat "10" {
                    fire {
                        sequence "0"
                        refBullet "laser" ["$1"]
                    }
                    wait "14"
                }
                vanish
            })
        }
        defBullet "laser" {
            speed "0.01"
            doActs (body {
                fire {
                    relative "1"
                    speed "$1"
                    plain
                }
                wait "1"
                repeat "3" {
                    fire {
                        sequence "2"
                        speed "$1"
                        plain
                    }
                    wait "1"
                    fire {
                        sequence "-2"
                        speed "$1"
                        plain
                    }
                    wait "1"
                }
                fire {
                    sequence "-2"
                    speed "$1"
                    plain
                }
                wait "1"
                repeat "3" {
                    fire {
                        sequence "-2"
                        speed "$1"
                        plain
                    }
                    wait "1"
                    fire {
                        sequence "2"
                        speed "$1"
                        plain
                    }
                    wait "1"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、原点回帰その六 by 白い弾幕くん
  /// [Original]_btb_6.xml
  let btb_6 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、原点回帰その六 by 白い弾幕くん" {
        top {
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "360"; "-7"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "330"; "-6"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "300"; "-5"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "270"; "-4"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "240"; "-3"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "210"; "-2"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "180"; "-1"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "180"; "1"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "150"; "2"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "120"; "3"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "90"; "4"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "60"; "5"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "30"; "6"]
            }
            fire {
                refBullet "halfwinder" ["1.8"; "2"; "0"; "7"]
            }
            wait "700"
        }
        defBullet "halfwinder" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    refBullet "bit" ["$1"; "$2"; "$3"; "$4"]
                }
                fire {
                    refBullet "changecolor" ["$1"; "$2"; "$3"; "-$4"]
                }
                vanish
            })
        }
        defBullet "changecolor" {
            absolute "0"
            speed "0"
            doActs (body {
                wait "$2 * 2"
                fire {
                    refBullet "bit" ["$1"; "$2"; "$3"; "$4"]
                }
                vanish
            })
        }
        defBullet "bit" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$3"
                    refBullet "laser" ["$1"; "$2"]
                }
                wait "$2 * (15 - 9 * $rank)"
                repeat "300 / (15 - 9 * $rank)" {
                    fire {
                        sequence "$4"
                        refBullet "laser" ["$1"; "$2"]
                    }
                    wait "$2 * (15 - 9 * $rank)"
                }
                vanish
            })
        }
        defBullet "laser" {
            speed "0.01"
            doActs (body {
                repeat "1 + 3 * $rank" {
                    fire {
                        relative "0"
                        speed "$1 * (0.5 + 0.5 * $rank)"
                        plain
                    }
                    wait "$2"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、検閲済 by 白い弾幕くん
  /// [Original]_censored.xml
  let censored =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、検閲済 by 白い弾幕くん" {
        top {
            fire {
                absolute "180"
                speed "3.0"
                refBullet "center" ["0"]
            }
            wait "800 - 50 * $rank"
        }
        defBullet "center" {
            doActs (body {
                wait "15"
                fire {
                    absolute "-10 + $1"
                    speed "2.0"
                    refBullet "arm" []
                }
                fire {
                    absolute "80 + $1"
                    speed "2.0"
                    refBullet "arm" []
                }
                fire {
                    absolute "170 + $1"
                    speed "2.0"
                    refBullet "arm" []
                }
                fire {
                    absolute "260 + $1"
                    speed "2.0"
                    refBullet "arm" []
                }
                vanish
            })
        }
        defBullet "arm" {
            doActs (body {
                wait "25"
                changeSpeed "0" "1"
                wait "5"
                fire {
                    refBullet "halfwinder" ["330"; "-8"]
                }
                fire {
                    refBullet "halfwinder" ["270"; "-5"]
                }
                fire {
                    refBullet "halfwinder" ["210"; "-2"]
                }
                fire {
                    refBullet "halfwinder" ["150"; "2"]
                }
                fire {
                    refBullet "halfwinder" ["90"; "5"]
                }
                fire {
                    refBullet "halfwinder" ["30"; "8"]
                }
                vanish
            })
        }
        defBullet "halfwinder" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    refBullet "bit" ["$1"; "$2"]
                }
                fire {
                    refBullet "changecolor" ["$1"; "-$2"]
                }
                vanish
            })
        }
        defBullet "changecolor" {
            absolute "0"
            speed "0"
            doActs (body {
                wait "(62 - 50 * $rank)/3"
                fire {
                    refBullet "bit" ["$1"; "$2"]
                }
                vanish
            })
        }
        defBullet "bit" {
            absolute "0"
            speed "0"
            doActs (body {
                fire {
                    absolute "$1"
                    speed "0.7 + 1.1 * $rank"
                    plain
                }
                wait "62 - 50 * $rank"
                repeat "600 / (62 - 50 * $rank)" {
                    fire {
                        sequence "$2"
                        speed "0.7 + 1.1 * $rank"
                        plain
                    }
                    wait "62 - 50 * $rank"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、キメラ by 白い弾幕くん
  /// [Original]_chimera.xml
  let chimera =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、キメラ by 白い弾幕くん" {
        top {
            fire {
                aim "0"
                refBullet "centerbit" []
            }
            wait "450"
        }
        defBullet "centerbit" {
            absolute "180"
            speed "3.0"
            doActs (body {
                wait "40"
                changeSpeed "0.001" "1"
                wait "5"
                changeDirectionAbs "160 + 40 * $rand" "90"
                fire {
                    absolute "60"
                    refBullet "sidebit" ["-40"]
                }
                fire {
                    absolute "-60"
                    refBullet "sidebit" ["40"]
                }
                wait "90"
                repeat "3" {
                    changeDirectionAim "170 + 20 * $rand" "1"
                    changeSpeed "0.85" "1"
                    wait "40"
                    changeSpeed "0.001" "1"
                    wait "5"
                    fire {
                        aim "60"
                        refBullet "sidebit" ["-40"]
                    }
                    fire {
                        aim "-60"
                        refBullet "sidebit" ["40"]
                    }
                    wait "45"
                }
                vanish
            })
        }
        defBullet "sidebit" {
            speed "0.5"
            doActs (body {
                wait "30"
                changeSpeed "0.001" "1"
                wait "5"
                repeat "30 + 220 * $rank * $rank" {
                    fire {
                        relative "$1 - 45 + 90 * $rand"
                        speed "3.5 + 1.0 * $rand"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// オリジナル、円を描きながらの自機狙い3way by 白い弾幕くん
  /// [Original]_circle.xml
  let circle =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル、円を描きながらの自機狙い3way by 白い弾幕くん" {
        defAction "5sp" {
            fire {
                aim "$1"
                speed "1"
                plain
            }
            repeat "3" {
                fire {
                    speedSeq "0.4"
                    plain
                }
            }
        }
        topFireAs "maru" {
            absolute "90"
            speed "4"
            ofBullet (bulletAnon {
                doActs (body {
                    changeDirectionSeq "4" "1000"
                    repeat "90" {
                        actionRef "5sp" ["0"]
                        actionRef "5sp" ["70-$rank*40"]
                        actionRef "5sp" ["-70+$rank*40"]
                        wait "3"
                    }
                    vanish
                })
            })
        }
        top {
            fireRef "maru" []
            wait "320"
        }
    }

  /// オリジナル、どかんと一発 by 白い弾幕くん
  /// [Original]_dokkaan.xml
  let dokkaan =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル、どかんと一発 by 白い弾幕くん" {
        top {
            repeat "200+200*$rank" {
                fire {
                    dir "120*$rand-60"
                    speed "0.5+$rand*2"
                    plain
                }
            }
            wait "150"
        }
    }

  /// 大原さんのオリジナル、楕円ボム by 白い弾幕くん
  /// [Original]_ellipse_bomb.xml
  let ellipse_bomb =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、楕円ボム by 白い弾幕くん" {
        top {
            fire {
                absolute "215 + 20 * $rand"
                speed "2.0"
                refBullet "bit" ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"]
            }
            wait "40"
            fire {
                absolute "145 - 20 * $rand"
                speed "2.0"
                refBullet "bit" ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"]
            }
            wait "40"
            fire {
                absolute "170 + 20 * $rand"
                speed "2.0"
                refBullet "bit" ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"]
            }
            wait "600 - 250 * $rank"
        }
        defBullet "bit" {
            doActs (body {
                wait "15"
                changeSpeed "0" "1"
                wait "15"
                fire {
                    absolute "360 * $rand"
                    refBullet "ellipse" ["$1"; "$2"; "$3"]
                }
                vanish
            })
        }
        defBullet "ellipse" {
            speed "0.001"
            doActs (body {
                fire {
                    relative "5"
                    speed "$1"
                    refBullet "red" ["$2"; "$3"]
                }
                repeat "6" {
                    fire {
                        sequence "10"
                        speedSeq "-($1 * 0.04)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                repeat "2" {
                    fire {
                        sequence "10"
                        speedSeq "-($1 * 0.01)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                fire {
                    sequence "10"
                    speedSeq "0"
                    refBullet "red" ["$2"; "$3"]
                }
                repeat "2" {
                    fire {
                        sequence "10"
                        speedSeq "($1 * 0.01)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                repeat "6" {
                    fire {
                        sequence "10"
                        speedSeq "($1 * 0.04)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                fire {
                    sequence "10"
                    speedSeq "0"
                    refBullet "red" ["$2"; "$3"]
                }
                repeat "6" {
                    fire {
                        sequence "10"
                        speedSeq "-($1 * 0.04)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                repeat "2" {
                    fire {
                        sequence "10"
                        speedSeq "-($1 * 0.01)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                fire {
                    sequence "10"
                    speedSeq "0"
                    refBullet "red" ["$2"; "$3"]
                }
                repeat "2" {
                    fire {
                        sequence "10"
                        speedSeq "($1 * 0.01)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                repeat "6" {
                    fire {
                        sequence "10"
                        speedSeq "($1 * 0.04)"
                        refBullet "red" ["$2"; "$3"]
                    }
                }
                vanish
            })
        }
        defBullet "red" {
            doActs (body {
                wait "35"
                changeSpeed "0.001" "1"
                wait "5"
                wait "$2"
                fire {
                    relative "140"
                    speed "$1"
                    plain
                }
                wait "$2"
                repeat "12" {
                    fire {
                        sequence "7"
                        speedSeq "$1 * 0.03"
                        plain
                    }
                    wait "$2"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、EntangledSpace by 白い弾幕くん
  /// [Original]_entangled_space.xml
  let entangled_space =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、EntangledSpace by 白い弾幕くん" {
        top {
            fire {
                refBullet "dummy" ["2.5"; "1.2 * (0.5 + 0.5 * $rank)"; "11 * (3.5 - 2.5 * $rank)"; "0"; "10"]
            }
            fire {
                refBullet "dummy" ["1.5"; "1.5 * (0.5 + 0.5 * $rank)"; "7 * (3.5 - 2.5 * $rank)"; "0"; "-8"]
            }
            fire {
                refBullet "dummy" ["0.5"; "1.8 * (0.5 + 0.5 * $rank)"; "5 * (3.5 - 2.5 * $rank)"; "0"; "6"]
            }
            wait "1050 - 150 * $rank"
        }
        defBullet "dummy" {
            absolute "180"
            speed "2.0"
            doActs (body {
                wait "15"
                changeSpeed "0" "1"
                wait "15"
                fire {
                    absolute "360 * $rand"
                    speed "$1"
                    refBullet "bit" ["$2"; "$3"; "$4"; "$5"]
                }
                repeat "3" {
                    fire {
                        sequence "90"
                        speed "$1"
                        refBullet "bit" ["$2"; "$3"; "$4"; "$5"]
                    }
                }
                wait "20"
                repeat "2" {
                    fire {
                        sequence "30"
                        speed "$1"
                        refBullet "bit" ["$2"; "$3"; "$4"; "$5"]
                    }
                    repeat "3" {
                        fire {
                            sequence "90"
                            speed "$1"
                            refBullet "bit" ["$2"; "$3"; "$4"; "$5"]
                        }
                    }
                    wait "20"
                }
                repeat "200 / $3" {
                    fire {
                        sequence "$5 * 2.5"
                        speed "$2 * 0.6"
                        plain
                    }
                    repeat "3" {
                        fire {
                            sequence "0"
                            speedSeq "$2 * 0.05"
                            plain
                        }
                    }
                    repeat "2" {
                        fire {
                            sequence "120"
                            speed "$2 * 0.6"
                            plain
                        }
                        repeat "3" {
                            fire {
                                sequence "0"
                                speedSeq "$2 * 0.05"
                                plain
                            }
                        }
                    }
                    wait "$3 * 3"
                }
                vanish
            })
        }
        defBullet "bit" {
            doActs (body {
                wait "15"
                changeSpeed "0.001" "1"
                wait "1"
                changeDirectionRel "-(17 * $4)" "1"
                wait "1"
                wait "200 - 100 * $rank"
                changeSpeed "0.13" "1"
                wait "1"
                fire {
                    relative "$3"
                    speed "$1"
                    plain
                }
                wait "$2"
                repeat "96 / $2" {
                    repeat "3" {
                        fire {
                            sequence "$4"
                            speedSeq "-($1 * 0.1)"
                            plain
                        }
                        wait "$2"
                    }
                    repeat "3" {
                        fire {
                            sequence "$4"
                            speedSeq "($1 * 0.1)"
                            plain
                        }
                        wait "$2"
                    }
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、邪眼 by 白い弾幕くん
  /// [Original]_evil_eye.xml
  let evil_eye =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、邪眼 by 白い弾幕くん" {
        top {
            fire {
                absolute "90"
                speed "0.02"
                refBullet "cannonbit" []
            }
            fire {
                absolute "90"
                speed "0.06"
                refBullet "cannonbit" []
            }
            fire {
                absolute "90"
                speed "0.10"
                refBullet "cannonbit" []
            }
            fire {
                absolute "-90"
                speed "0.02"
                refBullet "cannonbit" []
            }
            fire {
                absolute "-90"
                speed "0.06"
                refBullet "cannonbit" []
            }
            fire {
                absolute "-90"
                speed "0.10"
                refBullet "cannonbit" []
            }
            wait "120"
            repeat "5 + 10 * $rank" {
                actionRef "5way" ["30"]
                wait "27 - 20 * $rank"
                actionRef "5way" ["20"]
                wait "27 - 20 * $rank"
            }
            wait "60"
        }
        defAction "5way" {
            fire {
                aim "$1 * (-2)"
                speed "1.3"
                refBullet "bit" ["$1 * 2"]
            }
            fire {
                aim "$1 * (-1)"
                speed "1.3"
                refBullet "bit" ["$1 * 1"]
            }
            fire {
                aim "0"
                speed "1.3"
                refBullet "bit" ["0"]
            }
            fire {
                aim "$1 * 1"
                speed "1.3"
                refBullet "bit" ["$1 * (-1)"]
            }
            fire {
                aim "$1 * 2"
                speed "1.3"
                refBullet "bit" ["$1 * (-2)"]
            }
        }
        defBullet "bit" {
            doActs (body {
                wait "30"
                fire {
                    relative "$1 - 15 + 30 * $rand"
                    speed "1.3 + 1.0 * $rank"
                    plain
                }
                repeat "2 + 3 * $rank" {
                    fire {
                        sequence "0"
                        speedSeq "0.1"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "cannonbit" {
            doActs (body {
                wait "60"
                repeat "80" {
                    fire {
                        absolute "180"
                        speed "0.0001 + 12.0 * $rand"
                        refBullet "weak" []
                    }
                }
                vanish
            })
        }
        defBullet "weak" {
            doActs (body {
                wait "10"
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、偽婦人乱舞 by 白い弾幕くん
  /// [Original]_fujin_ranbu_fake.xml
  let fujin_ranbu_fake =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、偽婦人乱舞 by 白い弾幕くん" {
        top {
            fire {
                refBullet "dummy" ["0.6 + 0.6 * $rank"; "48 - 41 * $rank"; "8 * $rank"]
            }
            wait "400 - 100 * $rank"
        }
        defBullet "dummy" {
            absolute "0"
            speed "0"
            doActs (body {
                changeDirectionAbs "180" "1"
                changeSpeed "2" "20"
                repeat "3" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                }
                changeSpeed "0" "20"
                repeat "1" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                }
                changeDirectionAbs "0" "1"
                changeSpeed "2" "30"
                repeat "999" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                }
                vanish
            })
        }
        defBullet "bit" {
            doActs (body {
                fire {
                    aim "0"
                    refBullet "shotgun" ["$1"]
                }
                wait "$2"
                repeat "200" {
                    fire {
                        sequence "$3"
                        refBullet "shotgun" ["$1"]
                    }
                    wait "$2"
                }
                vanish
            })
        }
        defBullet "shotgun" {
            speed "0.001"
            doActs (body {
                fire {
                    relative "0"
                    speed "$1"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.1"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.21"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.331"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.4641"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.610510"
                    plain
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、真婦人乱舞 by 白い弾幕くん
  /// [Original]_fujin_ranbu_true.xml
  let fujin_ranbu_true =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、真婦人乱舞 by 白い弾幕くん" {
        top {
            fire {
                refBullet "dummy" ["0.6 + 0.6 * $rank"; "48 - 41 * $rank"; "8 * $rank"]
            }
            wait "400 - 100 * $rank"
        }
        defBullet "dummy" {
            absolute "0"
            speed "0"
            doActs (body {
                changeDirectionAbs "180" "1"
                changeSpeed "2" "20"
                repeat "3" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                }
                changeSpeed "0" "20"
                repeat "1" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                }
                changeDirectionAbs "0" "1"
                changeSpeed "2" "30"
                repeat "999" {
                    fire {
                        absolute "180 + 60"
                        refBullet "bit" ["$1"; "$2"; "-$3"]
                    }
                    wait "10"
                    fire {
                        absolute "180 - 60"
                        refBullet "bit" ["$1"; "$2"; "$3"]
                    }
                    wait "10"
                }
                vanish
            })
        }
        defBullet "bit" {
            doActs (body {
                fire {
                    aim "0"
                    refBullet "shotgun" ["$1"]
                }
                wait "$2"
                repeat "200" {
                    fire {
                        sequence "$3"
                        refBullet "shotgun" ["$1"]
                    }
                    wait "$2"
                }
                vanish
            })
        }
        defBullet "shotgun" {
            speed "0.001"
            doActs (body {
                fire {
                    relative "0"
                    speed "$1"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.1"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.21"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.331"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.4641"
                    plain
                }
                fire {
                    relative "0"
                    speed "$1 * 1.610510"
                    plain
                }
                vanish
            })
        }
    }

  /// オリジナル、ぐるぐる by 白い弾幕くん
  /// [Original]_guruguru.xml
  let guruguru =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル、ぐるぐる by 白い弾幕くん" {
        defAction "issyuu" {
            repeat "360/(20-$rank*10)+1" {
                fire {
                    sequence "18-$rank*6"
                    speedSeq "0"
                    plain
                }
            }
        }
        topFireAs "guruguru" {
            speed "0.3"
            ofBullet (bulletAnon {
                doActs (body {
                    fire {
                        speed "0.7"
                        plain
                    }
                    actionRef "issyuu" []
                    repeat "30" {
                        wait "12"
                        fire {
                            sequence "-356"
                            speedSeq "0"
                            plain
                        }
                        actionRef "issyuu" []
                    }
                    vanish
                })
            })
        }
        top {
            fireRef "guruguru" []
            wait "500"
        }
    }

  /// オリジナル。ぐるちょ。 by 白い弾幕くん
  /// [Original]_gurutyo.xml
  let gurutyo =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。ぐるちょ。 by 白い弾幕くん" {
        topFireAs "gurutyo" {
            absolute "90"
            speed "$1*(3+$rank*4)"
            ofBullet (bulletAnon {
                doActs (body {
                    changeDirectionSeq "$1*6" "1000"
                    repeat "500" {
                        fire {
                            relative "0"
                            plain
                        }
                        wait "1"
                    }
                    vanish
                })
            })
        }
        top {
            fireRef "gurutyo" ["1"]
            fireRef "gurutyo" ["-1"]
            wait "550"
        }
    }

  /// オリジナル。逆噴射。by 白い弾幕くん
  /// [Original]_gyakuhunsya.xml
  let gyakuhunsya =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。逆噴射。by 白い弾幕くん" {
        defAction "gyakuhunsya" {
            changeDirectionAbs "180" "1"
            changeSpeed "2" "20"
            wait "60"
            changeSpeed "0" "20"
            wait "20"
            changeDirectionAbs "0" "1"
            changeSpeed "2" "30"
            fire {
                speed "0.6"
                plain
            }
            repeat "80" {
                repeat "2+$rank*2" {
                    fire {
                        dir "$rand*120-60"
                        speedSeq "0.005"
                        plain
                    }
                }
                wait "1"
            }
            changeSpeed "0" "1"
            wait "10"
        }
        top {
            actionRef "gyakuhunsya" []
        }
    }

  /// 大原さんのオリジナル、ハジケリスト by 白い弾幕くん
  /// [Original]_hajike.xml
  let hajike =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、ハジケリスト by 白い弾幕くん" {
        top {
            repeat "2" {
                fire {
                    absolute "180 - 60"
                    refBullet "wave" ["5"; "42"]
                }
                wait "30 - 15 * $rank"
                fire {
                    absolute "180 + 60"
                    refBullet "wave" ["-5"; "-42"]
                }
                wait "30 - 15 * $rank"
                fire {
                    absolute "180 - 62"
                    refBullet "wave" ["5"; "40"]
                }
                wait "30 - 15 * $rank"
                fire {
                    absolute "180 + 62"
                    refBullet "wave" ["-5"; "-40"]
                }
                wait "30 - 15 * $rank"
                fire {
                    absolute "180 - 58"
                    refBullet "wave" ["5"; "40"]
                }
                wait "30 - 15 * $rank"
                fire {
                    absolute "180 + 58"
                    refBullet "wave" ["-5"; "-40"]
                }
                wait "30 - 15 * $rank"
            }
            wait "300 - 50 * $rank"
        }
        defBullet "wave" {
            speed "1.0"
            doActs (body {
                fire {
                    relative "$2"
                    speed "0.3"
                    refBullet "cross" []
                }
                wait "3"
                repeat "10 + 20 * $rank * $rank" {
                    fire {
                        sequence "$1"
                        speedSeq "0.05"
                        refBullet "cross" []
                    }
                    wait "3"
                }
                vanish
            })
        }
        defBullet "cross" {
            doActs (body {
                wait "100"
                fire {
                    relative "0"
                    speedRel "0.6 * $rank"
                    plain
                }
                fire {
                    relative "90"
                    speedRel "0.6 * $rank"
                    plain
                }
                fire {
                    relative "180"
                    speedRel "0.6 * $rank"
                    plain
                }
                fire {
                    relative "270"
                    speedRel "0.6 * $rank"
                    plain
                }
                vanish
            })
        }
    }

  /// オリジナル。はさみ。by 白い弾幕くん
  /// [Original]_hasami.xml
  let hasami =
    createBulletmlInfo <|
    untyped "オリジナル。はさみ。by 白い弾幕くん" {
        defAction "curve" {
            fire {
                aim "$1"
                speed "1.2+$rank*0.6"
                ofBullet (bulletAnon {
                    doActs (body {
                        changeDirectionAim "$1*-1.5" "100"
                    })
                })
            }
            fire {
                aim "-$1"
                speed "1.3+$rank*0.4"
                ofBullet (bulletAnon {
                    doActs (body {
                        changeDirectionAim "$1*1.5" "100"
                    })
                })
            }
        }
        top {
            repeat "200" {
                wait "2"
                actionRef "curve" ["$rand*90"]
            }
            wait "50"
        }
    }

  /// オリジナル。ひらひら。by 白い弾幕くん
  /// [Original]_hirahira.xml
  let hirahira =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。ひらひら。by 白い弾幕くん" {
        defAction "5way" {
            repeat "4" {
                fire {
                    sequence "20"
                    plain
                }
            }
        }
        defAction "hira" {
            fire {
                aim "-40+$1*60"
                plain
            }
            actionRef "5way" []
            repeat "60" {
                wait "8-$rank*2"
                fire {
                    sequence "-80-$1*2+$rand-0.5"
                    plain
                }
                actionRef "5way" []
            }
        }
        defAction "top1" {
            actionRef "hira" ["-1"]
            wait "60"
        }
        defAction "top2" {
            actionRef "hira" ["1"]
            wait "60"
        }
    }

  /// オリジナル。放水っぽい感じ。by 白い弾幕くん
  /// [Original]_housya.xml
  let housya =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。放水っぽい感じ。by 白い弾幕くん" {
        top {
            fire {
                dir "-80"
                plain
            }
            repeat "70" {
                repeat "3*($rank+0.5)" {
                    fire {
                        sequence "($rand*20-9)/($rank+0.5)"
                        speedSeq "0.01/($rank+0.5)"
                        plain
                    }
                }
                wait "1"
            }
        }
    }

  /// 大原さんのオリジナル、カゴメ by 白い弾幕くん
  /// [Original]_kagome.xml
  let kagome =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、カゴメ by 白い弾幕くん" {
        top {
            repeat "10 + 10 * $rank" {
                fire {
                    absolute "75"
                    speed "1.1"
                    refBullet "matrixbit" []
                }
                fire {
                    absolute "255"
                    speed "1.1"
                    refBullet "matrixbit" []
                }
                wait "30 * (2.0 - 1.0 * $rank)"
            }
            wait "150"
        }
        defBullet "matrixbit" {
            speed "0.5"
            doActs (body {
                wait "30 * (2.0 - 1.0 * $rank)"
                repeat "999" {
                    fire {
                        relative "90"
                        speed "1.1"
                        refBullet "finalbit" []
                    }
                    fire {
                        relative "-90"
                        speed "1.1"
                        refBullet "finalbit" []
                    }
                    fire {
                        aim "-10 + 20 * $rand"
                        speed "1.1"
                        plain
                    }
                    wait "60 * (2.0 - 1.0 * $rank)"
                }
            })
        }
        defBullet "finalbit" {
            doActs (body {
                wait "30 * (2.0 - 1.0 * $rank)"
                repeat "999" {
                    fire {
                        relative "30"
                        speed "1.1 * (2 / 1.7320508)"
                        plain
                    }
                    fire {
                        relative "-30"
                        speed "1.1 * (2 / 1.7320508)"
                        plain
                    }
                    wait "60 * (2.0 - 1.0 * $rank)"
                }
            })
        }
    }

  /// 大原さんのオリジナル、毛玉 by 白い弾幕くん
  /// [Original]_kedama.xml
  let kedama =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、毛玉 by 白い弾幕くん" {
        top {
            repeat "3" {
                fire {
                    aim "90"
                    refBullet "bit" ["1"]
                }
                fire {
                    aim "-90"
                    refBullet "bit" ["-1"]
                }
                wait "90"
            }
            wait "250"
        }
        defBullet "bit" {
            speed "3.0"
            doActs (body {
                wait "10"
                changeSpeed "0.6" "1"
                wait "5"
                changeDirectionRel "-105 * $1" "1"
                wait "5"
                fire {
                    relative "60 * $1"
                    speed "0.6 + 0.7 * $rank"
                    plain
                }
                wait "12 - 10 * $rank"
                repeat "999" {
                    fire {
                        sequence "113 * $1"
                        speed "0.6 + 0.7 * $rank"
                        plain
                    }
                    wait "12 - 10 * $rank"
                }
            })
        }
    }

  /// 大原さんのオリジナル、弾幕の騎士その一 by 白い弾幕くん
  /// [Original]_knight_1.xml
  let knight_1 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、弾幕の騎士その一 by 白い弾幕くん" {
        top {
            fire {
                refBullet "bit" []
            }
            wait "450"
        }
        defBullet "bit" {
            aim "0"
            speed "0.5"
            doActs (body {
                changeSpeed "1.1" "120"
                repeat "4" {
                    repeat "6" {
                        changeDirectionAim "0" "10"
                        wait "10"
                    }
                    fire {
                        aim "-70 + 20 * $rand"
                        refBullet "kick" []
                    }
                }
                vanish
            })
        }
        defBullet "kick" {
            speed "4.0"
            doActs (body {
                changeSpeed "0.001" "30"
                wait "30"
                changeDirectionAim "0" "1"
                wait "5"
                fire {
                    relative "-20"
                    speed "0.7"
                    plain
                }
                repeat "4 + 25 * $rank" {
                    fire {
                        sequence "10 - 8 * $rank"
                        speedSeq "0.05"
                        plain
                    }
                }
                wait "10"
                fire {
                    relative "0"
                    speed "0.7"
                    plain
                }
                repeat "4 + 25 * $rank" {
                    fire {
                        sequence "10 - 8 * $rank"
                        speedSeq "0.05"
                        plain
                    }
                }
                wait "10"
                fire {
                    relative "20"
                    speed "0.7"
                    plain
                }
                repeat "4 + 25 * $rank" {
                    fire {
                        sequence "10 - 8 * $rank"
                        speedSeq "0.05"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、弾幕の騎士その二 by 白い弾幕くん
  /// [Original]_knight_2.xml
  let knight_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、弾幕の騎士その二 by 白い弾幕くん" {
        top {
            fire {
                absolute "120"
                refBullet "bit" ["90"; "1.0"]
            }
            wait "10"
            fire {
                absolute "150"
                refBullet "bit" ["70"; "1.2"]
            }
            wait "10"
            fire {
                absolute "180"
                refBullet "bit" ["50"; "1.4"]
            }
            wait "10"
            fire {
                absolute "210"
                refBullet "bit" ["30"; "1.6"]
            }
            wait "10"
            fire {
                absolute "240"
                refBullet "bit" ["10"; "1.8"]
            }
            wait "300 - 100 * $rank"
        }
        defBullet "bit" {
            speed "2.5"
            doActs (body {
                wait "30"
                changeSpeed "0" "1"
                wait "$1"
                fire {
                    aim "0"
                    speed "$2 * (0.5 + 0.5 * $rank)"
                    plain
                }
                repeat "19 + 100 * $rank" {
                    fire {
                        sequence "360 / (19 + 100 * $rank)"
                        speed "$2 * (0.5 + 0.5 * $rank)"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、弾幕の騎士その三 by 白い弾幕くん
  /// [Original]_knight_3.xml
  let knight_3 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、弾幕の騎士その三 by 白い弾幕くん" {
        top {
            fire {
                absolute "60"
                refBullet "bit" []
            }
            wait "300"
        }
        defBullet "bit" {
            speed "3.0"
            doActs (body {
                wait "20"
                changeSpeed "0" "1"
                wait "2"
                fire {
                    refBullet "groundbit" []
                }
                wait "30"
                fire {
                    refBullet "skybit" []
                }
                vanish
            })
        }
        defBullet "skybit" {
            absolute "210"
            speed "3.0"
            doActs (body {
                wait "30"
                repeat "4" {
                    wait "10"
                    fire {
                        absolute "110 + 20 * $rand"
                        refBullet "dummy" []
                    }
                }
                vanish
            })
        }
        defBullet "groundbit" {
            absolute "180"
            speed "3.0"
            doActs (body {
                wait "20"
                repeat "3" {
                    wait "10"
                    fire {
                        absolute "230 + 20 * $rand"
                        refBullet "dummy" []
                    }
                }
                vanish
            })
        }
        defBullet "dummy" {
            speed "0.001"
            doActs (body {
                repeat "5 + 15 * $rank" {
                    fire {
                        relative "-10 + 20 * $rand"
                        speed "1.0 + 1.4 * $rank"
                        plain
                    }
                    repeat "5" {
                        fire {
                            sequence "0"
                            speedSeq "0.1"
                            plain
                        }
                    }
                    wait "12 - 10 * $rank"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、弾幕の騎士その四 by 白い弾幕くん
  /// [Original]_knight_4.xml
  let knight_4 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、弾幕の騎士その四 by 白い弾幕くん" {
        top {
            fire {
                absolute "120"
                refBullet "parentbit" ["1.6"]
            }
            wait "300 - 100 * $rank"
        }
        defBullet "parentbit" {
            aim "0"
            speed "1.5"
            doActs (body {
                wait "20 + 10 * $rand"
                repeat "3" {
                    fire {
                        relative "60"
                        refBullet "bit" ["-120"; "$1"]
                    }
                    wait "10"
                    fire {
                        relative "-60"
                        refBullet "bit" ["120"; "$1"]
                    }
                    wait "10"
                }
                vanish
            })
        }
        defBullet "bit" {
            speed "2.5"
            doActs (body {
                wait "10"
                changeSpeed "0.0001" "1"
                wait "5"
                repeat "4 + 4 * $rank" {
                    fire {
                        relative "$1"
                        speed "$2 * (0.5 + 0.5 * $rank)"
                        plain
                    }
                    wait "5"
                }
                vanish
            })
        }
    }

  /// オリジナル。固体。 by 白い弾幕くん
  /// [Original]_kotai.xml
  let kotai =
    createBulletmlInfo <|
    untyped "オリジナル。固体。 by 白い弾幕くん" {
        defBullet "src" {
            doActs (body {
                wait "10"
                repeat "25" {
                    fire {
                        absolute "$rand*360"
                        speed "$rand*10"
                        ofBullet (bulletAnon {
                            doActs (body {
                                wait "1"
                                fire {
                                    absolute "180"
                                    speed "1.8"
                                    plain
                                }
                                vanish
                            })
                        })
                    }
                }
            })
        }
        top {
            repeat "350/(10-$rank*6)" {
                wait "10-$rank*6"
                fire {
                    absolute "90"
                    speed "-10+$rand*20"
                    refBullet "src" []
                }
            }
            wait "30"
        }
    }

  /// 大原さんのオリジナル、鯨幕砲 by 白い弾幕くん
  /// [Original]_kujira.xml
  let kujira =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、鯨幕砲 by 白い弾幕くん" {
        top {
            fire {
                refBullet "bit" ["2"; "1.0"]
            }
            fire {
                refBullet "bit" ["-2"; "1.0"]
            }
            wait "10"
            fire {
                refBullet "bit" ["2.5"; "1.05"]
            }
            fire {
                refBullet "bit" ["-2.5"; "1.05"]
            }
            wait "10"
            fire {
                refBullet "bit" ["3"; "1.1"]
            }
            fire {
                refBullet "bit" ["-3"; "1.1"]
            }
            wait "10"
            fire {
                refBullet "bit" ["3.5"; "1.15"]
            }
            fire {
                refBullet "bit" ["-3.5"; "1.15"]
            }
            wait "160"
            repeat "5" {
                fire {
                    absolute "30"
                    speed "1.55"
                    refBullet "kujira" ["179"]
                }
                fire {
                    absolute "40"
                    speed "1.4"
                    refBullet "kujira" ["170"]
                }
                fire {
                    absolute "50"
                    speed "1.25"
                    refBullet "kujira" ["160"]
                }
                fire {
                    absolute "60"
                    speed "1.1"
                    refBullet "kujira" ["150"]
                }
                fire {
                    absolute "70"
                    speed "0.95"
                    refBullet "kujira" ["140"]
                }
                fire {
                    absolute "80"
                    speed "0.8"
                    refBullet "kujira" ["130"]
                }
                fire {
                    absolute "-30"
                    speed "1.55"
                    refBullet "kujira" ["-179"]
                }
                fire {
                    absolute "-40"
                    speed "1.4"
                    refBullet "kujira" ["-170"]
                }
                fire {
                    absolute "-50"
                    speed "1.25"
                    refBullet "kujira" ["-160"]
                }
                fire {
                    absolute "-60"
                    speed "1.1"
                    refBullet "kujira" ["-150"]
                }
                fire {
                    absolute "-70"
                    speed "0.95"
                    refBullet "kujira" ["-140"]
                }
                fire {
                    absolute "-80"
                    speed "0.8"
                    refBullet "kujira" ["-130"]
                }
                wait "2"
            }
            wait "250 - 100 * $rank"
        }
        defBullet "kujira" {
            doActs (body {
                wait "5"
                changeDirectionRel "$1" "60"
                wait "60 + 5"
                changeSpeed "3.5 * (0.75 + 0.25 * $rank)" "30"
                repeat "10 / (2.0 - 1.0 * $rank)" {
                    changeDirectionAim "0" "8 * (2.0 - 1.0 * $rank)"
                    wait "8 * (2.0 - 1.0 * $rank)"
                }
            })
        }
        defBullet "bit" {
            absolute "0"
            speed "0.0"
            doActs (body {
                fire {
                    absolute "0"
                    speed "$2 * (0.5 + 0.5 * $rank)"
                    plain
                }
                wait "2 * (3.5 - 2.5 * $rank)"
                repeat "100 / (3.5 - 2.5 * $rank)" {
                    fire {
                        sequence "$1 * (3.5 - 2.5 * $rank)"
                        speed "$2 * (0.5 + 0.5 * $rank)"
                        plain
                    }
                    wait "2 * (3.5 - 2.5 * $rank)"
                }
                vanish
            })
        }
    }

  /// オリジナル。くねくね。by 白い弾幕くん
  /// [Original]_kunekune.xml
  let kunekune =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。くねくね。by 白い弾幕くん" {
        defAction "fire" {
            fire {
                relative "0"
                speedRel "-0.5"
                plain
            }
        }
        topFireAs "src" {
            dir "(30+$rank*20)*$1"
            speed "2"
            ofBullet (bulletAnon {
                doActs (body {
                    repeat "10" {
                        accel "20" {
                            horizontalRel "$1*4"
                        }
                        repeat "10" {
                            actionRef "fire" ["$1"]
                            wait "2"
                        }
                        accel "20" {
                            horizontalRel "-$1*4"
                        }
                        repeat "10" {
                            actionRef "fire" ["$1"]
                            wait "2"
                        }
                    }
                })
            })
        }
        top {
            repeat "3" {
                fireRef "src" ["1"]
                fireRef "src" ["-1"]
                wait "80"
            }
            wait "60"
        }
    }

  /// オリジナル。扇状弾二つ。by 白い弾幕くん
  /// [Original]_oogi_hutatsu.xml
  let oogi_hutatsu =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。扇状弾二つ。by 白い弾幕くん" {
        defAction "oogiSeq" {
            repeat "20" {
                fire {
                    sequence "8"
                    speedSeq "0"
                    plain
                }
            }
        }
        defAction "oogi" {
            fire {
                dir "-80"
                speed "$2"
                plain
            }
            actionRef "oogiSeq" []
            repeat "10+$rank*10" {
                wait "2"
                fire {
                    sequence "-$1"
                    speedSeq "-0.04"
                    plain
                }
                actionRef "oogiSeq" []
            }
        }
        top {
            actionRef "oogi" ["161"; "0.8+$rank*0.4"]
            wait "30"
            actionRef "oogi" ["159"; "1+$rank*0.6"]
            wait "150"
        }
    }

  /// 大原さんのオリジナル、光学探査兵器 by 白い弾幕くん
  /// [Original]_optic_seeker.xml
  let optic_seeker =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、光学探査兵器 by 白い弾幕くん" {
        top {
            repeat "2 + 8 * $rank" {
                fire {
                    aim "-30 + 60 * $rand"
                    speed "3.0 + 2.0 * $rank"
                    refBullet "seal" []
                }
                wait "25 - 20 * $rank"
                repeat "3" {
                    fire {
                        aim "-60 + 120 * $rand"
                        speed "3.0 + 2.0 * $rank"
                        refBullet "reflect" []
                    }
                    wait "25 - 20 * $rank"
                }
            }
            wait "100"
        }
        defBullet "reflect" {
            doActs (body {
                wait "(30 + 20 * $rand) * (1.0 - 0.5 * $rank)"
                repeat "3" {
                    changeDirectionRel "60 + 240 * $rand" "1"
                    wait "(10 + 10 * $rand) * (1.0 - 0.5 * $rank)"
                }
                changeDirectionAim "0" "1"
            })
        }
        defBullet "seal" {
            doActs (body {
                wait "10 + 10 * $rand"
                changeSpeed "0" "1"
                wait "5"
                fire {
                    aim "0"
                    speed "3.0 + 2.0 * $rank"
                    plain
                }
                wait "5"
                repeat "7" {
                    fire {
                        sequence "45"
                        speed "3.0 + 2.0 * $rank"
                        plain
                    }
                    wait "5"
                }
                fire {
                    aim "0"
                    speed "3.0 + 2.0 * $rank"
                    plain
                }
                vanish
            })
        }
    }

  /// オリジナル。ぱん。by 白い弾幕くん
  /// [Original]_pan.xml
  let pan =
    createBulletmlInfo <|
    untyped "オリジナル。ぱん。by 白い弾幕くん" {
        defBullet "pan" {
            doActs (body {
                wait "10"
                fire {
                    relative "95*$1"
                    speed "1.8+$rank"
                    plain
                }
                vanish
            })
        }
        top {
            fire {
                aim "-90"
                speed "0.001"
                refBullet "pan" ["1"]
            }
            repeat "20" {
                fire {
                    sequence "3"
                    speedSeq "0.2+$rank*0.4"
                    refBullet "pan" ["1"]
                }
            }
            fire {
                aim "90"
                speed "0.001"
                refBullet "pan" ["-1"]
            }
            repeat "20" {
                fire {
                    sequence "-3"
                    speedSeq "0.2+$rank*0.4"
                    refBullet "pan" ["-1"]
                }
            }
            wait "40"
        }
    }

  /// オリジナル。弱誘導弾から左右に弾幕。by 白い弾幕くん
  /// [Original]_progear_cheap_fake.xml
  let progear_cheap_fake =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。弱誘導弾から左右に弾幕。by 白い弾幕くん" {
        topFireAs "weekHoming" {
            aim "0"
            speed "0.1"
            ofBullet (bulletAnon {
                doActs (body {
                    repeat "3" {
                        changeSpeed "1.5" "30"
                        wait "30"
                        repeat "2+$rank*4" {
                            fire {
                                relative "90"
                                speed "1.3"
                                plain
                            }
                            fire {
                                relative "-90"
                                speed "1.3"
                                plain
                            }
                            wait "2"
                        }
                        changeDirectionAim "60-120*$rand" "20"
                        changeSpeed "0.1" "20"
                        wait "20"
                    }
                    changeSpeed "1.5" "30"
                    wait "30"
                })
            })
        }
        top {
            repeat "10" {
                fireRef "weekHoming" []
                wait "60-$rank*30"
            }
            wait "180"
        }
    }

  /// オリジナル。炸裂弾。by 白い弾幕くん
  /// [Original]_sakuretudan.xml
  let sakuretudan =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。炸裂弾。by 白い弾幕くん" {
        top {
            repeat "10" {
                fire {
                    dir "$rand*360"
                    speed "2"
                    ofBullet (bulletAnon {
                        doActs (body {
                            changeDirection "0" "60"
                            wait "60"
                            repeat "15+$rank*20" {
                                repeat "2" {
                                    fire {
                                        dir "360*$rand"
                                        speed "5"
                                        ofBullet (bulletAnon {
                                            doActs (body {
                                                wait "$rand*5"
                                                fire {
                                                    speed "$rand*4+0.5"
                                                    plain
                                                }
                                                vanish
                                            })
                                        })
                                    }
                                }
                                wait "1"
                            }
                            vanish
                        })
                    })
                }
                wait "60"
            }
            wait "100"
        }
    }

  /// 大原さんのオリジナル、シューティングスター by 白い弾幕くん
  /// [Original]_shooting_star.xml
  let shooting_star =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、シューティングスター by 白い弾幕くん" {
        top {
            fire {
                absolute "75 + 10 * $rand"
                speed "1.5"
                refBullet "star" ["215 + 10 * $rand"]
            }
            wait "50"
            fire {
                absolute "-(75 + 10 * $rand)"
                speed "1.5"
                refBullet "star" ["-(215 + 10 * $rand)"]
            }
            wait "30"
            fire {
                absolute "75 + 10 * $rand"
                speed "1.5"
                refBullet "star" ["215 + 10 * $rand"]
            }
            wait "10"
            fire {
                absolute "-(75 + 10 * $rand)"
                speed "1.5"
                refBullet "star" ["-(215 + 10 * $rand)"]
            }
            wait "650 - 150 * $rank"
        }
        defBullet "star" {
            doActs (body {
                wait "45"
                changeSpeed "0" "1"
                wait "15"
                changeDirectionAbs "$1" "1"
                changeSpeed "2.0" "180"
                wait "1"
                repeat "180" {
                    fire {
                        relative "150 + 60 * $rand"
                        refBullet "tail" ["1.5 * (0.25 + 0.75 * $rank)"]
                    }
                    wait "1"
                }
                fire {
                    refBullet "head" []
                }
                vanish
            })
        }
        defBullet "head" {
            absolute "0"
            speed "0"
            doActs (body {
                repeat "20 * (0.25 + 0.75 * $rank)" {
                    fire {
                        absolute "360 * $rand"
                        speed "(0.3 + 0.3 * $rand) * (0.25 + 0.75 * $rank)"
                        plain
                    }
                }
                repeat "30 * (0.25 + 0.75 * $rank)" {
                    fire {
                        absolute "360 * $rand"
                        speed "(0.5 + 0.5 * $rand) * (0.25 + 0.75 * $rank)"
                        plain
                    }
                }
                repeat "50 * (0.25 + 0.75 * $rank)" {
                    fire {
                        absolute "360 * $rand"
                        speed "(0.8 + 0.8 * $rand) * (0.25 + 0.75 * $rank)"
                        plain
                    }
                }
                fire {
                    absolute "360 * $rand"
                    speed "1.6 * (0.25 + 0.75 * $rank)"
                    plain
                }
                repeat "12 + 24 * $rank" {
                    fire {
                        sequence "360 / (12 + 24 * $rank)"
                        speed "1.6 * (0.25 + 0.75 * $rank)"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "tail" {
            speed "0.001"
            doActs (body {
                repeat "1 + 3 * $rank * $rank" {
                    fire {
                        relative "-3 + 6 * $rand"
                        speed "$1 * (1.0 + (0.1 + 0.2 * $rank) * $rand)"
                        plain
                    }
                    wait "1"
                }
                vanish
            })
        }
    }

  /// 大原さんのオリジナル、あの空に星を by 白い弾幕くん
  /// [Original]_star_in_the_sky.xml
  let star_in_the_sky =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、あの空に星を by 白い弾幕くん" {
        top {
            fire {
                absolute "360 * $rand"
                refBullet "dummy" []
            }
            repeat "11" {
                fire {
                    sequence "30"
                    refBullet "dummy" []
                }
            }
            wait "450 - 200 * $rank"
        }
        defBullet "dummy" {
            speed "0.0001"
            doActs (body {
                fire {
                    relative "0"
                    speed "0.6"
                    refBullet "star" []
                }
                wait "2"
                repeat "7" {
                    fire {
                        sequence "-7"
                        speedSeq "0.05"
                        refBullet "star" []
                    }
                    wait "2"
                }
                vanish
            })
        }
        defBullet "star" {
            doActs (body {
                wait "55"
                changeSpeed "0.0001" "1"
                wait "5"
                fire {
                    relative "-170"
                    speed "0.6 + 0.7 * $rank"
                    plain
                }
                wait "5"
                repeat "3 * 4 * $rank" {
                    fire {
                        sequence "11"
                        speedSeq "0.05"
                        plain
                    }
                    wait "5"
                }
                vanish
            })
        }
    }

  /// オリジナル。池に落ちた石六個。 by 白い弾幕くん
  /// [Original]_stone6.xml
  let stone6 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。池に落ちた石六個。 by 白い弾幕くん" {
        top {
            fire {
                absolute "150"
                speed "4"
                refBullet "roll" []
            }
            wait "2"
            fire {
                absolute "210"
                speed "4"
                refBullet "roll" []
            }
            wait "2"
            fire {
                absolute "135"
                speed "3"
                refBullet "roll" []
            }
            wait "2"
            fire {
                absolute "225"
                speed "3"
                refBullet "roll" []
            }
            wait "2"
            fire {
                absolute "135"
                speed "2"
                refBullet "roll" []
            }
            wait "2"
            fire {
                absolute "225"
                speed "2"
                refBullet "roll" []
            }
            wait "60"
        }
        defBullet "roll" {
            doActs (body {
                wait "10"
                changeSpeed "0" "1"
                repeat "45" {
                    fire {
                        sequence "8"
                        speed "1.3+$rank"
                        plain
                    }
                }
                vanish
            })
        }
    }

  /// オリジナル。道を探せ。by 白い弾幕くん
  /// [Original]_stop_and_run.xml
  let stop_and_run =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。道を探せ。by 白い弾幕くん" {
        defBullet "stopAndRun" {
            doActs (body {
                changeSpeed "0" "60"
                wait "180-$rank*120"
                accel "120" {
                    vertical "3"
                }
                wait "120"
            })
        }
        top {
            repeat "200" {
                fire {
                    dir "$rand*360"
                    speed "3*$rand+0.5"
                    refBullet "stopAndRun" []
                }
            }
            wait "260-$rank*120"
        }
    }

  /// 大原さんのオリジナル、時空転換 by 白い弾幕くん
  /// [Original]_time_twist.xml
  let time_twist =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、時空転換 by 白い弾幕くん" {
        top {
            actionRef "ancient" []
            wait "60 - 40 * $rank"
            actionRef "future" []
            wait "60 - 40 * $rank"
            actionRef "modern" []
            wait "60 - 40 * $rank"
            actionRef "medieval" []
            wait "60 - 40 * $rank"
            actionRef "primal" []
            wait "450"
        }
        defAction "ancient" {
            fire {
                absolute "90"
                speed "1.5"
                refBullet "ancientBit" []
            }
            fire {
                absolute "-90"
                speed "1.5"
                refBullet "ancientBit" []
            }
            fire {
                absolute "0"
                speed "0.0"
                refBullet "ancientBit" []
            }
        }
        defBullet "ancientBit" {
            doActs (body {
                wait "30"
                changeSpeed "0" "1"
                wait "5"
                repeat "40" {
                    fire {
                        absolute "180"
                        speed "2.0"
                        plain
                    }
                    wait "5"
                }
                vanish
            })
        }
        defAction "modern" {
            repeat "20" {
                fire {
                    absolute "-45 + 90 * $rand"
                    refBullet "modernBit" []
                }
                wait "6"
            }
        }
        defBullet "modernBit" {
            speed "0.5"
            doActs (body {
                changeDirectionAbs "160 + 40 * $rand" "90"
                wait "30"
                repeat "5 + 5 * $rank" {
                    fire {
                        absolute "-45 + 90 * $rand"
                        refBullet "modernScore" []
                    }
                    wait "3"
                }
                vanish
            })
        }
        defBullet "modernScore" {
            speed "0.5"
            doActs (body {
                changeDirectionAbs "160 + 40 * $rand" "90"
                changeSpeed "1.0 + 2.0 * $rank" "300"
            })
        }
        defAction "future" {
            fire {
                refBullet "futureBit" []
            }
        }
        defBullet "futureBit" {
            absolute "180"
            speed "1.5"
            doActs (body {
                wait "30"
                fire {
                    absolute "360 * $rand"
                    refBullet "futureTriangle" []
                }
                repeat "10" {
                    fire {
                        sequence "120"
                        refBullet "futureTriangle" []
                    }
                }
                vanish
            })
        }
        defBullet "futureTriangle" {
            speed "1.1"
            doActs (body {
                wait "30"
                changeDirectionRel "120" "1"
                wait "5"
                changeDirectionRel "150" "60"
                repeat "20" {
                    fire {
                        relative "45"
                        speed "0.8"
                        plain
                    }
                    wait "1"
                    fire {
                        relative "-45"
                        speed "1.0"
                        plain
                    }
                    wait "1"
                    fire {
                        relative "-60"
                        speed "1.2"
                        plain
                    }
                    wait "1"
                }
            })
        }
        defAction "medieval" {
            fire {
                refBullet "medievalBit" []
            }
        }
        defBullet "medievalBit" {
            absolute "180"
            speed "1.5"
            doActs (body {
                wait "60"
                changeSpeed "0.0" "1"
                wait "5"
                repeat "50" {
                    fire {
                        absolute "360 * $rand"
                        refBullet "medievalStar" ["90"]
                    }
                    fire {
                        absolute "360 * $rand"
                        refBullet "medievalStar" ["-90"]
                    }
                    wait "2"
                }
                vanish
            })
        }
        defBullet "medievalStar" {
            speed "1.1"
            doActs (body {
                wait "60"
                fire {
                    absolute "$1"
                    speed "1.1"
                    plain
                }
                vanish
            })
        }
        defAction "primal" {
            fire {
                refBullet "primalBit" []
            }
        }
        defBullet "primalBit" {
            aim "0"
            speed "0.5"
            doActs (body {
                changeSpeed "1.1" "120"
                repeat "24" {
                    changeDirectionAim "0" "5"
                    wait "5"
                }
                changeSpeed "0.0" "120"
                repeat "24" {
                    changeDirectionAim "0" "5"
                    wait "5"
                }
                wait "60"
                repeat "80 + 220 * $rank" {
                    actionRef "primalRock" ["180 * $rand"; "1"]
                    actionRef "primalRock" ["180 * $rand"; "-1"]
                }
                vanish
            })
        }
        defAction "primalRock" {
            fire {
                absolute "$1 * $2"
                speed "\r\n          (0.8 + 1.1*$1*(180-$1)/(90*90)) * (0.5+0.5*$rand) * (0.5+0.5*$rank)\r\n        "
                plain
            }
        }
    }

  /// 大原さんのオリジナル、津波。by 白い弾幕くん
  /// [Original]_tsunami.xml
  let tsunami =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "大原さんのオリジナル、津波。by 白い弾幕くん" {
        top {
            repeat "80+$rank*120" {
                fire {
                    absolute "360 * $rand"
                    speed "0.6 - 0.5 * $rank + 0.15 * $rand"
                    plain
                }
                fire {
                    absolute "60 + 240 * $rand"
                    speed "0.6 - 0.5 * $rank + 0.15 * $rand"
                    plain
                }
                fire {
                    absolute "120 + 120 * $rand"
                    speed "0.6 - 0.5 * $rank + 0.15 * $rand"
                    plain
                }
                wait "2"
            }
            wait "10 + 210 * $rank * $rank"
            fire {
                absolute "70 + 40 * $rand"
                speed "0.6 - 0.3 * $rank"
                refBullet "layer1" []
            }
            fire {
                sequence "180"
                speed "0.6 - 0.3 * $rank"
                refBullet "layer1" []
            }
            wait "600"
        }
        defBullet "layer1" {
            doActs (body {
                wait "15"
                repeat "100" {
                    fire {
                        relative "90"
                        speed "0.8 - 0.4 * $rank"
                        refBullet "layer2" []
                    }
                    fire {
                        relative "-90"
                        speed "0.8 - 0.4 * $rank"
                        refBullet "layer2" []
                    }
                    wait "60"
                }
            })
        }
        defBullet "layer2" {
            doActs (body {
                wait "15"
                repeat "100" {
                    fire {
                        relative "45"
                        speed "1.0 - 0.5 * $rank"
                        refBullet "layer3" []
                    }
                    fire {
                        relative "-45"
                        speed "1.0 - 0.5 * $rank"
                        refBullet "layer3" []
                    }
                    wait "60"
                }
            })
        }
        defBullet "layer3" {
            doActs (body {
                wait "15"
                repeat "100" {
                    fire {
                        relative "30"
                        speed "0.2 + 0.3 * $rank * $rand"
                        plain
                    }
                    fire {
                        relative "-30"
                        speed "0.2 + 0.3 * $rank * $rand"
                        plain
                    }
                    wait "60"
                }
            })
        }
    }

  /// オリジナル。二つ十字。by 白い弾幕くん
  /// [Original]_two_cross.xml
  let two_cross =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。二つ十字。by 白い弾幕くん" {
        defAction "add3" {
            repeat "3" {
                fire {
                    sequence "90"
                    plain
                }
            }
        }
        topFireAs "slow" {
            dir "(50-$rank*20)*$1"
            speed "2"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "5"
                    repeat "100" {
                        fire {
                            sequence "4*$1"
                            plain
                        }
                        actionRef "add3" []
                        wait "4"
                    }
                })
            })
        }
        top {
            repeat "3" {
                fireRef "slow" ["1"]
                fireRef "slow" ["-1"]
                wait "80"
            }
            wait "60"
        }
    }

  /// オリジナル。うねりを作る。by 白い弾幕くん
  /// [Original]_uneri.xml
  let uneri =
    createBulletmlInfo <|
    untyped "オリジナル。うねりを作る。by 白い弾幕くん" {
        defAction "src" {
            fire {
                aim "$1"
                speed "1.5"
                plain
            }
            repeat "450/(4-$rank*2)" {
                wait "4.5-$rank*3+$rand"
                fire {
                    sequence "$rand*10-5"
                    speed "1.5"
                    plain
                }
            }
            vanish
        }
        topFireAs "srcFire" {
            absolute "90"
            speed "$1"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "5"
                    changeSpeed "0" "1"
                    actionRef "src" ["$2"]
                })
            })
        }
        top {
            fireRef "srcFire" ["0"; "0"]
            fireRef "srcFire" ["4"; "10"]
            fireRef "srcFire" ["8"; "20"]
            fireRef "srcFire" ["-4"; "-10"]
            fireRef "srcFire" ["-8"; "-20"]
            wait "500"
        }
    }

  /// オリジナル。ワナ。by 白い弾幕くん
  /// [Original]_wana.xml
  let wana =
    createBulletmlInfo <|
    untyped "オリジナル。ワナ。by 白い弾幕くん" {
        defAction "XWayFan" {
            repeat "$1-1" {
                fire {
                    sequence "$2"
                    speedSeq "$3"
                    plain
                }
            }
        }
        defAction "top2" {
            fire {
                absolute "-10"
                speed "0"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            actionRef "main" ["20"; "5"; "1.10"; "-0.04"; "1.10"; "-0.04"]
            actionRef "main" ["15-$rank*6"; "5"; "1.06"; "0.04"; "1.06"; "0.04"]
            actionRef "main" ["5+$rank*20"; "5"; "1.10"; "-0.04"; "1.10"; "-0.04"]
        }
        defAction "top1" {
            fire {
                absolute "215"
                speed "0"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            actionRef "main" ["20"; "-5"; "1.10"; "-0.04"; "1.10"; "-0.04"]
            actionRef "main" ["15-$rank*6"; "-5"; "1.06"; "0.04"; "1.06"; "0.04"]
            actionRef "main" ["5+$rank*20"; "-5"; "1.10"; "-0.04"; "1.06"; "0.04"]
        }
        defAction "main" {
            repeat "$1" {
                fire {
                    sequence "36+$2"
                    speed "$3"
                    plain
                }
                actionRef "XWayFan" ["5"; "1"; "$4"]
                repeat "4" {
                    fire {
                        sequence "36"
                        speed "$3"
                        plain
                    }
                    actionRef "XWayFan" ["5"; "1"; "$4"]
                }
                repeat "4" {
                    fire {
                        sequence "36"
                        speed "$5"
                        plain
                    }
                    actionRef "XWayFan" ["5"; "1"; "$6"]
                }
                wait "15"
            }
        }
    }

  /// オリジナル。横加速。by 白い弾幕くん
  /// [Original]_yokokasoku.xml
  let yokokasoku =
    createBulletmlInfo <|
    untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "オリジナル。横加速。by 白い弾幕くん" {
        topFireAs "accelShot" {
            absolute "90"
            speed "0"
            ofBullet (bulletAnon {
                doActs (body {
                    accel "80" {
                        horizontal "3*$1"
                    }
                })
            })
        }
        top {
            repeat "20" {
                fire {
                    aim "$rand*90-45"
                    speed "1"
                    ofBullet (bulletAnon {
                        doActs (body {
                            repeat "9999" {
                                wait "$rand*20+30-$rank*20"
                                fireRef "accelShot" ["1"]
                                fireRef "accelShot" ["-1"]
                            }
                        })
                    })
                }
                wait "10"
            }
            wait "120"
        }
    }

  /// オリジナル。雑魚で突撃。by 白い弾幕くん
  /// [Original]_zako_atack.xml
  let zako_atack =
    createBulletmlInfo <|
    untyped "オリジナル。雑魚で突撃。by 白い弾幕くん" {
        top {
            repeat "40+$rank*20" {
                wait "4"
                fire {
                    aim "$rand*180-90"
                    speed "1.5"
                    ofBullet (bulletAnon {
                        doActs (body {
                            repeat "3" {
                                fire {
                                    aim "(0.5-$rand)*$rank*10"
                                    speed "1.5"
                                    plain
                                }
                                wait "20+$rand*$rank*40"
                            }
                        })
                    })
                }
            }
        }
    }
