namespace FsBulletML.Core.Tests

open NUnit.Framework
open FsUnit

module Divergence =

  /// 行に割る。Trace.fs の出力は末尾が必ず改行なので、そのまま Split すると
  /// 末尾に空行が 1 つ増える。長さの違う軌跡を比べたとき、その空行が相手の
  /// 実在する行とかち合って「行なし」より先に食い違い扱いされてしまうので、
  /// 割る前に末尾の改行 1 つぶんだけ落とす。
  let private lines (s: string) : string[] =
    let normalized = s.Replace("\r\n", "\n")
    let trimmed = if normalized.EndsWith "\n" then normalized.Substring(0, normalized.Length - 1) else normalized
    trimmed.Split('\n')

  /// 一致すれば None。割れていれば最初の場所を指す説明を返す
  let firstDivergence (a: string) (b: string) : string option =
    let la = lines a
    let lb = lines b
    let n = max la.Length lb.Length
    let mutable frame = "(先頭のフレームより前)"
    let mutable found : string option = None
    let mutable i = 0
    while found.IsNone && i < n do
      let x = if i < la.Length then la.[i] else "(行なし)"
      let y = if i < lb.Length then lb.[i] else "(行なし)"
      if x.StartsWith "f" then frame <- x
      if x <> y then
        found <- Some (sprintf "%s の %d 行め で割れた\n  旧: %s\n  新: %s" frame (i + 1) x y)
      i <- i + 1
    found

/// 2 つの軌跡を比べて、最初に食い違った場所を出す。
///
/// 指紋だけだと「違う」しか言えない。橋が割れたときに 60 フレーム × 227 本 の
/// 中から人が探すことになるので、フレームと行を指すところまでを道具にする。
[<TestFixture>]
type Divergence() =

  [<Test>]
  member _.``同じ軌跡なら None``() =
    let a = "f00\n  b0 P- x=1.000\nf01\n  b0 P- x=2.000\n"
    Divergence.firstDivergence a a |> should equal None

  [<Test>]
  member _.``割れた行の、直前のフレームと両方の中身を出す``() =
    let a = "f00\n  b0 P- x=1.000\nf01\n  b0 P- x=2.000\n"
    let b = "f00\n  b0 P- x=1.000\nf01\n  b0 P- x=9.000\n"
    match Divergence.firstDivergence a b with
    | Some msg ->
        msg |> should haveSubstring "f01"
        msg |> should haveSubstring "x=2.000"
        msg |> should haveSubstring "x=9.000"
    | None -> Assert.Fail "割れているのに None が返った"

  [<Test>]
  member _.``行数が足りないほうも割れたと言う``() =
    let a = "f00\n  b0 P- x=1.000\nf01\n  b0 P- x=2.000\n"
    let b = "f00\n  b0 P- x=1.000\n"
    match Divergence.firstDivergence a b with
    | Some msg -> msg |> should haveSubstring "行なし"
    | None -> Assert.Fail "行数が違うのに None が返った"

  [<Test>]
  member _.``最初の食い違いだけを出す``() =
    let a = "f00\n  b0 P- x=1.000\nf01\n  b0 P- x=2.000\n"
    let b = "f00\n  b0 P- x=8.000\nf01\n  b0 P- x=9.000\n"
    match Divergence.firstDivergence a b with
    | Some msg ->
        msg |> should haveSubstring "x=1.000"
        msg |> should not' (haveSubstring "x=2.000")
    | None -> Assert.Fail "割れているのに None が返った"
