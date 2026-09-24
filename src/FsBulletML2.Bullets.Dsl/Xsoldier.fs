namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun

open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Xsoldier
[<RequireQualifiedAccess>]
module Xsoldier =

    /// XSoldierの8面ボスの主砲 by 白い弾幕くん
    /// [xsoldier]_8_boss_main.xml
    let b8_boss_main =
        createBulletmlInfo
        <| verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "XSoldierの8面ボスの主砲 by 白い弾幕くん" {
            top {
                fire {
                    absolute "0"
                    speed "0"
                    refBullet "dummy" [ "90" ]
                }

                fire {
                    absolute "0"
                    speed "0"
                    refBullet "dummy" [ "270" ]
                }

                wait "100 - $rank * 90"

                fire {
                    absolute "0"
                    speed "0"
                    refBullet "allway" [ "0"; "1.5" ]
                }

                wait "5"

                fire {
                    absolute "0"
                    speed "0"
                    refBullet "allway" [ "2.5"; "1.8" ]
                }

                wait "20"
            }

            defBullet "dummy" {
                doActs (
                    body {
                        fire {
                            absolute "$1"
                            speed "0"
                            refBullet "bit" []
                        }

                        fire {
                            absolute "$1"
                            speed "0.5"
                            refBullet "bit" []
                        }

                        fire {
                            absolute "$1"
                            speed "1.0"
                            refBullet "bit" []
                        }

                        fire {
                            absolute "$1"
                            speed "1.5"
                            refBullet "bit" []
                        }

                        vanish
                    }
                )
            }

            defBullet "allway" {
                doActs (
                    body {
                        fire {
                            absolute "$1"
                            speed "$2"
                            plain
                        }

                        repeat "71" {
                            fire {
                                sequence "5"
                                speed "$2"
                                plain
                            }
                        }

                        vanish
                    }
                )
            }

            defBullet "bit" {
                doActs (
                    body {
                        wait "20"
                        changeSpeed "0" "1"
                        wait "105 - $rank * 90"

                        repeat "20" {
                            fire {
                                absolute "180"
                                speed "3"
                                plain
                            }

                            fire {
                                absolute "180"
                                speed "3.5"
                                plain
                            }

                            fire {
                                absolute "180"
                                speed "4"
                                plain
                            }

                            wait "1"
                        }

                        vanish
                    }
                )
            }
        }
