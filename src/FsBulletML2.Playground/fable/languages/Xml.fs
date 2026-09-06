/// XML の補完。**v0.3 で中身が在る唯一の言語モジュール。**
///
/// 語彙は持たない —— host が `Core/DTD.fs` から焼いたものを受け取る。
/// ここが持つのは「カーソルがどこに居るか」の判定だけ。
///
/// **精度より、止まらないこと。** 打っている途中の XML は必ず壊れているので、
/// パーサは使わずに `<` から左へ数えるだけにする。閉じていないタグも
/// 引用符の中の `>` も、そのぶんだけ数えて先へ進む。
module FsBulletML2.Playground.Languages.Xml

open System.Text
open FsBulletML2.Playground.SourceLanguage

/// カーソルの居場所
type Context =
  /// 本文。直近に開いている要素（無ければ根の外）
  | InContent of string option
  /// `<name ` の中。属性名を出す
  | InStartTag of string
  /// `<name attr="` の中。属性値を出す
  | InAttrValue of string * string

let private isSpace c = c = ' ' || c = '\t' || c = '\r' || c = '\n'

/// タグの中にカーソルが居るときの居場所。
/// `text` は `<` からカーソルまで
let private inTag (text: string) : Context option =
  if text.StartsWith "</" || text.StartsWith "<!" || text.StartsWith "<?" then None
  else
    let mutable k = 1
    while k < text.Length && not (isSpace text.[k] || text.[k] = '/' || text.[k] = '>') do
      k <- k + 1
    let name = text.Substring(1, k - 1)
    // まだ要素名を打っている（`<act|`）。**要素の候補を出したいので content 扱い**
    if k >= text.Length then None
    else
      // 属性の領域。引用符が開いたままならその中に居る
      let mutable m = k
      let mutable quote = '\000'
      let mutable lastAttr = ""
      let pending = StringBuilder()
      while m < text.Length do
        let c = text.[m]
        if quote <> '\000' then
          (if c = quote then quote <- '\000')
        elif c = '"' || c = '\'' then quote <- c
        elif c = '=' then
          lastAttr <- pending.ToString().Trim()
          pending.Clear() |> ignore
        elif isSpace c then pending.Clear() |> ignore
        else pending.Append c |> ignore
        m <- m + 1
      if quote <> '\000' then Some(InAttrValue(name, lastAttr)) else Some(InStartTag name)

/// `<` から左へ数えて、カーソルの居場所を出す
let contextAt (src: string) (offset: int) : Context =
  let cursor = max 0 (min offset src.Length)
  let stack = ResizeArray<string>()
  let mutable i = 0
  let mutable result = None
  // **カーソルより先へ進まない。** 進むと後ろの閉じタグまで札を下ろしてしまい、
  // どこに居ても「根」に見える（カーソルがタグの中のときだけ正しく出るので、
  // 属性の補完は当たり、本文の補完だけ静かに外れる）
  while result.IsNone && i < src.Length && i < cursor do
    if src.[i] <> '<' then i <- i + 1
    else
      // タグの終わり。**引用符の中の `>` は数えない**
      let mutable m = i + 1
      let mutable quote = '\000'
      let mutable fin = -1
      while fin < 0 && m < src.Length do
        let c = src.[m]
        if quote <> '\000' then
          (if c = quote then quote <- '\000')
        elif c = '"' || c = '\'' then quote <- c
        elif c = '>' then fin <- m
        m <- m + 1
      // 閉じていなければ末尾までがタグ
      let tagEnd = if fin < 0 then src.Length else fin
      if cursor > i && cursor <= tagEnd then
        result <-
          match inTag (src.Substring(i, cursor - i)) with
          | Some c -> Some c
          | None -> Some(InContent(if stack.Count > 0 then Some stack.[stack.Count - 1] else None))
      else
        // タグを閉じ札に反映してから先へ
        if src.[i + 1] = '/' then
          if stack.Count > 0 then stack.RemoveAt(stack.Count - 1)
        elif src.[i + 1] = '!' || src.[i + 1] = '?' then ()
        else
          // **自己閉じは積まない。** ここを見落とすと、`<bullet/>` から先が
          // ずっと bullet の中に居ることになる
          let selfClosing = fin > i + 1 && src.[fin - 1] = '/'
          if not selfClosing then
            let mutable k = i + 1
            while k < src.Length && not (isSpace src.[k] || src.[k] = '/' || src.[k] = '>') do
              k <- k + 1
            stack.Add(src.Substring(i + 1, k - i - 1))
        i <- tagEnd + 1
  match result with
  | Some c -> c
  | None -> InContent(if stack.Count > 0 then Some stack.[stack.Count - 1] else None)

/// 語彙を引いて候補を出す。**語彙は引数で受け取る** ——
/// このモジュールが host を知らないので、次の言語も同じ形で書ける
type XmlLanguage(vocabulary: unit -> VocabElement list) =

  let find name = vocabulary () |> List.tryFind (fun e -> e.Name = name)

  member _.Candidates(source: string, offset: int) =
    match contextAt source offset with
    | InContent None ->
      // 根の外。置けるのは bulletml だけ
      vocabulary () |> List.filter (fun e -> e.Name = "bulletml") |> List.map (fun e -> e.Name)
    | InContent (Some parent) ->
      match find parent with
      | Some e -> e.Children
      | None -> []
    | InStartTag element ->
      match find element with
      | Some e -> e.Attrs |> List.map (fun a -> a.Name)
      | None -> []
    | InAttrValue (element, attr) ->
      match find element with
      | Some e ->
        match e.Attrs |> List.tryFind (fun a -> a.Name = attr) with
        | Some a -> a.Values
        | None -> []
      | None -> []

  interface ISourceLanguage with
    member _.Kind = SourceKind.Xml
    member _.MonacoLanguage = "xml"
    member this.Complete source offset = this.Candidates(source, offset)
