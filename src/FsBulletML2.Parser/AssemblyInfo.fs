module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// 版・題・説明・会社・著作権は SDK が Directory.Build.props から作る。
// ここに書くと二重になって落ちる

[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("88DCD0C9-CEEE-428B-9AB9-8FD72B774275")>]

[<assembly: InternalsVisibleTo("FsBulletML2.Parser.Tests")>]

do ()
