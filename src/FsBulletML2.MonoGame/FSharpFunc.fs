namespace FsBulletML2.MonoGame

open System
open System.Runtime.CompilerServices

/// C# と F# の関数を行き来させる変換。呼ばれているぶんだけ置いてある。
///
/// 以前は `Action` / `Func` / `FSharpFunc` の 3 モジュール に 0 から 16 引数
/// までを並べ、Unity2D 側にも namespace 1 行 しか違わない写しが在った。
/// 5 引数 から上は `NET40` という定数で囲ってあり、`net10.0` を建てるのに
/// その名前を明示的に立てて有効にしていた。
///
/// F# から呼ぶときは `CompiledName` ではなく元の名前（`toFSharpFunc2`）
/// になる。C# 側の名前だけで数えると、生きているものを死んでいると読む。
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
