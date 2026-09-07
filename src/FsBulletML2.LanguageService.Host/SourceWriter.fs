namespace FsBulletML2.LanguageService

open FsBulletML2

/// **表記ごとの「書く口」。`ISourceReader` の対。**
///
/// v1.4 まで、書けるのは XML だけだった。だから Playground は
/// 「表記を変えても本文は触らない」と決めていた —— **決めたのではなく、
/// できなかった。**
///
/// ## 中身は 1 本 の歩きと、受け口 3 つ
///
/// XML / sxml / fsb は「要素名 + 属性 + 本文 + 子」で同じ形なので、
/// 木を歩くのは `Core` の `BulletmlWriter.writeTo` 1 本。
/// **違うのは受け口だけ**で、それは**その文法を読むパーサと同じファイル**に
/// 置いてある（`Sxml.fs` / `Offside.fs`）——
/// 離すと、文法を直したときに書く側が置いていかれる。
///
/// F# の CE だけはこの形に乗らない（要素名ではなく DSL の名前で書く）。
/// あちらは `FsharpCe.write`。
///
/// ## 書けないことが在る
///
/// 返りが `Result` なのはそのため ——
///
///     fsb   本文に空白が入れられない（式は空白を落として書く）
///     sxml  属性値に通せない字が在る
///     CE    `description` と 名前の無い弾幕 を書く口が `Dsl` に無い
///
/// **黙って落とさない。** 落とすと、読み直したときに値が変わる。
type ISourceWriter =
  abstract Kind: SourceKind
  /// 弾幕をその表記の字にする。**書けなければ理由**
  abstract Write: bulletml: Bulletml -> Result<string, string>

module SourceWriter =

  let private ofParts kind write =
    { new ISourceWriter with
        member _.Kind = kind
        member _.Write bulletml = write bulletml }

  /// **定数を畳まない側を使う。** `Parser` の `ToIndentedXmlString` は
  /// `foldConstants` を通すので `8` が `8.0000000000` になる ——
  /// 表記を行き来する用途では、人が書いた字が化ける。
  ///
  /// 同梱カタログを焼くところ（`SelectPattern`）は畳む側のままにしてある
  let xml = ofParts SourceKind.Xml (fun b -> Result.Ok(BulletmlWriter.toIndentedXml 4 b))
  let sxml = ofParts SourceKind.Sxml Sxml.write
  let fsb = ofParts SourceKind.Fsb Offside.write
  let fsharp = ofParts SourceKind.FSharpDsl FsharpCe.write

  /// 並びは `SourceKind.all` と同じ順。**読む口と揃えておく**
  let all: ISourceWriter list = [ xml; sxml; fsb; fsharp ]

  let tryFind (kind: SourceKind) = all |> List.tryFind (fun w -> w.Kind = kind)
