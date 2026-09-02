namespace FsBulletML2

open System
open System.Globalization

/// BulletML の数値式を、文字列でなく木で持つ。
///
/// いまは `<direction>` `<speed>` `<term>` `<times>` `<wait>` などの中身が
/// `string` のまま DU に入っていて、値が要るたびに Processable.getValue が
/// 文字を置き換え、Regex を new し、XPathDocument を組み立て、XPath を
/// 評価している。1 回 2.49 us で、走行時間の 27〜61% がここに乗る
/// （bench の BREAKDOWN.md）。
///
/// ここは「同じ文字列を同じ意味で読む」木と評価器で、いまの getValue と
/// 1 ビットも違わない値を返すことを目標にする。速さは、その次。
///
/// ## 何に合わせるのか
///
/// 現行の Util.TryParse.xpathNumber は
///
///   1. + - * の前後に空白を入れる
///   2. "/" を " div " に、"%" を " mod " に置き換える
///   3. number(その式) を XPath として評価する（値は IEEE 754 の double）
///   4. Convert.ToString して Single.Parse する
///
/// をしている。つまり意味論は **XPath 1.0 の数値式**で、精度は double、
/// 最後に float32 へ丸める。ここもそれに合わせる。
///
/// ## 現行と違うところ（2 つ、どちらも現行が落ちる側）
///
/// **1. 小さい $rand / $rank で現行は例外になる。**
/// getValue は木を作る前に $rand / $rank を「その値の文字列」へ置き換える。
/// float32 の ToString は 1e-4 未満で "1E-07" のような指数表記を吐き、
/// xpathNumber がそれを " + - * " の空白入れで "1E - 07" に割ってしまう。
/// XPath はこれを読めず **XPathException を投げる**（NaN ではない）。
/// $rand は [0,1) の一様乱数なので、1e-4 未満を引くたびに落ちる
/// —— おおよそ 1 万 回 に 1 回。同梱の FixedManager は 0.5 しか返さないので
/// 控え 227 本 では一度も踏まない。この木は数として評価するので落ちない。
///
/// **2. 読めない式で現行は例外、ここは NaN。**
/// number(abc) は XPath ではノード集合の検査になり、
/// "Expression must evaluate to a node-set." で落ちる。ここは Invalid にして
/// NaN を返す。落とすほうへ寄せると、いま静かに進んでいる台本が落ちる
/// —— ただし同梱の 227 本 に読めない式は 1 つも無い（ExprTests が数える）
/// ので、この差で走行が変わる台本は無い。
///
/// どちらも「現行が落ち、こちらは落ちない」向きの差。直っている側だが、
/// 差であることを書いておく。
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
    /// 読めなかった式。現行は XPath の number() が NaN を返すので、それに合わせる。
    /// 例外にしない —— 例外にすると、いま静かに NaN で進んでいる台本が落ちる
    | Invalid

  // ---- 読む ----

  /// 現行の getValue が置き換える順を写す。
  ///
  ///   "$rand" を先に、次に "$rank"、残った "$" + 数字 を 0 に。
  ///
  /// 順が要るのは、Replace が部分一致だから。"$random" は "$rand" が先に
  /// 当たって "0.5om" になる（現行がそうなっている）。ここも同じ順で読む
  let private tryVar (s: string) (i: int) =
    if i + 5 <= s.Length && String.CompareOrdinal(s, i, "$rand", 0, 5) = 0 then Some (Rand, i + 5)
    elif i + 5 <= s.Length && String.CompareOrdinal(s, i, "$rank", 0, 5) = 0 then Some (Rank, i + 5)
    elif i < s.Length && s.[i] = '$' then
      // 現行の Regex.Replace(s, "\$\d*", "0")。$ のあとの数字を食べて 0 にする。
      // 数字が 0 個 でも当たる（"$" だけでも 0 になる）
      let mutable j = i + 1
      while j < s.Length && Char.IsDigit s.[j] do j <- j + 1
      Some (Num 0.0, j)
    else None

  /// 数値リテラル。XPath 1.0 の Number は  digits ('.' digits?)?  |  '.' digits
  /// 指数表記は無い（現行が number() に投げているので、ここも受けない）
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
  ///
  /// / と % は現行が div / mod へ置き換えているので、XPath の
  /// MultiplicativeExpr と同じ段。単項マイナスはその下
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
      // XPath 1.0 に単項プラスは無い。現行は number() が NaN を返すので合わせる
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

  /// 文字列を木にする。読めなければ Invalid（現行の number() が NaN を返す形）
  let parse (s: string) : Node =
    if isNull s then Invalid
    else
      match parseExpr s 0 with
      | Some (node, pos) when skipWs s pos = s.Length -> node
      | _ -> Invalid

  // ---- 評価する ----

  /// float32 を、現行の getValue と同じ道筋で double にする。
  ///
  /// 現行は $rand / $rank を「その float32 の ToString」で式へ埋め、
  /// XPath がその**文字列を double として**読む。だから
  ///
  ///   0.987654f -> "0.987654" -> double の 0.987654       （現行）
  ///   0.987654f -> double の 0.98765397071838379...        （そのまま widen）
  ///
  /// で 1e-9 ずれ、float32 に戻したときの最後の 1 ビットが違う。
  /// **同梱の FixedManager は 0.5 を返し、0.5 は両方で同じになるので、
  /// 控え 227 本 ではこの違いが一度も出ない。** 実際の rank を使うと出る。
  ///
  /// NaN / 無限大 は、現行だと "NaN" "Infinity" という文字が式に埋まって
  /// XPath の number() が NaN を返すので、ここも NaN にする
  let private widen (v: float32) : double =
    if Single.IsNaN v || Single.IsInfinity v then Double.NaN
    else
      match Double.TryParse(v.ToString(CultureInfo.InvariantCulture),
                            NumberStyles.Float, CultureInfo.InvariantCulture) with
      | true, d -> d
      | _ -> Double.NaN

  /// 木が $rand / $rank を使うか。使わないなら widen を通らずに済む。
  /// 実物の式の 61% は数値リテラルだけなので、ここで落とせる数が多い
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

  /// 現行の getValue と同じ型で返す。
  ///
  /// **乱数は 1 回 の評価につき 1 回 だけ引く。$rand が何個 あっても、
  /// 1 個 も無くても、必ず 1 回。** 現行の getValue が
  ///
  ///   let rand = env.Rand ()
  ///   s.Replace("$rand", rand.ToString(...))
  ///
  /// と書いていて、引くのが式の中身と無関係に 1 回 だからで、しかも
  /// 同じ式の中の $rand は全部 同じ値になる。引く回数は乱数の並びを
  /// 進めるので、ここを写し違えると 227 本 の軌跡が丸ごとずれる。
  ///
  /// 現行は double を Convert.ToString して Single.Parse している。
  /// .NET の double -> string は往復できる最短表記なので、読み直すと
  /// 「その double にいちばん近い float32」になる。float32 への直接の
  /// 変換と同じはずだが、同じであることは ExprTests が実物で確かめる
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
  /// **文字列を捨てない。** actionRef / fireRef / bulletRef の実引数は
  /// Param.replace が「文字の置き換え」で入れていて、置き換えは優先順位を
  /// 変えるから（実引数 "1+2" を "$1*3" へ入れると 1+2*3 = 7 になる。
  /// 木の上で節として差し込むと (1+2)*3 = 9 で、値が変わる）。
  /// 同梱の 227 本 のうち 50 本 が、トップレベルに二項の + / - を持つ
  /// 実引数を含んでいる。だから置き換えは文字のままやり、その結果を読み直す。
  ///
  /// 読むのは「文字列が決まった時点」で 1 回。走行中は木を評価するだけ
  [<CustomEquality; NoComparison>]
  type NumExpr =
    { /// もとの文字列。Param.replace と、XML へ書き戻すときに要る
      Source : string
      /// Source を読んだ木
      Ast : Node
      /// この式が $rand を使うか。読んだときに 1 回 数える。
      /// 評価のたびに木を歩いて調べると、木にした意味が無くなる
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
    /// $rand / $rank を使うかも、ここで 1 回 だけ数える
    let ofString (s: string) : NumExpr =
      let ast = parse s
      let needRand, needRank = uses ast
      { Source = s; Ast = ast; NeedRand = needRand; NeedRank = needRank }

    let text (e: NumExpr) = e.Source

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
