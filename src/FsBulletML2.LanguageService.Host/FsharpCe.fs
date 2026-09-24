namespace FsBulletML2.LanguageService

open System.Text
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FsBulletML2
open FsBulletML2.Dsl

/// F# の CE を読む。parse して木を歩く。型検査しない。
/// 知らない形は「読めない」と位置つきで言う。黙って落とすな。
module FsharpCe =

    /// 読めなかった場所と理由。位置は 1 起点（`Failure` と同じ）
    exception private CannotRead of line: int * col: int * message: string

    let private fail (r: range) (message: string) =
        raise (CannotRead(r.StartLine, r.StartColumn + 1, message))

    // --- 木をほどく ------------------------------------------------------------

    /// `f a b c` を `(f, [a; b; c])` に。括弧は透かす
    let rec private flatten (e: SynExpr) (acc: SynExpr list) =
        match e with
        | SynExpr.App(funcExpr = f; argExpr = a) -> flatten f (a :: acc)
        | SynExpr.Paren(expr = inner) -> flatten inner acc
        | _ -> e, acc

    /// 頭の名前。`Ident` も `LongIdent`（`Dsl.wait` のような書き方）も同じに扱う
    let private headName (e: SynExpr) =
        match e with
        | SynExpr.Ident id -> Some id.idText
        | SynExpr.LongIdent(longDotId = SynLongIdent(id = ids)) when not ids.IsEmpty -> Some (List.last ids).idText
        | _ -> None

    /// CE の中身を「文の並び」に。`Sequential` をほどく
    let rec private statements (e: SynExpr) =
        match e with
        | SynExpr.Sequential(expr1 = a; expr2 = b) -> statements a @ statements b
        | SynExpr.Do(expr = inner) -> statements inner
        | SynExpr.Paren(expr = inner) -> statements inner
        | SynExpr.Typed(expr = inner) -> statements inner
        // `()` は空の本体（`body { () }`）。文を 0 個 として扱う
        | SynExpr.Const(SynConst.Unit, _) -> []
        | other -> [ other ]

    /// `{ ... }` の中身
    let private block (e: SynExpr) =
        match e with
        | SynExpr.ComputationExpr(expr = inner) -> Some inner
        | _ -> None

    let private str (e: SynExpr) =
        match e with
        | SynExpr.Const(SynConst.String(text = s), _) -> s
        | SynExpr.Paren(expr = inner) ->
            match inner with
            | SynExpr.Const(SynConst.String(text = s), _) -> s
            | _ -> fail inner.Range "文字列のはず"
        | other -> fail other.Range "文字列のはず"

    /// `["a"; "b"]`。空の `[]` も通る
    let private strList (e: SynExpr) =
        let items =
            match e with
            | SynExpr.ArrayOrList(exprs = xs) -> xs
            | SynExpr.ArrayOrListComputed(expr = inner) -> statements inner
            | SynExpr.Paren(expr = inner) ->
                match inner with
                | SynExpr.ArrayOrList(exprs = xs) -> xs
                | SynExpr.ArrayOrListComputed(expr = i2) -> statements i2
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
    //
    // `let rec ... and` は型変数を明示する。書かないと 2 つ 目 で型が合わない。
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
        // 名前を書かない根（v1.4）。本家の弾幕はこちらが普通
        | Some "verticalAnon", [ blk ] -> build Dsl.verticalAnon blk
        | Some "horizontalAnon", [ blk ] -> build Dsl.horizontalAnon blk
        | Some "noneAnon", [ blk ] -> build Dsl.noneAnon blk
        | Some "untypedAnon", [ blk ] -> build Dsl.untypedAnon blk
        | Some "verticalXmlnsAnon", [ xmlns; blk ] -> build (Dsl.verticalXmlnsAnon (str xmlns)) blk
        | Some "horizontalXmlnsAnon", [ xmlns; blk ] -> build (Dsl.horizontalXmlnsAnon (str xmlns)) blk
        | Some "noneXmlnsAnon", [ xmlns; blk ] -> build (Dsl.noneXmlnsAnon (str xmlns)) blk
        | Some "untypedXmlnsAnon", [ xmlns; blk ] -> build (Dsl.untypedXmlnsAnon (str xmlns)) blk
        | Some n, _ -> fail e.Range (sprintf "弾幕の根に書けない: %s" n)
        | None, _ -> fail e.Range "弾幕の根に書けない形"

    /// `createBulletmlInfo <| ...` の左辺を剥がす。無くても通す。
    /// `<|` の名前は `op_PipeLeft`。`op_LessBar` ではない。
    let rec private stripInfo (e: SynExpr) =
        match e with
        | SynExpr.Paren(expr = inner) -> stripInfo inner
        | _ ->
            // `createBulletmlInfo <| x` は `(<|) createBulletmlInfo x` の形になる
            let head, args = flatten e []

            match headName head, args with
            | Some "op_PipeLeft", [ _; x ] -> stripInfo x
            | Some "createBulletmlInfo", [ x ] -> stripInfo x
            | _ -> e

    // --- 入口 ------------------------------------------------------------------

    let private checker = lazy FSharpChecker.Create()

    /// ブラウザでは待てない。`Async.RunSynchronously` は使うな。固まる。
    /// 終わらなかったらそう言う。黙って待たない。
    let private runHere (computation: Async<'T>) : Result<'T, string> =
        let mutable result = None

        Async.StartWithContinuations(
            computation,
            (fun value -> result <- Some(Ok value)),
            (fun (ex: exn) -> result <- Some(Error ex.Message)),
            (fun _ -> result <- Some(Error "parse が取り消された"))
        )

        match result with
        | Some outcome -> outcome
        | None -> Error "parse がその場で終わらなかった（ブラウザでは待てない）"

    /// 本文の中の最初の弾幕を読む。`let` の並びから 1 本 目。
    /// 返りは `Ok` か、位置つきの `Error`。
    let read (source: string) : Result<Bulletml, int * int * string> =
        let opts =
            { FSharpParsingOptions.Default with
                SourceFiles = [| "editor.fsx" |]
            }

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
                    | ParsedInput.ImplFile(ParsedImplFileInput(contents = modules)) ->
                        modules
                        |> List.collect (fun (SynModuleOrNamespace(decls = decls)) ->
                            decls
                            |> List.collect (function
                                | SynModuleDecl.Let(bindings = bs) -> bs |> List.map (fun (SynBinding(expr = e)) -> e)
                                | SynModuleDecl.Expr(expr = e) -> [ e ]
                                | _ -> []))
                    | _ -> []

                match bindings with
                | [] -> Error(1, 1, "弾幕が 1 本 も書かれていない")
                | first :: _ ->
                    try
                        Ok(readBulletml (stripInfo first))
                    with
                    | CannotRead(line, col, message) -> Error(line, col, message)
                    | ex -> Error(0, 0, ex.Message)

    // --- 書く ------------------------------------------------------------------
    //
    // 書く口は `read` の隣。離すと、読めるのに書けない、が片方だけ直る。
    let private quote (s: string) =
        // 改行は `\n` に揃える。CE だけ `\r` を残すと、表記を変えたとき値だけ動く。
        let s = s.Replace("\r\n", "\n").Replace("\r", "\n")
        let b = StringBuilder()
        b.Append '"' |> ignore

        for c in s do
            match c with
            | '\\' -> b.Append "\\\\" |> ignore
            | '"' -> b.Append "\\\"" |> ignore
            | '\n' -> b.Append "\\n" |> ignore
            | '\r' -> b.Append "\\r" |> ignore
            | '\t' -> b.Append "\\t" |> ignore
            | c -> b.Append c |> ignore

        b.Append('"').ToString()

    let private plist (ps: Params) =
        "[ " + (ps |> List.map quote |> String.concat "; ") + " ]"

    /// 弾幕を F# の CE の字にする。
    /// 書けないものは `Error`。`description` を書く口は `Dsl` に無い。
    let write (bulletml: Bulletml) : Result<string, string> =
        let sb = StringBuilder()
        let bad = ResizeArray<string>()

        let line (depth: int) (text: string) =
            sb.Append(String.replicate (depth * 2) " ").Append(text).Append("\n") |> ignore

        let exprText (e: Expr.NumExpr) = Expr.NumExpr.text e

        /// 空の `{ }` は書けない。記録式に見えて `read` が落ちる。
        /// 中身が空なら `()` を置く。値は変わらない。
        let bodyOrUnit (depth: int) (write: unit -> unit) =
            let before = sb.Length
            write ()

            if sb.Length = before then
                line depth "()"

        /// direction は type ごとに名前が違う。型を書かない形が `dir`
        let dirCall (Direction(attrs, e)) =
            let name =
                match attrs with
                | None -> "dir"
                | Some a ->
                    match a.directionType with
                    | DirectionType.Aim -> "aim"
                    | DirectionType.Absolute -> "absolute"
                    | DirectionType.Relative -> "relative"
                    | DirectionType.Sequence -> "sequence"

            name + " " + quote (exprText e)

        let speedCall (Speed(attrs, e)) =
            let name =
                match attrs with
                | None -> "speed"
                | Some a ->
                    match a.speedType with
                    | SpeedType.Absolute -> "speedAbs"
                    | SpeedType.Relative -> "speedRel"
                    | SpeedType.Sequence -> "speedSeq"

            name + " " + quote (exprText e)

        /// `plain` は「何も書いていない弾」。そこだけ名前が 1 語
        let isPlain (b: BulletElm) =
            match b with
            | BulletElm.Bullet(attrs, None, None, []) -> attrs.bulletLabel.IsNone
            | _ -> false

        let rec writeActions (depth: int) (xs: Action list) =
            for a in xs do
                match a with
                | Action.Wait e -> line depth ("wait " + quote (exprText e))
                | Action.Vanish -> line depth "vanish"
                | Action.ChangeDirection(Direction(attrs, e), Term t) ->
                    let name =
                        match attrs with
                        | None -> "changeDirection"
                        | Some a ->
                            match a.directionType with
                            | DirectionType.Aim -> "changeDirectionAim"
                            | DirectionType.Absolute -> "changeDirectionAbs"
                            | DirectionType.Relative -> "changeDirectionRel"
                            | DirectionType.Sequence -> "changeDirectionSeq"

                    line depth (name + " " + quote (exprText e) + " " + quote (exprText t))
                | Action.ChangeSpeed(Speed(attrs, e), Term t) ->
                    let name =
                        match attrs with
                        | None -> "changeSpeed"
                        | Some a ->
                            match a.speedType with
                            | SpeedType.Absolute -> "changeSpeedAbs"
                            | SpeedType.Relative -> "changeSpeedRel"
                            | SpeedType.Sequence -> "changeSpeedSeq"

                    line depth (name + " " + quote (exprText e) + " " + quote (exprText t))
                | Action.Accel(h, v, Term t) ->
                    line depth ("accel " + quote (exprText t) + " {")
                    // horizontal も vertical も無い accel が在りうる。空の `{ }` は書けない
                    if h.IsNone && v.IsNone then
                        line (depth + 1) "()"

                    match h with
                    | Some(Horizontal.Horizontal(attrs, e)) ->
                        let name =
                            match attrs with
                            | None -> "horizontal"
                            | Some a ->
                                match a.horizontalType with
                                | HorizontalType.Absolute -> "horizontalAbs"
                                | HorizontalType.Relative -> "horizontalRel"
                                | HorizontalType.Sequence -> "horizontalSeq"

                        line (depth + 1) (name + " " + quote (exprText e))
                    | None -> ()

                    match v with
                    | Some(Vertical.Vertical(attrs, e)) ->
                        let name =
                            match attrs with
                            | None -> "vertical"
                            | Some a ->
                                match a.verticalType with
                                | VerticalType.Absolute -> "verticalAbs"
                                | VerticalType.Relative -> "verticalRel"
                                | VerticalType.Sequence -> "verticalSeq"

                        line (depth + 1) (name + " " + quote (exprText e))
                    | None -> ()

                    line depth "}"
                | Action.Repeat(Times t, child) ->
                    match child with
                    | ActionElm.ActionRef(attrs, ps) ->
                        line
                            depth
                            ("repeatRef "
                             + quote (exprText t)
                             + " "
                             + quote (ActionLabel.text attrs.actionRefLabel)
                             + " "
                             + plist ps)
                    | ActionElm.Action(attrs, inner) ->
                        match attrs.actionLabel with
                        | None -> line depth ("repeat " + quote (exprText t) + " {")
                        | Some l ->
                            line depth ("repeatAs " + quote (exprText t) + " " + quote (ActionLabel.text l) + " {")

                        bodyOrUnit (depth + 1) (fun () -> writeActions (depth + 1) inner)
                        line depth "}"
                | Action.Fire(attrs, d, s, bullet) ->
                    match attrs.fireLabel with
                    | None -> line depth "fire {"
                    | Some l -> line depth ("fireAs " + quote (FireLabel.text l) + " {")

                    bodyOrUnit (depth + 1) (fun () -> writeFireBody (depth + 1) d s bullet)
                    line depth "}"
                | Action.FireRef(attrs, ps) ->
                    line depth ("fireRef " + quote (FireLabel.text attrs.fireRefLabel) + " " + plist ps)
                | Action.Action(attrs, inner) ->
                    match attrs.actionLabel with
                    | None -> line depth "nest {"
                    | Some l -> line depth ("nestAs " + quote (ActionLabel.text l) + " {")

                    bodyOrUnit (depth + 1) (fun () -> writeActions (depth + 1) inner)
                    line depth "}"
                | Action.ActionRef(attrs, ps) ->
                    line depth ("actionRef " + quote (ActionLabel.text attrs.actionRefLabel) + " " + plist ps)

        and writeFireBody depth d s (bullet: BulletElm) =
            d |> Option.iter (fun x -> line depth (dirCall x))
            s |> Option.iter (fun x -> line depth (speedCall x))

            if isPlain bullet then
                line depth "plain"
            else
                match bullet with
                | BulletElm.BulletRef(attrs, ps) ->
                    line depth ("refBullet " + quote (BulletLabel.text attrs.bulletRefLabel) + " " + plist ps)
                | BulletElm.Bullet(attrs, bd, bs, children) ->
                    match attrs.bulletLabel with
                    | None -> line depth "ofBullet (bulletAnon {"
                    | Some l -> line depth ("ofBullet (bullet " + quote (BulletLabel.text l) + " {")

                    bodyOrUnit (depth + 1) (fun () -> writeBulletBody (depth + 1) bd bs children)
                    line depth "})"

        and writeBulletBody depth d s (children: ActionElm list) =
            d |> Option.iter (fun x -> line depth (dirCall x))
            s |> Option.iter (fun x -> line depth (speedCall x))

            for c in children do
                match c with
                | ActionElm.ActionRef(attrs, ps) ->
                    line depth ("refActs " + quote (ActionLabel.text attrs.actionRefLabel) + " " + plist ps)
                | ActionElm.Action(attrs, inner) ->
                    match attrs.actionLabel with
                    | None -> line depth "doActs (body {"
                    | Some l -> line depth ("doActs (bodyAs " + quote (ActionLabel.text l) + " {")

                    bodyOrUnit (depth + 1) (fun () -> writeActions (depth + 1) inner)
                    line depth "})"

        let writeRoot depth (e: BulletmlElm) =
            match e with
            | BulletmlElm.Action(attrs, xs) ->
                match attrs.actionLabel with
                | Some l when ActionLabel.text l = "top" -> line depth "top {"
                | Some l -> line depth ("defAction " + quote (ActionLabel.text l) + " {")
                | None -> line depth "defActionAnon {"

                bodyOrUnit (depth + 1) (fun () -> writeActions (depth + 1) xs)
                line depth "}"
            | BulletmlElm.Bullet(attrs, d, s, children) ->
                match attrs.bulletLabel with
                | Some l -> line depth ("defBullet " + quote (BulletLabel.text l) + " {")
                | None -> line depth "defBulletAnon {"

                bodyOrUnit (depth + 1) (fun () -> writeBulletBody (depth + 1) d s children)
                line depth "}"
            | BulletmlElm.Fire(attrs, d, s, bullet) ->
                match attrs.fireLabel with
                | Some l -> line depth ("topFireAs " + quote (FireLabel.text l) + " {")
                | None -> line depth "topFire {"

                bodyOrUnit (depth + 1) (fun () -> writeFireBody (depth + 1) d s bullet)
                line depth "}"

        match bulletml with
        | Bulletml.Bulletml(attrs, children) ->
            if attrs.bulletmlDescription.IsSome then
                bad.Add "description を書く口が Dsl に無い（CE では書けない）"

            let baseName =
                match attrs.bulletmlType with
                | Some ShootingDirection.BulletVertical -> "vertical"
                | Some ShootingDirection.BulletHorizontal -> "horizontal"
                | Some ShootingDirection.BulletNone -> "none"
                | None -> "untyped"
            // 名前は省ける。無いときは `…Anon` の入口。
            let head =
                match attrs.bulletmlXmlns, attrs.bulletmlName with
                | Some x, Some n -> baseName + "Xmlns " + quote x + " " + quote n
                | Some x, None -> baseName + "XmlnsAnon " + quote x
                | None, Some n -> baseName + " " + quote n
                | None, None -> baseName + "Anon"

            line 0 (head + " {")
            bodyOrUnit 1 (fun () -> children |> List.iter (writeRoot 1))
            line 0 "}"

        if bad.Count = 0 then
            Ok(sb.ToString())
        else
            Error(String.concat "\n" (List.ofSeq bad))
