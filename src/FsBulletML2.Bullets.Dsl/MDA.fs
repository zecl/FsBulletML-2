namespace FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun
open FsBulletML2
open FsBulletML2.Dsl

/// 白い弾幕くんより
/// MDA
[<RequireQualifiedAccess>]
module MAD =

  /// 紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん
  /// [MDA]_2f.xml
  let b2f =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん" {
        top {
            fireRef "seed" ["0"; "57"; "0.8"; "0.8"; "0"; "-0.8"; "0"]
            fireRef "seed" ["270"; "206"; "1.73"; "0"; "-1.2"; "0"; "1.2"]
            fireRef "seed2" []
            wait "3*(260+(60-($rank*60)))"
        }
        topFireAs "seed" {
            absolute "$2"
            speedAbs "$3"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "90"
                    fireRef "leaf" ["1"; "$1"; "$4"; "$5"; "$6"; "$7"]
                    fireRef "leaf" ["-1"; "$1"; "$4"; "$5"; "$6"; "$7"]
                    vanish
                })
            })
        }
        topFireAs "leaf" {
            absolute "50"
            speedAbs "0"
            ofBullet (bulletAnon {
                doActs (body {
                    fire {
                        absolute "$2"
                        speed "5.1"
                        refBullet "curve" ["$1"]
                    }
                    actionRef "move" ["35"; "$1"; "$1"]
                    actionRef "move" ["120"; "$1/2"; "$1"]
                    actionRef "move" ["45"; "0"; "$1"]
                    actionRef "move" ["90"; "$3/2"; "$1"]
                    actionRef "move" ["60-($rank*60)"; "0"; "$1"]
                    actionRef "move" ["120"; "$4*3/8"; "$1"]
                    actionRef "move" ["60-($rank*60)"; "0"; "$1"]
                    actionRef "move" ["90"; "$5/2"; "$1"]
                    actionRef "move" ["60-($rank*60)"; "0"; "$1"]
                    actionRef "move" ["120"; "$6*3/8"; "$1"]
                    actionRef "move" ["45"; "0"; "$1"]
                    vanish
                })
            })
        }
        defAction "move" {
            repeat "$1" {
                fire {
                    sequence "$2"
                    speed "5.1"
                    refBullet "curve" ["$3"]
                }
                wait "1"
            }
        }
        defBullet "curve" {
            doActs (body {
                changeDirectionRel "$1*85" "9-($rank*5)"
            })
        }
        topFireAs "seed2" {
            absolute "131.5"
            speedAbs "0.05"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "90"
                    changeSpeedAbs "0" "1"
                    repeat "10" {
                        fire {
                            aim "0"
                            speed "1.4+(0.4*$rank*$rank)"
                            plain
                        }
                        fire {
                            aim "3*$rand*$rank"
                            speed "1.4+(0.4*$rand*$rank*$rank)"
                            plain
                        }
                        fire {
                            aim "-3*$rand*$rank"
                            speed "1.4+(0.4*$rand*$rank*$rank)"
                            plain
                        }
                        wait "3*(260+(60-($rank*60)))/10"
                    }
                    vanish
                })
            })
        }
    }

  /// 紫月飴さんのオリジナル、花。by 白い弾幕くん
  /// [MDA]_10flower_2.xml
  let b10flower_2 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、花。by 白い弾幕くん" {
        top {
            fire {
                absolute "180"
                speed "5"
                refBullet "seed" ["36.1"; "144"]
            }
            fire {
                absolute "180"
                speed "5"
                refBullet "seed" ["-36.4"; "3.4+144"]
            }
            wait "164+316*$rank"
        }
        defBullet "seed" {
            doActs (body {
                changeSpeed "0" "4"
                wait "4"
                fire {
                    absolute "$2"
                    speed "1.1"
                    refBullet "dummy" []
                }
                repeat "21+79*$rank" {
                    repeat "10" {
                        fire {
                            sequence "$1"
                            speed "1.1"
                            plain
                        }
                    }
                    wait "4"
                }
                vanish
            })
        }
        defBullet "dummy" {
            doActs (body {
                vanish
            })
        }
    }

  /// 紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん
  /// [MDA]_14b_2-3w.xml
  let b14b_2_3w =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん" {
        top {
            fireRef "seed_a" []
            repeatRef "35+$rank*21" "seed_b" []
            wait "110"
        }
        defAction "seed_b" {
            fire {
                sequence "7"
                speedAbs "1.4"
                refBullet "shoot" []
            }
            repeat "$rank*6" {
                fire {
                    sequence "0"
                    speedSeq "-0.14"
                    refBullet "shoot" []
                }
            }
            fire {
                sequence "180"
                speedAbs "1.4"
                refBullet "shoot" []
            }
            repeat "$rank*6" {
                fire {
                    sequence "0"
                    speedSeq "-0.14"
                    refBullet "shoot" []
                }
            }
            wait "11"
        }
        defBullet "shoot" {
            doActs (body {
                wait "18"
                fire {
                    relative "0"
                    speed "1.4"
                    plain
                }
                repeat "7-1" {
                    fire {
                        sequence "360/7"
                        speed "1.4"
                        plain
                    }
                }
                vanish
            })
        }
        topFireAs "seed_a" {
            absolute "180"
            speedAbs "0"
            ofBullet (bulletAnon {
                doActs (body {
                    repeat "3+$rank*7" {
                        fireRef "fire1" ["1"; "1"]
                        fireRef "fire1" ["-1"; "1"]
                        fireRef "fire1" ["0.5"; "-2"]
                        fireRef "fire1" ["-0.5"; "-2"]
                        wait "63"
                    }
                    vanish
                })
            })
        }
        topFireAs "fire1" {
            absolute "$1*90"
            speedAbs "2.5"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "10"
                    changeSpeedAbs "0.5" "1"
                    wait "1"
                    repeat "4+$rank*5" {
                        fireRef "fire2" ["$2*$1"; "-1"]
                        fireRef "fire2" ["$2*$1"; "1"]
                        wait "3"
                    }
                    vanish
                })
            })
        }
        topFireAs "fire2" {
            aim "($1+$2)*7"
            speedAbs "2.2+$rank*1"
            ofBullet (bullet "dummy" {
                doActs (body {
                    ()
                })
            })
        }
    }

  /// 紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん
  /// [MDA]_75l-42.xml
  let b75l_42 =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん" {
        top {
            fire {
                ofBullet (bulletAnon {
                    absolute "120"
                    speed "9.2-$rank*4"
                    refActs "right" []
                })
            }
            fire {
                ofBullet (bulletAnon {
                    absolute "240"
                    speed "9.2-$rank*4"
                    refActs "left" []
                })
            }
            wait "40"
            fire {
                ofBullet (bulletAnon {
                    absolute "240"
                    speed "9.2-$rank*4"
                    refActs "right" []
                })
            }
            fire {
                ofBullet (bulletAnon {
                    absolute "120"
                    speed "9.2-$rank*4"
                    refActs "left" []
                })
            }
            wait "100"
        }
        defAction "right" {
            repeat "32" {
                fireRef "shoot" ["0+1"; "1.4"]
                fireRef "shoot" ["60+1"; "0.7"]
                fireRef "shoot" ["300+1"; "2.1"]
                wait "1"
            }
        }
        defAction "left" {
            repeat "32" {
                fireRef "shoot" ["360-1"; "1.4"]
                fireRef "shoot" ["300-1"; "0.7"]
                fireRef "shoot" ["60-1"; "2.1"]
                wait "1"
            }
        }
        topFireAs "shoot" {
            sequence "$1"
            speed "$2"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "45"
                    accel "120" {
                        vertical "4.2"
                    }
                })
            })
        }
    }

  /// 紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん
  /// [MDA]_acc_n_dec.xml
  let acc_n_dec =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん" {
        top {
            repeat "5+(20*$rank)" {
                fireRef "seed" ["90"]
                fireRef "seed" ["270"]
                wait "55-($rank*30)"
            }
        }
        topFireAs "seed" {
            absolute "$1"
            speed "2.0"
            ofBullet (bulletAnon {
                doActs (body {
                    changeSpeed "0" "20"
                    wait "20"
                    actionRef "way" []
                    vanish
                })
            })
        }
        defAction "way" {
            fire {
                aim "$rand*60-30-70"
                speed "4.2"
                refBullet "br" []
            }
            repeat "7" {
                fire {
                    sequence "8.5"
                    speed "1.05"
                    refBullet "ac" []
                }
                fire {
                    sequence "8.5"
                    speed "4.2"
                    refBullet "br" []
                }
            }
        }
        defBullet "br" {
            doActs (body {
                changeSpeed "1.05" "25"
            })
        }
        defBullet "ac" {
            doActs (body {
                changeSpeed "8.4" "150"
                wait "9999"
                fire {
                    plain
                }
            })
        }
    }

  /// 紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん
  /// [MDA]_circular.xml
  let circular =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん" {
        top {
            repeat "$rank*10" {
                fireRef "seed" ["90"; "2"; "355"]
                fireRef "seed" ["270"; "358"; "5"]
                fireRef "aimbl" []
                wait "20"
            }
            repeat "9" {
                fireRef "aimbl" []
                wait "20"
            }
        }
        topFireAs "seed" {
            absolute "$1"
            speed "2.8"
            refBullet "roll" ["$2"; "$3"]
        }
        defBullet "roll" {
            doActs (body {
                changeDirectionSeq "$1" "9999"
                actionRef "shoot" ["$2"]
                vanish
            })
        }
        defAction "shoot" {
            repeat "22" {
                fire {
                    sequence "$1"
                    speed "1.4"
                    plain
                }
                wait "4+$rand*8"
            }
        }
        topFireAs "aimbl" {
            aim "0"
            speed "2.8"
            plain
        }
    }

  /// 紫月飴さんのオリジナル、四方から。 by 白い弾幕くん
  /// [MDA]_circular_model.xml
  let circular_model =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、四方から。 by 白い弾幕くん" {
        top {
            fireRef "seed" ["90"; "2"; "355"]
            fireRef "seed" ["270"; "358"; "5"]
            wait "380-$rank*200"
        }
        topFireAs "seed" {
            absolute "$1"
            speed "2.8"
            refBullet "roll" ["$2"; "$3"]
        }
        defBullet "roll" {
            doActs (body {
                changeDirectionSeq "$1" "9999"
                actionRef "shoot" ["$2"]
                vanish
            })
        }
        defAction "shoot" {
            repeat "22*8" {
                fire {
                    sequence "$1"
                    speed "0.4+$rank"
                    plain
                }
                wait "1"
            }
        }
    }

  /// 紫月飴さんのオリジナル、春っぽい by 白い弾幕くん
  /// [MDA]_circular_sun.xml
  let circular_sun =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、春っぽい by 白い弾幕くん" {
        top {
            changeSpeedAbs "0.75" "1"
            changeDirectionAbs "90" "1"
            wait "1"
            changeDirectionSeq "0.7" "514"
            wait "2"
            repeat "32" {
                actionRef "shoot" []
                wait "16"
            }
            changeSpeedAbs "0" "1"
            wait "120"
        }
        defAction "shoot" {
            repeat "1+(63*$rank)" {
                fire {
                    sequence "360/(1+(63*$rank))"
                    speed "1.28+(0.08*$rand)"
                    refBullet "curve" []
                }
            }
        }
        defBullet "curve" {
            doActs (body {
                changeDirectionSeq "1.25-(1.6*$rand)" "360"
                wait "360"
                vanish
            })
        }
    }

  /// 紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん
  /// [MDA]_double_w.xml
  let double_w =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん" {
        top {
            actionRef "seed" ["0.31"]
            actionRef "seed" ["0.00"]
            wait "30"
        }
        defAction "seed" {
            repeat "($rank*$rank*50+21)*(2-$1)" {
                fireRef "shoot1" ["$1"]
                repeat "10*(1+$1)" {
                    fireRef "shoot2" []
                }
                wait "1"
            }
            wait "60"
        }
        topFireAs "shoot1" {
            sequence "41"
            speedAbs "1.0-$1"
            plain
        }
        topFireAs "shoot2" {
            sequence "-19"
            speedSeq "0.1"
            plain
        }
    }

  /// 紫月飴さんのオリジナル、袋詰め by 白い弾幕くん
  /// [MDA]_fukuro.xml
  let fukuro =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、袋詰め by 白い弾幕くん" {
        top {
            changeDirectionAbs "180" "1"
            changeSpeed "1" "1"
            wait "30"
            changeSpeed "0" "1"
            repeat "$rank*17+1" {
                fireRef "seed" ["3"; "$rank*18+1"]
                fireRef "seed" ["2"; "$rank*18+1"]
                fireRef "seed" ["1"; "$rank*18+1"]
                wait "10"
            }
            wait "(3*($rank*18*10))"
        }
        topFireAs "seed" {
            sequence "360/($2*3)"
            speedAbs "1.75*$1"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "10"
                    changeSpeedAbs "0" "10"
                    wait "(($1-1)*($2*10))+30"
                    actionRef "n_way" []
                    vanish
                })
            })
        }
        defAction "n_way" {
            fireRef "curve" ["2.00"; "60"]
            fireRef "curve" ["2.00"; "-60"]
            fireRef "curve" ["1.64"; "52.5"]
            fireRef "curve" ["1.64"; "-52.5"]
            fireRef "curve" ["1.41"; "45"]
            fireRef "curve" ["1.41"; "-45"]
            fireRef "curve" ["1.16"; "30"]
            fireRef "curve" ["1.16"; "-30"]
            fireRef "curve" ["1.04"; "15"]
            fireRef "curve" ["1.04"; "-15"]
            fireRef "curve" ["1.00"; "0"]
        }
        topFireAs "curve" {
            aim "$2"
            speedAbs "$1*2.0"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "5"
                    changeDirectionAim "0" "5"
                })
            })
        }
    }

  /// 紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん
  /// [MDA]_gnnnyari.xml
  let gnnnyari =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん" {
        top {
            actionRef "seed" []
            wait "120"
        }
        defAction "seed" {
            fire {
                absolute "180"
                speed "7.5"
                refBullet "shoot" []
            }
            wait "2"
            repeat "30+$rank*80" {
                fire {
                    sequence "27"
                    speed "7.5"
                    refBullet "shoot" []
                }
                wait "2"
            }
        }
        defBullet "shoot" {
            doActs (body {
                fire {
                    relative "0"
                    speed "1.0+0.4*$rank"
                    refBullet "dummy" []
                }
                repeat "11" {
                    fire {
                        sequence "30"
                        speed "1.0+0.4*$rank"
                        plain
                    }
                }
                vanish
            })
        }
        defBullet "dummy" {
            doActs (body {
                ()
            })
        }
    }

  /// 紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん
  /// [MDA]_mojya.xml
  let mojya =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん" {
        top {
            repeat "15+25*$rank" {
                fireRef "first" ["15"]
                fireRef "first" ["153"]
                wait "3"
            }
            wait "240"
        }
        topFireAs "first" {
            sequence "$1"
            speed "0.54"
            refBullet "second" []
        }
        defBullet "second" {
            doActs (body {
                wait "60"
                fireRef "third" ["21+$rand*84"]
                fireRef "third" ["-21-$rand*84"]
                fireRef "third" ["7+$rand*28"]
                fireRef "third" ["-7-$rand*28"]
                fireRef "third" ["$rand*14"]
                fireRef "third" ["$rand*(-14)"]
                fireRef "third" ["0"]
                vanish
            })
        }
        topFireAs "third" {
            aim "$1"
            speed "0.4+$rand*1.4"
            plain
        }
    }

  /// 紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん
  /// [MDA]_mossari.xml
  let mossari =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん" {
        top {
            actionRef "seed" ["-2"; "0"]
            actionRef "seed" ["25"; "10"]
            actionRef "seed" ["41"; "-10"]
            actionRef "center" []
            wait "180"
        }
        defAction "seed" {
            fire {
                absolute "180+$1"
                speedAbs "$1/4-2"
                ofBullet (bulletAnon {
                    refActs "shoot" ["-1*$2"]
                })
            }
            fire {
                absolute "180-$1"
                speedAbs "$1/4-2"
                ofBullet (bulletAnon {
                    refActs "shoot" ["$2"]
                })
            }
        }
        defAction "shoot" {
            wait "9"
            changeSpeedAbs "0" "4"
            wait "4"
            repeat "10+($rank*30)" {
                fireRef "shoot2" ["0+($rand*30)"; "$1"]
                fireRef "shoot2" ["0-($rand*30)"; "$1"]
                wait "24-($rand*12)"
            }
            vanish
        }
        topFireAs "shoot2" {
            aim "$1+$2"
            speedAbs "0.6"
            plain
        }
        defAction "center" {
            wait "10"
            repeat "12+($rank*20)" {
                actionRef "center2" []
                repeatRef "7-1" "center3" []
            }
        }
        defAction "center2" {
            fire {
                aim "-60"
                speedAbs "0.6"
                plain
            }
            repeat "12" {
                fire {
                    sequence "10"
                    speedAbs "0.6"
                    plain
                }
            }
            wait "4"
        }
        defAction "center3" {
            actionRef "wind" ["46"]
            actionRef "wind" ["16"]
            actionRef "wind" ["47.5"]
            actionRef "wind" ["15"]
        }
        defAction "wind" {
            fire {
                absolute "180+$1"
                speedAbs "2.7"
                plain
            }
            fire {
                absolute "180-$1"
                speedAbs "2.7"
                plain
            }
            wait "1"
        }
    }

  /// 紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん
  /// [MDA]_wind_cl.xml
  let wind_cl =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん" {
        top {
            fireRef "side" ["120"]
            fireRef "side" ["240"]
            wait "31"
            repeat "5+$rank*20" {
                fire {
                    refBullet "center" []
                }
                wait "30"
            }
            wait "100"
        }
        topFireAs "side" {
            absolute "$1"
            speed "18.6"
            ofBullet (bulletAnon {
                doActs (body {
                    wait "1"
                    changeSpeed "0.02" "2"
                    wait "30"
                    changeDirection "0" "1"
                    repeat "77+$rank*306" {
                        wait "2"
                        changeDirection "0" "30"
                        fireRef "3way" ["0"]
                        fireRef "3way" ["20"]
                        fireRef "3way" ["-20"]
                    }
                    vanish
                })
            })
        }
        topFireAs "3way" {
            relative "$1"
            speed "4.9"
            plain
        }
        defAction "2way" {
            fire {
                relative "$1"
                speed "2.3"
                refBullet "dummy" []
            }
            fire {
                relative "-$1"
                speed "2.3"
                refBullet "dummy" []
            }
            wait "5"
        }
        defBullet "dummy" {
            doActs (body {
                ()
            })
        }
        defBullet "center" {
            doActs (body {
                changeSpeed "0.01" "1"
                actionRef "2way" ["0"]
                actionRef "2way" ["8-$rank*4"]
                actionRef "2way" ["16-$rank*8"]
                vanish
            })
        }
    }
