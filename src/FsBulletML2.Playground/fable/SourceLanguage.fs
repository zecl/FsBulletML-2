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

// **`SourceKind` はここに無い。** host（`Main.fs`）が同じ字を受けるので
// `FsBulletML2.LanguageService` に出してある —— 前は同じ並びが 2 か所 に
// 書いてあり、片方 だけ変えても build も試験も落ちなかった
open FsBulletML2.LanguageService

/// 語彙の写し。**正本は Core の DTD.fs**（host が JSON にして渡す）。
/// ここは受け取った形をそのまま持つだけで、表を書かない
type VocabAttr =
  { Name: string
    Values: string list
    /// 書かなかったときに走る値。**並びで持つ** —— host 側が 1 個 に
    /// 絞れているかを門で見ているので、ここは受け取った形のまま
    Defaults: string list
    /// `<!ATTLIST ...>` 行
    Dtd: string
    /// hover に出す散文
    Spec: string
    /// 値ごとの散文
    ValueSpecs: (string * string) list }

type VocabElement =
  { Name: string
    Children: string list
    Attrs: VocabAttr list
    /// #PCDATA を取るか。取る要素の中では式（$rand / $rank）も候補になる
    Text: bool
    /// `<!ELEMENT ...>` 行
    Dtd: string
    /// hover に出す散文
    Spec: string }

type Vocab =
  { Elements: VocabElement list
    /// 式の中で使える字
    Expressions: string list }

/// 候補 1 つ。
///
/// **`Replace` を言語モジュールが決める。** Monaco の「語」に任せると、
/// `$rand` の `$` が語に入らない版で `$$rand` になる。手前 何文字 を
/// 置き換えるかはこちらが数える —— 何が語かを知っているのは言語のほう
type Completion =
  { Label: string
    /// 入れる字。`Snippet` なら Monaco の記法（`$0` がカーソル）
    Insert: string
    Snippet: bool
    /// カーソルの手前 何文字 を置き換えるか
    Replace: int }

module Completion =
  let plain (replace: int) (label: string) =
    { Label = label; Insert = label; Snippet = false; Replace = replace }

type ISourceLanguage =
  abstract Kind: SourceKind
  /// エディタ側の language id。表記と 1 対 1 とは限らないので別に持つ。
  ///
  /// **`MonacoLanguage` という名前だった。** 中身は Monaco 固有の値ではなく
  /// 「エディタに渡す language id」で、名前だけが唯一 Monaco を名指ししていた ——
  /// 抽象がエディタを名指しすると、載せ替えるときに抽象ごと直すことになる
  abstract EditorLanguageId: string
  /// 打った瞬間に候補を出す字。**語の文字は要らない** —— そちらは Monaco が
  /// 自分で出す。ここに置くのは「語ではないが、その直後に必ず候補が要る」字。
  /// **表記ごとに違う**（sxml なら括弧）ので言語モジュールが持つ
  abstract TriggerCharacters: string list
  /// 本文とカーソルの位置（文字数）から候補を出す。
  /// **WASM に行かない** —— 語彙は起動時にもらったものを引く
  abstract Complete: source: string -> offset: int -> Completion list
  /// カーソルの下に在るものの仕様。**返すのは markdown の字**で、
  /// Monaco 側の形（`contents` の配列）を組むのは呼ぶ側。
  /// 何の上でもなければ `None`（**空の字を返さない** ——
  /// 空でも枠が浮くので、出ていないことと見分けがつかなくなる）
  abstract Hover: source: string -> offset: int -> string option

[<Emit("JSON.parse($0)")>]
let private jsonParse (s: string) : obj = jsNative

[<Emit("$0[$1]")>]
let private item (arr: obj) (i: int) : obj = jsNative

/// 無い鍵は `undefined` で返る。**`string` に通すと "undefined" という
/// 字になる**ので、空に倒す
let private text (o: obj) : string =
  if isNull o then "" else string o

let private strings (arr: obj) : string list =
  if isNull arr then []
  else
    let len: int = arr?length
    [ for i in 0 .. len - 1 -> string (item arr i) ]

/// host の `Vocabulary()` が返す JSON を読む。
/// **形が食い違ったら候補が出なくなるだけ**なので、呼ぶ側が空を赤にする
let parseVocabulary (json: string) : Vocab =
  let root = jsonParse json
  if isNull root then { Elements = []; Expressions = [] }
  else
    let els = root?elements
    let len: int = if isNull els then 0 else els?length
    { Expressions = strings root?expressions
      Elements =
        [ for i in 0 .. len - 1 ->
            let e = item els i
            let attrs = e?attrs
            let alen: int = if isNull attrs then 0 else attrs?length
            { Name = string e?name
              Children = strings e?children
              Text = unbox<bool> e?text
              Dtd = text e?dtd
              Spec = text e?spec
              Attrs =
                [ for j in 0 .. alen - 1 ->
                    let a = item attrs j
                    let vs = a?valueSpecs
                    let vlen: int = if isNull vs then 0 else vs?length
                    { Name = string a?name
                      Values = strings a?values
                      Defaults = strings a?defaults
                      Dtd = text a?dtd
                      Spec = text a?spec
                      ValueSpecs =
                        [ for k in 0 .. vlen - 1 ->
                            let p = item vs k
                            text p?value, text p?spec ] } ] } ] }
