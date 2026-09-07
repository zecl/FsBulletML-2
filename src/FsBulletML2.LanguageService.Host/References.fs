namespace FsBulletML2.LanguageService

open System

/// **参照の欠けを、本文の字から全部 数える。**
///
/// Core は最初の 1 件 で `raise` して止まり、しかも位置を持たない
/// （`Bulletml` の DU に行番号が無い）。だから「無い label」を全部 波線に
/// するには、本文の側から数えるしかない。
///
/// **ここは Core より広い。** Core は `top` から到達した要素しか展開しないので、
/// 誰からも参照されていない枝の中の壊れた参照は素通りする（コーパスの
/// `readTest.xml` が実際にそう）。ここは到達を見ないので、そういう参照も挙げる。
/// **だから単独では使わない** —— `explain` が Core の判定を真として使う。
///
/// 判定を作り直している以上は二重化なので、コーパスで突き合わせる
/// （`Parser.Tests/ReferenceScan.fs`）——
///
///   - **本番と同じ道**（`explain`）で、Apply が通る弾幕には 1 本 も引かないこと
///   - 参照を 1 つ 壊して Core が落ちたとき、同じ名前をここも挙げること
///
/// **`internal` を外した（v0.8）。** 前は Playground の中に在り、
/// `Parser.Tests` が `Link` で借りて**同じ assembly として** compile して
/// いたので `internal` でも見えていた。器へ移して assembly が分かれた時点で
/// 見えなくなる —— **移して初めて出る形**で、build が赤くなって分かった。
module References =

  // --- 走る先は語彙から引く -------------------------------------------------

  /// 参照する要素 -> 参照される要素と、label の属性名。**表を持たない。**
  ///
  /// **規則は `Refs.pairs`（器の側）に 1 本。** v1.2 で移した ——
  /// ブラウザ側の rename が同じ規則を要るので、こちらに置いたままだと
  /// 2 か所 になる。**片方 だけ直すと「波線は出るのに rename は当たらない」
  /// という形になり、どちらも単独では正しく見える。**
  ///
  /// ここが持つのは、host の語彙をその規則に渡せる形へ落とすところだけ。
  ///
  /// **空なら呼ぶ側が何も挙げない**（reflection が効いていない印。
  /// `Parser.Tests` が 0 件 を赤にする）
  let pairs: (string * string * string)[] =
    Vocabulary.elements
    |> Array.toList
    |> List.map (fun e -> e.Name, (e.Attrs |> Array.toList |> List.map (fun a -> a.Name)))
    |> Refs.pairs
    |> List.toArray

  // --- 定義に無い参照 -------------------------------------------------------

  /// 定義に無い参照を全部。**本文に出てくる順**で返す。
  ///
  /// 同じ名前を 2 回 参照していたら 2 本 引く —— どちらも直す先なので。
  ///
  /// **字を数えるところは受け取る（v0.9）。** 前は `XmlScan.tags` を直に
  /// 呼んでいて、**器が 2 つ に割れても host 側は XML を名指ししたまま**だった。
  /// 名指しでも困らなかったのは実装が 1 本 しか無かったからで、
  /// **実装が 1 本 のうちは、抽象が足りないことが分からない。**
  ///
  /// 受け取るのは `ISourceReader.Tags`。表記ごとの 1 本 はあちらに束ねてある
  ///
  /// **数えるところは `Refs.missing`（器の側）に 1 本。** v1.3 で移した ——
  /// ブラウザ側の Quick Fix が同じ数え方を要る。ここが持つのは
  /// **人へ見せる文面**だけで、あちらは文面を持たない
  /// （持たせると波線と直し方で文面が割れ、どちらも単独では正しく見える）。
  let missing (scan: string -> TagHit list) (src: string) : Failure list =
    if pairs.Length = 0 then []
    else
      Refs.missing (List.ofArray pairs) (scan src)
      |> List.map (fun m ->
           { Line = m.Hit.Line
             Column = m.Hit.Column
             EndColumn = m.Hit.EndColumn
             Message = sprintf "%s が指す %s が無い: %s" m.RefName m.DefName m.Hit.Value })

  /// Apply の答えを、波線にできる形へ。**Core の判定が真。**
  ///
  /// 位置が在る層（XML の構文）は本文が読めていないので、そのまま 1 本。
  /// 位置が無い層のときだけ本文の字から数え直して位置を足す。
  /// **0 件 なら理由をそのまま位置なしで出す** —— 参照以外の層（式、輪、
  /// `top` が無い）はここでは何も足せない。
  ///
  /// `Main.fs` と `Parser.Tests` が同じこれを通る。**本番と別の組み合わせを
  /// 試験の側で組まない** —— 組むとそちらだけが緑になる。
  let explain (scan: string -> TagHit list) (failure: Failure option) (src: string) : Failure list =
    match failure with
    | None -> []
    | Some f when f.Line > 0 -> [ f ]
    | Some f ->
      match missing scan src with
      | [] -> [ f ]
      | ms -> ms
