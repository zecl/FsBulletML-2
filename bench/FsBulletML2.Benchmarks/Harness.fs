namespace FsBulletML2.Benchmarks

open System.Collections.Generic
open System.IO
open FsBulletML2
open FsBulletML2.Core.Tests

/// 弾幕を N フレーム 走らせるだけの下ごしらえ。
///
/// テストの TraceApi と同じ回し方をするが、軌跡の文字列を組まない。
/// 文字列の組み立て（StringBuilder と sprintf と Math.Round）は、測りたい
///
/// 2 つ 持つと、片方だけが古びて、ベンチとテストが別のものを測っていることに
/// 誰も気づかなくなる。
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

  /// XML を読んで DTD の木にするところ。走行ではない。
  ///
  /// StepBenchmarks は GlobalSetup でここを済ませ、測定区間から外している。
  /// 読んだ木は走行のあいだ使い回してよい（DTD の木は不変で、
  /// Runner.load は毎回 新しい台本と Progress を組む）。
  /// 使い回して答えが変わらないことは、StepBenchmarks の Setup が
  /// 読み直した木との弾数の一致で確かめている。
  let parseXml (xml: string) : Bulletml = readXmlString xml

  // ---------------------------------------------------------------------
  // 新 API（Runner.load / step / restart）で走らせる。ここが唯一の経路。
  //
  // 旧 API（BulletRunner.run）の列は落とした。 残していたのは
  //
  // aim は TraceApi が組むのと同じ式をここで書く —— 片方だけ直すと、
  // ベンチとテストが違うものを走らせることになる。式が割れていないことは、
  // 橋 227 本 と控えが軌跡の値で見ている。
  type LiveApi =
    { Bullet : FakeBullet
      mutable Run : BulletRun }

  type PreparedApi =
    { Script : BulletmlScript
      Live : List<LiveApi>
      Born : List<FakeBullet> }

  let private aimDirAt (x: float32) (y: float32) =
    float32 (System.Math.Atan2(float (BulletMLManager.GetPlayerPosX() - x),
                               float -(BulletMLManager.GetPlayerPosY() - y)))

  let private enemyAimDirAt (x: float32) (y: float32) =
    float32 (System.Math.Atan2(float (FakeEnemy.X - x), -1.0 * float (FakeEnemy.Y - y)))

  /// Env を 1 回 組む費用を測るための口。中身は envAt と同じ。
  ///
  /// Env の遅延化に伸びしろが在るかを、見積もりでなく掛け算で出すため。
  /// 天井 = これ × `--counts` の「aim を組む」（＝生の回数）。
  /// aim を遅延にしても、実際に読まれるぶんは残るので、これは上界。
  ///
  /// `countEnvBuilds` の数を掛けてはいけない。 あちらは `HasNoScript` を
  /// 通さずに数えるが、計時している `runPreparedApi` は通す。掛けると
  /// 5way で 60 倍 の見積もりになる
  let envCost (x: float32) (y: float32) : Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      Aim = { ToPlayer = aimDirAt x y; ToEnemy = enemyAimDirAt x y }
      Spawn = { ToPlayer = aimDirAt 0.0f 0.0f; ToEnemy = enemyAimDirAt 0.0f 0.0f } }

  let private envAt (x: float32) (y: float32) : Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      Aim = { ToPlayer = aimDirAt x y; ToEnemy = enemyAimDirAt x y }
      // 産まれた弾は原点に出る（下で FakeBullet を位置を入れずに作る）。
      // TraceApi の spawnAim と同じ値になるようにしてある
      Spawn = { ToPlayer = aimDirAt 0.0f 0.0f; ToEnemy = enemyAimDirAt 0.0f 0.0f } }

  /// 木を組む段に渡すもの。Env ではない —— `Runner.load` が読むのは
  /// 乱数とランクだけで、aim はこの段では読まれない
  let loadRand : unit -> float32 = BulletMLManager.GetRandom
  let loadRank () : float32 = BulletMLManager.GetRank ()

  /// aim を読まないと分かっているコマの Env（`BulletRun.HasNoScript`）
  let noAimEnv () : Domain.Env =
    { Rand = loadRand
      Rank = loadRank ()
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

  let prepareApi (doc: Bulletml) : PreparedApi =
    let script = Runner.load loadRand (loadRank ()) doc
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    root.Init()
    let live = List<LiveApi>()
    live.Add { Bullet = root; Run = Runner.newRoot BulletType.Enemy script }
    { Script = script; Live = live; Born = born }

  let runPreparedApi (p: PreparedApi) (frames: int) : int =
    for _ in 0 .. frames - 1 do
      // このコマで回す顔ぶれを先に固める。産まれた弾は次のコマから
      let count = p.Live.Count
      for i in 0 .. count - 1 do
        let it = p.Live.[i]
        let bo = it.Bullet
        if bo.Used then
          let body = { it.Run.Motion with Pos = { X = bo.X; Y = bo.Y } }
          // 台本が無い弾は aim を読まない（BulletRun.HasNoScript の但し書き）
          let env = if it.Run.HasNoScript then noAimEnv () else envAt bo.X bo.Y
          let f = Runner.stepWith env it.Run body
          bo.X <- bo.X + f.Delta.X
          bo.Y <- bo.Y + f.Delta.Y
          let after = f.Run.Motion
          bo.Speed <- after.Speed
          bo.Dir <- after.Dir
          if f.Vanished || f.Retired then bo.Used <- false
          it.Run <-
            if f.Finished then
              let renv = if f.Run.HasNoScript then noAimEnv () else envAt bo.X bo.Y
              Runner.restart renv f.Run
            else f.Run
          for child in f.Spawned do
            let cb = FakeBullet(p.Born.Count + 1, p.Born)
            cb.Init()
            cb.IsBullet <- true
            cb.X <- child.Motion.Pos.X
            cb.Y <- child.Motion.Pos.Y
            cb.Dir <- child.Motion.Dir
            cb.Speed <- child.Motion.Speed
            p.Born.Add cb
            p.Live.Add { Bullet = cb; Run = child }
    p.Born.Count

  /// 読んだ木から、下ごしらえも走行もまとめて 1 回。
  ///
  /// 下ごしらえ（Runner.load）の費用は測定区間に残っている。 外そうとして
  /// IterationSetup を試したが、BenchmarkDotNet が InvocationCount=1 /
  /// UnrollFactor=1 へ落ちて走行そのものが 1.4〜2.7 倍 重くなった
  /// （StepBenchmarks の但し書き）ので、外さずに大きさを測って書いておく
  /// ほうを採った。大きさは SetupBenchmarks が走行と同じ物差しで出す
  let runFramesOf (doc: Bulletml) (frames: int) : int =
    runPreparedApi (prepareApi doc) frames

  /// XML から直に回す。読む段まで入るので、走行だけを測りたい側では使わない。
  /// CorpusBenchmarks（227 本 を 1 周）はこちらを使う —— あちらは 1 本ずつの
  /// 前後比較ではなく「選んだ 4 本 の外で起きた変化」を見る広い網で、
  /// 読む段は薄く、また DTD 違反で読む段に落ちる 3 本 の費用も
  /// 走行の一部として数えたいため
  let runFrames (xml: string) (frames: int) : int =
    runFramesOf (parseXml xml) frames

  /// 1 走行（60 コマ）で Env を何回 組むか。
  ///
  /// runPreparedApi と同じ形で回して、envAt を呼ぶ場所を数えるだけ。
  /// step の中身は本物を通す（通さないと弾が増えず、回数が実物と変わる）。
  let countEnvBuilds (doc: Bulletml) (frames: int) : int =
    let mutable builds = 0
    let script = Runner.load loadRand (loadRank ()) doc
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    root.Init()
    let live = List<LiveApi>()
    live.Add { Bullet = root; Run = Runner.newRoot BulletType.Enemy script }
    for _ in 0 .. frames - 1 do
      let count = live.Count
      for i in 0 .. count - 1 do
        let it = live.[i]
        let bo = it.Bullet
        if bo.Used then
          let body = { it.Run.Motion with Pos = { X = bo.X; Y = bo.Y } }
          builds <- builds + 1
          let f = Runner.step (envAt bo.X bo.Y) (it.Run.WithMotion body)
          bo.X <- bo.X + f.Delta.X
          bo.Y <- bo.Y + f.Delta.Y
          let after = f.Run.Motion
          bo.Speed <- after.Speed
          bo.Dir <- after.Dir
          if f.Vanished || f.Retired then bo.Used <- false
          it.Run <-
            if f.Finished then
              builds <- builds + 1
              Runner.restart (envAt bo.X bo.Y) f.Run
            else f.Run
          for child in f.Spawned do
            let cb = FakeBullet(born.Count + 1, born)
            cb.Init()
            cb.IsBullet <- true
            cb.X <- child.Motion.Pos.X
            cb.Y <- child.Motion.Pos.Y
            cb.Dir <- child.Motion.Dir
            cb.Speed <- child.Motion.Speed
            born.Add cb
            live.Add { Bullet = cb; Run = child }
    builds

  /// その台本が、その変更を見られるのかを数える。
  ///
  /// 今日の実測: 5way は run 呼び出しの 97%（8,614 / 8,910）が「生きている
  /// top が 1 本 も無い弾」だったが、move と homing はそれが 0 だった。
  let countLiveDead (doc: Bulletml) (frames: int) : int * int =
    let mutable liveCalls = 0
    let mutable deadCalls = 0
    let finished = HashSet<FakeBullet>(HashIdentity.Reference)
    let p = prepareApi doc
    for _ in 0 .. frames - 1 do
      let count = p.Live.Count
      for i in 0 .. count - 1 do
        let it = p.Live.[i]
        let bo = it.Bullet
        if bo.Used then
          if finished.Contains it.Bullet then deadCalls <- deadCalls + 1
          else liveCalls <- liveCalls + 1
          let body = { it.Run.Motion with Pos = { X = bo.X; Y = bo.Y } }
          let env = if it.Run.HasNoScript then noAimEnv () else envAt bo.X bo.Y
          let f = Runner.stepWith env it.Run body
          if f.Finished then finished.Add it.Bullet |> ignore
          bo.X <- bo.X + f.Delta.X
          bo.Y <- bo.Y + f.Delta.Y
          let after = f.Run.Motion
          bo.Speed <- after.Speed
          bo.Dir <- after.Dir
          if f.Vanished || f.Retired then bo.Used <- false
          it.Run <-
            if f.Finished then
              let renv = if f.Run.HasNoScript then noAimEnv () else envAt bo.X bo.Y
              Runner.restart renv f.Run
            else f.Run
          for child in f.Spawned do
            let cb = FakeBullet(p.Born.Count + 1, p.Born)
            cb.Init()
            cb.IsBullet <- true
            cb.X <- child.Motion.Pos.X
            cb.Y <- child.Motion.Pos.Y
            cb.Dir <- child.Motion.Dir
            cb.Speed <- child.Motion.Speed
            p.Born.Add cb
            p.Live.Add { Bullet = cb; Run = child }
    liveCalls, deadCalls

  /// 1 走行の確保をその場で出す。BenchmarkDotNet を通さないので数秒 で終わる。
  ///
  /// 構造を 1 手 変えるたびに 90 分 の走行を回すのは現実的でないので、確保だけを
  /// 即席で測る口を分けた。確保は決定的（同じコードなら同じ数）なので、
  /// これで前後の差が読める。時間はこの口では測れない —— あちらは台が動く。
  let private allocOf (run: unit -> int) : int64 =
    // JIT と静的初期化を測定の外へ出す。外さないと初回だけ数が跳ねる
    run () |> ignore
    System.GC.Collect()
    let before = System.GC.GetAllocatedBytesForCurrentThread()
    run () |> ignore
    System.GC.GetAllocatedBytesForCurrentThread() - before

  /// 1 走行の確保。
  ///
  /// 旧 API の列（allocOld）は落とした。 並べていた 2 列 は独立した実装
  /// ではなく、大部分が同じコードを通っていた —— 一致が既定の姿で、
  /// いちど それを「共通ドリフト＝効いていない」と読み違えている。
  /// 比較の相手は Program.fs の `baseline`（記録した絶対値）に移してある
  let allocApi (doc: Bulletml) (frames: int) : int64 =
    allocOf (fun () -> runPreparedApi (prepareApi doc) frames)

  /// その走行で場に出た弾の数（根を含む）。
  ///
  /// 確保の数と並べて出すため。 この口が数えている確保には、エンジンの
  /// ぶんだけでなく測定器自身が作る `FakeBullet` のぶんも入っている。
  /// 実際に踏んだ —— `FakeBullet` から旧 API の面を落としてフィールドが
  /// 減ったとき、5 本 とも確保が下がった。エンジンは 1 行 も変えていない。
  /// 弾 1 個 あたり 40 B で、`move`（弾 1 個）はちょうど -40 B だった。
  /// 弾数を並べておけば、その手の下がり方は掛け算で見分けられる。
  let bulletCount (doc: Bulletml) (frames: int) : int =
    runPreparedApi (prepareApi doc) frames + 1
