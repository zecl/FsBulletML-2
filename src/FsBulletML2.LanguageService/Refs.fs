/// **「どの要素が、どの要素を、どの属性で参照するか」の規則。1 本。**
///
/// 表を持たない —— 語彙の中で名前が `Ref` で終わり、`Ref` を落とした名前も
/// 語彙に在り、両方 が同じ属性を持つものだけを対にする。
///
/// ## 入口が 2 つ 在る
///
///     host 側     `References.fs`（波線。`Vocabulary.elements` から）
///     ブラウザ側   `Languages/Lookup.fs`（rename。起動時にもらった `Vocab` から）
///
/// 型が違うので、受け取るのは**名前と属性名だけに落としたもの。**
///
/// **規則を 2 か所 に書かない。** 書くと、片方 だけ直したときに
/// 「波線は出るのに rename は当たらない」（逆も）という形になる ——
/// **どちらも単独では正しく見える。**
///
/// ## `Fable.Core` に依存しない
///
/// このソースは host（.NET）と ブラウザ側（Fable が焼いた JS）の
/// **両方 で走る。**
module FsBulletML2.LanguageService.Refs

/// 名前が `Ref` で終わるか。
///
/// **`EndsWith` を使わない。** 引数 1 つ の `String.EndsWith` は
/// .NET では現在のカルチャで比べる —— 綴りが ASCII なので実害は出にくいが、
/// 「どちらで比べているか」が字に出ない。F# の文字列の等値は序数なので、
/// 切って比べれば読んだとおりに動く
let private endsWithRef (name: string) =
  name.Length > 3 && name.Substring(name.Length - 3) = "Ref"

/// (参照する要素, 参照される要素, label の属性名) の並び。
///
/// 受け取るのは (要素名, その要素の属性名の並び) の並び。
///
/// **空なら、呼ぶ側は何も挙げない**（語彙が引けていない印。
/// `Parser.Tests` が 0 件 を赤にする）
let pairs (elements: (string * string list) list) : (string * string * string) list =
  let attrsOf name =
    elements |> List.tryPick (fun (n, attrs) -> if n = name then Some attrs else None)
  elements
  |> List.choose (fun (name, attrs) ->
      if not (endsWithRef name) then None
      else
        match attrsOf (name.Substring(0, name.Length - 3)) with
        | None -> None
        | Some defAttrs ->
          attrs
          |> List.tryPick (fun a ->
               if List.contains a defAttrs
               then Some(name, name.Substring(0, name.Length - 3), a)
               else None))
