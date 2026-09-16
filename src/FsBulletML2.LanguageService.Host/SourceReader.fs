namespace FsBulletML2.LanguageService

open FsBulletML2

/// 表記ごとの「読む口」。`ISourceLanguage` の、host 側の相棒。
///
/// Fable 側に置けない —— 読む段は `Parser` を要り、器が Parser を
/// 参照すると FParsec で焼けなくなる（v0.8 の実測）。
type ISourceReader =
  abstract Kind: SourceKind
  /// 読んで、載せる。載せるところは呼ぶ側が渡す ——
  /// host は `Playfield` を作り、試験は `Runner.load` だけを通す。
  /// 返りが `None` なら成功
  abstract Apply: build: (Bulletml -> unit) -> source: string -> Failure option
  /// 参照の欠けを数えるための開始札。表記ごとに字の数え方が違うので
  /// ここから渡す（`References.missing` が受け取る側）
  abstract Tags: source: string -> TagHit list

/// 表記ごとの実体。中身は 1 行 ずつ —— 層分けは `Diagnosis`、
/// 字を数えるのは器のスキャナ。ここが持つのは「どれとどれが対か」だけ
module SourceReader =

  let private ofParts kind apply tags =
    { new ISourceReader with
        member _.Kind = kind
        member _.Apply build source = apply build source
        member _.Tags source = tags source }

  let xml = ofParts SourceKind.Xml Diagnosis.apply XmlScan.tags
  let sxml = ofParts SourceKind.Sxml Diagnosis.applySxml SxmlScan.tags

  /// F# の CE。v1.9 から `Tags` を返す。
  ///
  /// 表はここに書かない —— どの CE 名 が何番目 の文字列に名前を載せるかは
  /// `Spec.ceLabels`、名前を載せる属性は語彙から引いた対（`References.pairs`）。
  ///
  /// 対が 1 組 も無ければ空を返す（語彙が引けていない印）
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

  /// 読める表記。`SourceKind.all` と全部 揃った（v1.1）。
  ///
  /// 揃ったからといって、この 2 本 を 1 本 にしない —— あちらは「どの表記か」、
  /// こちらは「読む口が在るか」で、次に表記を足すとき、また割れる。
  ///
  /// 並びは `SourceKind.all` と同じ。揃えておかないと、
  /// 「どちらの並びを見た数か」で数え方が割れる
  let all: ISourceReader list = [ xml; sxml; fsb; fsharp ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun r -> r.Kind = kind)
