/// 表記ごとの口。**v0.3 の実装は XML 1 本 だけ。**
///
/// ここに口を置くのは、次の言語（sxml / fsb / F# CE）を足すときに
/// **触るのを言語モジュール 1 個 に閉じるため。** Monaco の配線も rAF も
/// `Playfield` も触らせない。
///
/// **`Diagnose` は入れない。** 設計書の口には在るが、v0.3 の診断は Apply 時に
/// host（WASM）が返すので、Fable 側の `Diagnose` を呼ぶ経路が無い。
/// 入れると XML モジュールが「`None` を返すだけの実装」を抱える ——
/// 設計書自身がリスクに挙げている形。呼ぶ経路ができる版で足す。
///
/// `Complete` が返すのは文字列の並び。**`{ Label; InsertText; Detail }` にしない** ——
/// v0.3 で使うのは Label だけで、残り 2 つ は誰も読まない。要る言語が来たら広げる。
module FsBulletML2.Playground.SourceLanguage

open Fable.Core
open Fable.Core.JsInterop

type SourceKind =
  | Xml
  | Sxml
  | Fsb
  | FSharpDsl

  /// host に渡す字。`ApplySource` の kind と同じもの
  member this.Id =
    match this with
    | Xml -> "xml"
    | Sxml -> "sxml"
    | Fsb -> "fsb"
    | FSharpDsl -> "fsharp"

/// 語彙の写し。**正本は Core の DTD.fs**（host が JSON にして渡す）。
/// ここは受け取った形をそのまま持つだけで、表を書かない
type VocabAttr =
  { Name: string
    Values: string list }

type VocabElement =
  { Name: string
    Children: string list
    Attrs: VocabAttr list
    Text: bool }

type ISourceLanguage =
  abstract Kind: SourceKind
  /// Monaco 側の language id。表記と 1 対 1 とは限らないので別に持つ
  abstract MonacoLanguage: string
  /// 本文とカーソルの位置（文字数）から候補を出す。
  /// **WASM に行かない** —— 語彙は起動時にもらったものを引く
  abstract Complete: source: string -> offset: int -> string list

[<Emit("JSON.parse($0)")>]
let private jsonParse (s: string) : obj = jsNative

[<Emit("$0[$1]")>]
let private item (arr: obj) (i: int) : obj = jsNative

let private strings (arr: obj) : string list =
  if isNull arr then []
  else
    let len: int = arr?length
    [ for i in 0 .. len - 1 -> string (item arr i) ]

/// host の `Vocabulary()` が返す JSON を読む。
/// **形が食い違ったら候補が出なくなるだけ**なので、呼ぶ側が空を赤にする
let parseVocabulary (json: string) : VocabElement list =
  let root = jsonParse json
  if isNull root then []
  else
    let len: int = root?length
    [ for i in 0 .. len - 1 ->
        let e = item root i
        let attrs = e?attrs
        let alen: int = if isNull attrs then 0 else attrs?length
        { Name = string e?name
          Children = strings e?children
          Text = unbox<bool> e?text
          Attrs =
            [ for j in 0 .. alen - 1 ->
                let a = item attrs j
                { Name = string a?name; Values = strings a?values } ] } ]
