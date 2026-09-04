// FsBulletML2.Bullets の弾幕を、FsBulletML2.Dsl の CE で書いた形へ写す。
//
// **値から起こす。** 元の .fs を構文解析するのではなく、焼いたアセンブリから
// Bulletml の値を取り出して CE の構文で印字する。木の形が唯一の入力なので、
// 元の書き方（改行やインデント）に左右されない。
//
// 元の .fs からは **namespace / module / 値の名前 / doc コメント**だけ拾う。
// これらは値に残っていないため。

#r @"..\..\src\FsBulletML2.Dsl\bin\Release\net10.0\FsBulletML2.Core.dll"
#r @"..\FsBulletML2.Bullets\bin\Release\net10.0\FsBulletML2.Bullets.dll"

open System
open System.IO
open System.Reflection
open System.Text
open FsBulletML2

let repo = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))
let srcDir = Path.Combine(repo, @"samples\FsBulletML2.Bullets")
let outDir = Path.Combine(repo, @"samples\FsBulletML2.Bullets.Dsl")
let bulletsDll = Path.Combine(repo, @"samples\FsBulletML2.Bullets\bin\Release\net10.0\FsBulletML2.Bullets.dll")

let gaps = System.Collections.Generic.Dictionary<string, int>()
let gap (what: string) =
  match gaps.TryGetValue what with
  | true, n -> gaps.[what] <- n + 1
  | _ -> gaps.[what] <- 1
  "(* GAP: " + what + " *)"

/// F# の文字列リテラルにする。
///
/// **制御文字は字ではなく名前で書く。** 式の中に改行を含む弾幕があり
/// （Original.time_twist の speed）、そのまま埋めるとリテラルが複数行 になる。
/// 生成ファイルを CRLF で書いているので、元の "\n" が "\r\n" に化けて
/// **1 個 だけ木が違う**という形で出た。
let q (s: string) =
  let b = System.Text.StringBuilder()
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

let ex (e: Expr.NumExpr) = q (Expr.NumExpr.text e)
let pad (n: int) = String(' ', n)

// ---- direction / speed / horizontal / vertical -----------------------------

let dirLine (d: Direction) =
  match d with
  | Direction (None, e) -> sprintf "dir %s" (ex e)
  | Direction (Some a, e) ->
    match a.directionType with
    | DirectionType.Aim -> sprintf "aim %s" (ex e)
    | DirectionType.Absolute -> sprintf "absolute %s" (ex e)
    | DirectionType.Relative -> sprintf "relative %s" (ex e)
    | DirectionType.Sequence -> sprintf "sequence %s" (ex e)

let speedLine (s: Speed) =
  match s with
  | Speed (None, e) -> sprintf "speed %s" (ex e)
  | Speed (Some a, e) ->
    match a.speedType with
    | SpeedType.Absolute -> sprintf "speedAbs %s" (ex e)
    | SpeedType.Relative -> sprintf "speedRel %s" (ex e)
    | SpeedType.Sequence -> sprintf "speedSeq %s" (ex e)

let horizontalLine (h: Horizontal) =
  match h with
  | Horizontal (None, e) -> sprintf "horizontal %s" (ex e)
  | Horizontal (Some a, e) ->
    match a.horizontalType with
    | HorizontalType.Absolute -> sprintf "horizontalAbs %s" (ex e)
    | HorizontalType.Relative -> sprintf "horizontalRel %s" (ex e)
    | HorizontalType.Sequence -> sprintf "horizontalSeq %s" (ex e)

let verticalLine (v: Vertical) =
  match v with
  | Vertical (None, e) -> sprintf "vertical %s" (ex e)
  | Vertical (Some a, e) ->
    match a.verticalType with
    | VerticalType.Absolute -> sprintf "verticalAbs %s" (ex e)
    | VerticalType.Relative -> sprintf "verticalRel %s" (ex e)
    | VerticalType.Sequence -> sprintf "verticalSeq %s" (ex e)

let changeDirLine (d: Direction) (t: Expr.NumExpr) =
  match d with
  | Direction (None, e) -> sprintf "changeDirection %s %s" (ex e) (ex t)
  | Direction (Some a, e) ->
    let name =
      match a.directionType with
      | DirectionType.Aim -> "changeDirectionAim"
      | DirectionType.Absolute -> "changeDirectionAbs"
      | DirectionType.Relative -> "changeDirectionRel"
      | DirectionType.Sequence -> "changeDirectionSeq"
    sprintf "%s %s %s" name (ex e) (ex t)

let changeSpdLine (s: Speed) (t: Expr.NumExpr) =
  match s with
  | Speed (None, e) -> sprintf "changeSpeed %s %s" (ex e) (ex t)
  | Speed (Some a, e) ->
    let name =
      match a.speedType with
      | SpeedType.Absolute -> "changeSpeedAbs"
      | SpeedType.Relative -> "changeSpeedRel"
      | SpeedType.Sequence -> "changeSpeedSeq"
    sprintf "%s %s %s" name (ex e) (ex t)

let paramList (ps: Params) =
  "[" + (ps |> List.map q |> String.concat "; ") + "]"

// ---- 印字本体 ---------------------------------------------------------------

let mutable sb = StringBuilder()
let line (indent: int) (s: string) = sb.Append(pad indent).Append(s).Append('\n') |> ignore

let rec printCommand (ind: int) (c: Action) =
  match c with
  | Action.Wait e -> line ind (sprintf "wait %s" (ex e))
  | Action.Vanish -> line ind "vanish"
  | Action.ChangeDirection (d, Term t) -> line ind (changeDirLine d t)
  | Action.ChangeSpeed (s, Term t) -> line ind (changeSpdLine s t)

  | Action.Accel (h, v, Term t) ->
    line ind (sprintf "accel %s {" (ex t))
    match h, v with
    | None, None -> line (ind + 4) "()"
    | _ ->
      h |> Option.iter (fun x -> line (ind + 4) (horizontalLine x))
      v |> Option.iter (fun x -> line (ind + 4) (verticalLine x))
    line ind "}"

  | Action.Repeat (Times e, ActionElm.Action (a, cmds)) ->
    let head =
      match a.actionLabel with
      | None -> sprintf "repeat %s {" (ex e)
      | Some (ActionLabel n) -> sprintf "repeatAs %s %s {" (ex e) (q n)
    line ind head
    if List.isEmpty cmds then line (ind + 4) "()"
    for c in cmds do printCommand (ind + 4) c
    line ind "}"
  | Action.Repeat (Times e, ActionElm.ActionRef (attrs, ps)) ->
    let (ActionLabel n) = attrs.actionRefLabel
    line ind (sprintf "repeatRef %s %s %s" (ex e) (q n) (paramList ps))

  | Action.Fire (attrs, d, s, b) ->
    let head =
      match attrs.fireLabel with
      | None -> "fire {"
      | Some (FireLabel n) -> sprintf "fireAs %s {" (q n)
    line ind head
    d |> Option.iter (fun x -> line (ind + 4) (dirLine x))
    s |> Option.iter (fun x -> line (ind + 4) (speedLine x))
    printBulletElm (ind + 4) b
    line ind "}"

  | Action.FireRef (attrs, ps) ->
    let (FireLabel n) = attrs.fireRefLabel
    line ind (sprintf "fireRef %s %s" (q n) (paramList ps))

  | Action.Action (a, cmds) ->
    let head =
      match a.actionLabel with
      | None -> "nest {"
      | Some (ActionLabel n) -> sprintf "nestAs %s {" (q n)
    line ind head
    if List.isEmpty cmds then line (ind + 4) "()"
    for c in cmds do printCommand (ind + 4) c
    line ind "}"

  | Action.ActionRef (attrs, ps) ->
    let (ActionLabel n) = attrs.actionRefLabel
    line ind (sprintf "actionRef %s %s" (q n) (paramList ps))

and printBulletElm (ind: int) (b: BulletElm) =
  match b with
  | BulletElm.BulletRef (attrs, ps) ->
    let (BulletLabel n) = attrs.bulletRefLabel
    line ind (sprintf "refBullet %s %s" (q n) (paramList ps))
  | BulletElm.Bullet (attrs, None, None, []) when attrs.bulletLabel.IsNone ->
    line ind "plain"
  | BulletElm.Bullet (attrs, d, s, acts) ->
    let head =
      match attrs.bulletLabel with
      | None -> "ofBullet (bulletAnon {"
      | Some (BulletLabel n) -> sprintf "ofBullet (bullet %s {" (q n)
    line ind head
    printBulletBody (ind + 4) d s acts
    line ind "})"

and printBulletBody (ind: int) (d: Direction option) (s: Speed option) (acts: ActionElm list) =
  if d.IsNone && s.IsNone && List.isEmpty acts then
    line ind "()"
  else
    d |> Option.iter (fun x -> line ind (dirLine x))
    s |> Option.iter (fun x -> line ind (speedLine x))
    for a in acts do
      match a with
      | ActionElm.ActionRef (attrs, ps) ->
        let (ActionLabel n) = attrs.actionRefLabel
        line ind (sprintf "refActs %s %s" (q n) (paramList ps))
      | ActionElm.Action (attrs, cmds) ->
        let head =
          match attrs.actionLabel with
          | None -> "doActs (body {"
          | Some (ActionLabel n) -> sprintf "doActs (bodyAs %s {" (q n)
        line ind head
        if List.isEmpty cmds then line (ind + 4) "()"
        for c in cmds do printCommand (ind + 4) c
        line ind "})"

let printTopElm (ind: int) (e: BulletmlElm) =
  match e with
  | BulletmlElm.Action (a, cmds) ->
    let head =
      match a.actionLabel with
      | Some (ActionLabel "top") -> "top {"
      | Some (ActionLabel n) -> sprintf "defAction %s {" (q n)
      | None -> "defActionAnon {"
    line ind head
    if List.isEmpty cmds then line (ind + 4) "()"
    for c in cmds do printCommand (ind + 4) c
    line ind "}"
  | BulletmlElm.Bullet (attrs, d, s, acts) ->
    let head =
      match attrs.bulletLabel with
      | None -> "defBulletAnon {"
      | Some (BulletLabel n) -> sprintf "defBullet %s {" (q n)
    line ind head
    printBulletBody (ind + 4) d s acts
    line ind "}"
  | BulletmlElm.Fire (attrs, d, s, b) ->
    let head =
      match attrs.fireLabel with
      | None -> "topFire {"
      | Some (FireLabel n) -> sprintf "topFireAs %s {" (q n)
    line ind head
    d |> Option.iter (fun x -> line (ind + 4) (dirLine x))
    s |> Option.iter (fun x -> line (ind + 4) (speedLine x))
    printBulletElm (ind + 4) b
    line ind "}"

let bulletmlHead (attrs: BulletmlAttrs) =
  let optStr (o: string option) = match o with Some s -> sprintf "(Some %s)" (q s) | None -> "None"
  match attrs.bulletmlType, attrs.bulletmlName, attrs.bulletmlXmlns, attrs.bulletmlDescription with
  | Some ShootingDirection.BulletVertical, Some n, None, None -> sprintf "vertical %s {" (q n)
  | Some ShootingDirection.BulletHorizontal, Some n, None, None -> sprintf "horizontal %s {" (q n)
  | Some ShootingDirection.BulletNone, Some n, None, None -> sprintf "none %s {" (q n)
  | None, Some n, None, None -> sprintf "untyped %s {" (q n)
  | Some ShootingDirection.BulletVertical, Some n, Some x, None -> sprintf "verticalXmlns %s %s {" (q x) (q n)
  | Some ShootingDirection.BulletHorizontal, Some n, Some x, None -> sprintf "horizontalXmlns %s %s {" (q x) (q n)
  | Some ShootingDirection.BulletNone, Some n, Some x, None -> sprintf "noneXmlns %s %s {" (q x) (q n)
  | None, Some n, Some x, None -> sprintf "untypedXmlns %s %s {" (q x) (q n)
  | t, n, x, d ->
    let typ =
      match t with
      | None -> "None"
      | Some ShootingDirection.BulletVertical -> "(Some ShootingDirection.BulletVertical)"
      | Some ShootingDirection.BulletHorizontal -> "(Some ShootingDirection.BulletHorizontal)"
      | Some ShootingDirection.BulletNone -> "(Some ShootingDirection.BulletNone)"
    sprintf "bulletmlOf %s %s %s %s {" typ (optStr n) (optStr x) (optStr d)

let printBulletml (ind: int) (bml: Bulletml) =
  match bml with
  | Bulletml (attrs, elms) ->
    line ind (bulletmlHead attrs)
    if List.isEmpty elms then line (ind + 4) "()"
    for e in elms do printTopElm (ind + 4) e
    line ind "}"

let renderBulletml (ind: int) (bml: Bulletml) =
  sb <- StringBuilder()
  printBulletml ind bml
  sb.ToString().TrimEnd('\n')

// ---- 元ファイルから namespace / module / 名前 / doc コメントを拾う ----------

type Entry = { Name: string; Docs: string list }
type Parsed =
  { File: string
    Namespace: string
    ModuleDocs: string list
    ModuleAttrs: string list
    ModuleName: string
    Entries: Entry list }

let parseFile (path: string) : Parsed =
  let lines = File.ReadAllLines path
  let mutable ns = ""
  let mutable modName = ""
  let mutable modDocs : string list = []
  let mutable modAttrs : string list = []
  let entries = ResizeArray<Entry>()
  let mutable pendingDocs : string list = []
  let mutable pendingAttrs : string list = []
  for raw in lines do
    let t = raw.Trim()
    if raw.StartsWith "namespace " then
      ns <- raw.Substring("namespace ".Length).Trim()
    elif t.StartsWith "///" then
      pendingDocs <- pendingDocs @ [t]
    elif t.StartsWith "[<" then
      pendingAttrs <- pendingAttrs @ [t]
    elif raw.StartsWith "module " then
      let after = raw.Substring("module ".Length)
      modName <- after.Split([| ' '; '=' |], StringSplitOptions.RemoveEmptyEntries).[0]
      modDocs <- pendingDocs
      modAttrs <- pendingAttrs
      pendingDocs <- []
      pendingAttrs <- []
    elif raw.StartsWith "  let " then
      let after = raw.Substring("  let ".Length)
      let name = after.Split([| ' '; '=' |], StringSplitOptions.RemoveEmptyEntries).[0]
      entries.Add { Name = name; Docs = pendingDocs }
      pendingDocs <- []
      pendingAttrs <- []
    elif t = "" || t.StartsWith "open " then
      ()
    else
      pendingDocs <- []
      pendingAttrs <- []
  { File = Path.GetFileName path
    Namespace = ns
    ModuleDocs = modDocs
    ModuleAttrs = modAttrs
    ModuleName = modName
    Entries = List.ofSeq entries }

// ---- 値を引く ---------------------------------------------------------------

let asm = Assembly.LoadFrom bulletsDll

/// 型（module）の中の静的プロパティから値を引く。
/// BulletmlInfo を返すものと Bulletml を返すものの 2 通り ある
let valueOf (ns: string) (modName: string) (name: string) =
  let t = asm.GetType(ns + "." + modName)
  if isNull t then failwithf "型が見つからない: %s.%s" ns modName
  let p = t.GetProperty(name, BindingFlags.Public ||| BindingFlags.Static)
  if isNull p then failwithf "値が見つからない: %s.%s.%s" ns modName name
  match p.GetValue(null) with
  | :? BulletmlInfo as info -> Choice1Of2 info.Bulletml
  | :? Bulletml as b -> Choice2Of2 b
  | other -> failwithf "扱えない型: %s (%s)" (other.GetType().FullName) name

// ---- 生成 -------------------------------------------------------------------

let header =
  [ "// **このファイルは生成物。手で直すと次の焼き直しで消える。**"
    "//"
    "// samples/FsBulletML2.Bullets の同名ファイルから、焼いたアセンブリの値を"
    "// 読んで CE の構文へ写している。元の .fs から拾うのは namespace / module /"
    "// 値の名前 / doc コメントだけ。"
    "//"
    "// 焼き直し:"
    "//     dotnet build samples/FsBulletML2.Bullets -c Release"
    "//     dotnet fsi samples/FsBulletML2.Bullets.Dsl/gen.fsx"
    "//"
    "// **焼き直したら必ず tests/FsBulletML2.Dsl.Tests を回すこと。**"
    "// 元と同じ木になることは、あそこが 179 個 を 1 個 ずつ突き合わせて言う。" ]

let generate (p: Parsed) =
  let out = StringBuilder()
  let w (s: string) = out.Append(s).Append('\n') |> ignore
  for h in header do w h
  w ""
  w ("namespace " + p.Namespace.Replace("FsBulletML2.Bullets", "FsBulletML2.Bullets.Dsl"))
  w "open FsBulletML2"
  w "open FsBulletML2.Dsl"
  w ""
  for d in p.ModuleDocs do w d
  for a in p.ModuleAttrs do w a
  w ("module " + p.ModuleName + " =")
  for e in p.Entries do
    w ""
    for d in e.Docs do w ("  " + d)
    w ("  let " + e.Name + " =")
    match valueOf p.Namespace p.ModuleName e.Name with
    | Choice1Of2 bml ->
      w "    createBulletmlInfo <|"
      w (renderBulletml 4 bml)
    | Choice2Of2 bml ->
      w (renderBulletml 4 bml)
  out.ToString()

let files =
  Directory.GetFiles(srcDir, "*.fs")
  |> Array.filter (fun f -> Path.GetFileName f <> "AssemblyInfo.fs")
  |> Array.sort

Directory.CreateDirectory outDir |> ignore

/// 全弾幕の一覧（All.fs）。**BulletmlInfo を持つものだけ載せる。**
///
/// PlayerBullet の 3 本 は Bulletml を直に持っていて型が違うので入らない。
/// あちらは自機弾として名前で使うもので、並べても意味がない。
let generateAll (names: string list) =
  let out = StringBuilder()
  let w (s: string) = out.Append(s).Append('\n') |> ignore
  w "// **このファイルは生成物。手で直すと次の焼き直しで消える。**"
  w "//"
  w "// 焼き直し:"
  w "//     dotnet build samples/FsBulletML2.Bullets -c Release"
  w "//     dotnet fsi samples/FsBulletML2.Bullets.Dsl/gen.fsx"
  w ""
  w "namespace FsBulletML2.Bullets.Dsl"
  w "open FsBulletML2"
  w ""
  w "[<RequireQualifiedAccess>]"
  w "module All ="
  w ""
  w (sprintf "  /// 同梱の弾幕 %d 個。**PlayerBullet の 3 本 は Bulletml を直に持つので入らない**" names.Length)
  w "  let bullets : BulletmlInfo list ="
  names
  |> List.iteri (fun i n ->
      let head = if i = 0 then "    [ " else "      "
      w (head + n))
  w "    ]"
  out.ToString()

let mutable total = 0
let allInfos = ResizeArray<string>()
for f in files do
  let p = parseFile f
  total <- total + p.Entries.Length
  let text = generate p
  let dest = Path.Combine(outDir, p.File)
  File.WriteAllText(dest, text.Replace("\n", "\r\n"), UTF8Encoding(false))
  // 一覧に載せるのは BulletmlInfo を持つものだけ
  let shortNs = p.Namespace.Substring("FsBulletML2.Bullets.".Length)
  for e in p.Entries do
    match valueOf p.Namespace p.ModuleName e.Name with
    | Choice1Of2 _ -> allInfos.Add(shortNs + "." + p.ModuleName + "." + e.Name)
    | Choice2Of2 _ -> ()

File.WriteAllText(
  Path.Combine(outDir, "All.fs"),
  (generateAll (List.ofSeq allInfos)).Replace("\n", "\r\n"),
  UTF8Encoding(false))

printfn "生成: %d ファイル / 値 %d 個（All.fs に載せた BulletmlInfo は %d 個）"
  (files.Length + 1) total allInfos.Count
printfn ""
printfn "=== 表現できなかった形 ==="
if gaps.Count = 0 then printfn "  0 件"
else gaps |> Seq.sortByDescending (fun kv -> kv.Value) |> Seq.iter (fun kv -> printfn "  %-44s %5d 件" kv.Key kv.Value)
