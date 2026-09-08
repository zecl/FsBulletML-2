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

  /// F# の CE。**v1.9 から `Tags` を返す。**
  ///
  /// v1.6 まで空だった。理由は「参照を数える側が探すのは要素名 + label 属性で、
  /// CE はそこが DSL の名前だから」と書いてあったが、**別なのは名前の載せ方で
  /// あって名前ではない** —— `defAction "x"` の `x` は `<action label="x">` の
  /// `x` そのもの。
  ///
  /// **表はここに書かない。** どの CE 名 が何番目 の文字列に名前を載せるかは
  /// `Spec.ceLabels`、名前を載せる属性は語彙から引いた対（`References.pairs`）。
  ///
  /// 対が 1 組 も無ければ空を返す（語彙が引けていない印）——
  /// **そのときだけ v1.6 と同じ振る舞いになる。**
  let private ceTags (source: string) =
    match References.pairs |> Array.tryHead with
    | None -> []
    | Some (_, _, attr) ->
      FsharpScan.tags
        (Spec.ceLabels
         |> List.map (fun (name, element, arg, fixedName, _) -> name, element, arg, fixedName)
         |> List.toArray)
        attr
        source

  let fsharp = ofParts SourceKind.FSharpDsl Diagnosis.applyFsharp ceTags
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
