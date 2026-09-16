namespace FsBulletML2.Unity2D

open System
open System.Runtime.CompilerServices

/// C# と F# の関数を行き来させる変換。呼ばれているぶんだけ置いてある。
/// MonoGame 側の同名と対だが、同じ本数ではない ——
/// 0 引数 版はあちらの C# サンプルだけが使う。
[<Extension; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Action =

  /// `DefaultBullet.RunTask` が、受け取った `Action` を F# 関数に戻すのに使う
  [<Extension; CompiledName "ToFSharpFunc">]
  let toFSharpFunc2 (f: Action<'T1, 'T2>) = fun t1 t2 -> f.Invoke(t1, t2)

[<Extension; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FSharpFunc =

  /// `DefaultBullet.Update` が、自分の `RunTask` を呼ぶのに使う
  [<Extension; CompiledName "ToAction">]
  let ToAction2 (f: 'T1 -> 'T2 -> unit) = Action<'T1, 'T2>(f)
