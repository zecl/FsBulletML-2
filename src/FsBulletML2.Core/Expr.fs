namespace FsBulletML2

open System
open System.Globalization

/// BulletML の数値式を、文字列でなく木で持つ。
///
/// 意味論は XPath 1.0 の数値式（精度は double、最後に float32 へ丸める）——
/// 旧の `getValueByXPath` に 1 ビット も違わない値を返すのが目標で、速さはその次。
///
/// 旧の評価器は Core に無い（`System.Xml.XPath` を引くと Fable で焼けなくなる）。
/// 突き合わせの相手は `tests/FsBulletML2.Core.Tests/XPathOracle.fs`。
module Expr =

  /// 式の木。値は double で持つ（XPath 1.0 の数値が double のため）
  type Node =
    | Num of double
    /// $rand
    | Rand
    /// $rank
    | Rank
    | Neg of Node
    | Add of Node * Node
    | Sub of Node * Node
    | Mul of Node * Node
    | Div of Node * Node
    | Mod of Node * Node
    /// 読めなかった式。旧は XPath の number() が NaN を返すので、それに合わせる。
    /// 例外にしない —— 例外にすると、いま静かに NaN で進んでいる台本が落ちる
    | Invalid

  // ---- 読む ----

  /// `getValueByXPath` が置き換える順を写す（`$rand` -> `$rank` -> `$` + 数字）。
  ///
  /// 順が要るのは Replace が部分一致だから —— `"$random"` は `"$rand"` が
  /// 先に当たって `"0.5om"` になる（旧がそうなっている）。
  let private tryVar (s: string) (i: int) =
    if i + 5 <= s.Length && String.CompareOrdinal(s, i, "$rand", 0, 5) = 0 then Some (Rand, i + 5)
    elif i + 5 <= s.Length && String.CompareOrdinal(s, i, "$rank", 0, 5) = 0 then Some (Rank, i + 5)
    elif i < s.Length && s.[i] = '$' then
      // 旧の Regex.Replace(s, "\$\d*", "0")。数字が 0 個 でも当たる
      let mutable j = i + 1
      while j < s.Length && Char.IsDigit s.[j] do j <- j + 1
      Some (Num 0.0, j)
    else None

  /// 数値リテラル。XPath 1.0 の Number は  digits ('.' digits?)?  |  '.' digits
  /// 指数表記は無い（旧が number() に投げているので、ここも受けない）
  let private tryNumber (s: string) (i: int) =
    let start = i
    let mutable j = i
    while j < s.Length && Char.IsDigit s.[j] do j <- j + 1
    let intLen = j - start
    let mutable fracLen = 0
    if j < s.Length && s.[j] = '.' then
      let dot = j
      j <- j + 1
      let fs = j
      while j < s.Length && Char.IsDigit s.[j] do j <- j + 1
      fracLen <- j - fs
      if intLen = 0 && fracLen = 0 then j <- dot   // "." だけは数値でない
    if intLen = 0 && fracLen = 0 then None
    else
      let text = s.Substring(start, j - start)
      match Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture) with
      | true, v -> Some (Num v, j)
      | _ -> None

  let private skipWs (s: string) (i: int) =
    let mutable j = i
    while j < s.Length && Char.IsWhiteSpace s.[j] do j <- j + 1
    j

  /// 再帰下降。XPath 1.0 の優先順位に合わせる。
  ///
  ///   expr    := term (('+' | '-') term)*
  ///   term    := unary (('*' | '/' | '%') unary)*
  ///   unary   := '-' unary | primary
  ///   primary := number | '$rand' | '$rank' | '$' digits* | '(' expr ')'
  let rec private parseExpr (s: string) (i: int) : (Node * int) option =
    match parseTerm s (skipWs s i) with
    | None -> None
    | Some (first, i) ->
      let mutable node = first
      let mutable pos = skipWs s i
      let mutable go = true
      while go do
        if pos < s.Length && (s.[pos] = '+' || s.[pos] = '-') then
          let op = s.[pos]
          match parseTerm s (skipWs s (pos + 1)) with
          | Some (rhs, next) ->
            node <- (if op = '+' then Add (node, rhs) else Sub (node, rhs))
            pos <- skipWs s next
          | None -> go <- false
        else go <- false
      Some (node, pos)

  and private parseTerm (s: string) (i: int) : (Node * int) option =
    match parseUnary s (skipWs s i) with
    | None -> None
    | Some (first, i) ->
      let mutable node = first
      let mutable pos = skipWs s i
      let mutable go = true
      while go do
        if pos < s.Length && (s.[pos] = '*' || s.[pos] = '/' || s.[pos] = '%') then
          let op = s.[pos]
          match parseUnary s (skipWs s (pos + 1)) with
          | Some (rhs, next) ->
            node <-
              (match op with
               | '*' -> Mul (node, rhs)
               | '/' -> Div (node, rhs)
               | _   -> Mod (node, rhs))
            pos <- skipWs s next
          | None -> go <- false
        else go <- false
      Some (node, pos)

  and private parseUnary (s: string) (i: int) : (Node * int) option =
    let i = skipWs s i
    if i < s.Length && s.[i] = '-' then
      match parseUnary s (skipWs s (i + 1)) with
      | Some (n, next) -> Some (Neg n, next)
      | None -> None
    elif i < s.Length && s.[i] = '+' then
      // XPath 1.0 に単項プラスは無い。旧は number() が NaN を返すので合わせる
      None
    else parsePrimary s i

  and private parsePrimary (s: string) (i: int) : (Node * int) option =
    let i = skipWs s i
    if i >= s.Length then None
    elif s.[i] = '(' then
      match parseExpr s (i + 1) with
      | Some (n, next) ->
        let next = skipWs s next
        if next < s.Length && s.[next] = ')' then Some (n, next + 1) else None
      | None -> None
    else
      match tryVar s i with
      | Some r -> Some r
      | None -> tryNumber s i

  /// 文字列を木にする。読めなければ Invalid（旧の number() が NaN を返す形）
  let parse (s: string) : Node =
    if isNull s then Invalid
    else
      match parseExpr s 0 with
      | Some (node, pos) when skipWs s pos = s.Length -> node
      | _ -> Invalid

  // ---- 評価する ----

  /// float32 を、`getValueByXPath` と同じ道筋で double にする。
  ///
  /// そのまま widen しない —— 旧は float32 の `ToString` を式へ埋め、
  /// XPath がその文字列を double として読むので、1e-9 ずれて
  /// float32 に戻したときの最後の 1 ビットが違う。
  let private widen (v: float32) : double =
    if Single.IsNaN v || Single.IsInfinity v then Double.NaN
    else
      match Double.TryParse(v.ToString(CultureInfo.InvariantCulture),
                            NumberStyles.Float, CultureInfo.InvariantCulture) with
      | true, d -> d
      | _ -> Double.NaN

  /// 木が $rand / $rank を使うか。使わないなら widen を通らずに済む
  /// （実物の式の 61% は数値リテラルだけ）
  let rec private uses (node: Node) =
    match node with
    | Rand -> true, false
    | Rank -> false, true
    | Num _ | Invalid -> false, false
    | Neg a -> uses a
    | Add (a, b) | Sub (a, b) | Mul (a, b) | Div (a, b) | Mod (a, b) ->
      let ra, ka = uses a
      let rb, kb = uses b
      ra || rb, ka || kb

  let usesRand (node: Node) = fst (uses node)
  let usesRank (node: Node) = snd (uses node)

  /// 引いた乱数と rank を渡して計算する。double で計算するのは
  /// XPath 1.0 の数値が double だから。途中で float32 へ落とすと桁がずれる
  let rec private evalWith (randValue: double) (rank: double) (node: Node) : double =
    match node with
    | Num v -> v
    | Rand -> randValue
    | Rank -> rank
    | Neg a -> -(evalWith randValue rank a)
    | Add (a, b) -> evalWith randValue rank a + evalWith randValue rank b
    | Sub (a, b) -> evalWith randValue rank a - evalWith randValue rank b
    | Mul (a, b) -> evalWith randValue rank a * evalWith randValue rank b
    | Div (a, b) -> evalWith randValue rank a / evalWith randValue rank b
    | Mod (a, b) -> evalWith randValue rank a % evalWith randValue rank b
    | Invalid -> Double.NaN

  /// `getValueByXPath` と同じ型で返す。
  ///
  /// 乱数は 1 回 の評価につき 1 回 だけ引く。`$rand` が何個 あっても、
  /// 1 個 も無くても、必ず 1 回（同じ式の中の `$rand` は全部 同じ値）。
  /// 引く回数は乱数の並びを進めるので、ここを写し違えると 227 本 の軌跡が
  /// 丸ごとずれる。
  let eval (rand: unit -> float32) (rank: float32) (node: Node) : float32 =
    // 引く回数を合わせるため、値を使わなくても必ず 1 回 引く
    let r = rand ()
    let rd = if usesRand node then widen r else 0.0
    let kd = if usesRank node then widen rank else 0.0
    float32 (evalWith rd kd node)

  /// 乱数を引かずに、値を直に渡して計算する。テストと、乱数を
  /// 引く回数を自分で管理したい呼び出し側のため
  let evalWithValues (randValue: float32) (rank: float32) (node: Node) : float32 =
    let rd = if usesRand node then widen randValue else 0.0
    let kd = if usesRank node then widen rank else 0.0
    float32 (evalWith rd kd node)

  // ---- 木を持ち回るための入れ物 ----

  /// 数値式。元の文字列と、それを読んだ木の組。
  ///
  /// 文字列を捨てない。 参照の実引数は `Param.replace` が「文字の置き換え」で
  /// 入れていて、置き換えは優先順位を変える（`"1+2"` を `"$1*3"` へ入れると
  /// `1+2*3 = 7`。木の上で節として差し込むと `(1+2)*3 = 9`）。
  /// 同梱 227 本 のうち 50 本 が当たる。
  ///
  /// 読むのは「文字列が決まった時点」で 1 回。走行中は木を評価するだけ。
  [<CustomEquality; NoComparison>]
  type NumExpr =
    { /// もとの文字列。Param.replace と、XML へ書き戻すときに要る
      Source : string
      /// Source を読んだ木
      Ast : Node
      /// この式が $rand を使うか。読んだときに 1 回 数える
      /// （評価のたびに木を歩くと、木にした意味が無くなる）
      NeedRand : bool
      /// この式が $rank を使うか
      NeedRank : bool }

    override this.ToString () = this.Source
    /// 等値は文字列で見る。木は文字列から決まるので、同じ文字列なら同じ木
    override this.Equals (o: obj) =
      match o with
      | :? NumExpr as other -> this.Source = other.Source
      | _ -> false
    override this.GetHashCode () = if isNull this.Source then 0 else this.Source.GetHashCode()

  module NumExpr =
    /// 文字列から作る唯一の入口。ここを通さずに作れないので、
    /// 「読んでいない式」が木の中に紛れ込まない。
    let ofString (s: string) : NumExpr =
      let ast = parse s
      let needRand, needRank = uses ast
      { Source = s; Ast = ast; NeedRand = needRand; NeedRank = needRank }

    let text (e: NumExpr) = e.Source

    /// 読めた式か。 読めなかった節（`Invalid`）が 1 つ でも在れば false。
    ///
    /// 走行は読めない式を NaN で素通りさせるが、定数を畳む段は昔から
    /// 落ちていた（`Diagnosis` の 4 層 目）。XPath を外したので同じ線をここで引く。
    let isReadable (e: NumExpr) : bool =
      let rec ok node =
        match node with
        | Invalid -> false
        | Num _ | Rand | Rank -> true
        | Neg a -> ok a
        | Add (a, b) | Sub (a, b) | Mul (a, b) | Div (a, b) | Mod (a, b) -> ok a && ok b
      ok e.Ast

    /// 走行中の入口。木を歩くのは計算のときだけで、
    /// $rand / $rank を使うかは読んだときに決まっている
    let eval (rand: unit -> float32) (rank: float32) (e: NumExpr) : float32 =
      // 引く回数を合わせるため、値を使わなくても必ず 1 回 引く
      let r = rand ()
      let rd = if e.NeedRand then widen r else 0.0
      let kd = if e.NeedRank then widen rank else 0.0
      float32 (evalWith rd kd e.Ast)

    let evalWithValues (randValue: float32) (rank: float32) (e: NumExpr) : float32 =
      let rd = if e.NeedRand then widen randValue else 0.0
      let kd = if e.NeedRank then widen rank else 0.0
      float32 (evalWith rd kd e.Ast)

    /// 文字を置き換えてから読み直す。Param.replace の置き換え関数を受け取る
    let mapSource (f: string -> string) (e: NumExpr) : NumExpr = ofString (f e.Source)
