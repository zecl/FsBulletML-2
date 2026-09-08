/// F# の CE の形。**hover は出す。候補はまだ出さない。**
///
/// --- なぜ `Lookup` の `Shape` に載せないか
///
/// XML も sxml も fsb も、`Shape` の中身は「字の数え方」と「どう書くか」で、
/// **どれも要素名を打つ表記**だった。CE はそこが違う ——
///
///     要素名    action / fire / bullet / changeSpeed …
///     CE の名前  top / defAction / refBullet / doActs / speedSeq / aim …
///
/// `Shape.ContextAt` も `AttrSnippet` も `AttrReplace` も、**候補を出すため
/// だけに在る。** hover しか出さないここに載せると「呼ぶ経路の無い口」を
/// 4 つ 抱えることになる（設計書がリスクに挙げている形）。
///
/// --- hover の中身は 1 本 のまま
///
/// **`Token` から先は `Lookup.hover`** —— ほかの 3 表記 と同じ関数を通る。
/// 違うのは `Token` の作り方だけで、
///
///     ほかの 3 つ  字を数えて「要素名・属性・属性値」を直に取る
///     CE          名前を 1 つ 取り、それが何を作るかを語彙（`Vocab.Ce`）で引く
///
/// **散文を新しく書いていない。** CE で書いていても読んでいるのは BulletML で、
/// `fire` の意味は表記が変わっても変わらない。出す字は DTD 由来の
/// 既存の散文（正本は `Core/DTD.fs` と host の `Spec`）。
///
/// --- 1 つ の名前が 2 つ を指すことがある
///
///     changeDirectionAbs  向きを変える要素と、その中の向きの型
///     vertical            根の画面の向きと、`accel` の中の縦の加速度
///
/// **要素名をここに書かない**（門が見ている）—— 語彙は host が渡す。
///
/// **どちらか に決めない。** 決めるには入れ子の型を追うことになり、それは
/// 字を数える話ではなくなる。**両方 並べる**ほうが、読む人が選べる。
///
/// --- 候補（Complete）を出さない
///
/// hover は「いま在る名前」を引くだけだが、候補は「その場所に置ける名前」が
/// 要る。CE の置ける場所は**入れ子の型**で決まる（`bullet` の中と `fire` の
/// 中で置ける CustomOperation が違う）—— 字の数え方では出せないので、
/// **ここで出さないと決めている。**
///
/// --- rename も直し方も出さない
///
/// 参照を数える側（`Refs.missing`）が探すのは「要素名 + label 属性」で、
/// CE はそこが DSL の名前（`defAction "x"` / `actionRef "x" []`）。
/// 別の語彙なので当てない。**黙って空なのではなく、ここで空だと決めている**
/// （`Parser.Tests/FsharpCeCorpus.fs` が字で固定している）。
///
/// --- エディタの色分けは付く
///
/// `EditorLanguageId` は `fsharp`。Monaco に組み込みで在るので、
/// 候補が無くても色と括弧の対応は効く。
module FsBulletML2.LanguageService.Languages.Fsharp

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

type FsharpLanguage(vocabulary: unit -> Vocab) =

  /// 見出し。**打った字と、それが作るものを並べる** ——
  /// `aim` だけ出しても、それが `<direction>` の話だと分からない
  static let title (name: string) (token: Token) =
    match token with
    | Element element -> name + " → <" + element + ">"
    | Attribute (element, attr) -> name + " → <" + element + " " + attr + "=…>"
    | AttrValue (element, attr, value) ->
      name + " → <" + element + " " + attr + "=\"" + value + "\">"
    | Nothing -> name

  interface ISourceLanguage with
    member _.Kind = SourceKind.FSharpDsl
    member _.EditorLanguageId = "fsharp"
    /// **空。** 候補を出さないので、打った瞬間に出す字も無い
    member _.TriggerCharacters = []
    member _.Complete _ _ = []

    /// カーソルの下の名前を引く。**知らない名前なら `None`** ——
    /// 本文には CE でない字も混ざる（`let` も `[]` も F# の一部）ので、
    /// 「名前の上に居ること」と「その名前が CE であること」は別
    member _.Hover source offset =
      match FsharpScan.wordAt source offset with
      | None -> None
      | Some name ->
        let v = vocabulary ()
        match v.Ce |> List.filter (fun c -> c.Name = name) with
        | [] -> None
        | hits ->
          let blocks =
            hits
            |> List.choose (fun c ->
                 let token =
                   if c.Attr = "" then Element c.Element
                   else AttrValue(c.Element, c.Attr, c.Value)
                 Lookup.hover v (title name) token)
          // **語彙に無いものしか引けなければ `None`。** 空の字を返さない ——
          // 空でも枠は浮くので、出ていないことと見分けがつかなくなる
          match blocks with
          | [] -> None
          | _ -> Some(String.concat "\n\n---\n\n" blocks)

    /// **rename も出さない。** CE の label は DSL の名前で書かれていて
    /// （`defAction "x"` / `actionRef "x" []`）、「要素名 + label 属性」の形では
    /// ない —— 字から数える側（`Tags`）が最初から空
    member _.Usages _ _ = []
    /// **直し方も出さない。** 数え直す材料（`Tags`）が最初から空なので、
    /// 「無い参照」が 1 件 も見つからない
    member _.Fixes _ _ = []
