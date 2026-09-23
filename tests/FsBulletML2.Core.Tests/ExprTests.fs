namespace FsBulletML2.Core.Tests

open System
open System.IO
open System.Xml
open NUnit.Framework
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Eval

/// 式を文字列でなく木で持つ `Expr` が、`getValueByXPath` と同じ値を返すか。
module internal ExprCorpus =

  /// 式が入る要素。DTD の #PCDATA がここに来る
  let elements = [ "direction"; "speed"; "horizontal"; "vertical"; "term"; "times"; "wait"; "param" ]

  let private loadDoc (path: string) =
    let settings = XmlReaderSettings(DtdProcessing = DtdProcessing.Ignore, XmlResolver = null)
    let doc = XmlDocument(XmlResolver = null)
    use r = XmlReader.Create(path, settings)
    doc.Load r
    doc

  /// samples から式を全部 集めて、重複を潰す。
  let all : Lazy<string list> =
    lazy (
      CorpusData.uniqueSamples ()
      |> List.collect (fun f ->
        try
          let doc = loadDoc f
          [ for e in elements do
              for n in doc.GetElementsByTagName e do
                let t = n.InnerText
                if not (isNull t) then
                  let t = t.Trim()
                  if t.Length > 0 then yield t ]
        with _ -> [])
      |> List.distinct
      |> List.sort)

  /// 参照の実引数だけ（`<param>`）。仮引数へ差し込んだ形も試すため
  let paramArgs : Lazy<string list> =
    lazy (
      CorpusData.uniqueSamples ()
      |> List.collect (fun f ->
        try
          let doc = loadDoc f
          [ for n in doc.GetElementsByTagName "param" do
              let t = n.InnerText
              if not (isNull t) then
                let t = t.Trim()
                if t.Length > 0 then yield t ]
        with _ -> [])
      |> List.distinct
      |> List.sort)

  /// `$N` を使う式だけ
  let paramUsers : Lazy<string list> =
    lazy (all.Value |> List.filter (fun s -> Text.RegularExpressions.Regex.IsMatch(s, @"\$\d")))

  /// 突き合わせに使う環境。乱数は「引いた回数」も揃えたいので、
  /// 呼ばれたら決まった値を返すだけのものにする
  let envOf (randValue: float32) (rank: float32) : Env =
    { Rand = (fun () -> randValue)
      Rank = rank
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

  /// float32 を 1 ビットも違わずに比べる。NaN は NaN と等しいとみなす
  /// （旧も新しいほうも、読めない式で NaN を返すため）
  let same (a: float32) (b: float32) =
    if Single.IsNaN a && Single.IsNaN b then true
    else BitConverter.SingleToInt32Bits a = BitConverter.SingleToInt32Bits b

  /// 振る値。
  let cases =
    [ 0.0f, 0.0f
      0.5f, 0.5f
      1.0f, 1.0f
      0.25f, 0.75f
      0.123456f, 0.987654f
      0.9999999f, 0.0001f ]

  /// getValueByXPath は落ちうる。落ちたことも結果として扱う
  type Outcome =
    | Value of float32
    | Threw of string

  let current (env: Env) (s: string) =
    try Value (getValueByXPath env s) with e -> Threw (e.GetType().Name)


/// `NonParallelizable` を外した。 グローバル（`BulletMLManager`）を
/// `SetUp` で書き換えていたのが唯一の理由で、その `SetUp` ごと消えた。
[<TestFixture>]
type ExprTests() =

  /// 当てる先が本当に在るか。0 件を緑にしない
  [<Test>]
  member _.``samples から式が集まっている``() =
    let exprs = ExprCorpus.all.Value
    Assert.That(List.length exprs, Is.GreaterThan 500,
                sprintf "式が %d 種 しか集まっていません。走査が壊れています（%s）"
                        (List.length exprs) CorpusData.samplesDir)
    TestContext.WriteLine(sprintf "式 %d 種 / うち $N を使うもの %d 種 / 実引数 %d 種"
                            (List.length exprs)
                            (List.length ExprCorpus.paramUsers.Value)
                            (List.length ExprCorpus.paramArgs.Value))

  /// 本体。実物の式ぜんぶを、両方に食わせて突き合わせる
  [<Test>]
  member _.``実物の式ぜんぶで getValueByXPath と同じ値になる``() =
    let exprs = ExprCorpus.all.Value
    let mutable checkedCount = 0
    let diffs = ResizeArray<string>()
    let threw = ResizeArray<string>()
    for s in exprs do
      let ast = Expr.parse s
      for randValue, rank in ExprCorpus.cases do
        let env = ExprCorpus.envOf randValue rank
        let actual = Expr.evalWithValues randValue rank ast
        checkedCount <- checkedCount + 1
        match ExprCorpus.current env s with
        | ExprCorpus.Threw name ->
            threw.Add(sprintf "  [%s]  rand=%g rank=%g  旧が %s" s randValue rank name)
        | ExprCorpus.Value expected ->
            if not (ExprCorpus.same expected actual) then
              diffs.Add(sprintf "  [%s]  rand=%g rank=%g  旧 %.9g / 木 %.9g"
                                s randValue rank expected actual)
    TestContext.WriteLine(sprintf "突き合わせ %d 通り（式 %d 種 x 振り %d 通り）／旧が落ちたのは %d 通り"
                            checkedCount (List.length exprs) (List.length ExprCorpus.cases) threw.Count)
    if threw.Count > 0 then
      let head = threw |> Seq.truncate 10 |> String.concat "\n"
      Assert.Fail(sprintf "旧が %d 通りで落ちました。値を突き合わせられません（先頭 10 件）:\n%s" threw.Count head)
    if diffs.Count > 0 then
      let head = diffs |> Seq.truncate 40 |> String.concat "\n"
      Assert.Fail(sprintf "%d 通りで値が違います（先頭 40 件）:\n%s" diffs.Count head)

  /// 実引数を仮引数へ「文字で」差し込んだ形も突き合わせる。
  [<Test>]
  member _.``実引数を差し込んだ形でも getValueByXPath と同じ値になる``() =
    let users = ExprCorpus.paramUsers.Value
    let args = ExprCorpus.paramArgs.Value
    Assert.That(List.length users, Is.GreaterThan 0, "$N を使う式が 1 つも見つかりません")
    Assert.That(List.length args, Is.GreaterThan 0, "実引数が 1 つも見つかりません")

    // 全部の掛け合わせは多すぎるので、実引数を間引く。
    // 「トップレベルに二項の + / - を持つもの」は優先順位が動きうるので必ず入れる
    let hasTopLevelAddSub (s: string) =
      let mutable depth = 0
      let mutable prev = ' '
      let mutable hit = false
      for c in s do
        if c = '(' then depth <- depth + 1
        elif c = ')' then depth <- depth - 1
        elif (c = '+' || c = '-') && depth = 0 then
          if Char.IsDigit prev || prev = ')' || Char.IsLetter prev then hit <- true
        if not (Char.IsWhiteSpace c) then prev <- c
      hit
    let risky = args |> List.filter hasTopLevelAddSub
    let others = args |> List.filter (hasTopLevelAddSub >> not) |> List.truncate 40
    let chosenArgs = risky @ others
    let chosenUsers = users |> List.truncate 60

    let mutable checkedCount = 0
    let diffs = ResizeArray<string>()
    for user in chosenUsers do
      for arg in chosenArgs do
        // Param.replace と同じ置き換え。$1 と $2 に同じ実引数を入れる
        let prams = Param.ofList [ arg; arg ]
        let substituted =
          try Param.replace user prams with _ -> user
        let ast = Expr.parse substituted
        // 振りは 2 通りでよい（式の種類のほうを増やしたいので）
        for randValue, rank in [ 0.5f, 0.5f; 0.123456f, 0.987654f ] do
          let env = ExprCorpus.envOf randValue rank
          let expected = getValueByXPath env substituted
          let actual = Expr.evalWithValues randValue rank ast
          checkedCount <- checkedCount + 1
          if not (ExprCorpus.same expected actual) then
            diffs.Add(sprintf "  仮[%s] 実[%s] -> [%s]  rand=%g rank=%g  旧 %.9g / 木 %.9g"
                              user arg substituted randValue rank expected actual)
    TestContext.WriteLine(sprintf "差し込んで突き合わせ %d 通り（仮 %d x 実 %d x 振り 2）"
                            checkedCount (List.length chosenUsers) (List.length chosenArgs))
    Assert.That(checkedCount, Is.GreaterThan 1000, "掛け合わせが少なすぎます。選び方が壊れています")
    if diffs.Count > 0 then
      let head = diffs |> Seq.truncate 40 |> String.concat "\n"
      Assert.Fail(sprintf "%d 通りで値が違います（先頭 40 件）:\n%s" diffs.Count head)

  /// 校正点。
  [<Test>]
  member _.``突き合わせは優先順位の間違いを拾える``() =
    // 左から順に計算する（* と + を区別しない）壊れた評価器
    let rec leftToRight (randValue: double) (rank: double) (node: Expr.Node) : double =
      // 木はもう優先順位どおりに組まれているので、木を壊すのではなく
      // 「掛け算を足し算として読む」形で崩す。
      match node with
      | Expr.Num v -> v
      | Expr.Rand -> randValue
      | Expr.Rank -> rank
      | Expr.Neg a -> -(leftToRight randValue rank a)
      | Expr.Add (a, b) -> leftToRight randValue rank a + leftToRight randValue rank b
      | Expr.Sub (a, b) -> leftToRight randValue rank a - leftToRight randValue rank b
      | Expr.Mul (a, b) -> leftToRight randValue rank a + leftToRight randValue rank b
      | Expr.Div (a, b) -> leftToRight randValue rank a - leftToRight randValue rank b
      | Expr.Mod (a, b) -> leftToRight randValue rank a % leftToRight randValue rank b
      | Expr.Invalid -> Double.NaN

    let exprs = ExprCorpus.all.Value
    let env = ExprCorpus.envOf 0.5f 0.5f
    let mutable caught = 0
    let mutable total = 0
    for s in exprs do
      let ast = Expr.parse s
      let expected = getValueByXPath env s
      let broken = float32 (leftToRight 0.5 0.5 ast)
      total <- total + 1
      if not (ExprCorpus.same expected broken) then caught <- caught + 1
    TestContext.WriteLine(sprintf "壊した評価器は %d / %d 種 で赤くなった" caught total)
    // 演算子を含む式は 1269 種 中 3000 箇所 近くあるので、数百は赤くなるはず。
    // ここが 0 に近いなら、突き合わせが値の違いを見ていない
    Assert.That(caught, Is.GreaterThan 300,
                sprintf "壊した評価器が %d 種 でしか赤くなりません。突き合わせが違いを拾えていない疑いがあります" caught)

  /// 見つけた穴 1。
  [<Test>]
  member _.``小さい rand rank で getValueByXPath は落ちる``() =
    let small = [ 0.0001f; 0.00001f; 0.0000001f; 1e-20f ]
    let s = "$rank*10"
    let crashed =
      small |> List.filter (fun v ->
        match ExprCorpus.current (ExprCorpus.envOf 0.5f v) s with
        | ExprCorpus.Threw _ -> true
        | ExprCorpus.Value _ -> false)
    TestContext.WriteLine(
      sprintf "旧が落ちた値: %s"
              (crashed |> List.map (fun v -> v.ToString(Globalization.CultureInfo.InvariantCulture)) |> String.concat ", "))
    Assert.That(List.length crashed, Is.GreaterThan 0,
                "旧が 1 つも落ちません。この穴が塞がったなら、この試験を消してよい")
    // 木のほうは同じ値で落ちず、素直に計算する
    for v in small do
      let got = Expr.evalWithValues 0.5f v (Expr.parse s)
      Assert.That(Single.IsNaN got, Is.False, sprintf "rank=%g で木が NaN になっています" v)

  /// 見つけた穴 2。
  [<Test>]
  member _.``読めない式は 旧が例外 木は NaN``() =
    let env = ExprCorpus.envOf 0.5f 0.5f
    let odd = [ ""; "   "; "abc"; "1+"; "*3"; "("; "()"; "1)"; "$foo"; "1 2" ]
    let mutable currentThrew = 0
    for s in odd do
      match ExprCorpus.current env s with
      | ExprCorpus.Threw _ -> currentThrew <- currentThrew + 1
      | ExprCorpus.Value _ -> ()
      // 木は何を渡されても落ちない
      let got = Expr.evalWithValues 0.5f 0.5f (Expr.parse s)
      Assert.That(Single.IsNaN got, Is.True, sprintf "[%s] は NaN のはずが %.9g" s got)
    TestContext.WriteLine(sprintf "読めない式 %d 個 のうち、旧が落ちたのは %d 個" (List.length odd) currentThrew)
    Assert.That(currentThrew, Is.GreaterThan 0,
                "旧が 1 つも落ちません。この試験が前提にしている差が無くなっています")

  /// 上の差 2 が同梱の台本で踏めないことを、数で押さえる。
  /// 読めない式が 1 つでもあれば、そこで旧と木の振る舞いが分かれる
  [<Test>]
  member _.``同梱の台本に読めない式は無い``() =
    let bad =
      ExprCorpus.all.Value
      |> List.filter (fun s -> Expr.parse s = Expr.Invalid)
    TestContext.WriteLine(sprintf "読めなかった式 %d 種 / %d 種" (List.length bad) (List.length ExprCorpus.all.Value))
    if not (List.isEmpty bad) then
      Assert.Fail(sprintf "読めない式があります（先頭 20 件）:\n%s"
                          (bad |> List.truncate 20 |> List.map (sprintf "  [%s]") |> String.concat "\n"))

  /// 木は文字列を捨てない。捨てると、実引数の文字置き換えができなくなる
  [<Test>]
  member _.``NumExpr はもとの文字列を保つ``() =
    let e = Expr.NumExpr.ofString "$1 * (0.5 + 0.5 * $rank)"
    Assert.That(Expr.NumExpr.text e, Is.EqualTo "$1 * (0.5 + 0.5 * $rank)")
    let replaced = e |> Expr.NumExpr.mapSource (fun s -> Param.replace s (Param.ofList [ "1+2" ]))
    Assert.That(Expr.NumExpr.text replaced, Is.EqualTo "1+2 * (0.5 + 0.5 * $rank)")
    // 文字で入れているので 1 + (2 * ...) になる。木の節として差し込むと
    // (1+2) * ... になって値が変わる。ここが変わっていないことを数で押さえる
    let v = Expr.NumExpr.evalWithValues 0.5f 0.5f replaced
    Assert.That(float v, Is.EqualTo(1.0 + 2.0 * 0.75).Within(1e-6),
                "文字の置き換えとして読めていません（木の節として差し込むと 2.25 になります）")

  /// 乱数は 1 回 の評価につき 1 回 だけ引く。$rand が無くても引く。
  /// 引く回数は乱数の並びを進めるので、ここがずれると軌跡が丸ごとずれる
  [<Test>]
  member _.``乱数は式の中身によらず 1 回だけ引く``() =
    let draws = ResizeArray<float32>()
    let rand () =
      let v = 0.25f * float32 (draws.Count % 4)
      draws.Add v
      v
    // $rand が 0 個
    draws.Clear()
    Expr.eval rand 0.5f (Expr.parse "30") |> ignore
    Assert.That(draws.Count, Is.EqualTo 1, "$rand が無くても 1 回 引くこと")
    // $rand が 1 個
    draws.Clear()
    Expr.eval rand 0.5f (Expr.parse "$rand*10") |> ignore
    Assert.That(draws.Count, Is.EqualTo 1)
    // $rand が 2 個 —— それでも 1 回。しかも両方 同じ値
    draws.Clear()
    let v = Expr.eval rand 0.5f (Expr.parse "$rand-$rand")
    Assert.That(draws.Count, Is.EqualTo 1, "$rand が 2 個 でも 1 回 だけ引くこと")
    Assert.That(float v, Is.EqualTo(0.0).Within(1e-9), "同じ式の中の $rand は同じ値になること")

  /// 読む費用は 1 回きり。
  [<Test>]
  [<Category("Timing")>]
  member _.``木の評価は getValueByXPath より桁で速い``() =
    let s = "(0.8 + 1.1*$1*(180-$1)/(90*90)) * (0.5+0.5*$rand) * (0.5+0.5*$rank)"
    let env = ExprCorpus.envOf 0.5f 0.5f
    // 走行中に使う形（NumExpr）で測る。$rand / $rank を使うかは
    // 読んだときに決まっていて、評価のたびに木を歩き直さない
    let e = Expr.NumExpr.ofString s
    // 温める
    for _ in 1 .. 200 do
      getValueByXPath env s |> ignore
      Expr.NumExpr.evalWithValues 0.5f 0.5f e |> ignore

    let n = 2000
    let sw = Diagnostics.Stopwatch.StartNew()
    for _ in 1 .. n do getValueByXPath env s |> ignore
    sw.Stop()
    let oldNs = sw.Elapsed.TotalMilliseconds * 1e6 / float n

    let sw2 = Diagnostics.Stopwatch.StartNew()
    let mutable acc = 0.0f
    for _ in 1 .. n do acc <- acc + Expr.NumExpr.evalWithValues 0.5f 0.5f e
    sw2.Stop()
    let newNs = sw2.Elapsed.TotalMilliseconds * 1e6 / float n
    Assert.That(acc, Is.Not.EqualTo 0.0f)

    TestContext.WriteLine(sprintf "getValueByXPath %.0f ns / 木の評価 %.0f ns（%.0f 倍）" oldNs newNs (oldNs / newNs))
    // 10 倍 は下限として置く。実測は 2 桁 出るが、締めると台で赤くなる
    Assert.That(oldNs / newNs, Is.GreaterThan 10.0,
                sprintf "getValueByXPath %.0f ns に対して木の評価が %.0f ns。桁で速くなっていません" oldNs newNs)
