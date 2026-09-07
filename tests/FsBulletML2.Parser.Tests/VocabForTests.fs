namespace FsBulletML2.Parser.Tests

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// **試験が言語モジュールへ渡す語彙。1 本。**
///
/// 本番はブラウザが `Vocabulary.toJson()` を受け取って組む（器の `Vocab`）。
/// 試験は WASM を挟まないので、host の `Vocabulary.elements` から同じ形へ
/// 落とす —— **その落とし方はどの表記でも同じ**なので、ここに 1 本 置く。
///
/// v1.1 まで `XmlCompletion` / `SxmlCompletion` / `FsbCompletion` に
/// **字まで同じものが 3 つ** 在った（並べてハッシュを取ったら一致した）。
/// 4 つ 目 を足す前に畳んである。
[<AutoOpen>]
module VocabForTests =

  let vocab: Vocab =
    { Elements =
        Vocabulary.elements
        |> Array.toList
        |> List.map (fun e ->
             { Name = e.Name
               Children = List.ofArray e.Children
               Text = e.Text
               Dtd = e.Dtd
               Spec = e.Spec
               Attrs =
                 e.Attrs
                 |> Array.toList
                 |> List.map (fun a ->
                      { Name = a.Name
                        Values = List.ofArray a.Values
                        Defaults = List.ofArray a.Defaults
                        Dtd = a.Dtd
                        Spec = a.Spec
                        ValueSpecs = List.ofArray a.ValueSpecs }) })
      Expressions = List.ofArray Vocabulary.expressions }
