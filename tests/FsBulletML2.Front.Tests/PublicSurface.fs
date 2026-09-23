namespace FsBulletML2.Front.Tests

open System
open System.Collections.Generic
open System.Reflection
open NUnit.Framework
open FsUnit
open FsBulletML2.Front

/// `FsBulletML2.Front` の公開面に `seq<_>`（`IEnumerable<_>`）が出ないこと。
/// 但し書きではなく門。次に口を足す人には但し書きが届かない。
[<TestFixture>]
type PublicSurface() =

  let asm = typeof<IFrontEnv>.Assembly

  let isEnumerable (t: Type) =
    let rec walk (t: Type) =
      if t = null then false
      elif t.IsByRef || t.IsArray || t.IsPointer then walk (t.GetElementType())
      elif t.IsGenericType
           && (t.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>
               || t.GetGenericTypeDefinition() = typedefof<IEnumerator<_>>) then true
      elif t.IsGenericType then t.GetGenericArguments() |> Array.exists walk
      else false
    walk t

  /// 型 1 つ の公開メンバを見て、`seq` が出ている場所の名前を返す
  let offenders (t: Type) =
    let flags = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.DeclaredOnly
    [ for m in t.GetMethods(flags) do
        if isEnumerable m.ReturnType then yield sprintf "%s.%s -> %s" t.Name m.Name m.ReturnType.Name
        for p in m.GetParameters() do
          if isEnumerable p.ParameterType then yield sprintf "%s.%s(%s)" t.Name m.Name p.Name
      for p in t.GetProperties(flags) do
        if isEnumerable p.PropertyType then yield sprintf "%s.%s" t.Name p.Name
      for c in t.GetConstructors(flags) do
        for p in c.GetParameters() do
          if isEnumerable p.ParameterType then yield sprintf "%s..ctor(%s)" t.Name p.Name ]

  [<Test>]
  member _.``公開面に seq が出ていない``() =
    let types = asm.GetTypes() |> Array.filter (fun t -> t.IsPublic || t.IsNestedPublic)
    // 当てる先が在るかを先に見る。 型が 0 本 なら 0 件 は緑ではない
    types.Length |> should be (greaterThan 2)
    let bad = types |> Array.collect (offenders >> Array.ofList) |> List.ofArray
    if not (List.isEmpty bad) then
      Assert.Fail(sprintf "公開面に seq が %d 件 出ている:\n  %s"
                    (List.length bad) (String.Join("\n  ", bad)))
    TestContext.WriteLine(sprintf "見た公開型 %d 本" types.Length)

  /// 較正。 上の門が「何を見ても 0 件」になっていないことを見る。
  /// `seq` を持つ型に同じ判定を当てたら、当たるはず
  [<Test>]
  member _.``較正: seq を持つ型なら当たる``() =
    let bad = offenders typeof<SeqBait>
    bad |> List.length |> should be (greaterThan 0)
    TestContext.WriteLine(sprintf "餌で当たった: %s" (String.Join(", ", bad)))

/// 較正用の餌。`FsBulletML2.Front` の外に置く —— 中に置くと本番の門が
/// これを拾って、いつでも赤くなる
and [<Sealed>] SeqBait() =
  member _.Enemies : seq<int> = Seq.empty
  member _.Take (xs: seq<int>) = Seq.length xs
