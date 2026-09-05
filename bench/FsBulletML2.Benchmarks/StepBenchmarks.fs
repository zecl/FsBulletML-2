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
///
/// XML を読む段は GlobalSetup で測定区間の外へ出してある（move で 8.0 us）。
/// **下ごしらえ（Runner.load）の段は中に残っている。**
///
/// 外した分と、残っている分（SetupBenchmarks が走行と同じ物差しで直に測る）:
///
///                            move          5way
///   XML を読んで木にする      8.0 us        7.3 us   外した
///   task を組む             31.9 us       16.8 us   残っている
///   走行こみの通し         150.1 us     1,526 us
///   残っている分の割合       21.3%          1.1%
///
/// **この表の「task を組む」は旧 API の `prepare` を測った数。**
/// 旧の列を落としたので、いまここが測っているのは `prepareApi`
/// （`Runner.load`）で、**別の関数**。まだ測り直していないので、
/// 上の 31.9 us / 16.8 us と割合の 3 行 は、次に BDN を回すまで古い。
/// **測り直したらこの但し書きごと差し替えること。**
///
/// 同じ「1 回 の下ごしらえ」が台本の重さで 21.3% にも 1.1% にもなるので、
/// 走行に効く変更は move では薄まって見える。**move で読める効きは、
/// 走行の 2 割 以上 を動かすものだけ。**
///
/// task を組む段も外そうとして IterationSetup を試したが、BenchmarkDotNet が
/// InvocationCount=1 / UnrollFactor=1 へ落ちて走行そのものが重くなった
/// （move 159 → 223 us、5way 1.62 → 3.49 ms、homing 2.61 → 7.05 ms、
/// 5way の StdErr は 0.71% → 3.20%）。31.9 us を外すために走行に 1.4〜2.7 倍 の
/// 枷を付けることになるので、**外さずに大きさを書いて置く**ほうを採った。
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

  /// 読んだ木を走行のあいだ使い回す。**使い回して答えが変わらないことを
  /// Setup で確かめてから返す**（読み直した木との弾数の一致）。
  /// 黙って通ると、2 回目 以降だけ違うものを測っていても誰も気づかない。
  ///
  /// **「新 API と旧 API で弾数が一致する」の照合は落とした。** 旧の列を
  /// 消したので相手が居ない。値そのものの一致は tests の橋 227 本 が
  /// 見ている（あちらは凍結した旧エンジンの軌跡との突き合わせで、
  /// ここに在ったシム同士の照合より強い網）
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

  // 新 API（Runner.step）。**出荷する経路で、いまはこれだけ。**
  //
  // 旧 API（BulletRunner.run）の 4 本 は落とした。同じプロセスで並べる
  // ためだけに残していたが、**対照ではなかった** —— 旧い口を新経路の上に
  // 載せたシムで、中では同じ Step.step を通っていた。
  [<Benchmark(Description = "move（撃たない）")>]
  member _.Move() = runPreparedApi (prepareApi move) 60

  [<Benchmark(Description = "5way（300 発）")>]
  member _.Way5() = runPreparedApi (prepareApi way5) 60

  [<Benchmark(Description = "10Way（600 発）")>]
  member _.Way10() = runPreparedApi (prepareApi way10) 60

  [<Benchmark(Description = "homing laser（毎コマ 引き直す）")>]
  member _.Homing() = runPreparedApi (prepareApi homing) 60


/// 下ごしらえが、いくら掛かるのか。
///
/// StepBenchmarks と同じ物差しで測るためにここに置く。**大きさを知らないまま
/// 外すと、次に誰かが「入れても大差ない」と戻してしまう。** 走行と並べて読む。
///
/// 外した分（XML を読む）と残っている分（Runner.load）を**それぞれ直に**測る。
/// 引き算で出さない —— 2 つ の数の差はどちらの誤差も乗るうえ、引く相手を
/// 間違えてももっともらしい数になる。実際、引き算では move 34.2 us と出たが、
/// 直に測ると 31.9 us だった。
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

  /// Env を 1 回 組む費用。aim 4 本 の atan2 がここ。
  ///
  /// **Env を遅延にしたときの天井 = これ × --counts の「aim を組む」回数。**
  /// 遅延にしても実際に読まれるぶんは残るので、その積は上界。
  /// 掛け算で出せる形にしてあるのは、伸びしろを「たぶん小さい」で
  /// 判断しないため。
  ///
  /// **「Env 構築」の列を掛けてはいけない。** あちらは `HasNoScript` を
  /// 通さずに数えた回数で、計時している経路は通す。掛けると 5way で
  /// 60 倍 になる（`--counts` の末尾の但し書き）
  [<Benchmark(Description = "Env を 1 回 組む（aim 4 本）")>]
  member _.EnvBuild() = envCost 12.0f 34.0f


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
