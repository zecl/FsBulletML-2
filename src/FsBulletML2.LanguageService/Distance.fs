/// **綴りの近さ。** 「1 文字 の 挿入・削除・置換 を 1 回 で同じになるか」だけ。
///
/// ## 数を返さない
///
/// 編集距離の表を組めば「2 です」も返せるが、**返すと閾値をどこかに書くことに
/// なる。** 上限が字に出ない形にして、広げたくなったらこの 1 本 を直す。
///
/// 上限を 1 にしたのは測ったから（v1.3 の頭）—— コーパスの参照から 1 文字 落として
/// 打ち間違いを作り、その弾幕の定義と突き合わせた。
///
///     候補が 1 個 の弾幕      26 本  近さは効いていない（分母から抜く）
///     候補が 2 個 以上 の弾幕 23 本
///       距離 1 以内 が 1 個    23 本（100%）
///       距離 1 以内 が 0 個     0 本
///
/// **2 まで広げる理由が測定に無い。** 広げると無関係な名前が「直し方」として出る。
///
/// ## `Fable.Core` に依存しない
///
/// このソースは host（.NET）と ブラウザ側（Fable が焼いた JS）の
/// **両方 で走る。**
module FsBulletML2.LanguageService.Distance

/// 同じ長さで、違う字が 1 つ 以下 か
let private differsByOneChar (a: string) (b: string) =
  let mutable diff = 0
  for i in 0 .. a.Length - 1 do
    if a.[i] <> b.[i] then diff <- diff + 1
  diff <= 1

/// `long` から 1 文字 落とせば `short` になるか
let private differsByOneGap (short: string) (long: string) =
  let mutable i = 0
  let mutable j = 0
  let mutable skipped = false
  let mutable ok = true
  while ok && i < short.Length do
    if j >= long.Length then ok <- false
    elif short.[i] = long.[j] then
      i <- i + 1
      j <- j + 1
    elif skipped then ok <- false
    else
      skipped <- true
      j <- j + 1
  ok

/// 1 文字 の 挿入・削除・置換 を **1 回 まで**で同じになるか。
///
/// 同じ字なら真（距離 0）。**呼ぶ側は「別の名前」しか渡さない**が、
/// ここは「1 以内 か」だけを答える
let within1 (a: string) (b: string) =
  if a = b then true
  else
    match a.Length - b.Length with
    | 0 -> differsByOneChar a b
    | 1 -> differsByOneGap b a
    | -1 -> differsByOneGap a b
    | _ -> false
