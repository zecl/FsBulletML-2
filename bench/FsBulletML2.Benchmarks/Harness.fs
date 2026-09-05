namespace FsBulletML2.Benchmarks

open System.Collections.Generic
open System.IO
open FsBulletML2
open FsBulletML2.Core.Tests

/// 弾幕を N フレーム 走らせるだけの下ごしらえ。
///
/// テストの TraceApi と同じ回し方をするが、軌跡の文字列を組まない。
/// 文字列の組み立て（StringBuilder と sprintf と Math.Round）は、測りたい
/// 走行そのものより重くなりうるので、ここには入れない。
///
/// 弾の実体は tests の FakeBullet をそのまま借りる（fsproj でリンクしている）。
/// **借りているのは物理量の箱と、`FakeEnemy` の位置。** 同じ形の実装を
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

  /// XML を読んで DTD の木にするところ。**走行ではない。**
  ///
  /// StepBenchmarks は GlobalSetup でここを済ませ、測定区間から外している。
  /// 読んだ木は走行のあいだ使い回してよい（DTD の木は不変で、
  /// Runner.load は毎回 新しい台本と Progress を組む）。
  /// 使い回して答えが変わらないことは、StepBenchmarks の Setup が
  /// 読み直した木との弾数の一致で確かめている。
  let parseXml (xml: string) : Bulletml = readXmlString xml

  // ---------------------------------------------------------------------
  // 新 API（Runner.load / step / restart）で走らせる。**ここが唯一の経路。**
  //
  // **旧 API（BulletRunner.run）の列は落とした。** 残していたのは
  // 「段階 4 で遅くなったか」を同じプロセスで比べるためだったが、あれは
  // 対照ではなかった —— 旧い口を新経路の上に載せたシムで、中では同じ
  // Step.step を通る。Sim / Step / Domain を触れば 2 列 とも同じだけ動く。
  // 校正の土台（新旧の比）は先に「新 API の絶対値と記録からの差」へ
  // 置き換えてある（Program.fs の baseline / baselineAt）。
  //
  // 弾の実体は tests の FakeBullet を借りる。新経路が使うのは位置と物理量だけ。
  // aim は TraceApi が組むのと**同じ式**をここで書く —— 片方だけ直すと、
  // ベンチとテストが違うものを走らせることになる。式が割れていないことは、
  // 橋 227 本 と控えが軌跡の値で見ている。
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
  /// 天井 = これ × `--counts` の「aim を組む」（＝生の回数）。
  /// aim を遅延にしても、実際に読まれるぶんは残るので、これは上界。
  ///
  /// **`countEnvBuilds` の数を掛けてはいけない。** あちらは `HasNoScript` を
  /// 通さずに数えるが、計時している `runPreparedApi` は通す。掛けると
  /// 5way で 60 倍 の見積もりになる
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
      // 産まれた弾は原点に出る（下で FakeBullet を位置を入れずに作る）。
      // TraceApi の spawnAim と同じ値になるようにしてある
      SpawnAimDir = aimDirAt 0.0f 0.0f
      SpawnEnemyAimDir = enemyAimDirAt 0.0f 0.0f }

  /// 木を組む段に渡すもの。**Env ではない** —— `Runner.load` が読むのは
  /// 乱数とランクだけで、aim はこの段では読まれない
  let loadRand : unit -> float32 = BulletMLManager.GetRandom
  let loadRank () : float32 = BulletMLManager.GetRank ()

  /// aim を読まないと分かっているコマの Env（`BulletRun.HasNoScript`）
  let noAimEnv () : Domain.Env =
    { Rand = loadRand
      Rank = loadRank ()
      AimDir = 0.0f
      EnemyAimDir = 0.0f
      SpawnAimDir = 0.0f
      SpawnEnemyAimDir = 0.0f }

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
          let f = Runner.stepWith p.Script env it.Run body
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
  /// **下ごしらえ（Runner.load）の費用は測定区間に残っている。** 外そうとして
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
  ///
  /// **この数を遅延化の天井に掛けてはいけない。** ここは `HasNoScript` を
  /// 通さず毎回 `envAt` を呼ぶが、計時している `runPreparedApi` は通す。
  /// Atan2 を実際に回すのは「生」のぶんだけなので、天井はそちらに掛ける
  /// （`--counts` の「aim を組む」列）。
  ///
  /// この数そのものは「1 走行で Env の箱を何回 作るか」——
  /// 箱の確保を減らす手を測るときの物差しとして残してある。
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
          let f = Runner.step script (envAt bo.X bo.Y) (it.Run.WithMotion body)
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
  /// 死んだコマにしか効かない変更を move や homing の数で判断すると、
  /// 効く仕組みが無いのに動いた数（＝走行間の台の動き）を効きだと読む。
  /// **数を比べる前にここを見て、その台本が対照なのか対象なのかを決める。**
  ///
  /// 「生きている top が 1 本 も無いコマ」は外から直には見えない
  /// （BulletState.Tops は internal）ので、いちど でも Frame.Finished が
  /// 立った弾はその後 死んだコマとして数える、で代用する。
  /// **下から数えている**（走らせ直しで生き返るコマも死に入るため、
  /// 実際の死より多く出ることはあっても、少なく出ることはない）。
  ///
  /// **旧 API（result.Processed）から Frame.Finished へ付け替えた。**
  /// どちらも「その弾の top が全部 終わった」で、旧の run はその値を
  /// RunResult.Processed に詰めていただけなので、数は動かない
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
          let f = Runner.stepWith p.Script env it.Run body
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
  /// **「確保は決定的」は、JIT の段が同じときにだけ成り立つ。**
  ///
  /// 以前ここには「同じバイナリを 3 回 走らせるとバイト単位で完全に一致する
  /// （実測）」と書いてあった。**その 3 回 では実際に一致した。だが一致は
  /// この口の性質ではない。** 走行ごとに変わりうるものが下に 1 つ ある。
  ///
  /// **段階的 JIT（tiered compilation）。** 同じメソッドでも tier-0 と tier-1
  /// で確保が違う。どちらで測るかは呼んだ回数と背景スレッドの時計で決まるので、
  /// **走行のたびに変わりうる。** 切り分けは 1 手 で付いた ——
  /// `DOTNET_TieredCompilation=0` にすると wide が -82,432 B（-2.6%）動く。
  /// 段が確保に効くこと自体は、これで確かめた。
  ///
  /// 実測した幅:
  ///
  ///   空回し 1 巡     いちばん軽い move（313 KB）で +6,192 B = **2.0%**
  ///   空回し 2 巡     おおむね 0.1% 未満。走行間で move が 288 B 動いた
  ///   段を切る        別の測定条件。上のどちらとも並べられない
  ///
  /// **Program.fs の --alloc は 1 巡 空けてから測る**（それでも 0 にはならない）。
  /// **1% 未満の差は読まない**、が実用上の線で、いまはその線に根拠がある。
  /// struct 化で動いたのは 14〜19% なので、この幅には埋もれない。
  ///
  /// もう 1 つ、これとは別の but:
  /// **バイナリが変われば、測定区間に関係ない変更でも数 KB 動くことがある**
  /// —— Program.fs の表示だけを直した手で、5way と 10Way が揃って
  /// 4,120 B 減った（move と homing は不動）。原因は追っていない。
  let private allocOf (run: unit -> int) : int64 =
    // JIT と静的初期化を測定の外へ出す。外さないと初回だけ数が跳ねる
    run () |> ignore
    System.GC.Collect()
    let before = System.GC.GetAllocatedBytesForCurrentThread()
    run () |> ignore
    System.GC.GetAllocatedBytesForCurrentThread() - before

  /// 1 走行の確保。
  ///
  /// **旧 API の列（allocOld）は落とした。** 並べていた 2 列 は独立した実装
  /// ではなく、大部分が同じコードを通っていた —— 一致が既定の姿で、
  /// いちど それを「共通ドリフト＝効いていない」と読み違えている。
  /// 比較の相手は Program.fs の `baseline`（記録した絶対値）に移してある
  let allocApi (doc: Bulletml) (frames: int) : int64 =
    allocOf (fun () -> runPreparedApi (prepareApi doc) frames)

  /// その走行で場に出た弾の数（根を含む）。
  ///
  /// **確保の数と並べて出すため。** この口が数えている確保には、エンジンの
  /// ぶんだけでなく**測定器自身が作る `FakeBullet` のぶんも入っている。**
  /// 実際に踏んだ —— `FakeBullet` から旧 API の面を落としてフィールドが
  /// 減ったとき、5 本 とも確保が下がった。エンジンは 1 行 も変えていない。
  /// 弾 1 個 あたり 40 B で、`move`（弾 1 個）はちょうど -40 B だった。
  /// **弾数を並べておけば、その手の下がり方は掛け算で見分けられる。**
  let bulletCount (doc: Bulletml) (frames: int) : int =
    runPreparedApi (prepareApi doc) frames + 1
