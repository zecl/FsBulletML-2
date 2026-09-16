namespace FsBulletML2.Dsl.Tests

open System
open System.Reflection
open NUnit.Framework
open FsUnit
open FsBulletML2

/// CE と DU カタログが同じ木になることを 1 個 ずつ突き合わせる門。
///
/// 人が書くのは `FsBulletML2.Bullets.Dsl`（CE）のほう。
/// `FsBulletML2.Bullets`（DU を直に組んだカタログ）は、その値を写した
///
/// それでも置いてあるのは、生成器が黙って壊れる形があるため —— 実際、
///
///   2. 名前の集合が一致するか（片方にしか無いものが無いか）
[<TestFixture>]
type BulletsEquivalence() =

  /// 参照を残すための錨。
  ///
  /// このテストはアセンブリをリフレクションで走査するが、コンパイラは
  /// 1 つ も型を使っていない参照を出力に残さない。 残らないと
  /// 実行時にそのアセンブリが読み込まれず、走査が 0 件 になる。
  /// 片側ずつ 1 個 だけ直に触っておく。
  static let anchorPlain = FsBulletML2.Bullets.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA
  static let anchorDsl = FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA

  /// 両側のアセンブリに在る弾幕の数。176 個 が同梱の BulletmlInfo、
  /// 17 個 が公式配布のサンプル（v2.4.1 で足した。`All.official`）、
  /// 3 個 が Bulletml 直（Player.fs の 3 本 は createBulletmlInfo を通していない）
  [<Literal>]
  static let Expected = 196

  /// アセンブリの中の弾幕を「名前 -> 木」で集める。
  /// 名前は namespace の接頭辞を落としたもので、両側で同じ形になる
  static let collect (assemblyName: string) (prefix: string) =
    let asm =
      AppDomain.CurrentDomain.GetAssemblies()
      |> Array.tryFind (fun a -> a.GetName().Name = assemblyName)
    match asm with
    | None -> failwithf "アセンブリが読み込まれていない: %s" assemblyName
    | Some asm ->
      asm.GetTypes()
      |> Array.collect (fun t ->
          if isNull t.FullName || not (t.FullName.StartsWith prefix) then [||]
          else
            t.GetProperties(BindingFlags.Public ||| BindingFlags.Static)
            |> Array.choose (fun p ->
                let value =
                  if p.PropertyType = typeof<BulletmlInfo> then
                    Some ((p.GetValue null :?> BulletmlInfo).Bulletml)
                  elif p.PropertyType = typeof<Bulletml> then
                    Some (p.GetValue null :?> Bulletml)
                  else None
                value
                |> Option.map (fun b ->
                    (t.FullName.Substring(prefix.Length) + "." + p.Name), b)))
      |> Map.ofArray

  static let plain = lazy (collect "FsBulletML2.Bullets" "FsBulletML2.Bullets.")
  static let dsl = lazy (collect "FsBulletML2.Bullets.Dsl" "FsBulletML2.Bullets.Dsl.")

  /// 錨が生きていること。これが落ちるなら参照が消えている
  [<Test>]
  member _.``両側のアセンブリが読み込まれている``() =
    anchorPlain.Bulletml |> should equal anchorDsl.Bulletml

  [<Test>]
  member _.``DU で書いた側の弾幕が 196 個 ある``() =
    plain.Value.Count |> should equal Expected

  [<Test>]
  member _.``CE で書いた側の弾幕も 196 個 ある``() =
    dsl.Value.Count |> should equal Expected

  /// 名前の集合が一致する。 数が同じでも中身がずれていれば、
  /// 突き合わせが「たまたま同数」で通ってしまう
  [<Test>]
  member _.``名前の集合が両側で一致する``() =
    let onlyPlain = plain.Value |> Map.toSeq |> Seq.map fst |> Seq.filter (fun k -> not (dsl.Value.ContainsKey k)) |> List.ofSeq
    let onlyDsl = dsl.Value |> Map.toSeq |> Seq.map fst |> Seq.filter (fun k -> not (plain.Value.ContainsKey k)) |> List.ofSeq
    if not (List.isEmpty onlyPlain) || not (List.isEmpty onlyDsl) then
      Assert.Fail(
        sprintf "DU 側にしかない: %A\nCE 側にしかない: %A" onlyPlain onlyDsl)

  /// 本体。 196 個 すべてについて木が完全に一致することを見る。
  /// 落ちたときにどれが違うかが分かるよう、名前を並べて出す
  [<Test>]
  member _.``CE で書き直した弾幕が、元と同じ木になる``() =
    let mismatches =
      plain.Value
      |> Map.toSeq
      |> Seq.choose (fun (name, before) ->
          match dsl.Value.TryFind name with
          | Some after when after = before -> None
          | Some _ -> Some (name + "（木が違う）")
          | None -> Some (name + "（CE 側に無い）"))
      |> List.ofSeq

    if not (List.isEmpty mismatches) then
      Assert.Fail(
        sprintf "%d 個 が一致しない:\n%s"
          mismatches.Length
          (String.Join("\n", mismatches |> List.truncate 40)))

    // 突き合わせた数も見る。 両方 空でも上は通ってしまう
    plain.Value.Count |> should equal Expected
