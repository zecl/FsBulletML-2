namespace FsBulletML2.LanguageService

open System
open Microsoft.FSharp.Reflection
open FsBulletML2

/// 読んだ木のノードと、字の札を順番で結ぶ。
/// 名前の表を書かない。DTD に要素が増えると添字がずれる。
module NodeOrder =

  let private camel (s: string) =
    if String.IsNullOrEmpty s then s
    else string (Char.ToLowerInvariant s.[0]) + s.Substring 1

  let private armsOf (t: Type) =
    FSharpType.GetUnionCases t |> Array.map (fun c -> camel c.Name)

  /// 木のノードになる要素名。`Progress` と同じ腕。
  /// `BulletmlOps.collect` は使うな。`repeat` / `wait` / `vanish` を落とす。
  let names : string[] =
    [| typeof<Bulletml>; typeof<BulletmlElm>; typeof<Action>
       typeof<ActionElm>; typeof<BulletElm> |]
    |> Array.collect armsOf
    |> Array.distinct
    |> Array.sort

  /// 読んだ木を書いてある順に歩いて、(要素名, ノード) を並べる。
  /// 名前は腕から引く。綴りを書くな。ノードは `box`。中身が同じ別ノードが在る。
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