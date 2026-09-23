namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

/// 台本が無い弾のコマで、Front がゲームに何も聞かないこと。
/// 答えはどちらでも 0 なので、軌跡の門ではこの省略は守られない。
[<TestFixture>]
type NoAimSkip() =

  /// 1 コマ目 に素の弾を撃つ。撃たれた弾は自分の action を持たないので
  /// `HasNoScript` が立つ
  let Xml = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="aim">0</direction><speed>2</speed><bullet/></fire>
    <wait>30</wait>
  </action>
</bulletml>"""

  /// 聞かれた回数を数える世界。値は全部 0 —— ここで見るのは
  /// 答えの中身ではなく、聞いたかどうか
  let counting () =
    let mutable asked = 0
    let w =
      { new IFrontEnv with
          member _.Rand = (fun () -> 0.5f)
          member _.Rank = 0.5f
          member _.PlayerX = asked <- asked + 1; 0.0f
          member _.PlayerY = asked <- asked + 1; 0.0f
          member _.TryTargetFrom(_x, _y, ex, ey) =
            asked <- asked + 1
            ex <- 0.0f
            ey <- 0.0f
            false
          member _.TrySpawnTargetFrom(_x, _y, ex, ey) =
            asked <- asked + 1
            ex <- 0.0f
            ey <- 0.0f
            false }
    w, (fun () -> asked)

  let load () =
    Runner.load (fun () -> 0.5f) 0.5f (Bulletml.readXmlString Xml)

  [<Test>]
  member _.``台本を持つ弾のコマでは、ゲームに聞く``() =
    let script = load ()
    let w, asked = counting ()
    let run = Runner.newRoot BulletType.Enemy script
    run.HasNoScript |> should equal false
    Driver.step w Space.YDown SpawnOrigin.AtOrigin run Motion.zero |> ignore
    // これが対照。 下の 0 件 が「そもそも通っていない」ではないと分かる
    asked () |> should be (greaterThan 0)

  [<Test>]
  member _.``台本が無い弾のコマでは、ゲームに何も聞かない``() =
    let script = load ()
    let w0, _ = counting ()
    let root = Runner.newRoot BulletType.Enemy script
    let f = Driver.step w0 Space.YDown SpawnOrigin.AtOrigin root Motion.zero
    // 当てる先が在るか先に見る
    f.Spawned |> should not' (be Empty)
    let child = f.Spawned |> List.head
    child.HasNoScript |> should equal true

    let w, asked = counting ()
    Driver.step w Space.YDown SpawnOrigin.AtOrigin child Motion.zero |> ignore
    asked () |> should equal 0

  /// 走らせ直しは台本が在っても聞かない。aim が結果に出ない。
  [<Test>]
  member _.``走らせ直しでは、台本が在ってもゲームに聞かない``() =
    let script = load ()
    let root = Runner.newRoot BulletType.Enemy script
    root.HasNoScript |> should equal false
    let w, asked = counting ()
    Driver.restart w root |> ignore
    asked () |> should equal 0
