namespace FsBulletML2.Core.Tests

open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Processable

/// 227 本の実物を突き合わせる橋。突き合わせる相手は 2 種類あり、
/// 意味がまったく違う。
///
///   1. 「227 本を、公開 API と Step.step 直接呼びで突き合わせる」
///      —— TraceApi.run（Runner.load / stepWith。**出荷する経路**）と
///      TraceNew.run（Step.step を直接呼ぶ経路）はどちらも同じ Step.step を
///      指す。ここが割れるのは、公開 API の層（Runner.load / stepWith /
///      restart / Env の組み方）が Step.step の直接呼びと食い違ったとき。
///      **新旧エンジンの比較ではない。**
///
///   2. 「227 本を、凍結した旧エンジン（4077ed6）の軌跡と突き合わせる」
///      —— 9d98954 で削除された、木にミュータブルを持たせた側の実装
///      （commit 4077ed6 時点）が変動する乱数列のもとで実際に出した軌跡を
///      あらかじめ凍結してあり（tests/TestData/trace/
///      corpus-trace-varying-old-4077ed6.tsv。作り直す道具と手順は
///      tools/frozen-corpus/DumpFrozenCorpus.fs の先頭コメント）、
///      TraceApi.run（HEAD）の出力と突き合わせる。**旧エンジンはもう
///      存在しないので、これが本物の新旧を見る唯一の場所。**
///
/// **1 は 3 度 意味が変わっている。** 削除より前は基準側が「旧エンジン」
/// そのものだったので新旧の比較だった。削除後は「BulletRunner 対 Step.step
/// 直接呼び」になり、名前だけが「新旧」のまま緑で残っていた（そのとき直した）。
/// 旧 API の互換シムを落としたいま、基準側は公開 API になった。
/// **緑のまま中身の意味が入れ替わる。名前と実際に比べているものを、
/// 意味が変わるたびに一致させること。**
///
/// **旧 API を基準にしていた 2 本 は 1 本 に畳んだ。** 3 つ の書き手
/// （BulletRunner 経由 / Step.step 直接 / 公開 API）のうち 1 つ が消えたので、
/// 残る対は 1 組 しかない。畳んだぶん、`viaRunnerCache`（同じ走行を
/// 2 本 の試験で共有するための辞書）と `NonParallelizable` も要らなくなった
/// —— グローバル可変（BulletMLManager）を触らなくなったため。
///
/// RunBoth が返す、突き合わせの結果。文字列（Text）だけだと「割れ 0」の
/// 部分文字列一致でしか門を書けず、220 本が同じ例外へ吸われて
/// 「一致 3 / 割れ 0 / 比べられず 224」のような形になっても
/// "割れ 0 " を含み "一致 0 " を含まないので緑のまま通ってしまう。
/// Ok / Ng / Skipped / Total を数として持たせ、呼ぶ側が実測した値そのものを
/// 門にできるようにする
type BridgeReport =
  { Ok : int
    Ng : int
    Skipped : int
    Total : int
    Text : string }

[<TestFixture>]
type Equivalence() =

  /// 走らせ方を 2 つ受け取り、227 本ぜんぶを突き合わせた報告を返す。
  /// どちらも「いまのソースを実際に走らせる」関数であることが前提
  /// （凍結データとの突き合わせには使わない。下の 2 番めの橋を参照）。
  ///
  /// 片方を先に評価して例外が飛ぶと、`let a = runA xml; let b = runB xml` の
  /// 書き方では b 側はそもそも評価されない。それだと「両方が同じ理由で
  /// 落ちた」と「片方だけ落ちた」の区別がつかない（後者は退行そのもの
  /// かもしれないのに、両方まとめて「比べられず」に吸われてしまう）。
  /// なので両方を独立に受けてから 4 通りに分ける:
  ///
  ///   両方成功         -> Divergence.firstDivergence で軌跡を比べる
  ///   両方とも同じ例外 -> 比べられず（本当に比べようがなかっただけ）
  ///   両方とも違う例外 -> 割れ
  ///   片方だけ例外     -> 割れ（片方が壊れた・片方だけ直った、のどちらもここ）
  static member RunBoth (runA: string -> string) (runB: string -> string) : BridgeReport =
    Equivalence.RunBothWith (fun _path xml -> runA xml) runB

  /// 基準側（A）に**パスも渡す**形。中身は RunBoth と同じ。
  ///
  /// **同じ基準を複数の試験が使うとき、走行を 1 回 で済ませるため。**
  /// パスをキーにできるので、呼ぶ側が軌跡をキャッシュできる
  /// （xml の中身をキーにすると 227 本 ぶんの文字列を毎回 ハッシュすることになる）。
  static member RunBothWith (runA: string -> string -> string) (runB: string -> string) : BridgeReport =
    let samples = CorpusData.uniqueSamples ()
    let mutable ok, ng, skipped = 0, 0, 0
    let diffs = StringBuilder()
    // 比べられず（両方が同じ例外で落ちた）の中身。数だけでは「何が」
    // 比べられなかったのか報告から読み戻せない
    let skippedNames = ResizeArray<string>()
    let tryRun (f: string -> string) xml =
      try Ok (f xml) with e -> Error (Equivalence.RenderException e)
    for path in samples do
      let name = CorpusData.relative path
      let xml = File.ReadAllText path
      match tryRun (runA path) xml, tryRun runB xml with
      | Ok a, Ok b ->
          match Divergence.firstDivergence a b with
          | None -> ok <- ok + 1
          | Some msg ->
              ng <- ng + 1
              diffs.AppendLine(sprintf "%s\n%s" name msg) |> ignore
      | Error ea, Error eb when ea = eb ->
          skipped <- skipped + 1
          skippedNames.Add(sprintf "%s（%s）" name ea)
      | Error ea, Error eb ->
          ng <- ng + 1
          diffs.AppendLine(sprintf "%s\n両方とも例外で落ちたが中身が違う\n  A: %s\n  B: %s" name ea eb) |> ignore
      | Error ea, Ok _ ->
          ng <- ng + 1
          diffs.AppendLine(sprintf "%s\n片方だけ例外で落ちた\n  A: %s\n  B: 例外なし" name ea) |> ignore
      | Ok _, Error eb ->
          ng <- ng + 1
          diffs.AppendLine(sprintf "%s\n片方だけ例外で落ちた\n  A: 例外なし\n  B: %s" name eb) |> ignore
    let skippedBlock =
      if skippedNames.Count = 0 then ""
      else
        "比べられず（両方とも同じ例外で落ちた）:\n"
        + (skippedNames |> Seq.map (sprintf "  %s") |> String.concat "\n")
        + "\n"
    let text =
      sprintf "一致 %d / 割れ %d / 比べられず %d（母数 %d）\n%s%s"
        ok ng skipped (List.length samples) skippedBlock (diffs.ToString())
    { Ok = ok; Ng = ng; Skipped = skipped; Total = List.length samples; Text = text }

  /// 例外の中身を、比べられて・表示もできる 1 行の文字列に畳む。
  /// 凍結データ（tools/frozen-corpus/DumpFrozenCorpus.fs、4077ed6 側の
  /// worktree で走らせたもの。使い方はそのファイルの先頭コメントに書いてある）
  /// もまったく同じ式で例外を文字列化してある。式がずれると
  /// 「同じ例外」の判定そのものがずれる
  static member RenderException (e: exn) =
    let rec inner (x: exn) = if isNull x.InnerException then x else inner x.InnerException
    let i = inner e
    sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))

  /// 軌跡そのものではなく、名前・撃った数・生存数・指紋の 4 つに畳む。
  /// 凍結データ（corpus-trace-varying-old-4077ed6.tsv）もこの畳み方で
  /// 作ってある。畳み方がずれると指紋が一致するはずのものまで割れる。
  ///
  /// 軌跡をまるごと残さないのは大きさのため。凍結を作る際に試したところ、
  /// times の大きい repeat が定数の $rand と違って早期に打ち切られず、
  /// 5way.xml 1 本だけで 140 万行・100MB を超えた（Corpus.fs の
  /// corpus-trace が指紋だけを控えに残しているのと同じ理由）
  static member private FoldTrace (t: string) =
    let lines = t.Split('\n')
    let fired = lines |> Array.filter (fun l -> l.Contains "  +b") |> Array.length
    let alive =
      match lines |> Array.tryFindIndexBack (fun l -> l.StartsWith "f") with
      | Some i -> lines.[i + 1 ..] |> Array.filter (fun l -> l.StartsWith "  b") |> Array.length
      | None -> 0
    use h = SHA256.Create()
    let digest =
      h.ComputeHash(Encoding.UTF8.GetBytes t)
      |> Array.take 6
      |> Array.map (fun b -> b.ToString("x2"))
      |> String.concat ""
    fired, alive, digest

  /// 凍結データを読む。先頭は `#` で始まる由来の控え（元コミット・
  /// 走らせたパラメータ・作った道具・読む側 —— この関数）で、そのあと
  /// 1 行 1 本、タブ区切りで
  ///   成功  名前 \t 撃った数 \t 生存数 \t 指紋
  ///   例外  名前 \t ERROR \t 型名: メッセージ
  ///
  /// 作り直し方・踏んだ 2 つの罠は
  /// tools/frozen-corpus/DumpFrozenCorpus.fs の先頭コメントに書いてある
  static member private LoadFrozenVarying () =
    let path =
      Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "TestData", "trace",
                                     "corpus-trace-varying-old-4077ed6.tsv"))
    File.ReadAllLines path
    |> Array.filter (fun l -> l.Trim() <> "" && not (l.StartsWith "#"))
    |> Array.map (fun line ->
        let cols = line.Split('\t')
        let name = cols.[0]
        if cols.Length >= 3 && cols.[1] = "ERROR" then
          name, Error cols.[2]
        else
          name, Ok (int cols.[1], int cols.[2], cols.[3]))
    |> Map.ofArray

  /// **公開 API だけで 227 本 が走り、Step.step 直接呼びと一致する。**
  ///
  /// TraceApi は internal を 1 つも使わない（あちらの docstring 参照）ので、
  /// これが緑ということは「フロントは公開の型だけで弾幕を走らせられる」
  /// ということ。旧 API では 19 メンバ の `IBulletmlObject` を実装する必要があった。
  ///
  /// 定数の $rand なので「引く回数・引く順」の割れは見えない
  /// （下のテストの docstring 参照）。回数の割れは下の橋（凍結した旧エンジン
  /// との突き合わせ）が見る。
  [<Test>]
  member _.``227 本を、公開 API と Step.step 直接呼びで突き合わせると全部 一致する``() =
    let rand, rank, px, py = 0.5f, 0.5f, 30.0f, 100.0f
    let viaApi (xml: string) = TraceApi.run (fun () -> rand) rank px py xml 60
    let direct (xml: string) = TraceNew.run (fun () -> rand) rank px py xml 60
    let report = Equivalence.RunBoth viaApi direct
    TestContext.WriteLine report.Text
    // 部分文字列一致（"割れ 0 " を含み "一致 0 " を含まない）だけだと、220 本が
    // 同じ例外へ吸われて「一致 3 / 割れ 0 / 比べられず 224」になっても
    // 通ってしまう。実測した数そのものを門にする
    report.Total |> should equal 227
    report.Ok |> should equal 224
    report.Ng |> should equal 0
    report.Skipped |> should equal 3

  /// 定数の $rand（上のテスト）は「引く回数・引く順」の割れを見せない。
  /// getValue は式の中身に関わらず env.Rand() を呼ぶが、FixedManager は
  /// 何回・どの順で呼ばれても同じ値しか返さないので、引きが 1 つ 足りない・
  /// 多い・入れ替わっているという不具合があっても軌跡の値には出ない。
  ///
  /// ここは HEAD（TraceApi.run。**公開 API の経路**）を SharedRandomStream で
  /// 走らせ、旧エンジン（4077ed6）が同じ乱数列のもとで実際に出した軌跡
  /// （凍結データ）と突き合わせる。新エンジン同士の突き合わせではない
  /// —— 旧エンジンはもう存在しないので、これが本物の新旧を見る唯一の場所
  /// （クラスの docstring 参照）。
  ///
  /// **基準側は Trace.run（BulletRunner 経由の旧い口）から付け替えてある。**
  /// 互換の口を落とすと Trace.run が消えるので、**消す前に**この門を旧い口から
  /// 切り離した。付け替えても軌跡が変わらないことは、当時まだ生きていた橋
  /// （Trace.run と TraceApi.run が 227 本 で一致する）で実測してある。
  /// **順が逆だと、付け替えが正しいことを確かめる相手が居なくなっていた。**
  ///
  /// 較正: rootProgress の Wait の腕を外す（木を組む段の wait の引きを
  /// 丸ごと消し、Progress.initial に落とす）と、このテストは
  /// 一致 110 / 割れ 114 / 比べられず 3（母数 227）になる。実測して確認した
  /// —— 2wayLeft.xml の誤植（bulletml 開始タグの空白抜け）を直して
  /// 比べられず 4 本のうち 1 本が実際に比べられるようになったぶん、
  /// 一致・比べられず が 1 ずつ動いた（109→110、4→3）。割れ（114）は
  /// 動いていない——2wayLeft.xml はこの変異のもとでも旧新が一致し続ける
  /// （root 直下に wait を持たない台本なので、この変異の影響を受けない）。
  /// 定数 $rand の橋（上のテスト）はこの変異でも 224/0/3 のまま緑で、
  /// この変異を検出できない。このテストが赤いままなら、まずここ
  /// （rootProgress / resetChild が実際に呼ばれているか）を疑うこと
  [<Test>]
  member _.``227 本を、凍結した旧エンジン（4077ed6）の軌跡と突き合わせると全部 一致する``() =
    let rank, px, py = 0.5f, 30.0f, 100.0f
    let stream = SharedRandomStream()
    let samples = CorpusData.uniqueSamples ()
    let frozen = Equivalence.LoadFrozenVarying ()
    let mutable ok, ng, skipped = 0, 0, 0
    let diffs = StringBuilder()
    // 比べられず（両方が同じ例外で落ちた）の中身。数だけでは読み戻せない
    let skippedNames = ResizeArray<string>()
    for path in samples do
      let name = CorpusData.relative path
      let xml = File.ReadAllText path
      stream.Reset()
      let newResult =
        try Ok (TraceApi.run (fun () -> stream.Next()) rank px py xml 60)
        with e -> Error (Equivalence.RenderException e)
      match Map.tryFind name frozen, newResult with
      | None, _ ->
          ng <- ng + 1
          diffs.AppendLine(
            sprintf "%s\n凍結データに無い名前。サンプルが増減した？ %s を作り直すこと"
              name "corpus-trace-varying-old-4077ed6.tsv") |> ignore
      | Some (Error oldMsg), Error newMsg when oldMsg = newMsg ->
          skipped <- skipped + 1
          skippedNames.Add(sprintf "%s（%s）" name oldMsg)
      | Some (Error oldMsg), Error newMsg ->
          ng <- ng + 1
          diffs.AppendLine(
            sprintf "%s\n両方とも例外で落ちたが中身が違う\n  旧(4077ed6): %s\n  新(HEAD):    %s"
              name oldMsg newMsg) |> ignore
      | Some (Error oldMsg), Ok _ ->
          ng <- ng + 1
          diffs.AppendLine(
            sprintf "%s\n片方だけ例外で落ちた\n  旧(4077ed6): %s\n  新(HEAD):    例外なし"
              name oldMsg) |> ignore
      | Some (Ok (oldFired, oldAlive, oldDigest)), Error newMsg ->
          ng <- ng + 1
          diffs.AppendLine(
            sprintf "%s\n片方だけ例外で落ちた\n  旧(4077ed6): 例外なし（撃った %d 発 残り %d 本 %s）\n  新(HEAD):    %s"
              name oldFired oldAlive oldDigest newMsg) |> ignore
      | Some (Ok (oldFired, oldAlive, oldDigest)), Ok newText ->
          let newFired, newAlive, newDigest = Equivalence.FoldTrace newText
          if newDigest = oldDigest then ok <- ok + 1
          else
            ng <- ng + 1
            diffs.AppendLine(
              sprintf "%s\n  旧(4077ed6) 撃った %4d 発 残り %3d 本 %s\n  新(HEAD)    撃った %4d 発 残り %3d 本 %s"
                name oldFired oldAlive oldDigest newFired newAlive newDigest) |> ignore
    let skippedBlock =
      if skippedNames.Count = 0 then ""
      else
        "比べられず（両方とも同じ例外で落ちた）:\n"
        + (skippedNames |> Seq.map (sprintf "  %s") |> String.concat "\n")
        + "\n"
    let report =
      sprintf "一致 %d / 割れ %d / 比べられず %d（母数 %d）\n%s%s"
        ok ng skipped (List.length samples) skippedBlock (diffs.ToString())
    TestContext.WriteLine report
    // 部分文字列一致だけだと、220 本が同じ例外へ吸われて
    // 「一致 3 / 割れ 0 / 比べられず 224」になっても通ってしまう
    // （設計文書 5.4 参照）。実測した数そのものを門にする
    List.length samples |> should equal 227
    ok |> should equal 224
    ng |> should equal 0
    skipped |> should equal 3

  [<Test>]
  member _.``橋は、1 か所だけ違う軌跡を見つける``() =
    let run (xml: string) = TraceApi.run (fun () -> 0.5f) 0.5f 30.0f 100.0f xml 60
    // 片方の軌跡の、真ん中あたりの 1 行だけを書き換える
    let broken (xml: string) =
      let lines = (run xml).Split('\n')
      let i = lines |> Array.tryFindIndex (fun l -> l.StartsWith "  b")
      match i with
      | Some i -> lines.[i] <- lines.[i] + " (細工)"
                  System.String.Join("\n", lines)
      | None -> run xml
    let report = Equivalence.RunBoth run broken
    report.Ng |> should be (greaterThan 0)
    report.Text |> should haveSubstring "(細工)"
