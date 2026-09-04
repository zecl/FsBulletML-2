namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// Noiz2sa
[<RequireQualifiedAccess>]
module Noiz2sa =

  /// Noiz2saより、88way。 by 白い弾幕くん
  /// [Noiz2sa]_88way.xml
  let b88way =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Noiz2saより、88way。 by 白い弾幕くん" {
        top {
            fire {
                absolute "180"
                speed "0.7"
                ofBullet (bulletAnon {
                    refActs "main" []
                })
            }
            wait "200"
        }
        defAction "main" {
            repeat "6+$rank*10" {
                fire {
                    sequence "360/(6+$rank*10)"
                    refBullet "16way" []
                }
                wait "100/(6+$rank*10)"
            }
            vanish
        }
        defBullet "16way" {
            speed "$rand+1"
            doActs (body {
                wait "20+$rand*40"
                repeat "16" {
                    fire {
                        sequence "22.5"
                        ofBullet (bulletAnon {
                            speed "1.7"
                        })
                    }
                }
                vanish
            })
        }
    }

  /// Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん
  /// [Noiz2sa]_bit.xml
  let bit =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん" {
        top {
            repeat "4+$rank*10" {
                fire {
                    refBullet "bit" []
                }
            }
            wait "180"
        }
        defBullet "bit" {
            speed "0"
            doActs (body {
                repeat "4" {
                    changeDirectionAbs "$rand*360" "20"
                    changeSpeed "2" "20"
                    wait "20"
                    changeSpeed "0" "20"
                    wait "20"
                    fire {
                        refBullet "seed" []
                    }
                    wait "0"
                }
                vanish
            })
        }
        defBullet "seed" {
            speed "0"
            doActs (body {
                fire {
                    aim "$rand*10-5"
                    refBullet "nrm" []
                }
                repeat "5" {
                    wait "6"
                    fire {
                        sequence "0"
                        refBullet "nrm" []
                    }
                }
                vanish
            })
        }
        defBullet "nrm" {
            speed "2"
        }
    }

  /// Noiz2saより、回る棒。by 白い弾幕くん
  /// [Noiz2sa]_rollbar.xml
  let rollbar =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "Noiz2saより、回る棒。by 白い弾幕くん" {
        defAction "top1" {
            fire {
                aim "$rand*50"
                speed "0"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            actionRef "main" []
        }
        defAction "top2" {
            fire {
                aim "180+$rank*50"
                speed "0"
                ofBullet (bulletAnon {
                    doActs (body {
                        vanish
                    })
                })
            }
            actionRef "main" []
        }
        defAction "main" {
            repeat "15+$rank*10" {
                fire {
                    sequence "180"
                    refBullet "firebar" ["90"]
                }
                fire {
                    sequence "160"
                    refBullet "firebar" ["-90"]
                }
                wait "200/(15+$rank*10)"
            }
        }
        defBullet "firebar" {
            speed "10"
            doActs (body {
                repeat "5" {
                    wait "1"
                    fire {
                        relative "$1"
                        ofBullet (bulletAnon {
                            speed "1.5"
                        })
                    }
                }
                vanish
            })
        }
    }
