namespace FsBulletML2.Bullets.Dsl.PlayerBullet

open FsBulletML2.Dsl

/// その他
[<RequireQualifiedAccess>]
module PlayerBullet =

    /// 2Way Left
    let b2wayLeftBullet =
        vertical "2Way Left" {
            top {
                fire {
                    absolute "-10"
                    speed "20"
                    plain
                }

                fire {
                    absolute "0"
                    speed "20"
                    plain
                }
            }
        }

    /// 2Way Right
    let b2wayRightBullet =
        vertical "2Way Right" {
            top {
                fire {
                    absolute "10"
                    speed "20"
                    plain
                }

                fire {
                    absolute "0"
                    speed "20"
                    plain
                }
            }
        }

    /// ホーミング弾
    let homing =
        horizontal "ホーミング弾" {
            top {
                repeat "2" {
                    fire {
                        dir "(-30+$rand*120)"
                        refBullet "hmgLsr" []
                    }

                    repeat "5" {
                        wait "1"

                        fire {
                            sequence "0"
                            refBullet "hmgLsr" []
                        }
                    }

                    wait "10"
                }
            }

            defBullet "hmgLsr" {
                speed "2"

                doActs (
                    body {
                        changeSpeed "0.3" "40"
                        wait "100"
                        changeSpeed "5" "90"
                    }
                )

                doActs (
                    body {
                        repeat "9999" {
                            changeDirectionAim "0" "40-$rank*20"
                            wait "5"
                        }
                    }
                )
            }
        }
