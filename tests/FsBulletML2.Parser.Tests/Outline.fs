namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 本文の構造（v2.4）。アウトラインと折りたたみが読む木。
///
/// 深さそのものは `Depth.fs` が見ている。**ここは範囲**（`Line` .. `EndLine`）。
///
/// --- 版の頭で踏んだこと
///
/// 最初の規則は「次に同じか浅い開き札が来るまで。尽きたら本文の終わりまで」で、
/// **最後の子が親の閉じ札を飲んだ** —— `bullet` を畳むと根の閉じ札が
/// 一緒に隠れる。
///
/// **並びは正しく、2 runtime も一致していた**ので、突き合わせの門
/// （`guard-fable-parity`）は緑のまま。**畳んで初めて見えた。**
/// ここで当てるのは「両側で同じか」ではなく「合っているか」。
///
/// --- 較正（当てた変異と、赤くなった点の数）
///
///   浅い閉じ札で止めない            4 点（最後の子が親の閉じ札を飲む）
///   閉じ札を中身から外す            3 点（根が本文の終わりまで届かない）
///   中身の最後を 1 つ 行き過ぎる     6 点（兄弟に食い込む）
///
/// **どれも `guard-fable-parity` は緑のまま**（焼き直して確かめた。143 件 /
/// 食い違い 0 件）—— 2 runtime は同じ 1 本 から焼かれるので、両側が同じに壊れる
[<TestFixture>]
type Outline() =

  static let catalog = Bullets.Dsl.All.bullets

  static let writeAs (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> None
    | Some w -> match w.Write b with Result.Ok t -> Some t | Result.Error _ -> None

  static let nodesOf (tags: string -> TagHit list) (src: string) =
    Outline.build "label" (Scan.lineColumn src) (tags src)

  static let xmlNodes (src: string) = nodesOf XmlScan.tags src

  /// 13 行。**根の閉じ札が、最後の子より後ろの行に在る**形
  static let sample =
    String.concat "\n"
      [ "<?xml version=\"1.0\" ?>"
        "<bulletml>"
        "  <action label=\"top\">"
        "    <fire>"
        "      <speed>2</speed>"
        "      <bullet/>"
        "    </fire>"
        "    <wait>10</wait>"
        "  </action>"
        "  <bullet label=\"b1\">"
        "    <speed>1</speed>"
        "  </bullet>"
        "</bulletml>" ]

  // --- 範囲 --------------------------------------------------------------------

  [<Test>]
  member _.``最後の子は親の閉じ札を飲まない``() =
    // **畳んで見えた形。** `bullet b1` は 12 行 目 で終わり、
    // 13 行 目 の閉じ札は根のもの
    let b1 = xmlNodes sample |> List.find (fun x -> x.Detail = "b1")
    (b1.Line, b1.EndLine) |> should equal (10, 12)

  [<Test>]
  member _.``根は本文の終わりまで``() =
    let root = xmlNodes sample |> List.find (fun x -> x.Depth = 0)
    (root.Name, root.Line, root.EndLine) |> should equal ("bulletml", 2, 13)

  [<Test>]
  member _.``兄弟は重ならない``() =
    let top = xmlNodes sample |> List.find (fun x -> x.Detail = "top")
    let b1 = xmlNodes sample |> List.find (fun x -> x.Detail = "b1")
    top.EndLine |> should lessThan b1.Line

  [<Test>]
  member _.``1 行 の要素は畳む先を持たない``() =
    // `<speed>2</speed>` は 1 行。**折りたたみはここを出さない**
    let speeds = xmlNodes sample |> List.filter (fun x -> x.Name = "speed")
    speeds |> List.length |> should equal 2
    speeds |> List.filter (fun x -> x.EndLine <> x.Line) |> should be Empty

  [<Test>]
  member _.``自己閉じも 1 行``() =
    let b = xmlNodes sample |> List.find (fun x -> x.Name = "bullet" && x.Detail = "")
    (b.Line, b.EndLine) |> should equal (6, 6)

  // --- 同梱 176 本 -------------------------------------------------------------

  [<Test>]
  member _.``終わりは自分の閉じ札の行``() =
    // **XML は答えを別の道で数えられる** —— 開き札と閉じ札を突き合わせれば
    // 範囲は一意に決まる。木の組み方とは別の数え方なので、当てる価値がある。
    // （閉じ札を持たない 3 表記 には、この当て方が無い）
    let broken =
      catalog
      |> List.choose (fun info ->
           match writeAs SourceKind.Xml info.Bulletml with
           | None -> None
           | Some src ->
             let ts = XmlScan.tags src
             let lineOf (t: TagHit) =
               let struct (l, _) = Scan.lineColumn src t.NameStart
               l
             // 開き札を積み、閉じ札で降ろす。**自己閉じは自分の行**
             let closedAt = System.Collections.Generic.Dictionary<int, int>()
             let stack = System.Collections.Generic.Stack<TagHit>()
             for t in ts do
               if t.Closing then
                 if stack.Count > 0 then closedAt.[stack.Pop().NameStart] <- lineOf t
               elif t.SelfClosing then closedAt.[t.NameStart] <- lineOf t
               else stack.Push t
             // 閉じ札が来ないまま終わった札は、本文の最後まで
             let opens = ts |> List.filter (fun t -> not t.Closing)
             let last = opens |> List.map lineOf |> List.max
             let wrong =
               List.zip opens (xmlNodes src)
               |> List.filter (fun (t, n) ->
                    let e = if closedAt.ContainsKey t.NameStart then closedAt.[t.NameStart] else last
                    e <> n.EndLine)
             if List.isEmpty wrong then None else Some(info.Name, List.length wrong))
    broken |> should be Empty

  [<Test>]
  member _.``3 表記 とも、次の兄弟に食い込まない``() =
    // **閉じ札を持たない 2 表記 に当たる唯一 の点。** 上の点は XML にしか
    // 当てられない（閉じ札が要る）ので、こちらが sxml と fsb の受け持ち。
    //
    // 「行を跨いで並んでいる兄弟」だけを見る —— 1 行 に並んでいる形は
    // 終わりも始まりも同じ行 になるので、そこは重なりではない
    let kinds =
      [ SourceKind.Xml, (XmlScan.tags: string -> TagHit list)
        SourceKind.Sxml, SxmlScan.tags
        SourceKind.Fsb, FsbScan.tags ]
    let broken =
      catalog
      |> List.collect (fun info ->
           kinds
           |> List.choose (fun (kind, tags) ->
                match writeAs kind info.Bulletml with
                | None -> None
                | Some src ->
                  let ns = nodesOf tags src |> List.toArray
                  let bad =
                    [ for i in 0 .. ns.Length - 2 do
                        let a = ns.[i]
                        let b = ns.[i + 1]
                        if b.Depth <= a.Depth && b.Line > a.Line && a.EndLine >= b.Line then
                          yield a.Name ]
                  if List.isEmpty bad then None else Some(info.Name, kind, List.length bad)))
    broken |> should be Empty

  [<Test>]
  member _.``終わりは始まりより前に行かない``() =
    let broken =
      catalog
      |> List.choose (fun info ->
           match writeAs SourceKind.Xml info.Bulletml with
           | None -> None
           | Some src ->
             let bad = xmlNodes src |> List.filter (fun n -> n.EndLine < n.Line)
             if List.isEmpty bad then None else Some info.Name)
    broken |> should be Empty

  [<Test>]
  member _.``畳める要素が在る``() =
    // 上の 3 点 は、**全部 が 1 行 でも緑**になる
    let src = writeAs SourceKind.Xml (List.head catalog).Bulletml |> Option.get
    xmlNodes src |> List.filter (fun n -> n.EndLine > n.Line) |> List.length
    |> should greaterThan 0

  // --- 添え字と端 --------------------------------------------------------------

  [<Test>]
  member _.``label は添え字になる``() =
    xmlNodes sample
    |> List.filter (fun n -> n.Detail <> "")
    |> List.map (fun n -> n.Name, n.Detail)
    |> should equal [ "action", "top"; "bullet", "b1" ]

  [<Test>]
  member _.``引く属性が無ければ添えない``() =
    // 語彙が引けていないとき。**属性名を決め打ちで探さない**
    Outline.build "" (Scan.lineColumn sample) (XmlScan.tags sample)
    |> List.filter (fun n -> n.Detail <> "")
    |> should be Empty

  [<Test>]
  member _.``閉じ札は節にならない``() =
    let opens = XmlScan.tags sample |> List.filter (fun t -> not t.Closing) |> List.length
    xmlNodes sample |> List.length |> should equal opens

  [<Test>]
  member _.``空の本文``() =
    xmlNodes "" |> should be Empty

  [<Test>]
  member _.``閉じ札だけでも落ちない``() =
    // 打っている途中の形
    xmlNodes "</a></b>" |> should be Empty

  [<Test>]
  member _.``名前の桁は名前をそのまま指す``() =
    let root = xmlNodes sample |> List.find (fun n -> n.Depth = 0)
    (root.Column, root.EndColumn) |> should equal (2, 2 + "bulletml".Length)

  [<Test>]
  member _.``describe は並びを字にする``() =
    // `guard-fable-parity` が両側で突き合わせる口。**形をここで固定する**
    Outline.describe "<a><b/></a>" |> should equal "a@1-1, b@1-1"
