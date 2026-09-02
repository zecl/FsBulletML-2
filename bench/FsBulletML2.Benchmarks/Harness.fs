namespace FsBulletML2.Benchmarks

open System.Collections.Generic
open System.IO
open FsBulletML2
open FsBulletML2.Processable
open FsBulletML2.Core.Tests

/// 弾幕を N フレーム 走らせるだけの下ごしらえ。
///
/// テストの Trace.run と同じ回し方をするが、軌跡の文字列を組まない。
/// 文字列の組み立て（StringBuilder と sprintf と Math.Round）は、測りたい
/// 走行そのものより重くなりうるので、ここには入れない。
///
/// 弾の実体は tests の FakeBullet をそのまま借りる（fsproj でリンクしている）。
/// 同じ形の実装を 2 つ 持つと、片方だけが古びて、ベンチとテストが別のものを
/// 測っていることに誰も気づかなくなる。
module Harness =

  /// samples に入っている実物の BulletML。tests の CorpusData と同じ集め方。
  /// 中身が同じ複製を潰して 227 本 にする。
  module Corpus =

    let samplesDir =
      Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "samples"))

    /// ビルドが吐いた複写を外す。入れると分母がビルド状態で動く
    let private isBuildOutput (p: string) =
      let s = p.Replace('\\', '/')
      [ "/bin/"; "/obj/"; "/Library/"; "/Temp/" ] |> List.exists s.Contains

    let unique () =
      if not (Directory.Exists samplesDir) then []
      else
        Directory.EnumerateFiles(samplesDir, "*.xml", SearchOption.AllDirectories)
        |> Seq.filter (isBuildOutput >> not)
        |> Seq.map (fun p -> p, File.ReadAllText p)
        |> Seq.groupBy snd
        |> Seq.map (fun (_, g) -> g |> Seq.map fst |> Seq.sort |> Seq.head)
        |> Seq.sort
        |> List.ofSeq

    /// 相対パスの末尾で 1 本 引く。ベンチの対象を名前で選ぶため
    let findBySuffix (suffix: string) =
      let s = suffix.Replace('\\', '/')
      unique () |> List.tryFind (fun p -> p.Replace('\\', '/').EndsWith s)

  /// 走らせるあいだ、乱数と rank と自機の位置を固定する。
  /// 固定しないと同じ入力でもコマごとに枝が変わり、時間が入力でなく運で動く
  let fixManager () = BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))

  /// XML 文字列を frames フレーム 回す。返すのは「産まれた弾の数」だけ。
  ///
  /// 数を返すのは、最適化で走行ごと消されないようにするため。
  /// BenchmarkDotNet は返り値を消費するので、これで走行が残る。
  let runFrames (xml: string) (frames: int) : int =
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    let o = root :> IBulletmlObject
    o.Init()
    o.Task <- BulletRunner.convertBulletmlTaskOption (readXmlString xml)

    let step (b: FakeBullet) =
      let bo = b :> IBulletmlObject
      if bo.Used then
        match bo.Task with
        | None -> ()
        | Some task ->
          let result = BulletRunner.run bo
          bo.X <- bo.X + result.X
          bo.Y <- bo.Y + result.Y
          if result.Processed then task.Init(BulletRunner.envOfGlobal bo)

    for _ in 0 .. frames - 1 do
      // このコマで回す顔ぶれを先に固める。途中で産まれた弾は次のコマから。
      // テストの Trace と同じ決め。ここを変えると測る対象が別物になる
      let live = Array.append [| root |] (born.ToArray())
      for b in live do step b

    born.Count
