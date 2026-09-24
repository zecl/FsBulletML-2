namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// EspRade
[<RequireQualifiedAccess>]
module EspRade =

    /// エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん
    /// [ESP_RADE]_round_5_alice_clone.xml
    let round_5_alice_clone =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん" {
            topFireAs "alice" {
                dir "$rand*360"
                speed "8"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "10*$rand"

                                fire {
                                    aim "$rand*30-15"
                                    plain
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            top {
                repeat "600" {
                    fireRef "alice" []
                    wait "$rank+1+$rand"
                }

                wait "100"
            }
        }

    /// エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_ares_2.xml
    let round_5_boss_ares_2 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん" {
            defAction "Stop" { changeSpeed "0" "1" }

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
                doActs (
                    body {
                        wait "10"

                        fire {
                            speed "0"
                            refBullet "aim3Impl" []
                        }

                        vanish
                    }
                )
            }

            defBullet "aim3Impl" {
                doActs (
                    body {
                        repeat "7" {
                            fire {
                                aim "-33+$rand*6"
                                speed "1.5"
                                plain
                            }

                            actionRef "XWay" [ "3"; "30" ]

                            repeat "2+$rank*3" {
                                wait "3"

                                fire {
                                    sequence "-60"
                                    speed "1.5"
                                    plain
                                }

                                actionRef "XWay" [ "3"; "30" ]
                            }

                            wait "54-$rank*9"
                        }

                        vanish
                    }
                )
            }

            defBullet "aim" {
                doActs (
                    body {
                        wait "10"

                        fire {
                            speed "0"
                            refBullet "aimImpl" []
                        }

                        vanish
                    }
                )
            }

            defBullet "aimImpl" {
                doActs (
                    body {
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
                    }
                )
            }

            defBullet "fan" {
                doActs (
                    body {
                        wait "10"
                        actionRef "Stop" []

                        repeat "3+$rank*4" {
                            fire {
                                absolute "$1-$2*3"
                                speed "$3"
                                plain
                            }

                            actionRef "XWay" [ "7"; "10" ]
                            wait "420/(3+$rank*4)"
                        }

                        vanish
                    }
                )
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
                    refBullet "fan" [ "-135"; "10"; "1.3" ]
                }

                fire {
                    absolute "-90"
                    speed "6"
                    refBullet "fan" [ "135"; "10"; "1.3" ]
                }

                fire {
                    absolute "110"
                    speed "4"
                    refBullet "fan" [ "-164"; "8"; "1.2" ]
                }

                fire {
                    absolute "-110"
                    speed "4"
                    refBullet "fan" [ "156"; "8"; "1.2" ]
                }

                fire {
                    absolute "130"
                    speed "2"
                    refBullet "fan" [ "180"; "8"; "1.1" ]
                }

                fire {
                    absolute "-130"
                    speed "2"
                    refBullet "fan" [ "180"; "5"; "1.1" ]
                }

                wait "430"
            }
        }

    /// エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_1_a.xml
    let round_5_boss_gara_1_a =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん" {
            defAction "sequenceThree" {
                fire {
                    sequence "12"
                    speedSeq "0"
                    plain
                }

                actionRef "sequenceTwo" []
            }

            defAction "sequenceTwo" {
                repeat "2" {
                    fire {
                        sequence "3"
                        speedSeq "0"
                        plain
                    }
                }
            }

            defAction "oogi" {
                fire {
                    dir "-90"
                    speed "1.5"
                    plain
                }

                actionRef "sequenceTwo" []
                repeatRef "11" "sequenceThree" []
                wait "10"
            }

            defAction "oogiOuHuku" {
                fire {
                    sequence "-213"
                    speed "1.5"
                    plain
                }

                actionRef "sequenceTwo" []
                repeatRef "11" "sequenceThree" []
                wait "10"
            }

            defAction "gara1a" {
                repeat "5" {
                    changeDirection "360*$rand" "1"
                    changeSpeed "0.5*$rand+0.5" "1"
                    actionRef "oogi" []
                    repeat "$rand*(3+$rank*2)+1+$rank*2" { actionRef "oogiOuHuku" [] }
                    changeSpeed "0" "1"
                    wait "50"
                }
            }

            top { actionRef "gara1a" [] }
        }

    /// エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_1_b.xml
    let round_5_boss_gara_1_b =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん" {
            defAction "8way2" {
                fire {
                    absolute "180-75"
                    speed "4"
                    plain
                }

                repeat "7" {
                    fire {
                        sequence "9"
                        speedSeq "-0.25"
                        plain
                    }
                }

                fire {
                    absolute "180+75"
                    speed "4"
                    plain
                }

                repeat "7" {
                    fire {
                        sequence "-9"
                        speedSeq "-0.25"
                        plain
                    }
                }
            }

            defAction "downShot" {
                fire {
                    aim "-60+$rand*120"
                    speed "4*$rand"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "20"

                                    fire {
                                        absolute "180"
                                        speed "1.2"
                                        plain
                                    }

                                    vanish
                                }
                            )
                        }
                    )
                }
            }

            defAction "gara" {
                changeDirectionAim "10+$rand*340" "1"
                changeSpeedAbs "0.3" "1"

                repeat "3+$rank*4" {
                    repeat "8" {
                        actionRef "downShot" []
                        wait "3*(3-$rank*2)*$rand"
                    }

                    actionRef "8way2" []
                }
            }

            top {
                repeatRef "5" "gara" []
                changeSpeed "0" "1"
                wait "30"
            }
        }

    /// エスプレイド、ガラ婦人第二形態 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_2.xml
    let round_5_boss_gara_2 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ婦人第二形態 by 白い弾幕くん" {
            defBullet "featherShot" {
                speed "6"

                doActs (
                    body {
                        changeSpeed "0" "20"
                        wait "20"
                        fire { refBullet "featherAim" [] }

                        repeat "150+$rank*100" {
                            fire {
                                dir "90*$rand-45"
                                plain
                            }

                            wait "3-$rank*2"
                        }

                        vanish
                    }
                )
            }

            defBullet "featherAim" {
                speed "0"

                doActs (
                    body {
                        repeat "7" {
                            fire {
                                speed "3"
                                plain
                            }

                            repeat "20" {
                                wait "2"

                                fire {
                                    sequence "0"
                                    speed "3"
                                    plain
                                }
                            }

                            wait "30"
                        }

                        vanish
                    }
                )
            }

            top {
                fire {
                    absolute "90"
                    refBullet "featherShot" []
                }

                fire {
                    absolute "-90"
                    refBullet "featherShot" []
                }

                wait "550"
            }
        }

    /// エスプレイド、ガラ第三形態 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_3.xml
    let round_5_boss_gara_3 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ第三形態 by 白い弾幕くん" {
            defAction "stop" {
                wait "15"
                changeSpeed "0" "1"
                wait "1"
            }

            defBullet "featherAllWay" {
                doActs (
                    body {
                        fire {
                            relative "$2*(180-(10-$1)*60)"
                            speed "0"
                            ofBullet (bulletAnon { doActs (body { vanish }) })
                        }

                        actionRef "stop" []

                        repeat "40" {
                            fire {
                                sequence "-2*(7-$1)*$2"
                                speed "0.9+0.2*(6-$1)"
                                plain
                            }

                            repeat "$1-1" {
                                fire {
                                    sequence "-2*$2"
                                    speedSeq "0"
                                    plain
                                }
                            }

                            wait "15"
                        }

                        vanish
                    }
                )
            }

            defBullet "featherAim" {
                doActs (
                    body {
                        actionRef "stop" []

                        repeat "10+$rank*20" {
                            fire {
                                aim "-3"
                                speed "1.2"
                                ofBullet (bulletAnon { doActs (body { () }) })
                            }

                            repeat "2" {
                                fire {
                                    sequence "3"
                                    speedSeq "0"
                                    ofBullet (bulletAnon { doActs (body { () }) })
                                }
                            }

                            wait "40-$rank*20"
                        }

                        vanish
                    }
                )
            }

            top {
                fire {
                    absolute "90"
                    speed "1"
                    refBullet "featherAim" []
                }

                fire {
                    absolute "-90"
                    speed "1"
                    refBullet "featherAim" []
                }

                fire {
                    absolute "70"
                    speed "2"
                    refBullet "featherAim" []
                }

                fire {
                    absolute "-70"
                    speed "2"
                    refBullet "featherAim" []
                }

                fire {
                    absolute "100"
                    speed "1.8"
                    refBullet "featherAllWay" [ "3"; "1" ]
                }

                fire {
                    absolute "-100"
                    speed "1.8"
                    refBullet "featherAllWay" [ "3"; "-1" ]
                }

                fire {
                    absolute "90"
                    speed "3"
                    refBullet "featherAllWay" [ "4"; "1" ]
                }

                fire {
                    absolute "-90"
                    speed "3"
                    refBullet "featherAllWay" [ "4"; "-1" ]
                }

                fire {
                    absolute "85"
                    speed "4"
                    refBullet "featherAllWay" [ "5"; "1" ]
                }

                fire {
                    absolute "-85"
                    speed "4"
                    refBullet "featherAllWay" [ "5"; "-1" ]
                }

                fire {
                    absolute "72"
                    speed "5"
                    refBullet "featherAllWay" [ "6"; "1" ]
                }

                fire {
                    absolute "-72"
                    speed "5"
                    refBullet "featherAllWay" [ "6"; "-1" ]
                }

                wait "700"
            }
        }

    /// エスプレイド、ガラ婦人第四形態 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_4.xml
    let round_5_boss_gara_4 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ婦人第四形態 by 白い弾幕くん" {
            defBullet "featherShot" {
                speed "7"

                doActs (
                    body {
                        changeSpeed "0" "20"
                        wait "20"

                        repeat "50" {
                            fire {
                                dir "20*$rand-10"
                                speed "2*$rand+0.7"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }

            top {
                repeat "4" {
                    fire {
                        absolute "90"
                        refBullet "featherShot" []
                    }

                    wait "30+$rank*30"

                    fire {
                        absolute "-90"
                        refBullet "featherShot" []
                    }

                    wait "30+$rank*30"
                }

                wait "120"

                repeat "4" {
                    fire {
                        absolute "90"
                        refBullet "featherShot" []
                    }

                    wait "40-$rank*20"

                    fire {
                        absolute "-90"
                        refBullet "featherShot" []
                    }

                    wait "40-$rank*20"
                }

                wait "60"
            }
        }

    /// エスプレイド、ガラ婦人最終形態 by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_gara_5.xml
    let round_5_boss_gara_5 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、ガラ婦人最終形態 by 白い弾幕くん" {
            defAction "accel" {
                changeDirection "360*$rand" "1"
                changeSpeed "0.5+$rand*0.5" "1"
            }

            defAction "stop" { changeSpeed "0" "1" }

            defAction "stopAndWait" {
                actionRef "stop" []
                wait "70-$rank*50"
            }

            defAction "ippon" {
                repeat "26" {
                    fire {
                        sequence "0"
                        speedSeq "0.12"
                        plain
                    }
                }
            }

            defAction "murasaki" {
                fire {
                    absolute "$2"
                    speed "0.8"
                    plain
                }

                repeat "3+$rand*17" {
                    actionRef "ippon" []
                    wait "6"

                    fire {
                        sequence "$1"
                        speed "0.8"
                        plain
                    }
                }
            }

            defAction "ao" {
                repeat "3+$rand*17" {
                    fire {
                        speed "0.8"
                        plain
                    }

                    actionRef "ippon" []
                    wait "6"
                }
            }

            defAction "gara5" {
                repeat "2" {
                    actionRef "accel" []
                    actionRef "murasaki" [ "5"; "180-$rand*90" ]
                    actionRef "stopAndWait" []
                    actionRef "accel" []
                    actionRef "ao" []
                    actionRef "stopAndWait" []
                    actionRef "accel" []
                    actionRef "murasaki" [ "-5"; "180+$rand*90" ]
                    actionRef "stopAndWait" []
                    actionRef "accel" []
                    actionRef "ao" []
                    actionRef "stopAndWait" []
                }
            }

            top { actionRef "gara5" [] }
        }

    /// エスプレイド、五行覚師、発狂。by 白い弾幕くん
    /// [ESP_RADE]_round_5_boss_kakusi_hakkyou.xml
    let round_5_boss_kakusi_hakkyou =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、五行覚師、発狂。by 白い弾幕くん" {
            defBullet "6shots" {
                speed "3"

                doActs (
                    body {
                        wait "3"

                        repeat "2" {
                            fire {
                                aim "-15+30*$rand"
                                speed "0.8+$rank+$rand"
                                plain
                            }
                        }

                        repeat "2" {
                            fire {
                                aim "-45+30*$rand"
                                speed "0.8+$rank+$rand"
                                plain
                            }
                        }

                        repeat "2" {
                            fire {
                                aim "15+30*$rand"
                                speed "0.8+$rank+$rand"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }

            defBullet "kakusi" {
                speed "6"

                doActs (
                    body {
                        changeSpeed "0" "10"
                        wait "10"

                        repeat "4+$rank*6" {
                            fire {
                                aim "90"
                                refBullet "6shots" []
                            }

                            fire {
                                aim "-90"
                                refBullet "6shots" []
                            }

                            wait "200/(4+$rank*6)"
                        }

                        vanish
                    }
                )
            }

            top {
                fire {
                    absolute "90"
                    refBullet "kakusi" []
                }

                fire {
                    absolute "270"
                    refBullet "kakusi" []
                }

                wait "200"
            }
        }

    /// エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん
    /// [ESP_RADE]_round_123_boss_izuna_hakkyou.xml
    let round_123_boss_izuna_hakkyou =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん" {
            defBullet "Red" { doActs (body { () }) }
            defBullet "Dummy" { doActs (body { vanish }) }
            defAction "Stop" { changeSpeed "0" "1" }
            defAction "XWay" { actionRef "XWayFan" [ "$1"; "$2"; "0" ] }

            defAction "XWayFan" {
                repeat "$1-1" {
                    fire {
                        sequence "$2"
                        speedSeq "$3"
                        plain
                    }
                }
            }

            defBullet "roll" {
                absolute "90*$1"
                speed "3"

                doActs (
                    body {
                        wait "10"
                        actionRef "Stop" []

                        fire {
                            relative "0"
                            speed "1.5+$rank"
                            refBullet "Dummy" []
                        }

                        repeat "10" {
                            wait "8"

                            fire {
                                sequence "5.3*$1"
                                speedSeq "-0.3-$rank*0.5"
                                plain
                            }

                            actionRef "XWay" [ "8"; "45" ]

                            repeat "5" {
                                wait "8"

                                fire {
                                    sequence "3*$1"
                                    speedSeq "0.06+$rank*0.1"
                                    plain
                                }

                                actionRef "XWay" [ "8"; "45" ]
                            }
                        }

                        vanish
                    }
                )
            }

            top {
                fire { refBullet "roll" [ "1" ] }
                fire { refBullet "roll" [ "-1" ] }

                fire {
                    absolute "180"
                    speed "2"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "10"
                                    actionRef "Stop" []
                                    actionRef "aim" []
                                    vanish
                                }
                            )
                        }
                    )
                }

                wait "500"
            }

            defAction "aim" {
                repeat "10" {
                    wait "50"

                    fire {
                        aim "-1"
                        speed "1.7"
                        refBullet "Red" []
                    }

                    fire {
                        sequence "2"
                        speedSeq "0"
                        refBullet "Red" []
                    }

                    repeat "2+$rank*6" {
                        fire {
                            sequence "-2"
                            speedSeq "0.1"
                            refBullet "Red" []
                        }

                        fire {
                            sequence "2"
                            speedSeq "0"
                            refBullet "Red" []
                        }
                    }
                }
            }
        }

    /// エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん
    /// [ESP_RADE]_round_123_boss_pelaboy_hakkyou.xml
    let round_123_boss_pelaboy_hakkyou =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん" {
            defBullet "Red" { doActs (body { () }) }
            defBullet "Dummy" { doActs (body { vanish }) }
            defAction "Stop" { changeSpeed "0" "1" }
            defAction "XWay" { actionRef "XWayFan" [ "$1"; "$2"; "0" ] }

            defAction "XWayFan" {
                repeat "$1-1" {
                    fire {
                        sequence "$2"
                        speedSeq "$3"
                        plain
                    }
                }
            }

            defBullet "subBatteryFan" {
                speed "4"

                doActs (
                    body {
                        wait "10"
                        actionRef "Stop" []
                        wait "250"

                        fire {
                            aim "-45"
                            speed "1.6"
                            plain
                        }

                        actionRef "XWay" [ "10+$rank*10"; "90/(10+$rank*10)" ]

                        repeat "2" {
                            wait "5"

                            fire {
                                sequence "-90"
                                speed "1.6"
                                plain
                            }

                            actionRef "XWay" [ "11+$rank*10"; "90/(10+$rank*10)" ]
                        }

                        vanish
                    }
                )
            }

            defBullet "aimFan" {
                speed "4"

                doActs (
                    body {
                        wait "5"

                        fire {
                            aim "-4-$rank*8"
                            speed "1.6"
                            plain
                        }

                        actionRef "XWay" [ "5+$rank*8"; "2" ]
                        vanish
                    }
                )
            }

            defBullet "subBatteryAim" {
                speed "1"

                doActs (
                    body {
                        wait "10"
                        actionRef "Stop" []

                        repeat "4" {
                            wait "100+$rand*50"

                            fire {
                                absolute "90"
                                refBullet "aimFan" []
                            }

                            fire {
                                absolute "-90"
                                refBullet "aimFan" []
                            }
                        }

                        vanish
                    }
                )
            }

            defBullet "soldier" {
                absolute "90*$1"
                speed "2"

                doActs (
                    body {
                        wait "10"
                        actionRef "Stop" []

                        fire {
                            absolute "$2"
                            speed "1.3"
                            refBullet "Dummy" []
                        }

                        repeat "120+$rank*200" {
                            wait "440/(120+$rank*200)+$rand"

                            fire {
                                sequence "17*$1"
                                speedSeq "0"
                                plain
                            }
                        }
                    }
                )
            }

            top {
                fire { refBullet "soldier" [ "1"; "90" ] }
                fire { refBullet "soldier" [ "-1"; "-80" ] }

                fire {
                    absolute "180"
                    refBullet "subBatteryAim" []
                }

                fire {
                    absolute "90"
                    refBullet "subBatteryFan" []
                }

                fire {
                    absolute "-90"
                    refBullet "subBatteryFan" []
                }

                fire {
                    absolute "180"
                    speed "2"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "10"

                                    fire {
                                        speed "0"

                                        ofBullet (
                                            bulletAnon {
                                                doActs (
                                                    body {
                                                        actionRef "mainBattery" []
                                                        vanish
                                                    }
                                                )
                                            }
                                        )
                                    }

                                    vanish
                                }
                            )
                        }
                    )
                }

                wait "500"
            }

            defAction "mainBattery" {
                repeat "15" {
                    wait "8"

                    fire {
                        aim "0"
                        speed "1.6"
                        plain
                    }
                }

                wait "195"

                repeat "4" {
                    wait "20"

                    fire {
                        absolute "88+$rand*4"
                        speed "1.6"
                        plain
                    }

                    actionRef "XWay" [ "12+$rank*16"; "180/(12+$rank*16)" ]
                    wait "20"

                    fire {
                        absolute "93+$rand*4"
                        speed "1.6"
                        plain
                    }

                    actionRef "XWay" [ "11+$rank*16"; "170/(11+$rank*16)" ]
                }

                wait "40"
            }
        }

    /// エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん
    /// [ESP_RADE]_round_123_boss_satoru_5way.xml
    let round_123_boss_satoru_5way =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん" {
            defAction "1way" {
                fire {
                    aim "$2+$1*$rand*2-$1"
                    speed "1"
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

            defAction "5way" {
                actionRef "1way" [ "$1"; "-30" ]
                actionRef "1way" [ "$1"; "-15" ]
                actionRef "1way" [ "$1"; "0" ]
                actionRef "1way" [ "$1"; "15" ]
                actionRef "1way" [ "$1"; "30" ]
            }

            defAction "idousite5way" {
                changeDirection "$rand*360" "1"
                changeSpeed "2" "1"
                wait "30"
                actionRef "5way" [ "$1" ]
                changeSpeed "0" "1"
                wait "90-$rank*60"
            }

            defAction "satoru" {
                actionRef "idousite5way" [ "1" ]
                actionRef "idousite5way" [ "2" ]
                actionRef "idousite5way" [ "3" ]
                actionRef "idousite5way" [ "4" ]
                actionRef "idousite5way" [ "5" ]
            }

            top {
                actionRef "satoru" []
                wait "30"
            }
        }
