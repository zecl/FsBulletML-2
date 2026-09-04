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

/// --counts と --alloc が回す台本。先頭 4 本 は StepBenchmarks と同じもので、
/// 表示の順を揃えるためにここに置く。
///
/// **wide は「子の多い action」を物差しに載せるために足した。**
///
/// `Step.action` の `ps / running` を配列にする手は、いちど 試して戻して
/// ある（Step.fs の但し書き）。形の上では O(n^2) だが、`List.toArray` を
/// 2 本 と `List.ofArray` を 1 本 作る固定費のほうが高くつく、という理由で、
/// 確保が 4 本 とも増えた。
///
/// **その 4 本 の action は、子が最大 2 / 2 / 2 / 3 個 しかない。**
/// 手が狙っているのは子の多い action なので、**却下の測定は当てる先に
/// 届いていなかった。** コーパス 227 本 を静的に数えると子は最大 32 個
/// （[OtakuTwo]_dis_bee_1、以下）で、15 個 以上 を持つ台本が 13 本 ある。
///
///     子の数    action の数   その子の総数
///     1〜3            1,205         2,180   全体の 41.6%
///     10 以上            69           938   全体の 17.9%
///     20 以上             4            94   全体の  1.8%
///
/// 却下そのものは 4 本 については正しい。**一般化されていたのが誤り。**
let private scenarios =
  [ "move",   "Content/xml/Enemy/move.xml"
    "5way",   "Content/xml/EnemyBullet/5way.xml"
    "10Way",  "Content/xml/EnemyBullet/10Way.xml"
    "homing", "Content/xml/EnemyBullet/[G_DARIUS]_homing_laser.xml"
    "wide",   "Content/xml/EnemyBullet/sdmkun/bosses.d/[OtakuTwo]_dis_bee_1.xml" ]

/// StepBenchmarks（時間）に載っているのは先頭 4 本 だけ。**wide は確保の
/// 物差しにしか載っていない。** 却下の根拠が確保だったので、まず確保で
/// 測り直せる形にした。確保で効きが出たら、そのとき時間の物差しへ載せる。
let private onBdn = 4

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
/// **三度 更新してある。** 400e425（aim skip を戻した版、比は
/// move -21.6 / 5way +4.3 / 10Way +4.1 / homing -3.2）で目盛りを合わせ、
/// 箱を 4 つ 値型にした手で比が 17 ポイント 動いたので d4361e7
/// （-25.6 / -13.0 / -12.4 / -4.9）で測り直し、Sim の 3 手（Emit を
/// voption / 包みを struct / bind と CE を inline）で動いたので bd40cd7
/// （-30.4 / -14.0 / -13.4 / -7.1）、Sim を型の別名にした手でまた動いたので
/// いまの値へ測り直した。
///
/// **口が軸の上に乗っていることは 3 回 確かめている。** どれも、この口が
/// 予測した確保の減りと BenchmarkDotNet の実測を突き合わせた。
///   1 回め  -15.5/-29.2/-28.5/-20.2 に対し -16.0/-29.2/-28.6/-20.2
///   2 回め  -23.3/ -7.7/ -8.4/-32.9 に対し -23.3/ -7.8/ -8.5/-32.9
///   3 回め  -33.1/-12.8/-13.8/-47.1 に対し -33.1/-13.2/-14.1/-47.1
/// （3 回め は起点 c268f3c からの通し。ずれの 0.3〜0.4 ポイント は、
/// 2 つ の口が測る区間が違う分——BDN は定常状態、--alloc は下ごしらえを
/// 1 回ぶん 含む——と、バイナリが変わると動く 4,120 B の分）
let private calibratedAt = "1967613（Sim を型の別名にした版）"

let private calibration = dict [ "move", -33.4; "5way", -14.7; "10Way", -14.2; "homing", -8.9 ]

/// **記録した確保。旧 API の列が消えても残る側の物差し。**
///
/// 上の calibration は**新旧の比**なので、旧の列を落とすと土台ごと無くなる。
/// こちらは新 API の絶対値そのものを控えておいて、走行のたびに
/// 「記録からどう動いたか」を出す。
///
/// **正しさの門ではない。** Core を触れば動くのが正しく、動いたときに
/// それが手の効きなのか、無関係な変更に伴うドリフト（既知の幅 4,120 B。
/// 下の「確保は決定的」の但し書き）なのかを読むための目印。
///
/// **軸の確認はこれではできない。** 軸は「この口が出した差」と
/// 「BDN が出した Allocated の差」を突き合わせて確かめるもので、
/// それは 3 回 やって README に記録がある。**次に BDN で測れる大きさの手を
/// 打ったとき、4 回目 をやってここを更新する。**
let private baselineAt = "e831bd9（走査の書き戻しを List.updateAt にした版）"

let private baseline =
  dict [ "move",     313_360L
         "5way",   3_605_592L
         "10Way",  7_490_320L
         "homing", 4_122_872L
         "wide",   3_163_432L ]

let private alloc () =
  fixManager ()
  printfn "1 走行（60 コマ）の確保。BenchmarkDotNet を通さない即席の物差し。"
  printfn ""
  // 1 度 だけ測って、2 つ の表に使い回す。allocApi / allocOld はどちらも
  // 中で 1 回 空回ししてから測るので、ここで 2 度 呼ぶと倍の時間がかかる
  let rows =
    [ for name, suffix in scenarios do
        match Corpus.findBySuffix suffix with
        | None ->
          printfn "%-8s %s が見つかりません" name suffix
        | Some path ->
          let doc = parseXml (System.IO.File.ReadAllText path)
          yield name, allocApi doc 60, allocOld doc 60 ]

  printfn "%-8s %14s %14s %10s %9s" "台本" "新 API" "記録" "差" "差(%)"
  for name, a, _ in rows do
    match baseline.TryGetValue name with
    | true, b ->
      printfn "%-8s %12d B %12d B %+9d B %+8.2f%%"
              name a b (a - b) (float (a - b) / float b * 100.0)
    | _ ->
      // **記録の無い台本を 0 や「一致」で埋めない。** 埋めると、
      // 控えていない列を控えたものと読む
      printfn "%-8s %12d B %14s %10s %9s" name a "—" "—" "—"
  printfn ""
  printfn "記録は %s で、この口自身が出した値。" baselineAt
  printfn "**正しさの門ではない。** Core を触れば動くのが正しく、動いたときに"
  printfn "それが手の効きなのか、無関係な変更に伴うドリフト（既知の幅 4,120 B）"
  printfn "なのかを読むための目印。**手を打ったら Program.fs の baseline と"
  printfn "baselineAt を更新する。**"
  printfn ""
  printfn "--- ここから下は旧 API の列。**廃止と一緒に消える** ---"
  printfn ""
  printfn "%-8s %14s %9s %9s %8s" "台本" "旧 API" "比" "校正値" "差"
  for name, a, o in rows do
    let ratio = (float a / float o - 1.0) * 100.0
    // 校正値は BDN の物差しに載っている台本にしかない。**無いものを
    // 0 や「一致」で埋めない** —— 埋めると、校正していない列を
    // 校正済みと読んでしまう
    match calibration.TryGetValue name with
    | true, bdn -> printfn "%-8s %12d B %+8.1f%% %+8.1f%% %+7.1f" name o ratio bdn (ratio - bdn)
    | _ ->         printfn "%-8s %12d B %+8.1f%% %9s %8s" name o ratio "—" "—"
  printfn ""
  printfn "校正値が「—」の台本は BDN の物差しに載っていない（%d 本 目 以降）。" (onBdn + 1)
  printfn "**絶対値の前後比較には使えるが、比の妥当性は誰も見ていない。**"
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
  printfn "前の走行の絶対値と並べて読むこと。"
  printfn ""
  printfn "**「旧 API」の列は対照ではない。** 呼んでいる BulletRunner は旧実装ではなく、"
  printfn "旧い口を新経路の上に載せたシムで、中では Step.step を通る。**Sim / Step /"
  printfn "Domain を触れば、この列も同じだけ動く。**"
  printfn ""
  printfn "  両方 動く   → Sim / Step / Domain を触った。比が動くのは口の故障ではない"
  printfn "  新だけ動く → 新 API の口（Runner.load / stepWith / Env の組み方）だけの手"
  printfn "  旧だけ動く → BulletRunner の側だけの手。新 API には効いていない"
  printfn ""
  printfn "2 列 が同じ**量**だけ減ると比は動く（分母が違うため）。止まるのは同じ**割合**で"
  printfn "動いたときだけ。**比が動いたことを、効きの有無や口の故障の判定に使わないこと。**"

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
