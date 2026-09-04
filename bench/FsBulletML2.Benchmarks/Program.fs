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
/// 「死」は step に入ったが、生きている top が 1 本 も無かった呼び出し。
/// 死んだコマにしか効かない変更（たとえば aim を組まずに 0 で済ませる
/// 早道。`BulletRun.HasNoScript` の枝）は、死が 0 の台本では原理的に効かない。
/// そこで数が動いていたら、それは効きではなく走行間の台の動き。
///
/// 効きの出どころが分かっていない変更では、逆に「死が 0 の台本」を対照として
/// 使える。対照が同じだけ動いていたら、対象の動きも台のもの。
let private counts () =
  fixManager ()
  printfn "台本ごとの step 呼び出し。60 コマ。"
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

/// **記録した確保。旧 API の列が消えたあとの物差し。**
///
/// 新 API の絶対値そのものを控えておいて、走行のたびに
/// 「記録からどう動いたか」を出す。
///
/// **正しさの門ではない。** Core を触れば動くのが正しく、動いたときに
/// それが手の効きなのか、無関係な変更に伴うドリフト（既知の幅 4,120 B。
/// 下の「確保は決定的」の但し書き）なのかを読むための目印。
///
/// **軸の確認はこれではできない。** 軸は「この口が出した差」と
/// 「BDN が出した Allocated の差」を突き合わせて確かめるもので、
/// それは 3 回 やって README に記録がある（上の但し書き）。**次に BDN で
/// 測れる大きさの手を打ったとき、4 回目 をやってここを更新する。**
/// **取り直した。前の記録との差は、エンジンではなく測定器が動いた分。**
///
/// 旧 API を落として `FakeBullet` から 19 メンバ の面が消え、弾 1 個 が
/// **40 B 小さくなった**。この口の確保には測定器の弾も入るので、5 本 とも下がる。
/// 掛け算で合う ——
///
///     move    弾   1  -40 B        1 x 40 = 40      ちょうど
///     5way    弾 301  -12,040 B  301 x 40 = 12,040  ちょうど
///     10Way   弾 601  -24,040 B  601 x 40 = 24,040  ちょうど
///     homing  弾  34  -2,032 B    34 x 40 = 1,360   +672 は下の雑音
///     wide    弾 131  -5,624 B   131 x 40 = 5,240   +384 は下の雑音
///
/// **残差 672 / 384 / 312 は、旧 API を落とす前から同じ台本に出ていた幅**
/// （Harness.allocOf の段階的 JIT の但し書き）。だから残差は新しい現象ではない。
///
/// 下の値は 3 回 走らせた最頻値。**±400 B 程度 は雑音**なので、
/// そこを追いかけて更新しないこと。
let private baselineAt = "572086a（旧 API 廃止。BulletRunner を消し、FakeBullet を小さくした版）"

let private baseline =
  dict [ "move",     313_320L
         "5way",   3_593_552L
         "10Way",  7_465_968L
         "homing", 4_120_840L
         "wide",   3_157_808L ]

/// 1 走行の確保を、BenchmarkDotNet を通さずに出す。**数秒 で終わる。**
///
/// 構造を 1 手 変えるたびに 90 分 の走行を回せないので、確保だけを即席で測る。
/// **時間はここでは測れない。**
///
/// **旧 API の列（と、その比で合わせていた校正値）は落とした。**
/// 校正の土台にしていた `calibration` は**新旧の比**だったが、その 2 列 は
/// 独立した実装ではなく大部分が同じコードを通っていた。いまの物差しは
/// `baseline`（新 API の絶対値を控えたもの）からの差。
///
/// **口が軸の上に乗っていることは 3 回 確かめてある。** どれも、この口が
/// 予測した確保の減りと BenchmarkDotNet の実測を突き合わせた。
///   1 回め  -15.5/-29.2/-28.5/-20.2 に対し -16.0/-29.2/-28.6/-20.2
///   2 回め  -23.3/ -7.7/ -8.4/-32.9 に対し -23.3/ -7.8/ -8.5/-32.9
///   3 回め  -33.1/-12.8/-13.8/-47.1 に対し -33.1/-13.2/-14.1/-47.1
/// （3 回め は起点 c268f3c からの通し。ずれの 0.3〜0.4 ポイント は、
/// 2 つ の口が測る区間が違う分——BDN は定常状態、--alloc は下ごしらえを
/// 1 回ぶん 含む——と、バイナリが変わると動く 4,120 B の分）
///
/// **この 3 回 はどれも旧の列に依らない**（口の差 vs BDN の Allocated の差）。
/// だから旧を落としても軸の確認は生きている。**次に BDN で測れる大きさの
/// 手を打ったとき、4 回目 をやること。**
let private alloc () =
  fixManager ()
  printfn "1 走行（60 コマ）の確保。BenchmarkDotNet を通さない即席の物差し。"
  printfn ""
  // **1 巡 空けてから測る。** allocOf は呼びごとに 1 回 空回ししているが、
  // それだけでは足りない —— 1 巡目 は move が +6,192 B（2.0%）動いたことが
  // ある。1 巡 空けると 0.1% 未満まで下がる（0 にはならない。原因は
  // 段階的 JIT。Harness.allocOf の但し書き）。
  // **捨てないと、いちばん軽い move（313 KB）では 2% の嘘になる。**
  let measure () =
    [ for name, suffix in scenarios do
        match Corpus.findBySuffix suffix with
        | None -> ()
        | Some path ->
          let doc = parseXml (System.IO.File.ReadAllText path)
          yield name, allocApi doc 60, bulletCount doc 60 ]

  measure () |> ignore
  let rows = measure ()
  for name, suffix in scenarios do
    if (Corpus.findBySuffix suffix).IsNone then
      printfn "%-8s %s が見つかりません" name suffix

  printfn "%-8s %14s %14s %10s %9s %7s %9s" "台本" "新 API" "記録" "差" "差(%)" "弾" "差/弾"
  for name, a, n in rows do
    match baseline.TryGetValue name with
    | true, b ->
      let d = a - b
      let perBullet = if n = 0 then "—" else sprintf "%+.1f B" (float d / float n)
      printfn "%-8s %12d B %12d B %+9d B %+8.2f%% %7d %9s"
              name a b d (float d / float b * 100.0) n perBullet
    | _ ->
      // **記録の無い台本を 0 や「一致」で埋めない。** 埋めると、
      // 控えていない列を控えたものと読む
      printfn "%-8s %12d B %14s %10s %9s %7d %9s" name a "—" "—" "—" n "—"
  printfn ""
  // 0 件 を緑にしない。台本が 1 本 も見つからなければ表は空のまま通る
  printfn "測った台本 %d 本（%d 本 中）。0 なら測れていない。" (List.length rows) (List.length scenarios)
  printfn "うち %d 本 は BDN の物差しにも載っている（%d 本 目 以降は確保だけ）。" onBdn (onBdn + 1)
  printfn ""
  printfn "**「差/弾」が 5 本 とも同じ値なら、動いたのは測定器のほう。**"
  printfn "この口の確保には、測定器自身が作る FakeBullet のぶんも入っている。"
  printfn "実際に踏んだ —— FakeBullet から旧 API の面を落としただけで 5 本 とも下がった。"
  printfn ""
  printfn "**1 巡 空けて 2 巡目 を出している。** それでも再現性は 0 にならない ——"
  printfn "段階的 JIT の段が走行ごとに変わりうるため（実測: 1 巡だと move が 2.0%%、"
  printfn "2 巡だと 0.1%% 未満）。**0.1%% 程度の差は雑音。1%% 未満は読まないこと。**"
  printfn ""
  printfn "記録は %s で、この口自身が出した値。" baselineAt
  printfn "**正しさの門ではない。** Core を触れば動くのが正しく、動いたときに"
  printfn "それが手の効きなのか、無関係な変更に伴うドリフト（既知の幅 4,120 B）"
  printfn "なのかを読むための目印。**手を打ったら Program.fs の baseline と"
  printfn "baselineAt を更新する。**"
  printfn ""
  printfn "**絶対値は BenchmarkDotNet の Allocated と一致しない。** あちらは"
  printfn "ウォームアップ後の定常状態を測り、こちらは下ごしらえ（木を組む段）を"
  printfn "1 回ぶん 含む。使うのは差だけ。"
  printfn ""
  printfn "**旧 API の列は落とした。** 対照に見えて対照ではなかった —— 旧い口を"
  printfn "新経路の上に載せたシムで、中では同じ Step.step を通っていた。"
  printfn "軸（この口の差 vs BDN の Allocated の差）は 3 回 確かめてあり、"
  printfn "どれも旧の列に依っていない。README の「測るときの約束」を見ること。"

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
