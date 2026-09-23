namespace FsBulletML2.Parser.Tests

open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// ノードを字の上で光らせる範囲（v3.2 の直し）。
/// 光る幅が変わるだけなので、走行も試験も落ちない。目で見て広いと思うしかなかった。
[<TestFixture>]
type NodeSpansTests() =

  static let catalog = Bullets.Dsl.All.bullets

  static let writeAs (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> None
    | Some w -> match w.Write b with Result.Ok t -> Some t | Result.Error _ -> None

  static let kinds =
    [ "xml", SourceKind.Xml, (XmlScan.tags: string -> TagHit list)
      "sxml", SourceKind.Sxml, SxmlScan.tags
      "fsb", SourceKind.Fsb, FsbScan.tags ]

  static let names = List.ofArray NodeOrder.names

  /// 名前が `fire` の span だけ。`fire` は必ず属性 か `>` を持つので、
  /// 「開き札は名前より広い」が等号に落ちない
  static let firesOf (src: string) (tagsOf: string -> TagHit list) =
    let tags = tagsOf src
    let spans = Scan.nodeSpans src tags names
    let open_ = tags |> List.filter (fun t -> not t.Closing && List.contains t.TagName names)
    List.zip open_ spans |> List.filter (fun (t, _) -> t.TagName = "fire") |> List.map snd

  static let lineOf (src: string) (pos: int) =
    let mutable n = 0
    for k in 0 .. (min pos src.Length) - 1 do
      if src.[k] = '\n' then n <- n + 1
    n

  [<Test>]
  member _.``開き札は名前を包み、1 行 に収まる``() =
    let mutable ran = 0
    for (name, kind, tagsOf) in kinds do
      let mutable seen = 0
      for info in catalog do
        match writeAs kind info.Bulletml with
        | None -> ()
        | Some src ->
            for sp in Scan.nodeSpans src (tagsOf src) names do
              seen <- seen + 1
              Assert.That(sp.OpenStart, Is.LessThanOrEqualTo sp.NameStart, name + ": 開き札が名前より後ろで始まった")
              Assert.That(sp.NameStop, Is.LessThanOrEqualTo sp.OpenStop, name + ": 開き札が名前より手前で終わった")
              // 1 行 に収まる。 これが要素まるごとと分かれる境目 ——
              // `Stop` だけで切ると sxml は `)` まで、fsb は次の行 まで伸びる
              Assert.That(lineOf src sp.OpenStop, Is.EqualTo(lineOf src sp.OpenStart),
                          name + ": 開き札が行をまたいだ")
      // 0 件 を緑にしない
      seen |> should be (greaterThan 0)
      ran <- ran + 1
    ran |> should equal 3

  [<Test>]
  member _.``開き札は名前より広くなる。xml の fire は札まるごと``() =
    // 上の試験は `>=` なので、範囲を名前と同じに潰しても通る。広がったことは別に見る。
    // fsb は属性が無いと名前と同一。3 表記では広い件数が 0 でないことだけを見る。
    let mutable ran = 0
    for (name, kind, tagsOf) in kinds do
      let mutable wider = 0
      let mutable fires = 0
      for info in catalog do
        match writeAs kind info.Bulletml with
        | None -> ()
        | Some src ->
            for sp in firesOf src tagsOf do
              fires <- fires + 1
              if sp.OpenStop - sp.OpenStart > sp.NameStop - sp.NameStart then wider <- wider + 1
      fires |> should be (greaterThan 0)
      Assert.That(wider, Is.GreaterThan 0, name + ": 開き札が 1 件 も名前より広くならない")
      ran <- ran + 1
    ran |> should equal 3
    // xml は字で当てる。ここが緑の見た目そのもの
    let mutable shown = 0
    for info in catalog do
      match writeAs SourceKind.Xml info.Bulletml with
      | None -> ()
      | Some src ->
          for sp in firesOf src XmlScan.tags do
            shown <- shown + 1
            let text = src.Substring(sp.OpenStart, sp.OpenStop - sp.OpenStart)
            Assert.That(text, Does.StartWith "<fire", "xml の開き札が `<fire` で始まらない")
            Assert.That(text, Does.EndWith ">", "xml の開き札が `>` で終わらない")
    shown |> should be (greaterThan 0)

  [<Test>]
  member _.``閉じ札の対は xml だけで組める``() =
    // sxml は括弧 1 組、fsb は閉じ札を持たない。そこでは開き札と同じ範囲が入り、呼ぶ側が 1 枚に畳む。
    let paired (kind: SourceKind) (tagsOf: string -> TagHit list) =
      let mutable apart = 0
      let mutable same = 0
      for info in catalog do
        match writeAs kind info.Bulletml with
        | None -> ()
        | Some src ->
            for sp in firesOf src tagsOf do
              if sp.CloseStart = sp.OpenStart && sp.CloseStop = sp.OpenStop then same <- same + 1
              else apart <- apart + 1
      apart, same
    let xmlApart, xmlSame = paired SourceKind.Xml XmlScan.tags
    xmlApart |> should be (greaterThan 0)
    xmlSame |> should equal 0
    for (name, kind, tagsOf) in kinds |> List.filter (fun (n, _, _) -> n <> "xml") do
      let apart, same = paired kind tagsOf
      same |> should be (greaterThan 0)
      Assert.That(apart, Is.EqualTo 0, name + " に閉じ札の対が出た")

  [<Test>]
  member _.``閉じ札は、その要素の閉じ札を指す``() =
    // 入れ子の同名を跨がない。手前の `</fire>` を掴んでも名前は同じなので、名前だけでは赤くならない。
    let mutable nested = 0
    let mutable checked_ = 0
    for info in catalog do
      match writeAs SourceKind.Xml info.Bulletml with
      | None -> ()
      | Some src ->
          for sp in firesOf src XmlScan.tags do
            checked_ <- checked_ + 1
            let text = src.Substring(sp.CloseStart, sp.CloseStop - sp.CloseStart)
            Assert.That(text, Is.EqualTo "</fire>", "閉じ札でない字を指した")
            // 開き札より後ろに在り、中身を挟んでいる
            Assert.That(sp.CloseStart, Is.GreaterThanOrEqualTo sp.OpenStop, "閉じ札が開き札に重なった")
            // 入れ子の `fire` を跨いだ件数を数える。 0 なら、この試験は
            // 「近い側で止まる」壊れ方を 1 度 も見ていない
            let inner = src.Substring(sp.OpenStop, sp.CloseStart - sp.OpenStop)
            if inner.Contains "<fire" then nested <- nested + 1
    checked_ |> should be (greaterThan 0)
    nested |> should be (greaterThan 0)

  [<Test>]
  member _.``1 行 に詰めて書いても、開き札で止まる``() =
    // 行末で切るだけでは足りない。整形した字は札ごとに改行がある。同梱には 1 件も無い形なので、ここで作る。
    let src = "<bulletml><action label=\"top\"><fire><direction>0</direction><speed>2</speed><bullet/></fire><wait>1</wait></action></bulletml>"
    let spans = Scan.nodeSpans src (XmlScan.tags src) names
    let tags = XmlScan.tags src |> List.filter (fun t -> not t.Closing && List.contains t.TagName names)
    let byName = List.zip (tags |> List.map (fun t -> t.TagName)) spans
    let fire = byName |> List.find (fun (n, _) -> n = "fire") |> snd
    // 開き札は `<fire>` まで。行末まで伸びると本文まるごとになる
    src.Substring(fire.OpenStart, fire.OpenStop - fire.OpenStart) |> should equal "<fire>"
    src.Substring(fire.CloseStart, fire.CloseStop - fire.CloseStart) |> should equal "</fire>"
    // 自己閉じの札は対を持たない（`<bullet/>`）—— 開き札と同じ範囲
    let bullet = byName |> List.find (fun (n, _) -> n = "bullet") |> snd
    src.Substring(bullet.OpenStart, bullet.OpenStop - bullet.OpenStart) |> should equal "<bullet/>"
    bullet.CloseStart |> should equal bullet.OpenStart
    bullet.CloseStop |> should equal bullet.OpenStop

  [<Test>]
  member _.``閉じ忘れが在っても、名前が合う札に対を組む``() =
    // 同梱カタログではこの枝に当たらない。名前を見なくても答えが合う。
    // 守りが要るのは打っている途中。閉じていない札が普通に在る。
    let src = "<bulletml><action label=\"top\"><fire><bullet></fire><wait>1</wait></action></bulletml>"
    let spans = Scan.nodeSpans src (XmlScan.tags src) names
    let tags = XmlScan.tags src |> List.filter (fun t -> not t.Closing && List.contains t.TagName names)
    let byName = List.zip (tags |> List.map (fun t -> t.TagName)) spans
    let fire = byName |> List.find (fun (n, _) -> n = "fire") |> snd
    // `</fire>` は `<bullet>` でなく `<fire>` の対。
    // 名前を見ないと、スタックの最後（`bullet`）に付いてしまう
    src.Substring(fire.CloseStart, fire.CloseStop - fire.CloseStart) |> should equal "</fire>"
    // 閉じていない札は対を持たない —— 開き札と同じ範囲
    let bullet = byName |> List.find (fun (n, _) -> n = "bullet") |> snd
    bullet.CloseStart |> should equal bullet.OpenStart
    bullet.CloseStop |> should equal bullet.OpenStop
    // 閉じ忘れの手前へ戻したあとも、外側の対は組める
    let action = byName |> List.find (fun (n, _) -> n = "action") |> snd
    src.Substring(action.CloseStart, action.CloseStop - action.CloseStart) |> should equal "</action>"
