namespace FsBulletML2.Bullets.Dsl.EnemyBullet
open FsBulletML2
open FsBulletML2.Dsl

/// BulletML 公式配布（bulletml0_21）のサンプル。
///
/// `All.bullets`（同梱 176 本）には混ぜない —— あちらは
/// 白い弾幕くん由来の集合で、そこに測った数（$rank を使う 173 本 /
/// 狙いを使う 103 本 / 横画面 9 本 …）が全部 紐づいている。
[<RequireQualifiedAccess>]
module Official =

  /// [1943]_rolling_fire.xml
  let g1943_rolling_fire =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[1943]_rolling_fire" {
      top {
        fire {
          refBullet "roll" [  ]
        }
      }
      defBullet "roll" {
        doActs (body {
          wait "40+$rand*20"
          changeDirectionRel "-90" "4"
          changeSpeed "3" "4"
          wait "4"
          changeDirectionSeq "15" "9999"
          wait "80+$rand*40"
          vanish
        })
      }
    }
    

  /// [G_DARIUS]_homing_laser.xml
  let g_darius_homing_laser =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[G_DARIUS]_homing_laser" {
      top {
        repeat "8" {
          fire {
            dir "-60+$rand*120"
            refBullet "hmgLsr" [  ]
          }
          repeat "8" {
            wait "1"
            fire {
              sequence "0"
              refBullet "hmgLsr" [  ]
            }
          }
          wait "10"
        }
      }
      defBullet "hmgLsr" {
        speed "2"
        doActs (body {
          changeSpeed "0.3" "30"
          wait "100"
          changeSpeed "5" "100"
        })
        doActs (body {
          repeat "9999" {
            changeDirectionAim "0" "60-$rank*20"
            wait "5"
          }
        })
      }
    }
    

  /// [Guwange]_round_2_boss_circle_fire.xml
  let guwange_round_2_boss_circle_fire =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Guwange]_round_2_boss_circle_fire" {
      topFireAs "circle" {
        sequence "$1"
        speed "6"
        ofBullet (bulletAnon {
          doActs (body {
            wait "3"
            fire {
              absolute "$2"
              speed "1.5+$rank"
              plain
            }
            vanish
          })
        })
      }
      defAction "fireCircle" {
        repeat "18" {
          fireRef "circle" [ "20"; "$1" ]
        }
      }
      top {
        actionRef "fireCircle" [ "180-45+90*$rand" ]
      }
    }
    

  /// [Guwange]_round_3_boss_fast_3way.xml
  let guwange_round_3_boss_fast_3way =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Guwange]_round_3_boss_fast_3way" {
      top {
        repeat "6+$rank*8" {
          fire {
            dir "$rand*360"
            speed "5"
            refBullet "seed" [ "5+$rand*10" ]
          }
          wait "20"
        }
      }
      defBullet "seed" {
        doActs (body {
          changeSpeed "0" "$1"
          wait "$1"
          fire {
            aim "-20"
            refBullet "3way" [  ]
          }
          repeat "2" {
            fire {
              sequence "20"
              refBullet "3way" [  ]
            }
          }
          wait "1"
          repeat "2" {
            fire {
              sequence "0"
              speedSeq "-0.1"
              refBullet "3way" [  ]
            }
            repeat "2" {
              fire {
                sequence "-20"
                speedSeq "0"
                refBullet "3way" [  ]
              }
            }
            wait "1"
            fire {
              sequence "0"
              speedSeq "-0.1"
              refBullet "3way" [  ]
            }
            repeat "2" {
              fire {
                sequence "20"
                speedSeq "0"
                refBullet "3way" [  ]
              }
            }
            wait "1"
          }
        })
      }
      defBullet "3way" {
        speed "1.8"
      }
    }
    

  /// [Guwange]_round_4_boss_eye_ball.xml
  let guwange_round_4_boss_eye_ball =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Guwange]_round_4_boss_eye_ball" {
      top {
        repeat "4+$rank*4" {
          fire {
            dir "$rand*360"
            refBullet "eye" [  ]
          }
          wait "30"
        }
      }
      defBullet "eye" {
        speed "0"
        doActs (body {
          changeSpeed "10" "400"
          changeDirectionSeq "$rand*5-2" "9999"
          repeat "9999" {
            fire {
              relative "0"
              refBullet "shadow" [  ]
            }
            wait "4"
          }
        })
      }
      defBullet "shadow" {
        speed "0"
        doActs (body {
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
        })
      }
    }
    

  /// [Progear]_round_1_boss_grow_bullets.xml
  let progear_round_1_boss_grow_bullets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_1_boss_grow_bullets" {
      top {
        fire {
          absolute "270-(4+$rank*6)*15/2"
          refBullet "seed" [  ]
        }
        repeat "4+$rank*6" {
          fire {
            sequence "15"
            refBullet "seed" [  ]
          }
        }
      }
      defBullet "seed" {
        speed "1.2"
        doActs (body {
          changeSpeed "0" "60"
          wait "60"
          fire {
            speed "0.75"
            plain
          }
          repeat "4+$rank*4" {
            fire {
              speedSeq "0.15"
              plain
            }
          }
          vanish
        })
      }
    }
    

  /// [Progear]_round_2_boss_struggling.xml
  let progear_round_2_boss_struggling =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_2_boss_struggling" {
      top {
        repeat "100" {
          fire {
            sequence "180"
            refBullet "changeStraight" [  ]
          }
          fire {
            sequence "160"
            refBullet "changeStraight" [  ]
          }
          wait "2"
        }
      }
      defBullet "changeStraight" {
        speed "0.6"
        doActs (body {
          wait "20+$rand*100"
          changeDirectionAbs "270" "60"
          changeSpeed "0" "40"
          wait "40"
          changeSpeed "0.5+$rand*0.7" "20"
        })
      }
    }
    

  /// [Progear]_round_3_boss_back_burst.xml
  let progear_round_3_boss_back_burst =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_3_boss_back_burst" {
      top {
        repeat "100" {
          fire {
            absolute "220+$rand*100"
            refBullet "backBurst" [  ]
          }
          wait "6-$rank*2"
        }
      }
      defBullet "backBurst" {
        speed "1.2"
        doActs (body {
          changeSpeed "0" "80"
          wait "60+$rand*20"
          repeat "2" {
            fire {
              absolute "50+$rand*80"
              refBullet "downAccel" [  ]
            }
          }
          vanish
        })
      }
      defBullet "downAccel" {
        speed "1.8"
        doActs (body {
          accel "250" {
            horizontal "-7"
          }
        })
      }
    }
    

  /// [Progear]_round_3_boss_wave_bullets.xml
  let progear_round_3_boss_wave_bullets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_3_boss_wave_bullets" {
      top {
        repeat "32" {
          fire {
            absolute "320"
            refBullet "wave" [ "-3" ]
          }
          wait "30"
          fire {
            absolute "220"
            refBullet "wave" [ "3" ]
          }
          wait "30"
        }
      }
      defBullet "wave" {
        speed "1"
        doActs (body {
          fire {
            dir "0"
            refBullet "nrm" [  ]
          }
          repeat "8+$rank*10" {
            fire {
              sequence "$1"
              refBullet "nrm" [  ]
            }
            wait "3"
          }
          vanish
        })
      }
      defBullet "nrm" {
        speed "1"
      }
    }
    

  /// [Progear]_round_4_boss_fast_rocket.xml
  let progear_round_4_boss_fast_rocket =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_4_boss_fast_rocket" {
      defAction "fireRoot" {
        fire {
          absolute "$1"
          speed "0.2"
          refBullet "rootBl" [  ]
        }
        repeat "3" {
          fire {
            absolute "$1"
            speedSeq "0.4"
            refBullet "rootBl" [  ]
          }
        }
      }
      top {
        actionRef "fireRoot" [ "$rand*16" ]
        actionRef "fireRoot" [ "180+$rand*16" ]
        wait "120"
      }
      defBullet "rootBl" {
        doActs (body {
          wait "40"
          fire {
            absolute "274+$rand*4"
            refBullet "rocket" [  ]
          }
          vanish
        })
      }
      defBullet "rocket" {
        speed "5+$rand"
        doActs (body {
          repeat "9999" {
            fire {
              absolute "0"
              speed "1"
              refBullet "downAccel" [  ]
            }
            fire {
              absolute "60"
              speed "1.8"
              refBullet "downAccel" [  ]
            }
            wait "3"
          }
        })
      }
      defBullet "downAccel" {
        doActs (body {
          accel "120" {
            vertical "2.7"
          }
        })
      }
    }
    

  /// [Progear]_round_5_boss_last_round_wave.xml
  let progear_round_5_boss_last_round_wave =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_5_boss_last_round_wave" {
      top {
        repeat "2+$rank*1.5" {
          fire {
            refBullet "rfRkt" [  ]
          }
          wait "45"
        }
        wait "60"
      }
      defBullet "rfRkt" {
        doActs (body {
          repeat "9999" {
            wait "1"
            fire {
              sequence "13"
              plain
            }
          }
        })
      }
    }
    

  /// [Progear]_round_5_middle_boss_rockets.xml
  let progear_round_5_middle_boss_rockets =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_5_middle_boss_rockets" {
      top {
        repeat "50" {
          fire {
            absolute "270"
            refBullet "rocket" [  ]
          }
          wait "10"
        }
      }
      defBullet "rocket" {
        doActs (body {
          repeat "9999" {
            fireRef "udBlt" [ "90" ]
            wait "20-$rank*8"
            fireRef "udBlt" [ "-90" ]
            wait "20-$rank*8"
          }
        })
      }
      topFireAs "udBlt" {
        relative "$1-25+$rand*50"
        plain
      }
    }
    

  /// [Progear]_round_6_boss_parabola_shot.xml
  let progear_round_6_boss_parabola_shot =
    createBulletmlInfo <|
    horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Progear]_round_6_boss_parabola_shot" {
      top {
        repeat "50" {
          fire {
            absolute "190+$rand*30"
            refBullet "seed" [  ]
          }
          wait "15-$rank*5"
        }
      }
      defBullet "seed" {
        speed "1"
        doActs (body {
          changeSpeed "0" "60"
          wait "60"
          fire {
            plain
          }
          fire {
            absolute "330+$rand*25"
            refBullet "downAccel" [  ]
          }
          repeat "3" {
            fire {
              sequence "0"
              speedSeq "-0.4"
              refBullet "downAccel" [  ]
            }
          }
          vanish
        })
      }
      defBullet "downAccel" {
        speed "2"
        doActs (body {
          accel "120" {
            vertical "3"
          }
        })
      }
    }
    

  /// [Psyvariar]_X-A_boss_opening.xml
  let psyvariar_x_a_boss_opening =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Psyvariar]_X-A_boss_opening" {
      top {
        repeat "100" {
          fire {
            dir "-45+$rand*90"
            speed "0.4+$rand*0.8"
            plain
          }
          wait "2"
        }
      }
    }
    

  /// [Psyvariar]_X-A_boss_winder.xml
  let psyvariar_x_a_boss_winder =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Psyvariar]_X-A_boss_winder" {
      defBullet "winderBullet" {
        speed "3"
      }
      topFireAs "fireWinder" {
        sequence "$1"
        refBullet "winderBullet" [  ]
      }
      defAction "roundWinder" {
        fireRef "fireWinder" [ "$1" ]
        repeat "11" {
          fireRef "fireWinder" [ "30" ]
        }
        wait "5"
      }
      defAction "winderSequence" {
        repeatRef "12" "roundWinder" [ "30" ]
        repeatRef "12" "roundWinder" [ "$1" ]
        repeatRef "12" "roundWinder" [ "30" ]
      }
      defAction "top1" {
        fire {
          absolute "2"
          refBullet "winderBullet" [  ]
        }
        actionRef "winderSequence" [ "31" ]
      }
      defAction "top2" {
        fire {
          absolute "-2"
          refBullet "winderBullet" [  ]
        }
        actionRef "winderSequence" [ "29" ]
      }
    }
    

  /// [Psyvariar]_X-B_colony_shape_satellite.xml
  let psyvariar_x_b_colony_shape_satellite =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[Psyvariar]_X-B_colony_shape_satellite" {
      top {
        repeat "5" {
          fire {
            absolute "152"
            refBullet "norm" [  ]
          }
          repeat "8" {
            fire {
              sequence "7"
              refBullet "norm" [  ]
            }
          }
          wait "8"
        }
        wait "10"
        repeat "7" {
          fire {
            absolute "180-45+$rand*90"
            refBullet "norm" [  ]
          }
          repeat "4" {
            fire {
              sequence "0"
              speed "1.5"
              refBullet "norm" [  ]
            }
            wait "4"
          }
        }
        wait "10"
        repeat "12" {
          fire {
            dir "0"
            speed "2"
            refBullet "norm" [  ]
          }
          wait "6"
        }
      }
      defBullet "norm" {
        speed "1"
      }
    }
    

  /// [XEVIOUS]_garu_zakato.xml
  let xevious_garu_zakato =
    createBulletmlInfo <|
    verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "[XEVIOUS]_garu_zakato" {
      top {
        fire {
          absolute "180"
          speed "3"
          refBullet "gzc" [  ]
        }
      }
      defBullet "gzc" {
        doActs (body {
          wait "10+$rand*10"
          repeat "16" {
            fire {
              sequence "360/16"
              refBullet "spr" [  ]
            }
          }
          repeat "4" {
            fire {
              sequence "90"
              refBullet "hrmSpr" [  ]
            }
          }
          vanish
        })
      }
      defBullet "spr" {
        speed "2"
      }
      defBullet "hrmSpr" {
        speed "0"
        doActs (body {
          changeSpeed "2" "60"
        })
        doActs (body {
          repeat "9999" {
            changeDirectionAim "0" "40"
            wait "1"
          }
        })
      }
    }
    
