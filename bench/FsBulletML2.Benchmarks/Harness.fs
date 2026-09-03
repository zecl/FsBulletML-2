namespace FsBulletML2.Benchmarks
// 旧 API（IBulletmlObject / BulletRunner.run）の Obsolete 警告を、
// **このファイルだけ**止める。ここは旧経路を「対照」として意図して測る側で、
// 新旧を同じプロセスに並べるのがこのファイルの仕事だから
// （bench/FsBulletML2.Benchmarks/README.md の「測るときの約束」）。
//
// プロジェクト単位（NoWarn）では止めない。止めると、**新しく書いた測定が
// うっかり旧 API を使っても警告が出なくなる**。効きがファイル単位であることは
// 較正済み —— nowarn を置いていないファイルで旧 API に触ると FS0044 が出る。
#nowarn "44"

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

  // ---------------------------------------------------------------------
  // 新 API（Runner.load / step / restart）で同じ走行を回す。
  //
  // **出荷する経路はこちら。** 上の prepare / runPrepared は旧 API
  // （[<Obsolete>] を付けた BulletRunner.run）を測っている。旧を残すのは、
  // 消すと「段階 4 で遅くなったか」を同じプロセスで比べられなくなるため
  // —— 別プロセスの引き算は効きにならない（README の「測るときの約束」）。
  //
  // 弾の実体は同じ FakeBullet を借りる。新経路が使うのは位置と物理量だけ。
  // aim は FakeBullet.GetAimDir と**同じ式**をここで組む —— 片方だけ直すと、
  // 旧と新で違うものを測ることになる。
  //
  // 並びは ResizeArray で持ち、その場で書き換える。**リストを毎回 組み直すと
  // 1 コマ O(n²) になり、600 発 の台本では測定器のほうが対象より重くなる**
  // （最初にそう書いて、走らせる前に気づいた）。
  // ---------------------------------------------------------------------

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
  /// **Env の遅延化に伸びしろが在るかを、見積もりでなく掛け算で出すため。**
  /// 天井 = これ × 1 走行で組む回数（countEnvBuilds）。
  /// aim を遅延にしても、実際に読まれるぶんは残るので、これは上界。
  let envCost (x: float32) (y: float32) : Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = aimDirAt x y
      EnemyAimDir = enemyAimDirAt x y
      SpawnAimDir = aimDirAt 0.0f 0.0f
      SpawnEnemyAimDir = enemyAimDirAt 0.0f 0.0f }

  let private envAt (x: float32) (y: float32) : Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = aimDirAt x y
      EnemyAimDir = enemyAimDirAt x y
      // 産まれた弾は原点に出る（FakeBullet.GetNewBullet が位置を入れずに作る）。
      // 旧経路と同じ値になるようにしてある
      SpawnAimDir = aimDirAt 0.0f 0.0f
      SpawnEnemyAimDir = enemyAimDirAt 0.0f 0.0f }

  /// 木を組む段の Env。aim はこの段では読まれない
  let loadEnv () : Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = 0.0f
      EnemyAimDir = 0.0f
      SpawnAimDir = 0.0f
      SpawnEnemyAimDir = 0.0f }

  let prepareApi (doc: Bulletml) : PreparedApi =
    let script = Runner.load (loadEnv ()) doc
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    (root :> IBulletmlObject).Init()
    let live = List<LiveApi>()
    live.Add { Bullet = root; Run = Runner.newRoot script }
    { Script = script; Live = live; Born = born }

  let runPreparedApi (p: PreparedApi) (frames: int) : int =
    for _ in 0 .. frames - 1 do
      // このコマで回す顔ぶれを先に固める。産まれた弾は次のコマから
      let count = p.Live.Count
      for i in 0 .. count - 1 do
        let it = p.Live.[i]
        let bo = it.Bullet :> IBulletmlObject
        if bo.Used then
          let body = { it.Run.Body with Pos = { X = bo.X; Y = bo.Y } }
          // 台本が無い弾は aim を読まない（BulletRun.HasNoScript の但し書き）
          let env = if it.Run.HasNoScript then loadEnv () else envAt bo.X bo.Y
          let f = Runner.stepWith p.Script env it.Run body
          bo.X <- bo.X + f.Delta.X
          bo.Y <- bo.Y + f.Delta.Y
          let after = f.Run.Body
          bo.Speed <- after.Speed
          bo.Dir <- after.Dir
          if f.Vanished || f.Retired then bo.Used <- false
          it.Run <-
            if f.Finished then
              let renv = if f.Run.HasNoScript then loadEnv () else envAt bo.X bo.Y
              Runner.restart renv f.Run
            else f.Run
          for child in f.Spawned do
            let cb = FakeBullet(p.Born.Count + 1, p.Born)
            let cbo = cb :> IBulletmlObject
            cbo.Init()
            cbo.IsBullet <- true
            cbo.X <- child.Body.Pos.X
            cbo.Y <- child.Body.Pos.Y
            cbo.Dir <- child.Body.Dir
            cbo.Speed <- child.Body.Speed
            p.Born.Add cb
            p.Live.Add { Bullet = cb; Run = child }
    p.Born.Count

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
  ///
  /// **Env 遅延化の天井 = envCost × この数。** 掛け算で出せる形にしてあるのは、
  /// 「たぶん小さい」で判断しないため。
  let countEnvBuilds (doc: Bulletml) (frames: int) : int =
    let mutable builds = 0
    let script = Runner.load (loadEnv ()) doc
    let born = List<FakeBullet>()
    let root = FakeBullet(0, born)
    (root :> IBulletmlObject).Init()
    let live = List<LiveApi>()
    live.Add { Bullet = root; Run = Runner.newRoot script }
    for _ in 0 .. frames - 1 do
      let count = live.Count
      for i in 0 .. count - 1 do
        let it = live.[i]
        let bo = it.Bullet :> IBulletmlObject
        if bo.Used then
          let body = { it.Run.Body with Pos = { X = bo.X; Y = bo.Y } }
          builds <- builds + 1
          let f = Runner.step script (envAt bo.X bo.Y) (it.Run.WithBody body)
          bo.X <- bo.X + f.Delta.X
          bo.Y <- bo.Y + f.Delta.Y
          let after = f.Run.Body
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
            let cbo = cb :> IBulletmlObject
            cbo.Init()
            cbo.IsBullet <- true
            cbo.X <- child.Body.Pos.X
            cbo.Y <- child.Body.Pos.Y
            cbo.Dir <- child.Body.Dir
            cbo.Speed <- child.Body.Speed
            born.Add cb
            live.Add { Bullet = cb; Run = child }
    builds

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

  /// 1 走行の確保をその場で出す。**BenchmarkDotNet を通さないので数秒 で終わる。**
  ///
  /// 構造を 1 手 変えるたびに 90 分 の走行を回すのは現実的でないので、確保だけを
  /// 即席で測る口を分けた。確保は決定的（同じコードなら同じ数）なので、
  /// これで前後の差が読める。時間はこの口では測れない —— あちらは台が動く。
  ///
  /// **絶対値は BenchmarkDotNet の Allocated と一致しない。** あちらは
  /// ウォームアップ後の定常状態を測り、こちらは下ごしらえ（木を組む段）を
  /// 1 回ぶん 含む。**使うのは差と比だけ。**
  ///
  /// 目盛りの合わせ方は --alloc の出力に書いてある。
  ///
  /// **「決定的」の範囲に but が付く。** 同じバイナリを 3 回 走らせると
  /// バイト単位で完全に一致する（実測）。だが**バイナリが変われば、測定区間に
  /// 関係ない変更でも数 KB 動くことがある** —— Program.fs の表示だけを直した
  /// 手で、5way と 10Way が揃って 4,120 B 減った（move と homing は不動）。
  /// 原因は追っていない。**1% 未満の差は読まない**、が実用上の線。
  /// 今回 struct 化で動いたのは 14〜19% なので、この幅には埋もれない。
  let private allocOf (run: unit -> int) : int64 =
    // JIT と静的初期化を測定の外へ出す。外さないと初回だけ数が跳ねる
    run () |> ignore
    System.GC.Collect()
    let before = System.GC.GetAllocatedBytesForCurrentThread()
    run () |> ignore
    System.GC.GetAllocatedBytesForCurrentThread() - before

  /// 新経路（出荷する側）の 1 走行の確保
  let allocApi (doc: Bulletml) (frames: int) : int64 =
    allocOf (fun () -> runPreparedApi (prepareApi doc) frames)

  /// 旧 API の口で 1 走行したときの確保。
  ///
  /// **対照ではない。** 呼んでいる BulletRunner は旧実装ではなく、
  /// **旧い口を新経路の上に載せたシム**で、中では Step.step を通る
  /// （BulletRunner.fs の envOfGlobal の但し書きと、Step.step を呼ぶ行）。
  /// **Sim / Step / Domain を触れば、この列も新 API 側と同じだけ動く。**
  /// Emit を voption にした手では、新旧の減りがバイト単位まで一致した。
  ///
  /// 読み方:
  ///   両方 動く   → Sim / Step / Domain を触った。比が動くのは口の故障ではない
  ///   新だけ動く → 新 API の口（Runner.load / stepWith / Env の組み方）だけの手
  ///   旧だけ動く → BulletRunner の側だけの手。新 API には効いていない
  ///
  /// **一致を「共通ドリフト＝効いていない」と読み違えたことがある。** この 2 列 は
  /// 独立した実装ではなく、大部分が同じコードなので、一致が既定の姿
  let allocOld (doc: Bulletml) (frames: int) : int64 =
    allocOf (fun () -> runPrepared (prepare doc) frames)
