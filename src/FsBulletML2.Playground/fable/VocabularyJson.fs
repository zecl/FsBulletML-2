/// host がくれた語彙の JSON を、`SourceLanguage` の型へ読む。
///
/// **ここだけ Fable でしか走らない。** `[<Emit>]` と `?` を使うので、
/// .NET で呼ぶと落ちる。だから器（`FsBulletML2.LanguageService`）には
/// 置かない —— **器は 2 つ の runtime で走る**ので、片方 でしか
/// 走らないものが混ざると、そこだけ .NET で当てられなくなる。
///
/// 読む先の型は器に在る。**形の正本はあちら。**
module FsBulletML2.Playground.VocabularyJson

open Fable.Core
open Fable.Core.JsInterop
open FsBulletML2.LanguageService.SourceLanguage

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
