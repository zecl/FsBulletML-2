namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Strikers1999
[<RequireQualifiedAccess>]
module Strikers1999 =

    /// ストライカーズ1999の花火かも。by 白い弾幕くん
    /// [Strikers1999]_hanabi.xml
    let hanabi =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "ストライカーズ1999の花火かも。by 白い弾幕くん" {
            top {
                repeat "3" {
                    fire {
                        absolute "180"
                        speed "3"
                        refBullet "fastHanabi" []
                    }

                    wait "110-$rank*60"
                }
            }

            defAction "fastFour" {
                fire {
                    sequence "5"
                    speed "2+$rank"
                    plain
                }

                fire {
                    sequence "-10"
                    speed "2+$rank"
                    plain
                }

                fire {
                    sequence "15"
                    speed "1.5+$rank"
                    plain
                }

                fire {
                    sequence "-20"
                    speed "1.5+$rank"
                    plain
                }
            }

            defAction "slowFour" {
                fire {
                    sequence "5"
                    speed "1+$rank*0.8"
                    plain
                }

                fire {
                    sequence "-10"
                    speed "1+$rank*0.8"
                    plain
                }

                fire {
                    sequence "15"
                    speed "0.7+$rank*0.8"
                    plain
                }

                fire {
                    sequence "-20"
                    speed "0.7+$rank*0.8"
                    plain
                }
            }

            defBullet "fastHanabi" {
                doActs (
                    body {
                        wait "15"

                        fire {
                            aim "0"
                            speed "2.5+$rank"
                            plain
                        }

                        actionRef "fastFour" []

                        repeat "16" {
                            fire {
                                sequence "32.5"
                                speed "2.5+$rank"
                                plain
                            }

                            actionRef "fastFour" []
                        }

                        fireRef "slowHanabi" []
                        vanish
                    }
                )
            }

            topFireAs "slowHanabi" {
                ofBullet (
                    bulletAnon {
                        doActs (
                            body {
                                fire {
                                    absolute "0"
                                    speed "1.3+$rank*0.8"
                                    plain
                                }

                                actionRef "slowFour" []

                                repeat "16" {
                                    fire {
                                        sequence "32.5"
                                        speed "1.3+$rank*0.8"
                                        plain
                                    }

                                    actionRef "slowFour" []
                                }

                                vanish
                            }
                        )
                    }
                )
            }
        }
