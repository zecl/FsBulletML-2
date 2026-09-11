/// **式が読めるか**を字から見る（v4.1）。**値は出さない。**
///
/// ## なぜ器に 2 本 目 の読み手が要るのか
///
/// `FsBulletML2.LanguageService` は `ProjectReference` を 1 本 も持たない ——
/// 語彙すら渡されたデータで受け取る。**`Core/Expr.fs` はブラウザ側から見えない。**
/// 打鍵ごとに式を見るなら、ここに読み手が要る。
///
/// **値は出さない。** 評価器を 2 本 持つと**同じ式が 2 通り の値**になり、
/// しかも「どちらも単独では正しく見える」。ここが答えるのは 2 つ だけ ——
///
///     読めるか
///     読めないなら、**どこまで読めたか**
///
/// ## 突き合わせは門でやる
///
/// 文法は `Core/Expr.fs` の `parseExpr` を写したもの。**写しである以上、
/// 片方 だけ直した形が黙って残る** —— `ExprParity` が
/// コーパス 427 件 と 同梱 4,582 件 の全件 で答えを突き合わせる。
///
/// ## 文法（XPath 1.0 の数値式）
///
///     expr    := term (('+' | '-') term)*
///     term    := unary (('*' | '/' | '%') unary)*
///     unary   := '-' unary | primary
///     primary := number | '$rand' | '$rank' | '$' digits* | '(' expr ')'
///
/// **単項プラスは無い**（XPath 1.0 に無く、旧は number() が NaN を返す）。
/// **指数表記も無い**（`1E-07` は読めない側）。
///
/// ## `Fable.Core` に依存しない
///
/// host（.NET）と ブラウザ側（Fable が焼いた JS）の**両方 で走る**（`Semantics` と同じ）。
module FsBulletML2.LanguageService.ExprCheck

open System

let private isDigit (c: char) = c >= '0' && c <= '9'

/// `s` の `i` から `w` が始まっているか。
///
/// **`String.CompareOrdinal` の 5 引数 版を使わない。** Core の `Expr.fs` は
/// そちらで書いてあるが、**Fable が焼いた JS では当たらなかった** ——
/// `guard-fable-parity` が拾った（.NET は `30-$rank*8` を読め、node は読めなかった）。
/// F# の側は通り、build も門も出ず、**焼いた JS だけが違う**形
let private startsAt (s: string) (i: int) (w: string) =
  i + w.Length <= s.Length && s.Substring(i, w.Length) = w

/// `$rand` / `$rank` / `$` + 数字。**順が要る** ——
/// `Replace` は部分一致なので、`$random` は `$rand` が先に当たる（Core も同じ順）
let private tryVar (s: string) (i: int) =
  if startsAt s i "$rand" then Some (i + 5)
  elif startsAt s i "$rank" then Some (i + 5)
  elif i < s.Length && s.[i] = '$' then
    let mutable j = i + 1
    while j < s.Length && isDigit s.[j] do j <- j + 1
    Some j
  else None

/// 数値リテラル。`digits ('.' digits?)?` か `'.' digits`。**指数表記は受けない**
let private tryNumber (s: string) (i: int) =
  let start = i
  let mutable j = i
  while j < s.Length && isDigit s.[j] do j <- j + 1
  let intLen = j - start
  let mutable fracLen = 0
  if j < s.Length && s.[j] = '.' then
    let dot = j
    j <- j + 1
    let fs = j
    while j < s.Length && isDigit s.[j] do j <- j + 1
    fracLen <- j - fs
    if intLen = 0 && fracLen = 0 then j <- dot
  if intLen = 0 && fracLen = 0 then None else Some j

let private skipWs (s: string) (i: int) =
  let mutable j = i
  while j < s.Length && Char.IsWhiteSpace s.[j] do j <- j + 1
  j

let rec private parseExpr (s: string) (i: int) : int option =
  match parseTerm s (skipWs s i) with
  | None -> None
  | Some i ->
    let mutable pos = skipWs s i
    let mutable go = true
    while go do
      if pos < s.Length && (s.[pos] = '+' || s.[pos] = '-') then
        match parseTerm s (skipWs s (pos + 1)) with
        | Some next -> pos <- skipWs s next
        | None -> go <- false
      else go <- false
    Some pos

and private parseTerm (s: string) (i: int) : int option =
  match parseUnary s (skipWs s i) with
  | None -> None
  | Some i ->
    let mutable pos = skipWs s i
    let mutable go = true
    while go do
      if pos < s.Length && (s.[pos] = '*' || s.[pos] = '/' || s.[pos] = '%') then
        match parseUnary s (skipWs s (pos + 1)) with
        | Some next -> pos <- skipWs s next
        | None -> go <- false
      else go <- false
    Some pos

and private parseUnary (s: string) (i: int) : int option =
  let i = skipWs s i
  if i < s.Length && s.[i] = '-' then parseUnary s (skipWs s (i + 1))
  elif i < s.Length && s.[i] = '+' then None
  else parsePrimary s i

and private parsePrimary (s: string) (i: int) : int option =
  let i = skipWs s i
  if i >= s.Length then None
  elif s.[i] = '(' then
    match parseExpr s (i + 1) with
    | Some next ->
      let next = skipWs s next
      if next < s.Length && s.[next] = ')' then Some (next + 1) else None
    | None -> None
  else
    match tryVar s i with
    | Some r -> Some r
    | None -> tryNumber s i

/// 読めるか。**`Core/Expr.parse` が `Invalid` を返さないのと同じ条件。**
let readable (s: string) : bool =
  if isNull s then false
  else
    match parseExpr s 0 with
    | Some pos -> skipWs s pos = s.Length
    | None -> false

/// **ただの数か**（v4.4）。`30` / `1.5` / `-3` / `.5` は真。
///
/// **値を横に出す意味が無い側。** `<wait>30</wait>` の横に `= 30` を出しても
/// 字が増えるだけ —— 出すのは**畳んで初めて数になる式**だけ。
///
/// **`readable` と同じ読み手を通す。** 別に数え直すと、
/// 「読めないのに ただの数 と言う」形が作れてしまう
let plainNumber (s: string) : bool =
  if isNull s then false
  else
    let i = skipWs s 0
    // 単項マイナスは数の一部 として扱う（`-3` は式ではなく数）
    let i = if i < s.Length && s.[i] = '-' then skipWs s (i + 1) else i
    match tryNumber s i with
    | Some pos -> skipWs s pos = s.Length
    | None -> false

/// **どこまで読めたか**（0 起点 の文字数）。読めるなら文字数そのもの。
///
/// 波線をここから引く —— 要素まるごとに引くと、
/// **`180+$rand*30` の 12 文字 が全部 赤くなって、どこが悪いか分からない。**
///
/// **`None` は「頭から読めない」**（`abc` など）—— そのときは 0 を返す
let readTo (s: string) : int =
  if isNull s then 0
  else
    match parseExpr s 0 with
    | Some pos -> skipWs s pos
    | None -> 0
