namespace FsBulletML2.Core.Tests

open NUnit.Framework

/// `<bulletml type="none|vertical|horizontal">` が走らせる側に効くか。
/// 読む側が居ないので、軌跡の控えはどの type でも緑になり何も担保しない。
[<TestFixture>]
type ShootingType() =

  let bmlOfType t =
    sprintf """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="%s" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
<action label="top">
  <fire>
    <direction type="absolute">30</direction>
    <speed>2</speed>
    <bullet/>
  </fire>
  <wait>3</wait>
</action>
</bulletml>""" t

  [<Test>]
  member _.``type を none vertical horizontal に振っても軌跡が変わらない``() =
    let none = TraceRun.std (bmlOfType "none") 6
    let vert = TraceRun.std (bmlOfType "vertical") 6
    let horz = TraceRun.std (bmlOfType "horizontal") 6
    Assert.Multiple(fun () ->
      Assert.That(vert, Is.EqualTo none, "none と vertical で軌跡が違う")
      Assert.That(horz, Is.EqualTo none, "none と horizontal で軌跡が違う"))
    // 3 つとも同じなので、代表 1 本だけ控えに残す
    none |> Golden.check "shooting-type-no-effect"

  /// DTD は type を省略可（既定 "none"）と定めている。
  [<Test>]
  member _.``type を省くとどうなるか``() =
    let noType = """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
<action label="top">
  <fire><direction type="absolute">30</direction><speed>2</speed><bullet/></fire>
  <wait>3</wait>
</action>
</bulletml>"""
    let result =
      try
        let t = TraceRun.std noType 4
        sprintf "落ちない。軌跡が出た（先頭 2 行）\n%s" (t.Split('\n') |> Array.truncate 2 |> String.concat "\n")
      with e ->
        let rec inner (x: exn) = if isNull x.InnerException then x else inner x.InnerException
        let i = inner e
        sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))
    sprintf "DTD の定め: <!ATTLIST bulletml type (none|vertical|horizontal) \"none\"> （省略可・既定 none）\n実装: %s" result
    |> Golden.check "shooting-type-omitted"

  /// BulletmlRead.fs の `| None ->` に届く入力があるかを探す。
  [<Test>]
  member _.``bulletml の属性をぜんぶ省くとどうなるか``() =
    let noAttrs = """<?xml version="1.0" ?>
<bulletml>
<action label="top">
  <fire><direction type="absolute">30</direction><speed>2</speed><bullet/></fire>
  <wait>3</wait>
</action>
</bulletml>"""
    let result =
      try
        let t = TraceRun.std noAttrs 4
        // 「落ちない」だけだと、黙って 1 つも走らなかった場合と見分けがつかない
        let fired = t.Split('\n') |> Array.filter (fun l -> l.Contains "  +b") |> Array.length
        sprintf "落ちない。撃った弾 %d 発" fired
      with e ->
        let rec inner (x: exn) = if isNull x.InnerException then x else inner x.InnerException
        let i = inner e
        sprintf "%s: %s" (i.GetType().Name) (i.Message.Replace("\r", "").Replace("\n", " "))
    sprintf "xmlns も type も name も無い <bulletml>\n実装: %s\n\n落ちないなら、attrs が None になる入力は見つかっていない"
      result
    |> Golden.check "bulletml-no-attrs"

  [<Test>]
  member _.``type に知らない値を書くと DTD 違反で落ちる``() =
    let ex =
      Assert.Throws<FsBulletML2.Exception.BulletmlDTDViolationException>(fun () ->
        TraceRun.std (bmlOfType "diagonal") 2 |> ignore)
    sprintf "例外: %s\nメッセージ: %s" (ex.GetType().Name) ex.Message
    |> Golden.check "shooting-type-unknown"
