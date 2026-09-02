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

  /// XML を読んで DTD の木にするところ。**走行ではない。**
  ///
  /// StepBenchmarks は GlobalSetup でここを済ませ、測定区間から外している。
  /// 読んだ木は走行のあいだ使い回してよい（DTD の木は不変で、
  /// convertBulletmlTask は毎回 新しい Rec の木と Progress を組む）。
  /// 使い回して答えが変わらないことは、StepBenchmarks の Setup が
  /// 読み直した木との弾数の一致で確かめている。
  let parseXml (xml: string) : Bulletml = readXmlString xml

  /// 1 回 の走行ぶんの下ごしらえ。task は状態を持つので走行ごとに組み直しが要る。
  ///
  /// **この費用は測定区間に残っている。** 外そうとして IterationSetup を
  /// 試したが、BenchmarkDotNet が InvocationCount=1 / UnrollFactor=1 へ落ちて
  /// 走行そのものが 1.4〜2.7 倍 重くなった（StepBenchmarks の但し書き）ので、
  /// 外さずに大きさを測って書いておくほうを採った。大きさは SetupBenchmarks が
  /// 走行と同じ物差しで出す。
  type Prepared =
    { Root : FakeBullet
      Born : List<FakeBullet> }

  let prepare (doc: Bulletml) : Prepared =
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    let o = root :> IBulletmlObject
    o.Init()
    o.Task <- BulletRunner.convertBulletmlTaskOption doc
    { Root = root; Born = born }

  /// 下ごしらえ済みのものを frames フレーム 回す。返すのは「産まれた弾の数」だけ。
  ///
  /// 数を返すのは、最適化で走行ごと消されないようにするため。
  /// BenchmarkDotNet は返り値を消費するので、これで走行が残る。
  let runPrepared (p: Prepared) (frames: int) : int =
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
      let live = Array.append [| p.Root |] (p.Born.ToArray())
      for b in live do step b

    p.Born.Count

  /// 読んだ木から、下ごしらえも走行もまとめて 1 回。StepBenchmarks はこれ。
  let runFramesOf (doc: Bulletml) (frames: int) : int =
    runPrepared (prepare doc) frames

  /// XML から直に回す。読む段まで入るので、走行だけを測りたい側では使わない。
  /// CorpusBenchmarks（227 本 を 1 周）はこちらを使う —— あちらは 1 本ずつの
  /// 前後比較ではなく「選んだ 4 本 の外で起きた変化」を見る広い網で、
  /// 読む段は薄く、また DTD 違反で読む段に落ちる 3 本 の費用も
  /// 走行の一部として数えたいため
  let runFrames (xml: string) (frames: int) : int =
    runFramesOf (parseXml xml) frames

  /// その台本が、その変更を見られるのかを数える。
  ///
  /// 今日の実測: 5way は run 呼び出しの 97%（8,614 / 8,910）が「生きている
  /// top が 1 本 も無い弾」だったが、move と homing はそれが 0 だった。
  /// 死んだコマにしか効かない変更を move や homing の数で判断すると、
  /// 効く仕組みが無いのに動いた数（＝走行間の台の動き）を効きだと読む。
  /// **数を比べる前にここを見て、その台本が対照なのか対象なのかを決める。**
  ///
  /// 「生きている top が 1 本 も無いコマ」は外から直には見えない
  /// （BulletmlTask.State は internal）ので、前のコマの result.Processed
  /// （その弾の top が全部 終わった）で代用する。前のコマで終わっていれば
  /// 次のコマは必ず死んだコマなので、死の数は下から数えていることになる。
  let countLiveDead (doc: Bulletml) (frames: int) : int * int =
    let mutable liveCalls = 0
    let mutable deadCalls = 0
    let finished = HashSet<FakeBullet>(HashIdentity.Reference)
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    let o = root :> IBulletmlObject
    o.Init()
    o.Task <- BulletRunner.convertBulletmlTaskOption doc

    let step (b: FakeBullet) =
      let bo = b :> IBulletmlObject
      if bo.Used then
        match bo.Task with
        | None -> ()
        | Some task ->
          if finished.Contains b then deadCalls <- deadCalls + 1
          else liveCalls <- liveCalls + 1
          let result = BulletRunner.run bo
          if result.Processed then finished.Add b |> ignore
          bo.X <- bo.X + result.X
          bo.Y <- bo.Y + result.Y
          if result.Processed then task.Init(BulletRunner.envOfGlobal bo)

    for _ in 0 .. frames - 1 do
      let live = Array.append [| root |] (born.ToArray())
      for b in live do step b

    liveCalls, deadCalls
