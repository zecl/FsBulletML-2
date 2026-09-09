namespace FsBulletML2.LanguageService

open System
open Microsoft.FSharp.Reflection
open FsBulletML2

/// 読んだ木のノードと、字の札を**順番で結ぶ**（v3.1 の段 4）。
///
/// **木は字の位置を持たない**（`XmlNode` は 名前・属性・子 だけ）ので、
/// 「このノードは字のどこか」は位置ではなく順番でしか言えない。
/// v2.9 でそこは測ってある —— **木の k 番目 と札の k 番目 が
/// 3 表記 で 176 / 176 揃う**（F# の CE だけは結べない）。
///
/// ここが持つのは 2 つ ——
///
///     names   木のノードになる要素名。**表を書かない**（腕から引く）
///     walk    読んだ木を書いてある順に歩いて、(名前, ノード) を並べる
///
/// **名前の表を書かない理由。** 落とすべき名前（`times` / `direction` /
/// `speed` / `horizontal` / `vertical` / `term` / `param`）を並べると、
/// DTD に要素が増えたときにここだけが古びる —— しかも**増えた側は
/// 「ノードでない」に落ちるので、黙って添字がずれる。**
/// 引くのは逆側（ノードになる腕）で、そちらは型そのもの。
module NodeOrder =

  let private camel (s: string) =
    if String.IsNullOrEmpty s then s
    else string (Char.ToLowerInvariant s.[0]) + s.Substring 1

  let private armsOf (t: Type) =
    FSharpType.GetUnionCases t |> Array.map (fun c -> camel c.Name)

  /// 木のノードになる要素名。**`Progress` が平行に組まれる腕と同じ集合。**
  ///
  /// `BulletmlOps.collect` は使えない —— あちらは名前の付いた要素だけを拾い、
  /// `repeat` / `wait` / `vanish` を落とす。
  ///
  /// **並びは名前の順に揃える**（呼ぶ側が中身で見分けられるように）。
  /// reflection なので `PublishTrimmed` は false のまま —— 空は呼ぶ側が赤にする
  let names : string[] =
    [| typeof<Bulletml>; typeof<BulletmlElm>; typeof<Action>
       typeof<ActionElm>; typeof<BulletElm> |]
    |> Array.collect armsOf
    |> Array.distinct
    |> Array.sort

  /// 読んだ木を**書いてある順**に歩いて、(要素名, ノード) を並べる。
  ///
  /// **名前は腕から引く**（`names` と同じ規則）—— 綴りを書くと、
  /// 歩きと `names` が別の綴りを持てるようになる。
  ///
  /// ノードは `box` で渡す。**受け取る側は参照を鍵にする** ——
  /// 走行から戻ってきたノードが並びの何番目 かは、参照でしか引けない
  /// （中身が同じ別のノードが在る）
  let walk (root: Bulletml) : ResizeArray<string * obj> =
    let out = ResizeArray<string * obj>()
    let add (ty: Type) (o: obj) =
      let info, _ = FSharpValue.GetUnionFields(o, ty)
      out.Add(camel info.Name, o)
    let rec command (c: Action) =
      add typeof<Action> (box c)
      match c with
      | Action.Action (_, children) -> children |> List.iter command
      | Action.Repeat (_, child) -> actionElm child
      | Action.Fire (_, _, _, child) -> bulletElm child
      | _ -> ()
    and actionElm (a: ActionElm) =
      add typeof<ActionElm> (box a)
      match a with
      | ActionElm.Action (_, children) -> children |> List.iter command
      | ActionElm.ActionRef _ -> ()
    and bulletElm (b: BulletElm) =
      add typeof<BulletElm> (box b)
      match b with
      | BulletElm.Bullet (_, _, _, children) -> children |> List.iter actionElm
      | BulletElm.BulletRef _ -> ()
    let topElm (t: BulletmlElm) =
      add typeof<BulletmlElm> (box t)
      match t with
      | BulletmlElm.Bullet (_, _, _, children) -> children |> List.iter actionElm
      | BulletmlElm.Fire (_, _, _, child) -> bulletElm child
      | BulletmlElm.Action (_, children) -> children |> List.iter command
    match root with
    | Bulletml (_, tops) ->
        add typeof<Bulletml> (box root)
        tops |> List.iter topElm
    out