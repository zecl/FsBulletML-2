/// host がくれた語彙の JSON を、`SourceLanguage` の型へ読む。
/// Fable でしか走らない。器に置くと .NET で当てられなくなる。
module FsBulletML2.Fable.VocabularyJson

open Fable.Core
open Fable.Core.JsInterop
open FsBulletML2.LanguageService.SourceLanguage

[<Emit("JSON.parse($0)")>]
let private jsonParse (s: string) : obj = jsNative

[<Emit("$0[$1]")>]
let private item (arr: obj) (i: int) : obj = jsNative

/// 無い鍵は空に倒す。`string` に通すと "undefined" になる。
let private text (o: obj) : string =
  if isNull o then "" else string o

let private strings (arr: obj) : string list =
  if isNull arr then []
  else
    let len: int = arr?length
    [ for i in 0 .. len - 1 -> string (item arr i) ]

/// host の `Vocabulary()` が返す JSON を読む。
/// 形が食い違ってもここでは落とさない。空は呼ぶ側が赤にする。
let parseVocabulary (json: string) : Vocab =
  let root = jsonParse json
  if isNull root then
    { Elements = []; Expressions = []; Ce = []; CeLabels = []; CePlaces = []; Frames = []; TopPrefix = "" }
  else
    let els = root?elements
    let len: int = if isNull els then 0 else els?length
    let ces = root?ce
    let clen: int = if isNull ces then 0 else ces?length
    let labels = root?ceLabels
    let llen: int = if isNull labels then 0 else labels?length
    let plcs = root?cePlaces
    let plen: int = if isNull plcs then 0 else plcs?length
    let frms = root?frames
    let flen: int = if isNull frms then 0 else frms?length
    // 雛形の骨（v2.6）。再帰で読む —— 子の並びは同じ形が入れ子になる
    let rec readFrame (f: obj) : Frame =
      let attrs = f?attrs
      let alen: int = if isNull attrs then 0 else attrs?length
      let kids = f?children
      let klen: int = if isNull kids then 0 else kids?length
      { Element = string f?element
        Text = text f?text
        Attrs = [ for i in 0 .. alen - 1 -> let a = item attrs i in string a?name, text a?value ]
        Children = [ for i in 0 .. klen - 1 -> readFrame (item kids i) ] }
    { Expressions = strings root?expressions
      // 根の名前の頭。読めなければ空。空なら意味の検査は top を出さない。
      TopPrefix = if isNull root?topPrefix then "" else string root?topPrefix
      // F# の CE の名前。 同じ名前が何個 在ってもよい（読む側が全部 拾う）
      Ce =
        [ for i in 0 .. clen - 1 ->
            let c = item ces i
            { Name = string c?name
              Element = text c?element
              Attr = text c?attr
              Value = text c?value } ]
      // CE の名前が載せる label。上と別の表（あちらは「作る要素」）
      CeLabels =
        [ for i in 0 .. llen - 1 ->
            let c = item labels i
            { Name = string c?name
              Element = text c?element
              LabelArg = int (unbox<float> c?labelArg)
              Fixed = text c?``fixed``
              Root = unbox<bool> c?root } ]
      // どこに置けて、何を開くか。候補がこれで決まる
      CePlaces =
        [ for i in 0 .. plen - 1 ->
            let c = item plcs i
            { Name = string c?name
              In = text c?``in``
              Opens = text c?opens } ]
      // 雛形。骨は要素名の木なので、器に書かない。
      Frames =
        [ for i in 0 .. flen - 1 ->
            let s = item frms i
            { Label = string s?label
              Detail = text s?detail
              In = strings s?``in``
              Frame = readFrame s?frame } ]
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
