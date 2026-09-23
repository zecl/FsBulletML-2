namespace FsBulletML2.Benchmarks

open System.Collections.Generic
open System.IO
open FsBulletML2
open FsBulletML2.Core.Tests

/// 弾幕を N フレーム 走らせる下ごしらえ。軌跡の文字列は組まない。
/// 弾の実体は tests の FakeBullet を借りる。写すと片方が古びる。
module Harness =

  /// tests の CorpusData と同じ集め方。中身が同じ複製は潰す。
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

  /// 固定しないと、同じ入力でも時間が運で動く。
  let fixManager () = BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))

  /// 走行ではない。読んだ木は使い回してよい。一致は StepBenchmarks の Setup が見る。
  let parseXml (xml: string) : Bulletml = readXmlString xml

  // 経路は Runner.load / step / restart だけ。
  // aim の式は TraceApi と同じものを書く。片方だけ直すと別物を測る。
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

  /// 中身は envAt と同じ。天井はこれ ×「aim を組む」。
  /// `countEnvBuilds` を掛けるな。あちらは `HasNoScript` を通さない。
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

  /// `Runner.load` が読むのは乱数とランクだけ。aim はこの段では読まれない。
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

  /// `Runner.load` は測定区間に残す。IterationSetup へ出すと走行そのものが重くなる。
  let runFramesOf (doc: Bulletml) (frames: int) : int =
    runPreparedApi (prepareApi doc) frames

  /// 読む段まで入る。走行だけを測る側では使わない。
  let runFrames (xml: string) (frames: int) : int =
    runFramesOf (parseXml xml) frames

  /// `envAt` を呼ぶ回数。step は本物を通す。通さないと弾が増えず回数が変わる。
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

  /// 死が 0 の台本は、死んだコマにしか効かない変更の対照。
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

  /// BenchmarkDotNet を通さない。時間は測れない。台が動く。
  let private allocOf (run: unit -> int) : int64 =
    // JIT と静的初期化を測定の外へ出す。外さないと初回だけ数が跳ねる
    run () |> ignore
    System.GC.Collect()
    let before = System.GC.GetAllocatedBytesForCurrentThread()
    run () |> ignore
    System.GC.GetAllocatedBytesForCurrentThread() - before

  /// 比較相手は Program.fs の `baseline`。旧 API の列は対照ではなかった。
  let allocApi (doc: Bulletml) (frames: int) : int64 =
    allocOf (fun () -> runPreparedApi (prepareApi doc) frames)

  /// 根を含む。確保には測定器の FakeBullet も入る。弾数と掛けて見分ける。
  let bulletCount (doc: Bulletml) (frames: int) : int =
    runPreparedApi (prepareApi doc) frames + 1
