namespace FsBulletML2.MonoGame.Tests

/// `FrontRun` の控え。別ファイルにしてある —— 本体（何を回すか）と
/// 控え（何が出たか）を分けておくと、差分を見たときに
/// 「手を入れたのか、出力が動いたのか」が 1 目 で分かる。
module FrontRunGolden =

  // 改行を揃えてから渡す。 控えは三重引用符でこのソースに埋まっているので、
  // 改行はファイルの改行そのもの —— `.gitattributes` の `text=auto` で
  // checkout した機械しだいで CRLF になる。生成側は `\n` に揃えてあるので、
  // 揃えないと全行 が字面どおりなのに割れる（差が `\r` なので目で出ない）。
  // 落ちるのは autocrlf が効く機械だけで、書いた人の手元は緑のまま
  let private normalize (s: string) = s.Replace("\r\n", "\n")
  let Expected = normalize """f00  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.4337 y=101.9524 d=2.9230 s=2.0000 used=true]  PB[]
f01  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.7241 y=103.9312 d=2.9959 s=2.0000 used=true]  PB[]
f02  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=105.9259 d=3.0687 s=2.0000 used=true]  PB[]
f03  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=107.9259 d=3.1416 s=2.0000 used=true | b1 x=240.4337 y=101.9524 d=2.9230 s=2.0000 used=true]  PB[]
f04  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=109.9259 d=3.1416 s=2.0000 used=true | b1 x=240.7241 y=103.9312 d=2.9959 s=2.0000 used=true]  PB[]
f05  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=111.9259 d=3.1416 s=2.0000 used=true | b1 x=240.8697 y=105.9259 d=3.0687 s=2.0000 used=true]  PB[]
f06  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=107.9259 d=3.1416 s=2.0000 used=true]  PB[]
f07  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=109.9259 d=3.1416 s=2.0000 used=true | b1 x=240.4337 y=101.9524 d=2.9230 s=2.0000 used=true]  PB[]
f08  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=111.9259 d=3.1416 s=2.0000 used=true | b1 x=240.7241 y=103.9312 d=2.9959 s=2.0000 used=true]  PB[]
f09  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=105.9259 d=3.0687 s=2.0000 used=true]  PB[]
f10  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=107.9259 d=3.1416 s=2.0000 used=true | b1 x=240.4337 y=101.9524 d=2.9230 s=2.0000 used=true]  PB[]
f11  E[e0 x=240.0000 y=100.0000 d=0.0000 s=0.0000 used=true]  EB[b0 x=240.8697 y=109.9259 d=3.1416 s=2.0000 used=true | b1 x=240.7241 y=103.9312 d=2.9959 s=2.0000 used=true]  PB[]
"""

  /// 撃った直後の姿。この文字列が何回 出るかが、撃った回数。
  /// repeat は 2 周 なので、3 回 以上 出ていれば走らせ直しが通っている
  [<Literal>]
  let Birth = "x=240.4337 y=101.9524 d=2.9230 s=2.0000"
