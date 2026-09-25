// 人が書くのは CE（このディレクトリの *.fs）。値から DU カタログと All.fs を焼く。
// 焼き直したら tests/FsBulletML2.Dsl.Tests を回す。
//     dotnet build src/FsBulletML2.Bullets.Dsl -c Release
//     dotnet fsi src/FsBulletML2.Bullets.Dsl/gen.fsx

#r @"bin\Release\net10.0\FsBulletML2.Core.dll"
#r @"bin\Release\net10.0\FsBulletML2.Dsl.dll"
#r @"bin\Release\net10.0\FsBulletML2.Bullets.Dsl.dll"

open System
open System.IO
open System.Reflection
open System.Text
open FsBulletML2

let repo = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))
let dslDir = Path.Combine(repo, @"src\FsBulletML2.Bullets.Dsl")
let duDir = Path.Combine(repo, @"samples\FsBulletML2.Bullets")

let dslDll =
    Path.Combine(dslDir, @"bin\Release\net10.0\FsBulletML2.Bullets.Dsl.dll")

/// F# の文字列リテラルにする。制御文字は字ではなく名前で書く。
/// 式に改行を含む弾幕がある（Original.time_twist の speed）。そのまま埋めると複数行になる。
let q (s: string) =
    let b = StringBuilder()
    b.Append('"') |> ignore

    for c in s do
        match c with
        | '\\' -> b.Append "\\\\" |> ignore
        | '"' -> b.Append "\\\"" |> ignore
        | '\r' -> b.Append "\\r" |> ignore
        | '\n' -> b.Append "\\n" |> ignore
        | '\t' -> b.Append "\\t" |> ignore
        | '\b' -> b.Append "\\b" |> ignore
        | c when c < ' ' || c = '\u007f' -> b.AppendFormat("\\u{0:x4}", int c) |> ignore
        | c -> b.Append c |> ignore

    b.Append('"') |> ignore
    b.ToString()

let ex (e: Expr.NumExpr) =
    sprintf "numExpr %s" (q (Expr.NumExpr.text e))

// ---- 属性・葉 ---------------------------------------------------------------

let duDirection (d: Direction) =
    match d with
    | Direction(None, e) -> sprintf "Direction (None, %s)" (ex e)
    | Direction(Some a, e) ->
        let t =
            match a.directionType with
            | DirectionType.Aim -> "Aim"
            | DirectionType.Absolute -> "Absolute"
            | DirectionType.Relative -> "Relative"
            | DirectionType.Sequence -> "Sequence"

        sprintf "Direction (Some {directionType = DirectionType.%s}, %s)" t (ex e)

let duSpeed (s: Speed) =
    match s with
    | Speed(None, e) -> sprintf "Speed (None, %s)" (ex e)
    | Speed(Some a, e) ->
        let t =
            match a.speedType with
            | SpeedType.Absolute -> "Absolute"
            | SpeedType.Relative -> "Relative"
            | SpeedType.Sequence -> "Sequence"

        sprintf "Speed (Some {speedType = SpeedType.%s}, %s)" t (ex e)

let duHorizontal (h: Horizontal) =
    match h with
    | Horizontal(None, e) -> sprintf "Horizontal (None, %s)" (ex e)
    | Horizontal(Some a, e) ->
        let t =
            match a.horizontalType with
            | HorizontalType.Absolute -> "Absolute"
            | HorizontalType.Relative -> "Relative"
            | HorizontalType.Sequence -> "Sequence"

        sprintf "Horizontal (Some {horizontalType = HorizontalType.%s}, %s)" t (ex e)

let duVertical (v: Vertical) =
    match v with
    | Vertical(None, e) -> sprintf "Vertical (None, %s)" (ex e)
    | Vertical(Some a, e) ->
        let t =
            match a.verticalType with
            | VerticalType.Absolute -> "Absolute"
            | VerticalType.Relative -> "Relative"
            | VerticalType.Sequence -> "Sequence"

        sprintf "Vertical (Some {verticalType = VerticalType.%s}, %s)" t (ex e)

let opt (f: 'a -> string) (o: 'a option) =
    match o with
    | None -> "None"
    | Some x -> sprintf "Some (%s)" (f x)

let duParams (ps: Params) =
    "[" + (ps |> List.map q |> String.concat "; ") + "]"

let actionLabel (a: ActionAttrs) =
    match a.actionLabel with
    | None -> "{actionLabel = None}"
    | Some(ActionLabel n) -> sprintf "{actionLabel = Some (ActionLabel %s)}" (q n)

let fireLabel (a: FireAttrs) =
    match a.fireLabel with
    | None -> "{fireLabel = None}"
    | Some(FireLabel n) -> sprintf "{fireLabel = Some (FireLabel %s)}" (q n)

let bulletLabel (a: BulletAttrs) =
    match a.bulletLabel with
    | None -> "{bulletLabel = None}"
    | Some(BulletLabel n) -> sprintf "{bulletLabel = Some (BulletLabel %s)}" (q n)

// ---- 木を行のリストで出す ---------------------------------------------------
// 深い入れ子を 1 行に畳むと行が数千文字になる（Original.fs）。
// リストは要素ごとに改行する。読みやすさより、行が伸びないこと。

let ind (lines: string list) = lines |> List.map (fun l -> "  " + l)

let duList (items: string list list) : string list =
    if List.isEmpty items then
        [ "[]" ]
    else
        [ "[" ] @ (items |> List.collect ind) @ [ "]" ]

let rec duCommand (c: Action) : string list =
    match c with
    | Action.Wait e -> [ sprintf "Action.Wait (%s)" (ex e) ]
    | Action.Vanish -> [ "Action.Vanish" ]
    | Action.ChangeDirection(d, Term t) -> [ sprintf "Action.ChangeDirection (%s, Term (%s))" (duDirection d) (ex t) ]
    | Action.ChangeSpeed(s, Term t) -> [ sprintf "Action.ChangeSpeed (%s, Term (%s))" (duSpeed s) (ex t) ]
    | Action.Accel(h, v, Term t) ->
        [
            sprintf "Action.Accel (%s, %s, Term (%s))" (opt duHorizontal h) (opt duVertical v) (ex t)
        ]
    | Action.Repeat(Times e, child) ->
        [ sprintf "Action.Repeat (Times (%s)," (ex e) ]
        @ ind (duActionElm child)
        @ [ ")" ]
    | Action.Fire(attrs, d, s, b) ->
        [
            sprintf "Action.Fire (%s, %s, %s," (fireLabel attrs) (opt duDirection d) (opt duSpeed s)
        ]
        @ ind (duBulletElm b)
        @ [ ")" ]
    | Action.FireRef(attrs, ps) ->
        let (FireLabel n) = attrs.fireRefLabel

        [
            sprintf "Action.FireRef ({fireRefLabel = FireLabel %s}, %s)" (q n) (duParams ps)
        ]
    | Action.Action(attrs, cmds) ->
        [ sprintf "Action.Action (%s," (actionLabel attrs) ]
        @ ind (duList (cmds |> List.map duCommand))
        @ [ ")" ]
    | Action.ActionRef(attrs, ps) ->
        let (ActionLabel n) = attrs.actionRefLabel

        [
            sprintf "Action.ActionRef ({actionRefLabel = ActionLabel %s}, %s)" (q n) (duParams ps)
        ]

and duActionElm (a: ActionElm) : string list =
    match a with
    | ActionElm.Action(attrs, cmds) ->
        [ sprintf "ActionElm.Action (%s," (actionLabel attrs) ]
        @ ind (duList (cmds |> List.map duCommand))
        @ [ ")" ]
    | ActionElm.ActionRef(attrs, ps) ->
        let (ActionLabel n) = attrs.actionRefLabel

        [
            sprintf "ActionElm.ActionRef ({actionRefLabel = ActionLabel %s}, %s)" (q n) (duParams ps)
        ]

and duBulletElm (b: BulletElm) : string list =
    match b with
    | BulletElm.BulletRef(attrs, ps) ->
        let (BulletLabel n) = attrs.bulletRefLabel

        [
            sprintf "BulletElm.BulletRef ({bulletRefLabel = BulletLabel %s}, %s)" (q n) (duParams ps)
        ]
    | BulletElm.Bullet(attrs, d, s, acts) ->
        [
            sprintf "BulletElm.Bullet (%s, %s, %s," (bulletLabel attrs) (opt duDirection d) (opt duSpeed s)
        ]
        @ ind (duList (acts |> List.map duActionElm))
        @ [ ")" ]

let duTopElm (e: BulletmlElm) : string list =
    match e with
    | BulletmlElm.Action(attrs, cmds) ->
        [ sprintf "BulletmlElm.Action (%s," (actionLabel attrs) ]
        @ ind (duList (cmds |> List.map duCommand))
        @ [ ")" ]
    | BulletmlElm.Bullet(attrs, d, s, acts) ->
        [
            sprintf "BulletmlElm.Bullet (%s, %s, %s," (bulletLabel attrs) (opt duDirection d) (opt duSpeed s)
        ]
        @ ind (duList (acts |> List.map duActionElm))
        @ [ ")" ]
    | BulletmlElm.Fire(attrs, d, s, b) ->
        [
            sprintf "BulletmlElm.Fire (%s, %s, %s," (fireLabel attrs) (opt duDirection d) (opt duSpeed s)
        ]
        @ ind (duBulletElm b)
        @ [ ")" ]

let duBulletml (bml: Bulletml) : string list =
    match bml with
    | Bulletml(attrs, elms) ->
        let optQ (o: string option) =
            match o with
            | Some s -> sprintf "Some %s" (q s)
            | None -> "None"

        let typ =
            match attrs.bulletmlType with
            | None -> "None"
            | Some ShootingDirection.BulletNone -> "Some ShootingDirection.BulletNone"
            | Some ShootingDirection.BulletVertical -> "Some ShootingDirection.BulletVertical"
            | Some ShootingDirection.BulletHorizontal -> "Some ShootingDirection.BulletHorizontal"

        [
            "Bulletml"
            sprintf
                "  ({bulletmlXmlns = %s; bulletmlType = %s; bulletmlName = %s; bulletmlDescription = %s},"
                (optQ attrs.bulletmlXmlns)
                typ
                (optQ attrs.bulletmlName)
                (optQ attrs.bulletmlDescription)
        ]
        @ ind (ind (duList (elms |> List.map duTopElm)))
        @ [ "  )" ]

// ---- CE のソースから namespace / module / 名前 / doc を拾う -----------------

type Entry = { Name: string; Docs: string list }

type Parsed =
    {
        File: string
        Namespace: string
        ModuleDocs: string list
        ModuleAttrs: string list
        ModuleName: string
        Entries: Entry list
    }

let parseFile (path: string) : Parsed =
    let lines = File.ReadAllLines path
    let mutable ns = ""
    let mutable modName = ""
    let mutable modDocs: string list = []
    let mutable modAttrs: string list = []
    let entries = ResizeArray<Entry>()
    let mutable pendingDocs: string list = []
    let mutable pendingAttrs: string list = []
    // module の中身 の字下げ。整形器 で変わる（Fantomas で 2 -> 4）ので、字 で決め打ち しない
    let mutable bodyIndent = -1
    let mutable anyLet = 0

    for raw in lines do
        let t = raw.Trim()
        let indent = raw.Length - raw.TrimStart().Length

        if modName <> "" && bodyIndent < 0 && t <> "" && indent > 0 then
            bodyIndent <- indent

        if t.StartsWith "let " then
            anyLet <- anyLet + 1

        if raw.StartsWith "namespace " then
            ns <- raw.Substring("namespace ".Length).Trim()
        elif t.StartsWith "///" then
            pendingDocs <- pendingDocs @ [ t ]
        elif t.StartsWith "[<" then
            pendingAttrs <- pendingAttrs @ [ t ]
        elif raw.StartsWith "module " then
            let after = raw.Substring("module ".Length)
            modName <- after.Split([| ' '; '=' |], StringSplitOptions.RemoveEmptyEntries).[0]
            modDocs <- pendingDocs
            modAttrs <- pendingAttrs
            pendingDocs <- []
            pendingAttrs <- []
        elif indent = bodyIndent && t.StartsWith "let " then
            let after = t.Substring("let ".Length)
            let name = after.Split([| ' '; '=' |], StringSplitOptions.RemoveEmptyEntries).[0]
            entries.Add { Name = name; Docs = pendingDocs }
            pendingDocs <- []
            pendingAttrs <- []
        elif t = "" || t.StartsWith "open " then
            ()
        else
            pendingDocs <- []
            pendingAttrs <- []

    // `let` が在るのに 1 つ も拾えなかったら、書き出す前 に落とす。黙って進むと空 の一覧 で上書き する
    if anyLet > 0 && entries.Count = 0 then
        failwithf "%s: let が %d 行 在るのに、module の直下 の定義 を 1 つ も拾えない（字下げ %d）" path anyLet bodyIndent

    {
        File = Path.GetFileName path
        Namespace = ns
        ModuleDocs = modDocs
        ModuleAttrs = modAttrs
        ModuleName = modName
        Entries = List.ofSeq entries
    }

// ---- 値を引く ---------------------------------------------------------------

let asm = Assembly.LoadFrom dslDll

let valueOf (ns: string) (modName: string) (name: string) =
    let t = asm.GetType(ns + "." + modName)

    if isNull t then
        failwithf "型が見つからない: %s.%s" ns modName

    let p = t.GetProperty(name, BindingFlags.Public ||| BindingFlags.Static)

    if isNull p then
        failwithf "値が見つからない: %s.%s.%s" ns modName name

    match p.GetValue(null) with
    | :? BulletmlInfo as info -> Choice1Of2 info.Bulletml
    | :? Bulletml as b -> Choice2Of2 b
    | other -> failwithf "扱えない型: %s (%s)" (other.GetType().FullName) name

// ---- 生成 -------------------------------------------------------------------

let duHeader =
    [
        "// このファイルは生成物。手で直すと次の焼き直しで消える。"
        "// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。"
    ]

let generateDu (p: Parsed) =
    let out = StringBuilder()
    let w (s: string) = out.Append(s).Append('\n') |> ignore

    for h in duHeader do
        w h

    w ""

    w (
        "namespace "
        + p.Namespace.Replace("FsBulletML2.Bullets.Dsl", "FsBulletML2.Bullets")
    )

    w "open FsBulletML2"
    w ""

    for d in p.ModuleDocs do
        w d

    for a in p.ModuleAttrs do
        w a

    w ("module " + p.ModuleName + " =")

    for e in p.Entries do
        w ""

        for d in e.Docs do
            w ("  " + d)

        w ("  let " + e.Name + " =")

        let lines, prefix =
            match valueOf p.Namespace p.ModuleName e.Name with
            | Choice1Of2 bml -> duBulletml bml, true
            | Choice2Of2 bml -> duBulletml bml, false

        if prefix then
            w "    createBulletmlInfo <|"

        for l in lines do
            w ("    " + l)

    out.ToString()

let generateAll (bundled: string list) (official: string list) =
    let out = StringBuilder()
    let w (s: string) = out.Append(s).Append('\n') |> ignore
    w "// 生成物。手で直すと次の焼き直しで消える。"
    w "//     dotnet build src/FsBulletML2.Bullets.Dsl -c Release"
    w "//     dotnet fsi src/FsBulletML2.Bullets.Dsl/gen.fsx"
    w ""
    w "namespace FsBulletML2.Bullets.Dsl"
    w "open FsBulletML2"
    w ""
    w "[<RequireQualifiedAccess>]"
    w "module All ="
    w ""

    let emit (xs: string list) =
        xs
        |> List.iteri (fun i n ->
            let head = if i = 0 then "    [ " else "      "
            w (head + n))

        w "    ]"

    w (sprintf "  /// 同梱の弾幕 %d 個。PlayerBullet の 3 本は Bulletml を直に持つので入らない。" bundled.Length)
    w "  /// 公式配布は混ぜない。出自が違うと、この数に紐づいた測定がどの集合か分からなくなる。"
    w "  let bullets : BulletmlInfo list ="
    emit bundled
    w ""
    w (sprintf "  /// BulletML 公式配布（bulletml0_21）のサンプル %d 個。同梱とは別の集合" official.Length)
    w "  ///"
    w "  /// v2.4.1 で足した。template.xml は入っていない（雛形なので）"
    w "  let official : BulletmlInfo list ="
    emit official
    out.ToString()

let files =
    Directory.GetFiles(dslDir, "*.fs")
    |> Array.filter (fun f -> Path.GetFileName f <> "All.fs")
    |> Array.sort

let mutable total = 0
let allInfos = ResizeArray<string>()
let officialInfos = ResizeArray<string>()

for f in files do
    let p = parseFile f
    total <- total + p.Entries.Length
    File.WriteAllText(Path.Combine(duDir, p.File), (generateDu p).Replace("\n", "\r\n"), UTF8Encoding(false))
    let shortNs = p.Namespace.Substring("FsBulletML2.Bullets.Dsl.".Length)
    // 公式配布のサンプルは別の一覧へ。混ぜると、同梱に紐づいた測定がどの集合か分からなくなる
    let sink =
        if p.ModuleName = "Official" then
            officialInfos
        else
            allInfos

    for e in p.Entries do
        match valueOf p.Namespace p.ModuleName e.Name with
        | Choice1Of2 _ -> sink.Add(shortNs + "." + p.ModuleName + "." + e.Name)
        | Choice2Of2 _ -> ()

File.WriteAllText(
    Path.Combine(dslDir, "All.fs"),
    (generateAll (List.ofSeq allInfos) (List.ofSeq officialInfos)).Replace("\n", "\r\n"),
    UTF8Encoding(false)
)

printfn "DU カタログ: %d ファイル / 値 %d 個" files.Length total
printfn "All.fs の bullets: %d 個 / official: %d 個" allInfos.Count officialInfos.Count
