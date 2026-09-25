module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// 版・題・説明・会社・著作権は SDK が Directory.Build.props から作る。
// ここに書くと二重になって落ちる

[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("2EBAF051-E207-4186-A395-FA81F4A27600")>]

[<assembly: InternalsVisibleTo("FsBulletML2.Parser")>]
[<assembly: InternalsVisibleTo("FsBulletML2.Parser.Tests")>]
[<assembly: InternalsVisibleTo("FsBulletML2.Core.Tests")>]

do ()
