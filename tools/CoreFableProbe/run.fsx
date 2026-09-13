// .NET の側。**同じ `Probe.fs` を読む** —— 写しを書くと、片方 だけ直した形が
// 黙って残る（答えの形も `Probe.fs` の `line` が 1 か所 で持つ）。
//
//   dotnet fsi -r:<Core.dll> -r:<Dsl.dll> run.fsx
//
// dll の場所は門が渡す。`#r` に書かないのは、**Debug と Release で置き場が違う**のと、
// 絶対パスを機械に貼り付けないため（`guard-abs-paths.ps1`）。
#load "Probe.fs"

printfn "%s" (Probe.line ())
