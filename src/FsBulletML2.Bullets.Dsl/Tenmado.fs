namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Tenmado
[<RequireQualifiedAccess>]
module Tenmado =

    /// tenmadoより、三面ボス「Disconnection」by 白い弾幕くん
    /// [tenmado]_3_boss_2.xml
    let b3_boss_2 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "tenmadoより、三面ボス「Disconnection」by 白い弾幕くん" {
            top {
                fire {
                    absolute "240"
                    speed "0.6"
                    refBullet "bitlaser" [ "60"; "10" ]
                }

                fire {
                    absolute "-240"
                    speed "0.6"
                    refBullet "bitlaser" [ "-60"; "-10" ]
                }

                fire {
                    absolute "240"
                    speed "0.6"
                    refBullet "bitaim" [ "60"; "10"; "35" ]
                }

                fire {
                    absolute "-240"
                    speed "0.6"
                    refBullet "bitaim" [ "-60"; "-10"; "5" ]
                }

                wait "60"

                repeat "600 / (6.0 - 4.0 * $rank)" {
                    fire {
                        aim "-30 + 60 * $rand"
                        speed "1.3+$rank*0.7"
                        plain
                    }

                    wait "6.0 - 4.0 * $rank"
                }

                wait "90"
            }

            defBullet "bitlaser" {
                doActs (
                    body {
                        wait "120"
                        changeSpeed "0" "1"
                        wait "30"
                        changeDirectionAbs "180" "1"
                        changeSpeed "0.6" "1"
                        wait "90"

                        fire {
                            absolute "$1"
                            speed "0.1"
                            refBullet "laser" [ "0.3" ]
                        }

                        repeat "6" {
                            wait "20"

                            fire {
                                sequence "$2"
                                speed "0.1"
                                refBullet "laser" [ "0.3" ]
                            }
                        }

                        changeSpeed "0" "1"
                        wait "30"
                        changeDirectionAbs "0" "1"
                        changeSpeed "0.8" "1"
                        wait "10"

                        fire {
                            absolute "$1 + 3.5 * $2"
                            speed "0.1"
                            refBullet "laser" [ "1.5" ]
                        }

                        repeat "4" {
                            wait "20"

                            fire {
                                sequence "-$2"
                                speed "0.1"
                                refBullet "laser" [ "1.5" ]
                            }
                        }

                        vanish
                    }
                )
            }

            defBullet "bitaim" {
                doActs (
                    body {
                        wait "120"
                        changeSpeed "0" "1"
                        wait "30"
                        changeDirectionAbs "180" "1"
                        changeSpeed "0.6" "1"
                        wait "40 - $3"

                        repeat "2" {
                            wait "70"

                            fire {
                                aim "-10 + 20 * $rand"
                                speed "0.6"
                                plain
                            }
                        }

                        wait "30 + $3"
                        changeSpeed "0" "1"
                        wait "30"
                        changeDirectionAbs "0" "1"
                        changeSpeed "0.8" "1"
                        wait "40 - $3"

                        fire {
                            aim "-10 + 20 * $rand"
                            speed "0.6"
                            plain
                        }

                        wait "50 + $3"
                        vanish
                    }
                )
            }

            defBullet "laser" {
                doActs (
                    body {
                        fire {
                            relative "0"
                            speed "$1"
                            plain
                        }

                        fire {
                            relative "0"
                            speed "$1 + 0.01"
                            plain
                        }

                        fire {
                            relative "0"
                            speed "$1 + 0.02"
                            plain
                        }

                        fire {
                            relative "0"
                            speed "$1 + 0.03"
                            plain
                        }

                        fire {
                            relative "0"
                            speed "$1 + 0.04"
                            plain
                        }

                        fire {
                            relative "0"
                            speed "$1 + 0.05"
                            plain
                        }

                        vanish
                    }
                )
            }
        }

    /// tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん
    /// [tenmado]_5_boss_1.xml
    let b5_boss_1 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん" {
            top {
                fire {
                    absolute "0"
                    speed "0"
                    refBullet "random" []
                }

                repeat "8" {
                    fire {
                        absolute "90"
                        speed "0.5"
                        refBullet "surprise" []
                    }

                    fire {
                        absolute "270"
                        speed "0.5"
                        refBullet "surprise" []
                    }

                    wait "100"
                }

                wait "20"
            }

            defBullet "surprise" {
                doActs (
                    body {
                        wait "100"
                        changeSpeed "0" "1"

                        repeat "5" {
                            repeat "30" {
                                fire {
                                    aim "3.5"
                                    speed "15+$rand*15"
                                    ofBullet (bulletAnon { doActs (body { () }) })
                                }

                                fire {
                                    aim "-3.5"
                                    speed "15+$rand*15"
                                    ofBullet (bulletAnon { doActs (body { () }) })
                                }
                            }

                            wait "1"
                        }

                        vanish
                    }
                )
            }

            defBullet "random" {
                doActs (
                    body {
                        wait "200"

                        repeat "6000/(130 - 100 * $rank) " {
                            fire {
                                aim "-22 + 44 * $rand"
                                speed "1.6 + 1.0 * $rand"
                                plain
                            }

                            wait "0.1 * (130 - 100 * $rank)"
                        }

                        vanish
                    }
                )
            }
        }

    /// tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん
    /// [tenmado]_5_boss_3.xml
    let b5_boss_3 =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん" {
            top {
                fire {
                    absolute "0"
                    speed "0"
                    refBullet "stardust" []
                }

                wait "120"

                repeat "840/(120 - 100 * $rank)" {
                    fire {
                        aim "0"
                        speed "1"
                        refBullet "laser" [ "2" ]
                    }

                    fire {
                        aim "0"
                        speed "1"
                        refBullet "laser" [ "2.05" ]
                    }

                    fire {
                        aim "0"
                        speed "1"
                        refBullet "laser" [ "2.1" ]
                    }

                    wait "0.5 * (120 - 100 * $rank)"
                }

                wait "60"
            }

            defBullet "stardust" {
                doActs (
                    body {
                        repeat "5+$rank*10" {
                            fire {
                                absolute "135 + 90 * $rand"
                                speed "0.3 + 1.7 * $rand"
                                refBullet "stardust2" [ "60"; "1.2"; "0.8" ]
                            }

                            fire {
                                absolute "135 + 90 * $rand"
                                speed "0.3 + 1.7 * $rand"
                                refBullet "stardust2" [ "68"; "0.8"; "1.2" ]
                            }

                            fire {
                                absolute "135 + 90 * $rand"
                                speed "0.3 + 1.7 * $rand"
                                refBullet "stardust2" [ "76"; "1.2"; "0.8" ]
                            }

                            fire {
                                absolute "135 + 90 * $rand"
                                speed "0.3 + 1.7 * $rand"
                                refBullet "stardust2" [ "84"; "0.8"; "1.2" ]
                            }

                            wait "960/(10+$rank*20)"
                        }

                        vanish
                    }
                )
            }

            defBullet "stardust2" {
                doActs (
                    body {
                        wait "$1"

                        fire {
                            absolute "0"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "30"
                            speed "$3"
                            plain
                        }

                        fire {
                            absolute "60"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "90"
                            speed "$3"
                            plain
                        }

                        fire {
                            absolute "120"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "150"
                            speed "$3"
                            plain
                        }

                        fire {
                            absolute "180"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "210"
                            speed "$3"
                            plain
                        }

                        fire {
                            absolute "240"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "270"
                            speed "$3"
                            plain
                        }

                        fire {
                            absolute "300"
                            speed "$2"
                            plain
                        }

                        fire {
                            absolute "330"
                            speed "$3"
                            plain
                        }

                        vanish
                    }
                )
            }

            defBullet "laser" { doActs (body { changeSpeed "$1" "1" }) }
        }
