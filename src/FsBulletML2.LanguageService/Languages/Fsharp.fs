/// F# の CE の形。**補完と hover は、まだ出さない。**
///
/// --- なぜ `Lookup` に載せないか
///
/// XML も sxml も、候補は `Core/DTD.fs` の語彙から出している（`Lookup` が
/// 引く）。**CE の語彙はそこに無い** —— 書くのは要素名ではなく DSL の名前で、
///
///     要素名   action / fire / bullet / changeSpeed …
///     CE の名前 top / defAction / refBullet / doActs / speedSeq / repeatRef …
///
/// **前者から後者は導けない。** `FsBulletML2.Dsl` を reflection で舐めれば
/// 取れるが、それは `Vocabulary.fs` が DTD にやっているのと同じ規模の別口で、
/// **この版では置かないと決めた。**
///
/// --- なぜ空の実装を置くのか
///
/// 設計書は「呼ぶ経路の無い口を作らない」と書いている。ここはその逆で、
/// **呼ぶ経路は在って、答えが無い。** CE モードで書いて Apply する道は
/// 通っているので、補完だけが空になる。
///
/// **黙って空なのではなく、ここで空だと決めている** ——
/// `Parser.Tests/FsharpCeCorpus.fs` が字で固定していて、
/// 語彙を置いた版で赤くなる（そこで外す点）。
///
/// --- エディタの色分けは付く
///
/// `EditorLanguageId` は `fsharp`。Monaco に組み込みで在るので、
/// **補完が無くても色と括弧の対応は効く。**
module FsBulletML2.LanguageService.Languages.Fsharp

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

type FsharpLanguage() =
  interface ISourceLanguage with
    member _.Kind = SourceKind.FSharpDsl
    member _.EditorLanguageId = "fsharp"
    /// **空。** 候補を出さないので、打った瞬間に出す字も無い
    member _.TriggerCharacters = []
    member _.Complete _ _ = []
    member _.Hover _ _ = None
