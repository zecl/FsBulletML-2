namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// F# の CE で、その場所に置ける名前を出せるか。
///
/// v1.6 まで空だった。理由は「置ける場所が入れ子の型で決まるので、字の
/// 数え方では出せない」と書いてあったが、入れ子の型は `{ }` の対で出せる。
///
/// 手で書くと 107 行 になり、DSL が動くと黙って古びる。
[<TestFixture>]
type FsharpComplete() =

  static let fsharp = Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> ISourceLanguage

  /// `|` の位置をカーソルとして候補を引く
  let at (marked: string) =
    fsharp.Complete (marked.Replace("|", "")) (marked.IndexOf '|')
    |> List.map (fun c -> c.Label)
    |> Set.ofList

  let write (b: Bulletml) = (SourceWriter.tryFind SourceKind.FSharpDsl).Value.Write b

  // --- 入れ物ごと -----------------------------------------------------------

  [<Test>]
  member _.``いちばん外では 根の名前だけ``() =
    let found = at "|"
    found |> should contain "untyped"
    found |> should contain "verticalXmlns"
    // 根の中に置くものは出さない
    found |> should not' (contain "defAction")
    found |> should not' (contain "wait")

  [<Test>]
  member _.``根の中では 定義だけ``() =
    let found = at "let x =\n  untyped \"n\" {\n    |\n  }\n"
    found |> should contain "defAction"
    found |> should contain "defBullet"
    found |> should contain "top"
    // action の中に置くものは出さない
    found |> should not' (contain "wait")
    found |> should not' (contain "fire")

  [<Test>]
  member _.``action の中では 動作``() =
    let found = at "let x =\n  untyped \"n\" {\n    top {\n      |\n    }\n  }\n"
    found |> should contain "wait"
    found |> should contain "fire"
    found |> should contain "repeat"
    found |> should contain "actionRef"
    // 根の中に置くものは出さない
    found |> should not' (contain "defAction")

  [<Test>]
  member _.``repeat の中は action の中と同じ``() =
    // 要素で引くとここが 0 個 になる。 `<repeat>` の子は
    // `(times, (action | actionRef))` で、`wait` は入っていない
    let inTop = at "let x =\n  untyped \"n\" {\n    top {\n      |\n    }\n  }\n"
    let inRepeat = at "let x =\n  untyped \"n\" {\n    top {\n      repeat \"4\" {\n        |\n      }\n    }\n  }\n"
    inRepeat |> should equal inTop
    inRepeat |> should contain "wait"

  [<Test>]
  member _.``fire の中では 向きと速さと弾``() =
    let found = at "let x =\n  untyped \"n\" {\n    top {\n      fire {\n        |\n      }\n    }\n  }\n"
    found |> should contain "aim"
    found |> should contain "speed"
    found |> should contain "refBullet"
    found |> should not' (contain "wait")

  [<Test>]
  member _.``bullet の中では 中身``() =
    let found =
      at "let x =\n  untyped \"n\" {\n    defBullet \"b\" {\n      |\n    }\n  }\n"
    found |> should contain "doActs"
    found |> should contain "speed"
    found |> should not' (contain "wait")

  [<Test>]
  member _.``accel の中では 縦と横``() =
    let found =
      at "let x =\n  untyped \"n\" {\n    top {\n      accel \"10\" {\n        |\n      }\n    }\n  }\n"
    found |> should contain "horizontal"
    found |> should contain "verticalSeq"
    found |> should not' (contain "wait")

  // --- 打っている途中 -------------------------------------------------------

  [<Test>]
  member _.``打っている途中の名前を 入れ物にしない``() =
    // `w` まで打ったところ。その `w` を「いま開いている入れ物」に
    // してはいけない —— 数えると候補が消える
    let found = at "let x =\n  untyped \"n\" {\n    top {\n      w|\n    }\n  }\n"
    found |> should contain "wait"

  [<Test>]
  member _.``置き換える幅は 打った名前のぶん``() =
    let src = "let x =\n  untyped \"n\" {\n    top {\n      wa\n    }\n  }\n"
    let at = src.IndexOf "wa" + 2
    fsharp.Complete src at
    |> List.map (fun c -> c.Replace)
    |> List.distinct
    |> should equal [ 2 ]

  [<Test>]
  member _.``同じ綴りが 2 つ の意味を持つ``() =
    // `vertical` は根の builder でもあり、`accel` の中の操作でもある。
    // `{ }` の手前 に在るのだから開く側を採る —— 採らないと、
    // `vertical "名" { }` で書かれた弾幕の中で候補が 1 つ も出ない
    // （コーパスの点で踏んだ）
    vocab.CePlaces
    |> List.filter (fun p -> p.Name = "vertical")
    |> List.length
    |> should equal 2
    let found = at "vertical \"n\" {\n  |\n}\n"
    found |> should contain "defAction"
    found |> should not' (contain "verticalSeq")

  [<Test>]
  member _.``知らない入れ物では 出さない``() =
    // `{ }` は F# のあちこちに在る。CE でない `{` の中で候補を出さない
    at "let x = seq {\n  |\n}\n" |> should be Empty

  [<Test>]
  member _.``文字列とコメントの中の 波括弧を数えない``() =
    // 名前の中に `{` が入っていても、入れ物は開かない
    let found = at "let x =\n  untyped \"a { b\" {\n    |\n  }\n"
    found |> should contain "defAction"

  // --- コーパスを覆う -------------------------------------------------------

  [<Test>]
  member _.``本文に書いてある名前は 必ず候補に在る``() =
    // 同梱 176 本 を焼いて、名前 1 つ ずつ その場所の候補を引く。
    // 書いてあるのに出ないものが 1 つ でも在れば、その入れ物で人は打てない
    let isIdent (c: char) =
      (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c = '_' || c = '\''
    // CE ではない字（F# の一部）
    let notCe = set [ "let"; "x"; "createBulletmlInfo" ]
    let known = vocab.CePlaces |> List.map (fun p -> p.Name) |> Set.ofList
    let mutable checked' = 0
    let bad = ResizeArray<string>()
    for info in Bullets.Dsl.All.bullets do
      match write info.Bulletml with
      | Result.Error _ -> ()
      | Result.Ok src ->
        let mutable i = 0
        while i < src.Length do
          if isIdent src.[i] && (i = 0 || not (isIdent src.[i - 1])) then
            let s = i
            let mutable e = i
            while e + 1 < src.Length && isIdent src.[e + 1] do
              e <- e + 1
            let w = src.Substring(s, e - s + 1)
            // 文字列とコメントの中は数えない。 弾幕の名前に `aim` や
            // `accel` が入っていることは在る —— `wordAt` は器の 1 本 で、
            // 文字列の中では `None` を返す
            let outside = FsharpScan.wordAt src s = Some w
            if outside && known.Contains w && not (notCe.Contains w) then
              checked' <- checked' + 1
              let offered = fsharp.Complete src s |> List.map (fun c -> c.Label) |> Set.ofList
              if not (offered.Contains w) then
                bad.Add(sprintf "%s: %s が出ない" info.Name w)
            i <- e + 1
          else i <- i + 1
    bad |> Seq.truncate 10 |> List.ofSeq |> should be Empty
    // 1 つ も当てていなければ、上は何も測っていない
    checked' |> should greaterThan 5000
