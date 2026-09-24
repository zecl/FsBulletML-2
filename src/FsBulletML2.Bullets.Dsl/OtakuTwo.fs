namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// OtakuTwo
[<RequireQualifiedAccess>]
module OtakuTwo =

    /// おたくツーさん作、円形発射弾・花火型 by 白い弾幕くん
    /// [OtakuTwo]_circle_fireworks.xml
    let circle_fireworks =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "おたくツーさん作、円形発射弾・花火型 by 白い弾幕くん" {
            top {
                changeDirectionAbs "180" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"

                repeat "20+$rank*40" {
                    fire {
                        aim "0"
                        speed "0.4+$rank*0.6"
                        plain
                    }

                    repeat "59" {
                        fire {
                            sequence "6"
                            speed "0.4+$rank*0.6"
                            plain
                        }
                    }

                    wait "30-$rank*20"

                    fire {
                        sequence "9"
                        speed "1.2+$rank*1.8"
                        plain
                    }

                    repeat "59" {
                        fire {
                            sequence "6"
                            speed "1.2+$rank*1.8"
                            plain
                        }
                    }

                    wait "30-$rank*20"
                }

                changeDirectionAbs "0" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"
            }
        }

    /// おたくツーさん作、円形発射弾・花火型弐式 by 白い弾幕くん
    /// [OtakuTwo]_circle_fireworks2.xml
    let circle_fireworks2 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "おたくツーさん作、円形発射弾・花火型弐式 by 白い弾幕くん" {
            top {
                changeDirectionAbs "180" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"

                repeat "20+$rank*40" {
                    fire {
                        aim "0"
                        speed "1.2+$rank*1.8"
                        plain
                    }

                    repeat "59" {
                        fire {
                            sequence "6"
                            speed "1.2+$rank*1.8"
                            plain
                        }
                    }

                    wait "30-$rank*20"

                    fire {
                        sequence "9"
                        speed "0.4+$rank*0.6"
                        plain
                    }

                    repeat "59" {
                        fire {
                            sequence "6"
                            speed "0.4+$rank*0.6"
                            plain
                        }
                    }

                    wait "30-$rank*20"
                }

                changeDirectionAbs "0" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"
            }
        }

    /// おたくツーさん作、円形発射弾・速度変化型 by 白い弾幕くん
    /// [OtakuTwo]_circle_trap.xml
    let circle_trap =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "おたくツーさん作、円形発射弾・速度変化型 by 白い弾幕くん" {
            top { repeat "10" { actionRef "main" [ "$rand"; "$rand" ] } }

            defAction "main" {
                actionRef "move" [ "100+$1*160"; "$2" ]
                wait "40-$rank*20"
                actionRef "round" []
                actionRef "move" [ "280+$1*160"; "$2" ]
                wait "25"
            }

            defAction "move" {
                changeDirectionAbs "$1" "1"
                changeSpeed "$2*1.5+$rank*1.5" "1"
                wait "40-$rank*20"
                changeSpeed "0" "1"
            }

            defAction "round" {
                fire {
                    absolute "$rand*360"
                    speed "0"
                    ofBullet (bulletAnon { doActs (body { vanish }) })
                }

                wait "1"

                repeat "6" {
                    repeat "30" {
                        fire {
                            sequence "1"
                            speed "0.8+$rank*0.4"
                            plain
                        }
                    }

                    repeat "30" {
                        fire {
                            sequence "1"
                            speed "0.6+$rank*0.3"
                            refBullet "speed" []
                        }
                    }
                }
            }

            defBullet "speed" {
                doActs (
                    body {
                        wait "100-$rank*50"
                        changeSpeed "1.2+$rank*0.6" "1"
                        wait "(100-$rank*50)/2"
                        changeSpeed "0.8+$rank*0.4" "1"
                    }
                )
            }
        }

    /// 最臭鬼畜兵器「非蜂」１：ニオイ波動 by 白い弾幕くん
    /// [OtakuTwo]_dis_bee_1.xml
    let dis_bee_1 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "最臭鬼畜兵器「非蜂」１：ニオイ波動 by 白い弾幕くん" {
            top {
                actionRef "position" [ "$rand"; "$rand"; "10"; "6-$rank*4" ]
                actionRef "position" [ "$rand"; "$rand"; "15"; "6-$rank*4" ]
                actionRef "position" [ "$rand"; "$rand"; "20"; "6-$rank*4" ]
                actionRef "position" [ "$rand"; "$rand"; "25"; "6-$rank*4" ]
                actionRef "position" [ "$rand"; "$rand"; "30"; "6-$rank*4" ]
            }

            defAction "position" {
                actionRef "move" [ "100+$1*160"; "$2" ]
                changeDirectionAbs "$rand*360" "1"
                wait "1"
                actionRef "wave" [ "$3"; "$4" ]
                actionRef "move" [ "(100+$1*160)+180"; "$2" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "1"
                changeSpeed "$2*6+$rank*6" "1"
                wait "10-$rank*5"
                changeSpeed "0" "1"
                wait "1"
            }

            defAction "wave" {
                repeat "3" {
                    actionRef "allrange" [ " 0.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 3.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 5.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 6.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 6.5"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 6.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 5.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 3.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ " 0.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-3.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-5.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-6.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-6.5"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-6.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-5.0"; "$1" ]
                    wait "$2"
                    actionRef "allrange" [ "-3.0"; "$1" ]
                    wait "$2"
                }
            }

            defAction "allrange" {
                fire {
                    relative "$1"
                    speed "0.8+$rank*1.6"
                    plain
                }

                repeat "$2-1" {
                    fire {
                        sequence "360/$2"
                        speed "0.8+$rank*1.6"
                        plain
                    }
                }
            }
        }

    /// 最臭鬼畜兵器「非蜂」２：壁花火 by 白い弾幕くん
    /// [OtakuTwo]_dis_bee_2.xml
    let dis_bee_2 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "最臭鬼畜兵器「非蜂」２：壁花火 by 白い弾幕くん" {
            top {
                actionRef "move" [ "180" ]
                changeDirectionAbs "$rand*360" "1"
                wait "5"

                repeat "20+$rank*20" {
                    actionRef "wall" [ "15" ]
                    wait "25-$rank*$rank*12"
                    actionRef "wall" [ " 0" ]
                    wait "25-$rank*$rank*12"
                }

                actionRef "move" [ "0" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "5"
                wait "6"
                changeSpeed "1" "50"
                wait "55"
                changeSpeed "0" "50"
                wait "55"
            }

            defAction "wall" {
                fire {
                    relative "$1"
                    speed "1+$rank*1.2"
                    plain
                }

                actionRef "wallbody" []

                repeat "11" {
                    fire {
                        sequence "15"
                        speed "1+$rank*1.2"
                        plain
                    }

                    actionRef "wallbody" []
                }
            }

            defAction "wallbody" {
                repeat "15" {
                    fire {
                        sequence "1"
                        speed "1+$rank*1.2"
                        plain
                    }
                }
            }
        }

    /// 最臭鬼畜兵器「非蜂」３：ぐるぐる風車 by 白い弾幕くん
    /// [OtakuTwo]_dis_bee_3.xml
    let dis_bee_3 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "最臭鬼畜兵器「非蜂」３：ぐるぐる風車 by 白い弾幕くん" {
            top {
                fire {
                    absolute "0"
                    speed "0"
                    refBullet "top" []
                }

                fire {
                    absolute "0"
                    refBullet "byakko" []
                }

                actionRef "byakko" []

                repeat "120+$rank*$rank*$rank*120" {
                    wait "8-$rank*$rank*$rank*4"

                    fire {
                        sequence "128+$rand*0.5"
                        refBullet "byakko" []
                    }

                    actionRef "byakko" []
                }
            }

            defAction "byakko" {
                repeat "2" {
                    fire {
                        sequence "120"
                        refBullet "byakko" []
                    }
                }
            }

            defBullet "byakko" {
                speed "6"

                doActs (
                    body {
                        fireRef "byakkoway" [ " 48.4-$rand*0.8" ]
                        fireRef "byakkoway" [ " 32.4-$rand*0.8" ]
                        fireRef "byakkoway" [ " 16.4-$rand*0.8" ]
                        fireRef "byakkoway" [ "  0.4-$rand*0.8" ]
                        fireRef "byakkoway" [ "-16.4+$rand*0.8" ]
                        fireRef "byakkoway" [ "-32.4+$rand*0.8" ]
                        fireRef "byakkoway" [ "-48.4+$rand*0.8" ]
                        vanish
                    }
                )
            }

            topFireAs "byakkoway" {
                relative "$1"
                speed "0.8+$rank*$rank*1"
                plain
            }

            defBullet "top" {
                doActs (
                    body {
                        fire {
                            absolute "0"
                            refBullet "backfire" []
                        }

                        actionRef "backfire" []

                        repeat "120+$rank*$rank*$rank*120" {
                            wait "8-$rank*$rank*$rank*4"

                            fire {
                                sequence "112+$rand*0.5-$rank*$rank*$rank*$rank*$rank*9.5"
                                refBullet "backfire" []
                            }

                            actionRef "backfire" []
                        }
                    }
                )
            }

            defAction "backfire" {
                repeat "2" {
                    fire {
                        sequence "120"
                        refBullet "backfire" []
                    }
                }
            }

            defBullet "backfire" {
                speed "10"

                doActs (
                    body {
                        wait "4"
                        fireRef "backfire" [ "100.5" ]
                        fireRef "backfire" [ "110.5" ]
                        fireRef "backfire" [ "120.5" ]
                        fireRef "backfire" [ "130.5" ]
                        fireRef "backfire" [ "140.5" ]
                        vanish
                    }
                )
            }

            topFireAs "backfire" {
                relative "$1-$rand"
                speed "1+$rank*$rank"
                plain
            }
        }

    /// おたくツーさん作、回転砲台・鶚型 by 白い弾幕くん
    /// [OtakuTwo]_roll_misago.xml
    let roll_misago =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "おたくツーさん作、回転砲台・鶚型 by 白い弾幕くん" {
            top {
                changeDirectionAbs "180" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"
                wait "10"

                repeat "6" {
                    fire {
                        sequence "30"
                        speed "0.5"
                        refBullet "white" []
                    }

                    fire {
                        sequence "30"
                        speed "0.5"
                        refBullet "black" []
                    }
                }

                repeat "120" {
                    fire {
                        sequence "3"
                        speed "0.48"
                        refBullet "normal" []
                    }
                }

                wait "1700"
                changeDirectionAbs "0" "1"
                changeSpeed "1" "1"
                wait "25"
                changeSpeed "0" "1"
            }

            defBullet "white" {
                doActs (
                    body {
                        wait "80"
                        changeDirectionRel "-90" "4"
                        wait "4"

                        repeat "8" {
                            changeDirectionRel "-135" "195"

                            repeat "30+$rank*15" {
                                fire {
                                    relative "90"
                                    speed "0.5"
                                    plain
                                }

                                wait "6-$rank*2"
                            }
                        }
                    }
                )
            }

            defBullet "black" {
                doActs (
                    body {
                        fire {
                            relative "-90"
                            speed "0.8"
                            refBullet "direction" []
                        }

                        wait "80"
                        changeDirectionRel "-90" "4"
                        wait "4"

                        repeat "8" {
                            changeDirectionRel "-135" "195"

                            repeat "30+$rank*30" {
                                fire {
                                    sequence "3.92307692307692307692307692307833*(2-$rank)"
                                    speed "0.5"
                                    plain
                                }

                                wait "6-$rank*3"
                            }
                        }
                    }
                )
            }

            defBullet "normal" {
                doActs (
                    body {
                        wait "80"
                        changeDirectionRel "-90" "4"
                        wait "4"

                        repeat "8" {
                            changeDirectionRel "-135" "195"
                            wait "180"
                        }
                    }
                )
            }

            defBullet "direction" { doActs (body { vanish }) }
        }

    /// 回転発射弾・四段風車形 by 白い弾幕くん
    /// [OtakuTwo]_self-0012.xml
    let self_0012 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "回転発射弾・四段風車形 by 白い弾幕くん" {
            defAction "top1" {
                fire {
                    absolute "0"
                    speed "0"
                    refBullet "4way" [ "$rand"; "$rand" ]
                }

                actionRef "3way" [ "$rand"; "$rand" ]
                wait "100"
            }

            defAction "3way" {
                repeat "200" {
                    fire {
                        sequence "123.7+$1"
                        speed "(1+$2*0.5)*(1+$rank*$rank*$rank*$rank)"
                        plain
                    }

                    repeat "2" {
                        fire {
                            sequence "120"
                            speed "(1+$2*0.5)*(1+$rank*$rank*$rank*$rank)"
                            plain
                        }
                    }

                    wait "8-$rank*4"
                }
            }

            defBullet "4way" {
                doActs (
                    body {
                        repeat "200" {
                            fire {
                                sequence "92.1+$1"
                                speed "(0.8+$2*0.8)*(1+$rank*$rank*$rank*$rank)"
                                plain
                            }

                            repeat "3" {
                                fire {
                                    sequence "90"
                                    speed "(0.8+$2*0.8)*(1+$rank*$rank*$rank*$rank)"
                                    plain
                                }
                            }

                            wait "8-$rank*4"
                        }

                        vanish
                    }
                )
            }

            defAction "top5" { actionRef "5way" [ "$rand"; "$rand" ] }

            defAction "5way" {
                repeat "200" {
                    fire {
                        sequence "78.7+$1"
                        speed "(0.6+$2*1.6)*(1+$rank*$rank*$rank*$rank)"
                        ofBullet (bulletAnon { doActs (body { changeSpeedRel "0" "9999" }) })
                    }

                    repeat "4" {
                        fire {
                            sequence "72"
                            speed "(0.6+$2*1.6)*(1+$rank*$rank*$rank*$rank)"
                            ofBullet (bulletAnon { doActs (body { changeSpeedRel "0" "9999" }) })
                        }
                    }

                    wait "8-$rank*4"
                }
            }

            defAction "top6" { actionRef "6way" [ "$rand"; "$rand" ] }

            defAction "6way" {
                repeat "200" {
                    fire {
                        sequence "63.3+$1"
                        speed "(0.6+$2*0.9)*(1+$rank*$rank*$rank*$rank)"

                        ofBullet (
                            bulletAnon {
                                doActs (
                                    body {
                                        wait "9999"

                                        fire {
                                            relative "0"
                                            speedRel "0"
                                            ofBullet (bulletAnon { doActs (body { vanish }) })
                                        }
                                    }
                                )
                            }
                        )
                    }

                    repeat "5" {
                        fire {
                            sequence "60"
                            speed "(0.6+$2*0.9)*(1+$rank*$rank*$rank*$rank)"

                            ofBullet (
                                bulletAnon {
                                    doActs (
                                        body {
                                            wait "9999"

                                            fire {
                                                relative "0"
                                                speedRel "0"
                                                ofBullet (bulletAnon { doActs (body { vanish }) })
                                            }
                                        }
                                    )
                                }
                            )
                        }
                    }

                    wait "8-$rank*4"
                }
            }
        }

    /// 自機拘束弾・不規則回転型 by 白い弾幕くん
    /// [OtakuTwo]_self-0062.xml
    let self_0062 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "自機拘束弾・不規則回転型 by 白い弾幕くん" {
            top {
                actionRef "move" [ "180" ]
                changeDirectionAim "0" "1"
                wait "1"

                repeat "50" {
                    wait "1"
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7" ]
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7.1" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7.1" ]
                }

                repeat "20" {
                    changeDirectionRel "(-60+$rand*120)" "100+$rank*50"

                    repeat "100-$rand*50" {
                        wait "1"
                        fireRef "winder" [ " 22.5-$rank*12.5"; "7" ]
                        fireRef "winder" [ "-22.5+$rank*12.5"; "7" ]
                        fireRef "winder" [ " 22.5-$rank*12.5"; "7.1" ]
                        fireRef "winder" [ "-22.5+$rank*12.5"; "7.1" ]
                    }
                }

                wait "50"
                actionRef "move" [ "0" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "1"
                changeSpeed "2" "1"
                wait "18"
                changeSpeed "0" "1"
                wait "5"
            }

            topFireAs "winder" {
                relative "$1"
                speed "$2"
                plain
            }
        }

    /// 自機拘束弾・不規則回転型改 by 白い弾幕くん
    /// [OtakuTwo]_self-0063.xml
    let self_0063 =
        createBulletmlInfo
        <| untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "自機拘束弾・不規則回転型改 by 白い弾幕くん" {
            top {
                actionRef "move" [ "180" ]
                changeDirectionAim "0" "1"
                wait "1"

                repeat "50" {
                    wait "1"
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7" ]
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7.1" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7.1" ]
                }

                repeat "5" {
                    actionRef "wall" []

                    fire {
                        absolute "0"
                        speed "0"
                        refBullet "round" []
                    }

                    actionRef "wall" []
                    actionRef "wall" []
                    actionRef "wall" []
                }

                wait "50"
                actionRef "move" [ "0" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "1"
                changeSpeed "2" "1"
                wait "18"
                changeSpeed "0" "1"
                wait "5"
            }

            defAction "wall" {
                changeDirectionRel "-60+$rand*120" "100+$rank*50"

                repeat "100-$rand*50" {
                    wait "1"
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7" ]
                    fireRef "winder" [ " 22.5-$rank*12.5"; "7.1" ]
                    fireRef "winder" [ "-22.5+$rank*12.5"; "7.1" ]
                }
            }

            topFireAs "winder" {
                relative "$1"
                speed "$2"
                plain
            }

            defBullet "round" {
                doActs (
                    body {
                        wait "100*$rand"

                        fire {
                            absolute "$rand*360"
                            speed "1.0+$rank*1.0"
                            plain
                        }

                        fire {
                            sequence "        4"
                            speed "0.8+$rank*0.8"
                            plain
                        }

                        repeat "44" {
                            fire {
                                sequence "4"
                                speed "1.0+$rank*1.0"
                                plain
                            }

                            fire {
                                sequence "4"
                                speed "0.8+$rank*0.8"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }
        }

    /// 回転砲台・鶚型副産物 by 白い弾幕くん
    /// [OtakuTwo]_self-0071.xml
    let self_0071 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "回転砲台・鶚型副産物 by 白い弾幕くん" {
            top {
                actionRef "move" [ "180" ]
                actionRef "way" [ "6"; "$rand" ]
                actionRef "way" [ "8"; "$rand" ]
                actionRef "way" [ "10"; "$rand" ]
                actionRef "way" [ "12"; "$rand" ]
                wait "50"
                actionRef "move" [ "0" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "2"
                wait "3"
                changeSpeed "2" "25"
                wait "27"
                changeSpeed "0" "25"
                wait "27"
            }

            defAction "way" {
                fire {
                    absolute "$rand*360"
                    speed "1"
                    refBullet "turn" [ "$2" ]
                }

                repeat "$1-1" {
                    fire {
                        sequence "360/$1"
                        speed "1"
                        refBullet "turn" [ "$2" ]
                    }
                }

                wait "500"
            }

            defBullet "turn" {
                doActs (
                    body {
                        changeSpeed "0" "100"
                        wait "90"

                        fire {
                            relative "180"
                            speed "0.1"
                            refBullet "bit" [ "$1" ]
                        }

                        vanish
                    }
                )
            }

            defBullet "bit" {
                doActs (
                    body {
                        changeSpeed "0.5" "50"
                        changeDirectionRel "0.1+$rank*$rank*$rank*90" "1000-$rank*$rank*$rank*500"

                        fire {
                            relative "$1*360"
                            speed "0"
                            ofBullet (bulletAnon { doActs (body { vanish }) })
                        }

                        repeat "9999" {
                            fire {
                                sequence "4.6"
                                speed "0.8+$rank*$rank*0.4"
                                plain
                            }

                            wait "10-$rank*5"
                        }
                    }
                )
            }
        }

    /// 加速弾・巨大弾落下型 by 白い弾幕くん
    /// [OtakuTwo]_self-0081.xml
    let self_0081 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "加速弾・巨大弾落下型 by 白い弾幕くん" {
            top {
                repeat "50" {
                    fireRef "seed" [ " 1"; "$rand"; "$rand" ]
                    wait "15"
                    fireRef "seed" [ "-1"; "$rand"; "$rand" ]
                    wait "15"
                }
            }

            topFireAs "seed" {
                absolute "90*$1"
                speed "$rand*3"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "20"

                                repeat "40" {
                                    fire {
                                        sequence "9"
                                        speed "0.5+$rank"
                                        refBullet "roundbase" [ "$2"; "$3" ]
                                    }
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            defBullet "roundbase" {
                doActs (
                    body {
                        wait "10"

                        fire {
                            absolute "90"
                            speed "$1"
                            refBullet "round" []
                        }

                        fire {
                            absolute "270"
                            speed "$2"
                            refBullet "round" []
                        }

                        vanish
                    }
                )
            }

            defBullet "round" { doActs (body { accel "250" { vertical "10" } }) }
        }

    /// 「緋蜂のような物体」超速青弾part1 by 白い弾幕くん
    /// [OtakuTwo]_self-1020.xml
    let self_1020 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "「緋蜂のような物体」超速青弾part1 by 白い弾幕くん" {
            top {
                repeat "50" {
                    fire {
                        sequence "30+1.1*5"
                        speed "2+$rank*$rank*2"
                        plain
                    }

                    repeat "11" {
                        fire {
                            sequence "30"
                            speed "2+$rank*$rank*2"
                            plain
                        }
                    }

                    repeat "4+$rank*10" {
                        wait "1"

                        fire {
                            sequence "31.1"
                            speed "2+$rank*$rank*2"
                            plain
                        }

                        repeat "11" {
                            fire {
                                sequence "30"
                                speed "2+$rank*$rank*2"
                                plain
                            }
                        }
                    }

                    wait "10"
                }
            }

            defAction "top2" {
                repeat "50" {
                    fire {
                        sequence "-30-0.7*5"
                        speed "2+$rank*$rank*2"
                        plain
                    }

                    repeat "11" {
                        fire {
                            sequence "-30"
                            speed "2+$rank*$rank*2"
                            plain
                        }
                    }

                    repeat "4+$rank*10" {
                        wait "1"

                        fire {
                            sequence "-30.7"
                            speed "2+$rank*$rank*2"
                            plain
                        }

                        repeat "11" {
                            fire {
                                sequence "-30"
                                speed "2+$rank*$rank*2"
                                plain
                            }
                        }
                    }

                    wait "10"
                }
            }
        }

    /// 「緋蜂のような物体」超速青弾part2 by 白い弾幕くん
    /// [OtakuTwo]_self-1021.xml
    let self_1021 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "「緋蜂のような物体」超速青弾part2 by 白い弾幕くん" {
            defAction "top1" {
                wait "10"
                actionRef "cyclone" [ "0.1+$rand"; " 1" ]
            }

            defAction "top2" {
                actionRef "cyclone" [ "0.1+$rand"; "-1" ]
                wait "10"
            }

            defAction "cyclone" {
                fire {
                    absolute "($rand*360)*$2"
                    refBullet "speed" []
                }

                repeat "11" {
                    fire {
                        sequence "(30)*$2"
                        refBullet "speed" []
                    }
                }

                wait "1"

                repeat "9" {
                    fire {
                        sequence "(30+$1)*$2"
                        refBullet "speed" []
                    }

                    repeat "11" {
                        fire {
                            sequence "(30)*$2"
                            refBullet "speed" []
                        }
                    }

                    wait "1"
                }

                wait "10"

                repeat "39" {
                    fire {
                        sequence "(30+$1*10)*$2"
                        refBullet "speed" []
                    }

                    repeat "11" {
                        fire {
                            sequence "(30)*$2"
                            refBullet "speed" []
                        }
                    }

                    wait "1"

                    repeat "9" {
                        fire {
                            sequence "(30+$1)*$2"
                            refBullet "speed" []
                        }

                        repeat "11" {
                            fire {
                                sequence "(30)*$2"
                                refBullet "speed" []
                            }
                        }

                        wait "1"
                    }

                    wait "10"
                }
            }

            defBullet "speed" { speed "1+$rank*2" }
        }

    /// rRootageより妄想　Part01 by 白い弾幕くん
    /// [OtakuTwo]_self-2010.xml
    let self_2010 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "rRootageより妄想　Part01 by 白い弾幕くん" {
            top {
                actionRef "main" [ "$rand"; " 1"; " 1"; " 1"; " 1" ]
                actionRef "main" [ "$rand"; "-1"; "-1"; "-1"; "-1" ]
                actionRef "main" [ "$rand"; "-1"; "-1"; " 1"; " 1" ]
                actionRef "main" [ "$rand"; " 1"; " 1"; "-1"; "-1" ]
                actionRef "main" [ "$rand"; " 1"; "-1"; "-1"; " 1" ]
                actionRef "main" [ "$rand"; "-1"; " 1"; " 1"; "-1" ]
                actionRef "main" [ "$rand"; "-1"; " 1"; "-1"; " 1" ]
                actionRef "main" [ "$rand"; " 1"; "-1"; " 1"; "-1" ]
            }

            defAction "main" {
                fire {
                    absolute "45"
                    refBullet "cross" [ "$1"; "$2" ]
                }

                fire {
                    sequence "90"
                    refBullet "cross" [ "$1"; "$3" ]
                }

                fire {
                    sequence "90"
                    refBullet "cross" [ "$1"; "$4" ]
                }

                fire {
                    sequence "90"
                    refBullet "cross" [ "$1"; "$5" ]
                }

                wait "200-$rank*100"
            }

            defBullet "cross" {
                speed "1"

                doActs (
                    body {
                        wait "15"
                        changeSpeed "0" "1"

                        fire {
                            absolute "360*$1*$2"
                            speed "1+$rank*$rank*1"
                            plain
                        }

                        fire {
                            sequence "180*$2"
                            speed "1+$rank*$rank*1"
                            plain
                        }

                        repeat "49" {
                            wait "4-$rank*2"

                            fire {
                                sequence "187.7*$2"
                                speed "1+$rank*$rank*1"
                                plain
                            }

                            fire {
                                sequence "180*$2"
                                speed "1+$rank*$rank*1"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }
        }

    /// rRootageより妄想　Part01-ANOTHER by 白い弾幕くん
    /// [OtakuTwo]_self-2011.xml
    let self_2011 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "rRootageより妄想　Part01-ANOTHER by 白い弾幕くん" {
            top {
                actionRef "main" [ "$rand"; " 1"; " 1"; " 1"; " 1" ]
                actionRef "main" [ "$rand"; "-1"; "-1"; "-1"; "-1" ]
                actionRef "main" [ "$rand"; "-1"; "-1"; " 1"; " 1" ]
                actionRef "main" [ "$rand"; " 1"; " 1"; "-1"; "-1" ]
                actionRef "main" [ "$rand"; " 1"; "-1"; "-1"; " 1" ]
                actionRef "main" [ "$rand"; "-1"; " 1"; " 1"; "-1" ]
                actionRef "main" [ "$rand"; "-1"; " 1"; "-1"; " 1" ]
                actionRef "main" [ "$rand"; " 1"; "-1"; " 1"; "-1" ]
            }

            defAction "main" {
                fire {
                    absolute "45"
                    speed "1"
                    refBullet "cross" [ "$1"; "$2" ]
                }

                repeat "1+$rank*2" {
                    fire {
                        sequence "0"
                        speedSeq "0.2"
                        refBullet "cross" [ "$1"; "$2" ]
                    }
                }

                fire {
                    sequence "90"
                    speed "1"
                    refBullet "cross" [ "$1"; "$3" ]
                }

                repeat "1+$rank*2" {
                    fire {
                        sequence "0"
                        speedSeq "0.2"
                        refBullet "cross" [ "$1"; "$3" ]
                    }
                }

                fire {
                    sequence "90"
                    speed "1"
                    refBullet "cross" [ "$1"; "$4" ]
                }

                repeat "1+$rank*2" {
                    fire {
                        sequence "0"
                        speedSeq "0.2"
                        refBullet "cross" [ "$1"; "$4" ]
                    }
                }

                fire {
                    sequence "90"
                    speed "1"
                    refBullet "cross" [ "$1"; "$5" ]
                }

                repeat "1+$rank*2" {
                    fire {
                        sequence "0"
                        speedSeq "0.2"
                        refBullet "cross" [ "$1"; "$5" ]
                    }
                }

                wait "200-$rank*100"
            }

            defBullet "cross" {
                doActs (
                    body {
                        wait "15"
                        changeSpeed "0" "1"

                        fire {
                            absolute "360*$1*$2"
                            speed "1+$rank*$rank*1"
                            plain
                        }

                        fire {
                            sequence "180*$2"
                            speed "1+$rank*$rank*1"
                            plain
                        }

                        repeat "49" {
                            wait "4-$rank*2"

                            fire {
                                sequence "187.7*$2"
                                speed "1+$rank*$rank*1"
                                plain
                            }

                            fire {
                                sequence "180*$2"
                                speed "1+$rank*$rank*1"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }
        }

    /// rRootageより妄想　Part02 by 白い弾幕くん
    /// [OtakuTwo]_self-2020.xml
    let self_2020 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "rRootageより妄想　Part02 by 白い弾幕くん" {
            top {
                changeDirectionAbs "0" "1"
                wait "1"
                changeSpeed "5" "1"
                wait "15"
                changeSpeed "0" "1"

                repeat "45" {
                    fireRef "seed" [ " 1" ]
                    fireRef "seed" [ "-1" ]
                    wait "30"
                }

                wait "450"
            }

            topFireAs "seed" {
                absolute "90*$1"
                speed "(1.5-$rank*0.5)"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "$rand*(30-$rank*15)"

                                repeat "9999" {
                                    fire {
                                        absolute "180"
                                        speed "1"
                                        refBullet "bomb" []
                                    }

                                    wait "30-$rank*15"
                                }
                            }
                        )
                    }
                )
            }

            defBullet "bomb" {
                doActs (
                    body {
                        wait "225"

                        fire {
                            aim "-20+($rand-0.5)*$rank*$rank*$rank*$rank*$rank*$rank*$rank*$rank*20"
                            speed "1.5+$rank*$rank"
                            plain
                        }

                        repeat "2" {
                            fire {
                                sequence "20"
                                speed "1.5+$rank*$rank"
                                plain
                            }
                        }
                    }
                )
            }
        }

    /// おたくツーさん作、自機拘束弾・低速移動型 by 白い弾幕くん
    /// [OtakuTwo]_slow_move.xml
    let slow_move =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "おたくツーさん作、自機拘束弾・低速移動型 by 白い弾幕くん" {
            top {
                fireRef "lr" [ " 90"; "1.5" ]
                fireRef "lr" [ "-90"; "1.5" ]
                actionRef "move" [ "  0"; "0.9" ]
                wait "150"

                repeat "100+100*$rank" {
                    fireRef "bara" [ " 90" ]
                    fireRef "bara" [ "-90" ]
                    wait "10-$rank*5"
                }

                wait "30"
                actionRef "move" [ "180"; "0.9" ]
            }

            defAction "move" {
                changeDirectionAbs "$1" "10"
                wait "12"
                changeSpeed "$2" "50"
                wait "55"
                changeSpeed " 0" "50"
                wait "55"
            }

            topFireAs "bara" {
                absolute "$1"
                speed "1+$rank*5"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "10"

                                fire {
                                    aim "30-$rand*60"
                                    speed "1"
                                    plain
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            topFireAs "lr" {
                absolute "0"
                speed "0"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                actionRef "move" [ "$1"; "$2" ]
                                fireRef "tb" [ "  0"; "0.9" ]
                                fireRef "tb" [ "180"; "3.0" ]
                                vanish
                            }
                        )
                    }
                )
            }

            topFireAs "tb" {
                speed "0"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                actionRef "move" [ "$1"; "$2" ]
                                wait "20"
                                changeDirectionAim "0" "5"
                                wait "10"
                                actionRef "shot" []
                                vanish
                            }
                        )
                    }
                )
            }

            defAction "shot" {
                repeat "500" {
                    changeDirectionAim "0" "10+$rank*10"
                    fireRef "winder" [ " 20-$rank*10" ]
                    fireRef "winder" [ "-20+$rank*10" ]
                    wait "2"
                }

                vanish
            }

            topFireAs "winder" {
                relative "$1"
                speed "8"
                plain
            }
        }
