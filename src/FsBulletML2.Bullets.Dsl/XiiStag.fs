namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Xevious
[<RequireQualifiedAccess>]
module XiiStag =

    /// トゥエルブスタッグ３ボス by 白い弾幕くん
    /// [XII_STAG]_3b.xml
    let b3b =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "トゥエルブスタッグ３ボス by 白い弾幕くん" {
            top {
                actionRef "bara" [ "1" ]
                actionRef "bara" [ "-1" ]
                actionRef "3way" [ "180-55"; "-5" ]
                actionRef "3way" [ "180"; "0" ]
                actionRef "3way" [ "180+55"; "5" ]
                actionRef "roll" [ "180+45"; "1" ]
                actionRef "roll" [ "180-45"; "-1" ]
                actionRef "straight" [ "1" ]
                actionRef "straight" [ "-1" ]
                wait "50"

                repeat "3*$rank" {
                    fire {
                        speedAbs "0"
                        ofBullet (bulletAnon { refActs "fin1" [ "1" ] })
                    }

                    fire {
                        speedAbs "0"
                        ofBullet (bulletAnon { refActs "fin1" [ "-1" ] })
                    }

                    fire { ofBullet (bulletAnon { refActs "white1" [] }) }
                    wait "5*(6+(12*$rank))"
                }

                wait "110"
            }

            defAction "fin1" {
                actionRef "fin2" [ "$1" ]
                vanish
            }

            defAction "fin2" {
                fire {
                    aim "$1*90"
                    speedAbs "1.7+(0.8*$rank)"
                    plain
                }

                wait "2/($rank+0.2)"

                repeat "1+$rank*32" {
                    fire {
                        sequence "-1*$1*((2.3/($rank+0.01))+0.35)"
                        speedAbs "1.7+(0.8*$rank)"
                        plain
                    }

                    wait "2/($rank+0.2)"
                }
            }

            defAction "white1" {
                fireRef "white2" [ "0"; "0.00001"; "0" ]
                fireRef "white2" [ "90"; "1"; "1.5" ]
                fireRef "white2" [ "-90"; "1"; "-1.5" ]
                vanish
            }

            topFireAs "white2" {
                relative "$1"
                speedAbs "$2"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "4"
                                changeSpeedAbs "0.00001" "1"
                                wait "83-(70*$rank)"

                                repeat "6+(12*$rank)" {
                                    fire {
                                        relative "-1*$1+$3"
                                        speed "2.9"
                                        plain
                                    }

                                    wait "5"
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            defAction "bara" {
                fireRef "5c" [ "44*$1"; "4.1"; "4" ]
                fireRef "5c" [ "55.5*$1"; "3.45"; "3" ]
                fireRef "5c" [ "55*$1"; "4.2"; "2" ]
                fireRef "5c" [ "70*$1"; "3"; "0" ]
                fireRef "5c" [ "68*$1"; "3.74"; "1" ]
            }

            topFireAs "5c" {
                absolute "180+$1"
                speed "$2/1.1"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "10"
                                changeSpeedAbs "0" "1"
                                wait "5+($3*5)"

                                repeat "10-(5/($rank+0.001))" {
                                    repeatRef "3" "almond1" []
                                    wait "85-(40*$rank)"
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            defAction "almond1" {
                fire {
                    aim "3.5-(7*$rand)"
                    speedAbs "0+(0.3*$rand)"
                    refBullet "almond2" []
                }

                wait "3"
            }

            defBullet "almond2" { doActs (body { changeSpeedRel "1.8+(0.8*$rank)" "10" }) }

            defAction "3way" {
                fire {
                    absolute "180+$2"
                    speedAbs "3.5"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "10"
                                    changeSpeedAbs "0" "1"
                                    wait "1"

                                    repeat "7+(10*$rank)" {
                                        fireRef "9way" [ "$1+16" ]
                                        fireRef "9way" [ "$1" ]
                                        fireRef "9way" [ "$1-16" ]
                                        wait "25"
                                    }

                                    vanish
                                }
                            )
                        }
                    )
                }
            }

            topFireAs "9way" {
                absolute "$1"
                speedAbs "1.5"
                plain
            }

            defAction "roll" {
                fire {
                    absolute "180+(11*$2)"
                    speedAbs "10"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "5"
                                    changeSpeedAbs "0" "1"
                                    wait "1"

                                    fire {
                                        absolute "$1"
                                        speedAbs "1.5"
                                        plain
                                    }

                                    fire {
                                        absolute "$1+(30*$2)"
                                        speedAbs "1.5"
                                        plain
                                    }

                                    wait "15"

                                    repeat "11+(17*$rank)" {
                                        fire {
                                            sequence "-35*$2"
                                            speedAbs "1.5"
                                            plain
                                        }

                                        fire {
                                            sequence "30*$2"
                                            speedAbs "1.5"
                                            plain
                                        }

                                        wait "15"
                                    }

                                    vanish
                                }
                            )
                        }
                    )
                }
            }

            defAction "straight" {
                fire {
                    absolute "180+(82*$1)"
                    speedAbs "2.7"

                    ofBullet (
                        bulletAnon {
                            doActs (
                                body {
                                    wait "13"
                                    changeSpeedAbs "0" "1"
                                    wait "1"
                                    repeatRef "3+(5*$rank)" "fall" []
                                    vanish
                                }
                            )
                        }
                    )
                }
            }

            defAction "fall" {
                repeat "7" {
                    fire {
                        absolute "180"
                        speedAbs "2.9"
                        plain
                    }

                    wait "5"
                }

                wait "15"
            }
        }
