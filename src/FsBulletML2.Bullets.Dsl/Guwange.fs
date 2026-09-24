namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Guwange
[<RequireQualifiedAccess>]
module Guwange =

    /// ぐわんげ、二面ボス by 白い弾幕くん
    /// [Guwange]_round_2_boss_circle_fire.xml
    let round_2_boss_circle_fire =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ぐわんげ、二面ボス by 白い弾幕くん" {
            topFireAs "circle" {
                sequence "$1"
                speed "6"

                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                wait "3"

                                fire {
                                    absolute "$2"
                                    speed "1.5+$rank"
                                    plain
                                }

                                vanish
                            }
                        )
                    }
                )
            }

            defAction "fireCircle" { repeat "18" { fireRef "circle" [ "20"; "$1" ] } }

            top {
                actionRef "fireCircle" [ "180-45+90*$rand" ]
                wait "10"
            }
        }

    /// ぐわんげ、三面ボス by 白い弾幕くん
    /// [Guwange]_round_3_boss_fast_3way.xml
    let round_3_boss_fast_3way =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ぐわんげ、三面ボス by 白い弾幕くん" {
            top {
                repeat "10+$rank*50" {
                    fire {
                        dir "$rand*360"
                        speed "5"
                        refBullet "seed" [ "5+$rand*10" ]
                    }

                    wait "20-$rank*10"
                }
            }

            defBullet "seed" {
                doActs (
                    body {
                        changeSpeed "0" "$1"
                        wait "$1"

                        fire {
                            aim "-20"
                            refBullet "3way" []
                        }

                        repeat "2" {
                            fire {
                                sequence "20"
                                refBullet "3way" []
                            }
                        }

                        wait "6"

                        repeat "2" {
                            fire {
                                sequence "0"
                                speedSeq "-0.1"
                                refBullet "3way" []
                            }

                            repeat "2" {
                                fire {
                                    sequence "-20"
                                    speedSeq "0"
                                    refBullet "3way" []
                                }
                            }

                            wait "6"

                            fire {
                                sequence "0"
                                speedSeq "-0.1"
                                refBullet "3way" []
                            }

                            repeat "2" {
                                fire {
                                    sequence "20"
                                    speedSeq "0"
                                    refBullet "3way" []
                                }
                            }

                            wait "6"
                        }

                        vanish
                    }
                )
            }

            defBullet "3way" { speed "3" }
        }

    /// ぐわんげ、四面ボス by 白い弾幕くん
    /// [Guwange]_round_4_boss_eye_ball.xml
    let round_4_boss_eye_ball =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ぐわんげ、四面ボス by 白い弾幕くん" {
            top {
                repeat "10+$rank*10" {
                    fire {
                        dir "$rand*360"
                        refBullet "eye" []
                    }

                    wait "30"
                }

                wait "120"
            }

            defBullet "eye" {
                speed "0"

                doActs (
                    body {
                        changeSpeed "10" "400"
                        changeDirectionSeq "$rand*5-2" "9999"

                        repeat "9999" {
                            fire {
                                relative "0"
                                refBullet "shadow" []
                            }

                            wait "4"
                        }
                    }
                )
            }

            defBullet "shadow" {
                speed "0.1"

                doActs (
                    body {
                        wait "20"

                        fire {
                            relative "90"
                            speed "0.6"
                            plain
                        }

                        fire {
                            relative "-90"
                            speed "0.6"
                            plain
                        }

                        vanish
                    }
                )
            }
        }
