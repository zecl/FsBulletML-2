namespace FsBulletML2.LanguageService

open System

/// 参照の欠けを、本文の字から全部 数える。Core は最初の 1 件 で止まる。
/// ここは Core より広い。単独では使わない。
module References =

  // --- 走る先は語彙から引く -------------------------------------------------

  /// 参照する要素から、参照される要素と label の属性名。表を持たない。
  /// 規則は `Refs.pairs`。2 か所 に書くと波線と rename がずれる。
  let pairs: (string * string * string)[] =
    Vocabulary.elements
    |> Array.toList
    |> List.map (fun e -> e.Name, (e.Attrs |> Array.toList |> List.map (fun a -> a.Name)))
    |> Refs.pairs
    |> List.toArray

  // --- 定義に無い参照 -------------------------------------------------------

  /// 定義に無い参照を全部。本文に出てくる順。
  /// 数えるところは `Refs.missing`。ここが持つのは文面だけ。
  let missing (scan: string -> TagHit list) (src: string) : Failure list =
    if pairs.Length = 0 then []
    else
      Refs.missing (List.ofArray pairs) (scan src)
      |> List.map (fun m ->
           { Line = m.Hit.Line
             Column = m.Hit.Column
             EndColumn = m.Hit.EndColumn
             Message = sprintf "%s が指す %s が無い: %s" m.RefName m.DefName m.Hit.Value })

  /// Apply の答えを、波線にできる形へ。Core の判定が真。
  /// 位置が無い層だけ本文から数え直す。参照以外はここでは足せない。
  /// 本番と試験で別の組み合わせを組むな。
  let explain (scan: string -> TagHit list) (failure: Failure option) (src: string) : Failure list =
    match failure with
    | None -> []
    | Some f when f.Line > 0 -> [ f ]
    | Some f ->
      match missing scan src with
      | [] -> [ f ]
      | ms -> ms
