namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsBulletML2

/// ここまでの控えが 1 度も通っていなかった 2 つ。
[<TestFixture>]
type PlayerAndTops() =

    let bml body =
        """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
"""
        + body
        + "\n</bulletml>"

    let runOr frames xml =
        try
            TraceRun.std xml frames
        with e ->
            let rec inner (x: exn) =
                if isNull x.InnerException then
                    x
                else
                    inner x.InnerException

            let i = inner e
            sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))

    let firedBullets (trace: string) =
        trace.Split('\n')
        |> Array.filter (fun l -> l.Contains "  +b")
        |> Array.map (fun l -> l.Trim())
        |> String.concat "\n"

    /// 根の弾を Player にして回す。
    let runAsPlayer (xml: string) (frames: int) =
        TraceApi.runSpawnedAs BulletType.Player (fun () -> 0.5f) 0.5f 30.0f 100.0f xml frames

    /// direction を省くと aim になる。Player なら「敵を狙う」、Enemy なら「自機を狙う」。
    /// fireCommand の aim の枝（Player なら GetEnemyAimDir）がここで初めて通る。
    [<Test>]
    member _.``Player の弾は aim の相手が変わる``() =
        let x =
            bml
                """<action label="top">
  <fire><bullet/></fire>
  <wait>10</wait>
</action>"""

        let asEnemy = TraceRun.std x 3 |> firedBullets
        let asPlayer = runAsPlayer x 3

        sprintf
            "Enemy として撃つ:\n%s\n\nPlayer として撃つ:\n%s\n\n\
             自機 (30,100) を狙う aim = 2.850 / 敵 (-40,-60) を狙う aim = atan2(-40,60) = -0.588 -> calcDir が 2pi 足して 5.695"
            asEnemy
            (asPlayer.TrimEnd('\n'))
        |> Golden.check "player-aim"

    /// 撃った弾に BulletType が伝わるか。GetNewBullet が親の型を写している。
    [<Test>]
    member _.``撃った弾は親の BulletType を継ぐ``() =
        bml
            """<action label="top">
  <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
  <wait>10</wait>
</action>"""
        |> fun x -> runAsPlayer x 3
        |> Golden.check "player-bullettype-inherit"

    /// label が top で始まる action が複数あるとき。
    /// StartsWith("top") なので top / top1 / top2 が全部 task になるはず。
    [<Test>]
    member _.``top で始まる label が 3 つあると、全部が走る``() =
        bml
            """<action label="top">
  <fire><direction type="absolute">0</direction><speed>1</speed><bullet/></fire>
  <wait>10</wait>
</action>
<action label="top1">
  <fire><direction type="absolute">90</direction><speed>2</speed><bullet/></fire>
  <wait>10</wait>
</action>
<action label="top2">
  <fire><direction type="absolute">180</direction><speed>3</speed><bullet/></fire>
  <wait>10</wait>
</action>"""
        |> runOr 3
        |> firedBullets
        |> Golden.check "multiple-top"

    /// 上で top / top1 / top2 のうち top しか撃たなかった。
    [<Test>]
    member _.``先頭の top の wait が、後ろの top を塞いでいるか``() =
        let withWait =
            bml
                """<action label="top">
  <fire><direction type="absolute">0</direction><speed>1</speed><bullet/></fire>
  <wait>10</wait>
</action>
<action label="top1">
  <fire><direction type="absolute">90</direction><speed>2</speed><bullet/></fire>
  <wait>10</wait>
</action>"""
        // 先頭に wait を置かない形。Stop を返さなければ後ろも回るはず
        let noWait =
            bml
                """<action label="top">
  <fire><direction type="absolute">0</direction><speed>1</speed><bullet/></fire>
</action>
<action label="top1">
  <fire><direction type="absolute">90</direction><speed>2</speed><bullet/></fire>
</action>"""

        sprintf
            "先頭に wait あり:\n%s\n\n先頭に wait なし:\n%s\n\n0 度 speed1 = top / 90 度(1.571) speed2 = top1"
            (runOr 3 withWait |> firedBullets)
            (runOr 3 noWait |> firedBullets)
        |> Golden.check "multiple-top-blocked"

    /// top で始まらない label は拾われないはず。topmost のような紛らわしい名前も見る。
    [<Test>]
    member _.``top で始まる名前とそうでない名前``() =
        bml
            """<action label="topmost">
  <fire><direction type="absolute">0</direction><speed>1</speed><bullet/></fire>
  <wait>10</wait>
</action>
<action label="nottop">
  <fire><direction type="absolute">90</direction><speed>9</speed><bullet/></fire>
  <wait>10</wait>
</action>"""
        |> runOr 3
        |> firedBullets
        |> fun s -> sprintf "topmost は拾われるか / nottop は拾われないか\n%s\n（0 度 speed1 だけなら topmost のみ）" s
        |> Golden.check "top-prefix-match"
