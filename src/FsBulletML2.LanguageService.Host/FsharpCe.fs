namespace FsBulletML2.LanguageService

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FsBulletML2
open FsBulletML2.Dsl

/// **F# の CE を読む。** `FSharp.Compiler.Service` で parse して、
/// 出てきた木を歩いて `Bulletml` を組む。
///
/// --- なぜ parse だけなのか
///
/// 型検査まで通すには参照アセンブリをブラウザへ配ることになり、
/// 6 MB では済まない。**parse だけなら FCS の 1 本 で足りる**
/// （v1.0 の頭で測った —— 弾幕 1 本 で 11〜199 ms、
/// ファイル丸ごと 2703 行 で 149〜324 ms）。
///
/// --- 意味論を書き直さない
///
/// **歩いたあとに呼ぶのは `FsBulletML2.Dsl` そのもの。**
/// builder は `Zero` / 各 `CustomOperation` / `Run` がどれも public なので、
/// 木の形から同じ呼び出しを組み立てれば、**正本は `BulletDsl.fs` 1 本 のまま。**
///
/// ここに在るのは「名前 -> どの呼び出しか」の振り分けだけで、
/// **その振り分けが合っているかはコーパス 176 本 で突き合わせる**
/// （`Parser.Tests/FsharpCeCorpus.fs`）。
///
/// --- 歩く相手は狭い
///
/// コーパス 36 ファイル を parse して出てきた `SynExpr` の腕は 11 種類 だけ ——
/// `App` / `Ident` / `Const` / `Sequential` / `ComputationExpr` /
/// `ArrayOrList` / `ArrayOrListComputed` / `Paren` / `LongIdent` / `Do` / `Typed`。
/// **`Lambda` も `Match` も `IfThenElse` も `For` も `LetOrUse` も 0 だった。**
///
/// だが**それはコーパスの狭さであって、F# の狭さではない** —— 人が書けば出る。
/// 知らない形は「読めない」と位置つきで言う（黙って落とさない）。
module FsharpCe =

  /// 読めなかった場所と理由。**位置は 1 起点**（`Failure` と同じ）
  exception private CannotRead of line: int * col: int * message: string

  let private fail (r: range) (message: string) =
    raise (CannotRead(r.StartLine, r.StartColumn + 1, message))

  // --- 木をほどく ------------------------------------------------------------

  /// `f a b c` を `(f, [a; b; c])` に。**括弧は透かす**
  let rec private flatten (e: SynExpr) (acc: SynExpr list) =
    match e with
    | SynExpr.App (funcExpr = f; argExpr = a) -> flatten f (a :: acc)
    | SynExpr.Paren (expr = inner) -> flatten inner acc
    | _ -> e, acc

  /// 頭の名前。`Ident` も `LongIdent`（`Dsl.wait` のような書き方）も同じに扱う
  let private headName (e: SynExpr) =
    match e with
    | SynExpr.Ident id -> Some id.idText
    | SynExpr.LongIdent (longDotId = SynLongIdent (id = ids)) when not ids.IsEmpty ->
      Some (List.last ids).idText
    | _ -> None

  /// CE の中身を「文の並び」に。`Sequential` をほどく
  let rec private statements (e: SynExpr) =
    match e with
    | SynExpr.Sequential (expr1 = a; expr2 = b) -> statements a @ statements b
    | SynExpr.Do (expr = inner) -> statements inner
    | SynExpr.Paren (expr = inner) -> statements inner
    | SynExpr.Typed (expr = inner) -> statements inner
    // `()` は空の本体（`body { () }`）。**文を 0 個 として扱う**
    | SynExpr.Const (SynConst.Unit, _) -> []
    | other -> [ other ]

  /// `{ ... }` の中身
  let private block (e: SynExpr) =
    match e with
    | SynExpr.ComputationExpr (expr = inner) -> Some inner
    | _ -> None

  let private str (e: SynExpr) =
    match e with
    | SynExpr.Const (SynConst.String (text = s), _) -> s
    | SynExpr.Paren (expr = inner) ->
      match inner with
      | SynExpr.Const (SynConst.String (text = s), _) -> s
      | _ -> fail inner.Range "文字列のはず"
    | other -> fail other.Range "文字列のはず"

  /// `["a"; "b"]`。空の `[]` も通る
  let private strList (e: SynExpr) =
    let items =
      match e with
      | SynExpr.ArrayOrList (exprs = xs) -> xs
      | SynExpr.ArrayOrListComputed (expr = inner) -> statements inner
      | SynExpr.Paren (expr = inner) ->
        match inner with
        | SynExpr.ArrayOrList (exprs = xs) -> xs
        | SynExpr.ArrayOrListComputed (expr = i2) -> statements i2
        | other -> fail other.Range "文字列の並びのはず"
      | other -> fail other.Range "文字列の並びのはず"
    items |> List.map str

  // --- accel ---------------------------------------------------------------

  let private readAccel (term: string) (body: SynExpr) =
    let b = Dsl.accel term
    let mutable s = b.Zero()
    for st in statements body do
      let head, args = flatten st []
      match headName head, args with
      | Some "horizontal", [ a ] -> s <- b.Horizontal(s, str a)
      | Some "horizontalAbs", [ a ] -> s <- b.HorizontalAbs(s, str a)
      | Some "horizontalRel", [ a ] -> s <- b.HorizontalRel(s, str a)
      | Some "horizontalSeq", [ a ] -> s <- b.HorizontalSeq(s, str a)
      | Some "vertical", [ a ] -> s <- b.Vertical(s, str a)
      | Some "verticalAbs", [ a ] -> s <- b.VerticalAbs(s, str a)
      | Some "verticalRel", [ a ] -> s <- b.VerticalRel(s, str a)
      | Some "verticalSeq", [ a ] -> s <- b.VerticalSeq(s, str a)
      | Some n, _ -> fail st.Range (sprintf "accel の中に書けない: %s" n)
      | None, _ -> fail st.Range "accel の中に書けない形"
    b.Run s

  // --- bullet --------------------------------------------------------------

  /// `bullet "x" { ... }` / `bulletAnon { ... }` / `defBullet "x" { ... }`
  /// **型変数を明示する。** `let rec ... and` の束は、書かないと最初の
  /// 使い方で固まって、2 つ 目 の当て方で型が合わなくなる
  let rec private readBulletSpec<'T> (b: BulletBuilder<'T>) (body: SynExpr) : 'T =
    let mutable s = b.Zero()
    for st in statements body do
      let head, args = flatten st []
      match headName head, args with
      | Some "aim", [ a ] -> s <- b.Aim(s, str a)
      | Some "absolute", [ a ] -> s <- b.Absolute(s, str a)
      | Some "relative", [ a ] -> s <- b.Relative(s, str a)
      | Some "sequence", [ a ] -> s <- b.Sequence(s, str a)
      | Some "dir", [ a ] -> s <- b.Dir(s, str a)
      | Some "speed", [ a ] -> s <- b.Speed(s, str a)
      | Some "speedAbs", [ a ] -> s <- b.SpeedAbs(s, str a)
      | Some "speedRel", [ a ] -> s <- b.SpeedRel(s, str a)
      | Some "speedSeq", [ a ] -> s <- b.SpeedSeq(s, str a)
      | Some "doActs", [ a ] -> s <- b.DoActs(s, readActionElm a)
      | Some "refActs", [ l; ps ] -> s <- b.RefActs(s, str l, strList ps)
      | Some n, _ -> fail st.Range (sprintf "bullet の中に書けない: %s" n)
      | None, _ -> fail st.Range "bullet の中に書けない形"
    b.Run s

  /// `body { ... }` / `bodyAs "x" { ... }` / `bodyRef "x" [...]`
  and private readActionElm (e: SynExpr) : ActionElm =
    let head, args = flatten e []
    match headName head, args with
    | Some "body", [ blk ] ->
      match block blk with
      | Some inner -> readActions Dsl.body inner
      | None -> fail e.Range "body には { } が要る"
    | Some "bodyAs", [ name; blk ] ->
      match block blk with
      | Some inner -> readActions (Dsl.bodyAs (str name)) inner
      | None -> fail e.Range "bodyAs には { } が要る"
    | Some "bodyRef", [ l; ps ] -> Dsl.bodyRef (str l) (strList ps)
    | Some n, _ -> fail e.Range (sprintf "action の置き場に書けない: %s" n)
    | None, _ -> fail e.Range "action の置き場に書けない形"

  // --- fire ----------------------------------------------------------------

  and private readFireSpec<'T> (b: FireBuilder<'T>) (body: SynExpr) : 'T =
    let mutable s = b.Zero()
    for st in statements body do
      let head, args = flatten st []
      match headName head, args with
      | Some "dir", [ a ] -> s <- b.Dir(s, str a)
      | Some "aim", [ a ] -> s <- b.Aim(s, str a)
      | Some "absolute", [ a ] -> s <- b.Absolute(s, str a)
      | Some "relative", [ a ] -> s <- b.Relative(s, str a)
      | Some "sequence", [ a ] -> s <- b.Sequence(s, str a)
      | Some "speed", [ a ] -> s <- b.Speed(s, str a)
      | Some "speedAbs", [ a ] -> s <- b.SpeedAbs(s, str a)
      | Some "speedRel", [ a ] -> s <- b.SpeedRel(s, str a)
      | Some "speedSeq", [ a ] -> s <- b.SpeedSeq(s, str a)
      | Some "plain", [] -> s <- b.Plain s
      | Some "plain", _ -> s <- b.Plain s
      | Some "refBullet", [ l; ps ] -> s <- b.RefBullet(s, str l, strList ps)
      | Some "ofBullet", [ a ] -> s <- b.OfBullet(s, readBulletElm a)
      | Some n, _ -> fail st.Range (sprintf "fire の中に書けない: %s" n)
      | None, _ -> fail st.Range "fire の中に書けない形"
    b.Run s

  and private readBulletElm (e: SynExpr) : BulletElm =
    let head, args = flatten e []
    match headName head, args with
    | Some "bullet", [ name; blk ] ->
      match block blk with
      | Some inner -> readBulletSpec (Dsl.bullet (str name)) inner
      | None -> fail e.Range "bullet には { } が要る"
    | Some "bulletAnon", [ blk ] ->
      match block blk with
      | Some inner -> readBulletSpec Dsl.bulletAnon inner
      | None -> fail e.Range "bulletAnon には { } が要る"
    | Some "bulletRef", [ l; ps ] -> Dsl.bulletRef (str l) (strList ps)
    | Some n, _ -> fail e.Range (sprintf "bullet の置き場に書けない: %s" n)
    | None, _ -> fail e.Range "bullet の置き場に書けない形"

  // --- action の列 -----------------------------------------------------------

  and private readActions<'T> (b: ActionBuilder<'T>) (body: SynExpr) : 'T =
    let items =
      statements body
      |> List.map (fun st ->
          let head, args = flatten st []
          match headName head, args with
          | Some "wait", [ a ] -> Dsl.wait (str a)
          | Some "vanish", _ -> Dsl.vanish
          | Some "changeDirection", [ a; t ] -> Dsl.changeDirection (str a) (str t)
          | Some "changeDirectionAim", [ a; t ] -> Dsl.changeDirectionAim (str a) (str t)
          | Some "changeDirectionAbs", [ a; t ] -> Dsl.changeDirectionAbs (str a) (str t)
          | Some "changeDirectionRel", [ a; t ] -> Dsl.changeDirectionRel (str a) (str t)
          | Some "changeDirectionSeq", [ a; t ] -> Dsl.changeDirectionSeq (str a) (str t)
          | Some "changeSpeed", [ a; t ] -> Dsl.changeSpeed (str a) (str t)
          | Some "changeSpeedAbs", [ a; t ] -> Dsl.changeSpeedAbs (str a) (str t)
          | Some "changeSpeedRel", [ a; t ] -> Dsl.changeSpeedRel (str a) (str t)
          | Some "changeSpeedSeq", [ a; t ] -> Dsl.changeSpeedSeq (str a) (str t)
          | Some "fireRef", [ l; ps ] -> Dsl.fireRef (str l) (strList ps)
          | Some "actionRef", [ l; ps ] -> Dsl.actionRef (str l) (strList ps)
          | Some "repeatRef", [ t; l; ps ] -> Dsl.repeatRef (str t) (str l) (strList ps)
          | Some "accel", [ t; blk ] ->
            match block blk with
            | Some inner -> readAccel (str t) inner
            | None -> fail st.Range "accel には { } が要る"
          | Some "fire", [ blk ] ->
            match block blk with
            | Some inner -> readFireSpec Dsl.fire inner
            | None -> fail st.Range "fire には { } が要る"
          | Some "fireAs", [ name; blk ] ->
            match block blk with
            | Some inner -> readFireSpec (Dsl.fireAs (str name)) inner
            | None -> fail st.Range "fireAs には { } が要る"
          | Some "repeat", [ times; blk ] ->
            match block blk with
            | Some inner -> readActions (Dsl.repeat (str times)) inner
            | None -> fail st.Range "repeat には { } が要る"
          | Some "repeatAs", [ times; name; blk ] ->
            match block blk with
            | Some inner -> readActions (Dsl.repeatAs (str times) (str name)) inner
            | None -> fail st.Range "repeatAs には { } が要る"
          | Some "nest", [ blk ] ->
            match block blk with
            | Some inner -> readActions Dsl.nest inner
            | None -> fail st.Range "nest には { } が要る"
          | Some "nestAs", [ name; blk ] ->
            match block blk with
            | Some inner -> readActions (Dsl.nestAs (str name)) inner
            | None -> fail st.Range "nestAs には { } が要る"
          | Some n, _ -> fail st.Range (sprintf "action の中に書けない: %s" n)
          | None, _ -> fail st.Range "action の中に書けない形")
    b.Run items

  // --- 根 -------------------------------------------------------------------

  let private readRootElm (e: SynExpr) : BulletmlElm =
    let head, args = flatten e []
    match headName head, args with
    | Some "top", [ blk ] ->
      match block blk with
      | Some inner -> readActions Dsl.top inner
      | None -> fail e.Range "top には { } が要る"
    | Some "defAction", [ name; blk ] ->
      match block blk with
      | Some inner -> readActions (Dsl.defAction (str name)) inner
      | None -> fail e.Range "defAction には { } が要る"
    | Some "defActionAnon", [ blk ] ->
      match block blk with
      | Some inner -> readActions Dsl.defActionAnon inner
      | None -> fail e.Range "defActionAnon には { } が要る"
    | Some "defBullet", [ name; blk ] ->
      match block blk with
      | Some inner -> readBulletSpec (Dsl.defBullet (str name)) inner
      | None -> fail e.Range "defBullet には { } が要る"
    | Some "defBulletAnon", [ blk ] ->
      match block blk with
      | Some inner -> readBulletSpec Dsl.defBulletAnon inner
      | None -> fail e.Range "defBulletAnon には { } が要る"
    | Some "topFire", [ blk ] ->
      match block blk with
      | Some inner -> readFireSpec Dsl.topFire inner
      | None -> fail e.Range "topFire には { } が要る"
    | Some "topFireAs", [ name; blk ] ->
      match block blk with
      | Some inner -> readFireSpec (Dsl.topFireAs (str name)) inner
      | None -> fail e.Range "topFireAs には { } が要る"
    | Some n, _ -> fail e.Range (sprintf "bulletml の直下に書けない: %s" n)
    | None, _ -> fail e.Range "bulletml の直下に書けない形"

  /// `vertical "名" { ... }` などの根。`createBulletmlInfo <| ...` は剥がして渡す
  let private readBulletml (e: SynExpr) : Bulletml =
    let head, args = flatten e []
    let build (b: BulletmlBuilder) (blk: SynExpr) =
      match block blk with
      | Some inner -> b.Run(statements inner |> List.map readRootElm)
      | None -> fail e.Range "bulletml には { } が要る"
    match headName head, args with
    | Some "vertical", [ name; blk ] -> build (Dsl.vertical (str name)) blk
    | Some "horizontal", [ name; blk ] -> build (Dsl.horizontal (str name)) blk
    | Some "none", [ name; blk ] -> build (Dsl.none (str name)) blk
    | Some "untyped", [ name; blk ] -> build (Dsl.untyped (str name)) blk
    | Some "verticalXmlns", [ xmlns; name; blk ] -> build (Dsl.verticalXmlns (str xmlns) (str name)) blk
    | Some "horizontalXmlns", [ xmlns; name; blk ] -> build (Dsl.horizontalXmlns (str xmlns) (str name)) blk
    | Some "noneXmlns", [ xmlns; name; blk ] -> build (Dsl.noneXmlns (str xmlns) (str name)) blk
    | Some "untypedXmlns", [ xmlns; name; blk ] -> build (Dsl.untypedXmlns (str xmlns) (str name)) blk
    | Some n, _ -> fail e.Range (sprintf "弾幕の根に書けない: %s" n)
    | None, _ -> fail e.Range "弾幕の根に書けない形"

  /// `createBulletmlInfo <| ...` の左辺を剥がす。無くても通す。
  ///
  /// **`<|` の名前は `op_PipeLeft`**（`op_LessBar` ではない。最初そう書いて
  /// コーパス 176 本 が全部 「弾幕の根に書けない」で落ちた）
  let rec private stripInfo (e: SynExpr) =
    match e with
    | SynExpr.Paren (expr = inner) -> stripInfo inner
    | _ ->
      // `createBulletmlInfo <| x` は `(<|) createBulletmlInfo x` の形になる
      let head, args = flatten e []
      match headName head, args with
      | Some "op_PipeLeft", [ _; x ] -> stripInfo x
      | Some "createBulletmlInfo", [ x ] -> stripInfo x
      | _ -> e

  // --- 入口 ------------------------------------------------------------------

  let private checker = lazy FSharpChecker.Create()

  /// **ブラウザ（wasm）では待てない。**
  ///
  /// FCS の `ParseFile` は `Async` を返すが、`Async.RunSynchronously` で
  /// 待つと**ブラウザで固まる** —— スレッドが 1 本 しか無いので、
  /// 続きを流す相手がそのスレッドで塞がれている。
  ///
  /// **build も、.NET の試験も、これでは落ちない。** 押した瞬間に
  /// 画面ごと止まって初めて分かる（v1.0 でブラウザに載せて踏んだ）。
  ///
  /// parse は CPU だけの仕事で実際には切り替わらないので、続きを直に
  /// 受け取って**その場で終わったかを見る。** 終わっていなければ
  /// 「終わらなかった」と言う —— **黙って待たない。**
  let private runHere (computation: Async<'T>) : Result<'T, string> =
    let mutable result = None
    Async.StartWithContinuations(
      computation,
      (fun value -> result <- Some(Ok value)),
      (fun (ex: exn) -> result <- Some(Error ex.Message)),
      (fun _ -> result <- Some(Error "parse が取り消された")))
    match result with
    | Some outcome -> outcome
    | None -> Error "parse がその場で終わらなかった（ブラウザでは待てない）"

  /// 本文の中の最初の弾幕を読む。**`let` の並びから 1 本 目 を採る。**
  ///
  /// 返りは `Ok` か、位置つきの `Error`（行・桁 は 1 起点）
  let read (source: string) : Result<Bulletml, int * int * string> =
    let opts = { FSharpParsingOptions.Default with SourceFiles = [| "editor.fsx" |] }
    match
      checker.Value.ParseFile("editor.fsx", SourceText.ofString source, opts)
      |> runHere
      with
    | Error message -> Error(0, 0, message)
    | Ok parsed ->

    let syntaxError =
      parsed.Diagnostics
      |> Array.tryFind (fun d -> d.Severity = FSharp.Compiler.Diagnostics.FSharpDiagnosticSeverity.Error)
    match syntaxError with
    | Some d -> Error(max 1 d.StartLine, max 1 (d.StartColumn + 1), d.Message)
    | None ->

    let bindings =
      match parsed.ParseTree with
      | ParsedInput.ImplFile (ParsedImplFileInput (contents = modules)) ->
        modules
        |> List.collect (fun (SynModuleOrNamespace (decls = decls)) ->
            decls
            |> List.collect (function
                | SynModuleDecl.Let (bindings = bs) ->
                  bs |> List.map (fun (SynBinding (expr = e)) -> e)
                | SynModuleDecl.Expr (expr = e) -> [ e ]
                | _ -> []))
      | _ -> []

    match bindings with
    | [] -> Error(1, 1, "弾幕が 1 本 も書かれていない")
    | first :: _ ->
      try Ok(readBulletml (stripInfo first))
      with
      | CannotRead (line, col, message) -> Error(line, col, message)
      | ex -> Error(0, 0, ex.Message)
