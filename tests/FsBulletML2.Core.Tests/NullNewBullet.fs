namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Processable

/// final review 7: GetNewBullet() が null を返す弾。
///
/// FsBulletML2.Unity2D/DefaultBullet.fs の GetBulletPrefubInstance は、
/// オーバーライドしなければ既定で null を返す（サンプルの未実装）。
/// FakeBullet.GetNewBullet() は常に非 null を返すので、Trace / Equivalence の
/// 橋はこの経路を一度も踏まない。ここでは経路そのものを直接 組み立てて見る
type private NullSpawnBullet() =
  let mutable ax = 0.0f
  let mutable ay = 0.0f
  let mutable x = 0.0f
  let mutable y = 0.0f
  let mutable speed = 0.0f
  let mutable dir = 0.0f
  let mutable task : BulletmlTask option = None
  let mutable bulletType = BulletType.Enemy
  let mutable shootingDirection = ShootingDirection.BulletVertical
  let mutable used = false
  let mutable isBullet = false
  let mutable bulletRoot = false

  interface IBulletmlObject with
    member _.AccelerationX with get () = ax and set v = ax <- v
    member _.AccelerationY with get () = ay and set v = ay <- v
    member _.X with get () = x and set v = x <- v
    member _.Y with get () = y and set v = y <- v
    member _.Speed with get () = speed and set v = speed <- v
    member _.Dir with get () = dir and set v = dir <- v
    member _.Vanish() = used <- false
    // 旧 fireCommand が踏む null 分岐そのもの
    member _.GetNewBullet() = Unchecked.defaultof<IBulletmlObject>
    member _.GetAimDir() = 0.0f
    member _.GetEnemyAimDir() = 0.0f
    member _.Init() =
      used <- true
      bulletRoot <- false
    member _.Task with get () = task and set v = task <- v
    member _.BulletType with get () = bulletType and set v = bulletType <- v
    member _.ShootingDirection with get () = shootingDirection and set v = shootingDirection <- v
    member _.Used with get () = used and set v = used <- v
    member _.IsBullet with get () = isBullet and set v = isBullet <- v
    member _.BulletRoot with get () = bulletRoot and set v = bulletRoot <- v

[<TestFixture>]
[<NonParallelizable>]
type NullNewBullet() =

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))

  /// 旧 fireCommand: GetNewBullet() が null なら pf.finish <- true; End で
  /// 即終わり、createTask も呼ばない。SrcSpeed / SpeedInit は fire ごとに
  /// GetNewBullet() の後ろでしか書き換わらないので、null が続くかぎり
  /// 一度も更新されない（fire 側の SrcDir だけは GetNewBullet() の前で
  /// 確定するので、そちらは新しい値のまま残る——この門は SrcSpeed 側だけを見る）。
  ///
  /// 直す前は Step.fire が GetNewBullet() の結果を待たずに常に
  /// SrcSpeed / SpeedInit を計算してしまうので、1 発め（absolute 5）で 5、
  /// 2 発め（sequence 3）で 8 になってしまっていた
  [<Test>]
  member _.``GetNewBullet が null を返すと、SrcSpeed / SpeedInit を更新せずに終える``() =
    let xml =
      bml """<action label="top">
  <fire><speed type="absolute">5</speed><bullet/></fire>
  <wait>1</wait>
  <fire><speed type="sequence">3</speed><bullet/></fire>
  <wait>30</wait>
</action>"""
    let bulletml = readXmlString xml
    let root = NullSpawnBullet() :> IBulletmlObject
    root.Init()
    root.Task <- BulletRunner.convertBulletmlTaskOption bulletml
    // frame0: fire1（absolute 5）が null で終わり、wait が Stop で止める
    // frame1: wait が Ended になり、fire2（sequence 3）も null で終わる
    BulletRunner.run root |> ignore
    BulletRunner.run root |> ignore
    match root.Task with
    | Some task ->
        match task.State.Tops with
        | (_, _, fc) :: _ ->
            fc.SrcSpeed |> should equal 0.0f
            fc.SpeedInit |> should equal false
        | [] -> Assert.Fail "Tops が空でした"
    | None -> Assert.Fail "Task が None でした"
