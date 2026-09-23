// .NET の側。同じ `Probe.fs` を読む。写しを書くと片方だけ直した形が残る。
// dll は門が渡す。`#r` に書かない。置き場が構成で違い、絶対パスも貼らない。
#load "Probe.fs"

printfn "%s" (Probe.line ())
