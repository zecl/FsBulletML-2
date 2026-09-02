module FsBulletML2.Benchmarks.Program

open BenchmarkDotNet.Running
open FsBulletML2.Benchmarks.Harness

/// 走らせ方
///
///   dotnet run -c Release --project bench/FsBulletML2.Benchmarks -- --filter *
///   dotnet run -c Release --project bench/FsBulletML2.Benchmarks -- --filter *StepBenchmarks*
///   dotnet run -c Release --project bench/FsBulletML2.Benchmarks -- --counts
///
/// Debug で走らせると BenchmarkDotNet が止める（最適化が効いていない数を
/// 出さないため）。測るときは必ず Release。
///
/// BenchmarkDotNet は台本 1 本 ごとに別プロセスを立てる。**別プロセスの数を
/// そのまま前後で引き算しない。** 走行と走行のあいだに台のほうが動く。

/// StepBenchmarks が測る 4 本。--counts と表示の順を揃えるためにここに置く
let private scenarios =
  [ "move",   "Content/xml/Enemy/move.xml"
    "5way",   "Content/xml/EnemyBullet/5way.xml"
    "10Way",  "Content/xml/EnemyBullet/10Way.xml"
    "homing", "Content/xml/EnemyBullet/[G_DARIUS]_homing_laser.xml" ]

/// その台本が、その変更を見られるのかを出す。**数を比べる前にここを見る。**
///
/// 「死」は BulletRunner.run に入ったが、生きている top が 1 本 も無かった
/// 呼び出し。死んだコマにしか効かない変更（たとえば
/// BulletRunner.envWithoutAim）は、死が 0 の台本では原理的に効かない。
/// そこで数が動いていたら、それは効きではなく走行間の台の動き。
///
/// 効きの出どころが分かっていない変更では、逆に「死が 0 の台本」を対照として
/// 使える。対照が同じだけ動いていたら、対象の動きも台のもの。
let private counts () =
  fixManager ()
  printfn "台本ごとの run 呼び出し。60 コマ。"
  printfn "%-8s %10s %10s %10s   %s" "台本" "生" "死" "死の割合" "この台本が使えるもの"
  for name, suffix in scenarios do
    match Corpus.findBySuffix suffix with
    | None -> printfn "%-8s %s が見つかりません" name suffix
    | Some path ->
      let doc = parseXml (System.IO.File.ReadAllText path)
      let live, dead = countLiveDead doc 60
      let total = live + dead
      let ratio = if total = 0 then 0.0 else float dead / float total * 100.0
      printfn "%-8s %10d %10d %9.1f%%   %s"
              name live dead ratio
              (if dead = 0 then "対照（死んだコマの変更は届かない）" else "対象")

[<EntryPoint>]
let main argv =
  if argv |> Array.contains "--counts" then
    counts ()
    0
  else
    BenchmarkSwitcher.FromAssembly(typeof<StepBenchmarks>.Assembly).Run(argv) |> ignore
    0
