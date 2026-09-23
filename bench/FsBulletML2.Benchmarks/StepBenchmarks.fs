namespace FsBulletML2.Benchmarks

open BenchmarkDotNet.Attributes
open FsBulletML2.Benchmarks.Harness

/// 弾幕 1 本 を 60 フレーム 走らせる費用。弾数の違うものに、毎コマ 引き直す homing を足してある。
[<MemoryDiagnoser>]
type StepBenchmarks() =

  let mutable move = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>
  let mutable way5 = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>
  let mutable way10 = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>
  let mutable homing = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>

  let load suffix =
    match Corpus.findBySuffix suffix with
    | Some p -> System.IO.File.ReadAllText p
    | None -> failwithf "台本が見つかりません: %s（samples の下を探した: %s）" suffix Corpus.samplesDir

  /// 読んだ木を使い回す。読み直しと弾数が違えば落とす。
  let loadDoc suffix =
    let xml = load suffix
    let doc = parseXml xml
    let reused1 = runFramesOf doc 60
    let fresh = runFramesOf (parseXml xml) 60
    let reused2 = runFramesOf doc 60
    if reused1 <> fresh || reused2 <> fresh then
      failwithf "木を使い回すと弾数が変わる: %s（読み直し %d、使い回し 1 回目 %d、2 回目 %d）"
                suffix fresh reused1 reused2
    // 0 発 を緑にしない。台本を取り違えて空の木を測っていても、上の 3 つは
    // 揃って 0 になるので通ってしまう
    if fresh = 0 && suffix.Contains "Enemy/move.xml" |> not then
      failwithf "1 発 も撃っていない: %s。台本か走らせ方が壊れている" suffix
    doc

  [<GlobalSetup>]
  member _.Setup() =
    fixManager ()
    move <- loadDoc "Content/xml/Enemy/move.xml"
    way5 <- loadDoc "Content/xml/EnemyBullet/5way.xml"
    way10 <- loadDoc "Content/xml/EnemyBullet/10Way.xml"
    homing <- loadDoc "Content/xml/EnemyBullet/[G_DARIUS]_homing_laser.xml"

  [<Benchmark(Description = "move（撃たない）")>]
  member _.Move() = runPreparedApi (prepareApi move) 60

  [<Benchmark(Description = "5way（300 発）")>]
  member _.Way5() = runPreparedApi (prepareApi way5) 60

  [<Benchmark(Description = "10Way（600 発）")>]
  member _.Way10() = runPreparedApi (prepareApi way10) 60

  [<Benchmark(Description = "homing laser（毎コマ 引き直す）")>]
  member _.Homing() = runPreparedApi (prepareApi homing) 60


/// 外した分と残っている分を、引き算ではなく直に測る。
[<MemoryDiagnoser>]
type SetupBenchmarks() =

  let mutable moveXml = ""
  let mutable way5Xml = ""
  let mutable moveDoc = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>
  let mutable way5Doc = Unchecked.defaultof<FsBulletML2.DTD.Bulletml>

  let load suffix =
    match Corpus.findBySuffix suffix with
    | Some p -> System.IO.File.ReadAllText p
    | None -> failwithf "台本が見つかりません: %s" suffix

  [<GlobalSetup>]
  member _.Setup() =
    fixManager ()
    moveXml <- load "Content/xml/Enemy/move.xml"
    way5Xml <- load "Content/xml/EnemyBullet/5way.xml"
    moveDoc <- parseXml moveXml
    way5Doc <- parseXml way5Xml

  [<Benchmark(Description = "move: XML を読んで木にする（測定区間の外）")>]
  member _.MoveParse() = parseXml moveXml

  [<Benchmark(Description = "move: 下ごしらえ Runner.load（測定区間の中）")>]
  member _.MovePrepare() = prepareApi moveDoc

  [<Benchmark(Description = "5way: XML を読んで木にする（測定区間の外）")>]
  member _.Way5Parse() = parseXml way5Xml

  [<Benchmark(Description = "5way: 下ごしらえ Runner.load（測定区間の中）")>]
  member _.Way5Prepare() = prepareApi way5Doc

  /// aim 4 本 の atan2。天井はこれ ×「aim を組む」。`countEnvBuilds` は掛けるな。
  [<Benchmark(Description = "Env を 1 回 組む（aim 4 本）")>]
  member _.EnvBuild() = envCost 12.0f 34.0f


/// 227 本 を 1 周 する。選んだ 4 本 の外を見る網。
[<MemoryDiagnoser>]
[<SimpleJob(launchCount = 1, warmupCount = 1, iterationCount = 3)>]
type CorpusBenchmarks() =

  let mutable scripts : string[] = [||]

  [<GlobalSetup>]
  member _.Setup() =
    fixManager ()
    let files = Corpus.unique ()
    if List.isEmpty files then
      failwithf "samples に xml が 1 本も ありません（%s）。測れていません" Corpus.samplesDir
    scripts <- files |> List.map System.IO.File.ReadAllText |> Array.ofList

  /// 台本の数。0 なら測れていないので、結果を読む前にここを見る
  [<Benchmark(Description = "227 本 を 60 フレーム")>]
  member _.All() =
    let mutable fired = 0
    for s in scripts do
      // 落ちる台本が混じっている（DTD 違反で例外に落ちる 3 本）。
      // 例外の費用も走行の一部なので握って続ける
      try fired <- fired + runFrames s 60 with _ -> ()
    fired
