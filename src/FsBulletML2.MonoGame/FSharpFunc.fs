namespace FsBulletML2.MonoGame

open System
open System.Runtime.CompilerServices

/// C# と F# の関数を行き来させる変換。呼ばれているぶんだけ置いてある。
/// F# から呼ぶときは `CompiledName` ではなく元の名前になる。
[<Extension; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Action =

    /// C# のサンプルが `Manager.CheckPlayerCollision` へ渡すのに使う
    [<Extension; CompiledName "ToFSharpFunc">]
    let toFSharpFunc0 (f: Action) = f.Invoke

    /// `BaseBullet.RunTask` が、受け取った `Action` を F# 関数に戻すのに使う
    [<Extension; CompiledName "ToFSharpFunc">]
    let toFSharpFunc2 (f: Action<'T1, 'T2>) = fun t1 t2 -> f.Invoke(t1, t2)

[<Extension; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FSharpFunc =

    /// `BaseBullet.Update` が、自分の `RunTask` を呼ぶのに使う。
    /// `RunTask` は C# から呼べるように `Action` を取るので、
    /// F# の中では包んですぐ戻すことになる
    [<Extension; CompiledName "ToAction">]
    let ToAction2 (f: 'T1 -> 'T2 -> unit) = Action<'T1, 'T2>(f)
