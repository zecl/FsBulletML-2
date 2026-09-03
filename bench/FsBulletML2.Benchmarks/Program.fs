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
  printfn "%-8s %10s %10s %10s %10s   %s" "台本" "生" "死" "死の割合" "Env 構築" "この台本が使えるもの"
  for name, suffix in scenarios do
    match Corpus.findBySuffix suffix with
    | None -> printfn "%-8s %s が見つかりません" name suffix
    | Some path ->
      let doc = parseXml (System.IO.File.ReadAllText path)
      let live, dead = countLiveDead doc 60
      let envs = countEnvBuilds doc 60
      let total = live + dead
      let ratio = if total = 0 then 0.0 else float dead / float total * 100.0
      printfn "%-8s %10d %10d %9.1f%% %10d   %s"
              name live dead ratio envs
              (if dead = 0 then "対照（死んだコマの変更は届かない）" else "対象")
  printfn ""
  printfn "「Env 構築」は新経路が 1 走行で Env を組む回数（step の前と、走らせ直しの前）。"
  printfn "**Env を遅延にしたときの天井 = SetupBenchmarks の「Env を 1 回 組む」× この数。**"
  printfn "掛け算で出せる形にしてあるのは、伸びしろを「たぶん小さい」で判断しないため。"
  printfn "遅延にしても実際に読まれるぶんは残るので、その積は上界。"

/// 1 走行の確保を、BenchmarkDotNet を通さずに出す。**数秒 で終わる。**
///
/// 構造を 1 手 変えるたびに 90 分 の走行を回せないので、確保だけを即席で測る。
/// 確保は決定的なので、これで前後の差が読める。**時間はここでは測れない。**
///
/// 目盛りは新旧の比で合わせた。下の `calibratedAt` の版を BenchmarkDotNet が
/// 同じプロセスで測った比が `calibration` で、口を作った時点で 4 本 とも
/// 1 ポイント以内 に入った。符号が違うものが 2 本 あるので「たまたま近い」では揃わない。
///
/// **以降 Core を変えれば、新旧の比そのものが動く。** 離れていくのは
/// 口の故障ではなく、校正値がその版のものだという意味。**離れたら BDN で
/// 測り直してここを更新する** —— 更新しないまま「外れ」を見続けると、
/// 次に本当に口が壊れたとき区別がつかなくなる。
let private calibratedAt = "400e425（aim skip をフロントへ戻した版）"

let private calibration = dict [ "move", -21.6; "5way", 4.3; "10Way", 4.1; "homing", -3.2 ]

let private alloc () =
  fixManager ()
  printfn "1 走行（60 コマ）の確保。BenchmarkDotNet を通さない即席の物差し。"
  printfn ""
  printfn "%-8s %14s %14s %9s %9s %8s" "台本" "新 API" "旧 API" "比" "校正値" "差"
  for name, suffix in scenarios do
    match Corpus.findBySuffix suffix with
    | None -> printfn "%-8s %s が見つかりません" name suffix
    | Some path ->
      let doc = parseXml (System.IO.File.ReadAllText path)
      let a = allocApi doc 60
      let o = allocOld doc 60
      let ratio = (float a / float o - 1.0) * 100.0
      let bdn = calibration.[name]
      printfn "%-8s %12d B %12d B %+8.1f%% %+8.1f%% %+7.1f"
              name a o ratio bdn (ratio - bdn)
  printfn ""
  printfn "**絶対値は BenchmarkDotNet の Allocated と一致しない。** あちらは"
  printfn "ウォームアップ後の定常状態を測り、こちらは下ごしらえ（木を組む段）を"
  printfn "1 回ぶん 含む。使うのは差と比だけ。"
  printfn ""
  printfn "校正値は %s を BenchmarkDotNet で測ったもの。" calibratedAt
  printfn "口を作った時点で 4 本 とも 1 ポイント以内 に入った（符号が違うものが"
  printfn "2 本 あるので、たまたまでは揃わない）。**Core を変えれば比そのものが"
  printfn "動くので、「差」が開くのは口の故障ではない。** 開いたら BDN で測り直して"
  printfn "Program.fs の calibration と calibratedAt を更新する。"
  printfn ""
  printfn "**この口が見ているのは新旧の比だけで、絶対値の正しさは見ていない。**"
  printfn "新旧が同じだけ増えると比は動かない。前の走行の絶対値と並べて読むこと。"

[<EntryPoint>]
let main argv =
  if argv |> Array.contains "--counts" then
    counts ()
    0
  elif argv |> Array.contains "--alloc" then
    alloc ()
    0
  else
    BenchmarkSwitcher.FromAssembly(typeof<StepBenchmarks>.Assembly).Run(argv) |> ignore
    0
