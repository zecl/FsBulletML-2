namespace FsBulletML2.Benchmarks

open BenchmarkDotNet.Attributes
open FsBulletML2.Benchmarks.Harness

/// 弾幕 1 本 を 60 フレーム 走らせる費用。
///
/// 対象は勘で選んでいない。控え（tests/TestData/trace/corpus-trace.txt）に
/// 「撃った弾数 / 最終フレームに残っていた数」が記録されているので、そこから
/// 撃つ量の違うものを取った。弾数だけで選ぶと「濃いが浅い」ものに偏るので、
/// 弾数は少ないが毎コマ 向きを引き直す homing を別枠で入れてある。
///
///   Enemy/move.xml                       撃った   0 発   木を歩く費用だけ
///   5way.xml                             撃った 300 発   中くらい
///   10Way.xml                            撃った 600 発   濃い
///   [G_DARIUS]_homing_laser.xml          撃った  33 発   弾は少ないが毎コマ 計算する
///
/// 60 フレームは控えと同じ長さ。ここを変えると控えと突き合わせられなくなる。
[<MemoryDiagnoser>]
type StepBenchmarks() =

  let mutable move = ""
  let mutable way5 = ""
  let mutable way10 = ""
  let mutable homing = ""

  let load suffix =
    match Corpus.findBySuffix suffix with
    | Some p -> System.IO.File.ReadAllText p
    | None -> failwithf "台本が見つかりません: %s（samples の下を探した: %s）" suffix Corpus.samplesDir

  [<GlobalSetup>]
  member _.Setup() =
    fixManager ()
    move <- load "Content/xml/Enemy/move.xml"
    way5 <- load "Content/xml/EnemyBullet/5way.xml"
    way10 <- load "Content/xml/EnemyBullet/10Way.xml"
    homing <- load "Content/xml/EnemyBullet/[G_DARIUS]_homing_laser.xml"

  [<Benchmark(Description = "move（撃たない）")>]
  member _.Move() = runFrames move 60

  [<Benchmark(Description = "5way（300 発）")>]
  member _.Way5() = runFrames way5 60

  [<Benchmark(Description = "10Way（600 発）")>]
  member _.Way10() = runFrames way10 60

  [<Benchmark(Description = "homing laser（毎コマ 引き直す）")>]
  member _.Homing() = runFrames homing 60


/// 227 本 を 1 周 する費用。橋と控えが見ているのと同じ母集団。
///
/// 1 本 ずつの数字だけを見ていると、選んだ 4 本 の外で起きた変化が見えない。
/// 通しは遅い（tests 側の計測で 8 秒 前後）ので、繰り返しの回数は少なくてよい。
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
