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

  /// 読める表記。**`SourceKind.all` と揃っていない** —— fsb と F# CE は
  /// まだ読む口を置いていない。揃っていないことを人へ見せるのは
  /// `ApplySource` の側（`未対応: …`）
  let all: ISourceReader list = [ xml; sxml ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun r -> r.Kind = kind)
