namespace FsBulletML2.LanguageService

open FsBulletML2

/// 表記ごとの「読む口」。**`ISourceLanguage` の、host 側の相棒。**
///
/// --- なぜ v0.9 で生まれたか
///
/// v0.3 からの主張は「次の表記を足すとき、**触るのは言語モジュール 1 個**」で、
/// その口が `ISourceLanguage`（器の側）。ブラウザ側はそれで足りている ——
/// sxml で足した口は 0 個。
///
/// **だが host 側には口が無かった。** v0.8 で `Diagnosis` と `References` が
/// この proj へ移ったが、どちらも XML を名指ししたままだった ——
///
///     Diagnosis.apply     tryReadXmlString  を直に呼ぶ
///     References.missing  XmlScan.tags      を直に呼ぶ
///
/// **器が 2 つ に割れたとき、口も 2 つ に割れるべきだった。**
/// 名指しでも誰も困らなかったのは、実装が 1 本 しか無かったから ——
/// **実装が 1 本 のうちは、抽象が足りないことが分からない。**
///
/// --- なぜ Fable 側に置けないか
///
/// 読む段は `Parser` を要る。器（Fable が焼く側）が Parser を参照すると
/// FParsec で焼けなくなる（v0.8 の実測）。**だから 2 つ 目 の口になる。**
type ISourceReader =
  abstract Kind: SourceKind
  /// 読んで、載せる。**載せるところは呼ぶ側が渡す** ——
  /// host は `Playfield` を作り、試験は `Runner.load` だけを通す。
  /// 返りが `None` なら成功
  abstract Apply: build: (Bulletml -> unit) -> source: string -> Failure option
  /// 参照の欠けを数えるための開始札。**表記ごとに字の数え方が違う**ので
  /// ここから渡す（`References.missing` が受け取る側）
  abstract Tags: source: string -> TagHit list

/// 表記ごとの実体。**中身は 1 行 ずつ** —— 層分けは `Diagnosis`、
/// 字を数えるのは器のスキャナ。ここが持つのは「どれとどれが対か」だけ
module SourceReader =

  let private ofParts kind apply tags =
    { new ISourceReader with
        member _.Kind = kind
        member _.Apply build source = apply build source
        member _.Tags source = tags source }

  let xml = ofParts SourceKind.Xml Diagnosis.apply XmlScan.tags
  let sxml = ofParts SourceKind.Sxml Diagnosis.applySxml SxmlScan.tags

  /// F# の CE。**`Tags` は空。**
  ///
  /// 参照の欠けを本文の字から数える側（`References.missing`）が探すのは
  /// 「要素名 + label 属性」の形。CE はそこが DSL の名前で書かれていて
  /// （`defAction "x"` / `actionRef "x" []`）、**要素名とは別の語彙**になる。
  ///
  /// **空だと決めてある。** そのぶん CE では参照の波線が出ず、出るのは
  /// 構文の位置と、Core が落ちた理由（位置なし）——
  /// **黙って 0 件 になっているのではない**ことを
  /// `Parser.Tests/FsharpCeCorpus.fs` が固定している
  let fsharp = ofParts SourceKind.FSharpDsl Diagnosis.applyFsharp (fun _ -> [])
  let fsb = ofParts SourceKind.Fsb Diagnosis.applyFsb FsbScan.tags

  /// 読める表記。**`SourceKind.all` と全部 揃った**（v1.1）。
  ///
  /// **揃ったからといって、この 2 本 を 1 本 にしない。** あちらは「どの表記か」で、
  /// こちらは「読む口が在るか」—— 次に表記を足すとき、また割れる。
  /// 揃っていない状態を人へ見せる口は `ApplySource` の側に残してある
  /// （`未対応: …`）
  /// 並びは `SourceKind.all` と同じ。**揃えておかないと、
  /// 「どちらの並びを見た数か」で数え方が割れる**
  let all: ISourceReader list = [ xml; sxml; fsb; fsharp ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun r -> r.Kind = kind)
