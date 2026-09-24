namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// `Runner.newRoot` と `Runner.newShot` が違うものを作ること。
[<TestFixture>]
type RootAndShot() =

    /// 1 コマ目 に撃って、2 コマ目 で全 top が終わる。
    let Xml =
        """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
    <wait>1</wait>
  </action>
</bulletml>"""

    let env: Env =
        {
            Rand = (fun () -> 0.5f)
            Rank = 0.5f
            Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
            Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f }
        }

    /// 全 top が終わるコマまで回し、そのコマの Frame と、走行ぜんぶで撃った数を返す。
    /// 締めのコマの Spawned は空なので、そこだけ見ると撃っていないと読める。
    let runToFinish (start: BulletRun) =
        let mutable run = start
        let mutable last = Runner.stepWith env run Motion.zero
        let mutable fired = List.length last.Spawned
        let mutable i = 0

        while not last.Finished && i < 10 do
            run <- last.Run
            last <- Runner.stepWith env run Motion.zero
            fired <- fired + List.length last.Spawned
            i <- i + 1

        last, fired

    [<Test>]
    member _.``撃たれた弾として起こすと、撃ったあと Retired が立つ``() =
        let script = Runner.load (fun () -> 0.5f) 0.5f (readXmlString Xml)
        let f, fired = runToFinish (Runner.newShot BulletType.Enemy script)
        // 当てる先が在るか先に見る。 終わっていないコマや、撃っていない
        // 走行の Retired はどちらにせよ false なので、0 件 の緑になる
        f.Finished |> should equal true
        fired |> should be (greaterThan 0)
        f.Retired |> should equal true

    [<Test>]
    member _.``根として起こすと、同じ台本でも Retired は立たない``() =
        let script = Runner.load (fun () -> 0.5f) 0.5f (readXmlString Xml)
        let f, fired = runToFinish (Runner.newRoot BulletType.Enemy script)
        f.Finished |> should equal true
        fired |> should be (greaterThan 0)
        // ここが 2 つ の唯一の違い。 敵そのものは撃ち終わっても回収しない
        f.Retired |> should equal false

    /// 狙う先（`Kind`）は両方で渡せる。`newShot` が `Player` を
    /// 落としていないことを見る
    [<Test>]
    member _.``Kind は newRoot でも newShot でも渡したものが入る``() =
        let script = Runner.load (fun () -> 0.5f) 0.5f (readXmlString Xml)
        (Runner.newRoot BulletType.Player script).Kind |> should equal BulletType.Player
        (Runner.newShot BulletType.Player script).Kind |> should equal BulletType.Player
        (Runner.newRoot BulletType.Enemy script).Kind |> should equal BulletType.Enemy
        (Runner.newShot BulletType.Enemy script).Kind |> should equal BulletType.Enemy
