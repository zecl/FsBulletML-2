namespace FsBulletML2

open System
open System.Runtime.CompilerServices

// BinaryFormatter は戻すな。深い複製が要る型が出たら、型の付いた clone を足す。

[<AutoOpen>]
module Monad =
    type MaybeBuilder() =
        member b.Bind(m, f) = Option.bind f m
        member b.Return(a) = Some a
        member b.ReturnFrom(m) = m
        member b.Zero() = None

    let maybe = new MaybeBuilder()

    [<Extension>]
    type OptionExtentions =
        [<Extension>]
        static member Match<'T>(this: 'T option, ifSome: Func<_, _>, ifNone: Func<_, _>) =
            match this with
            | Some x -> ifSome.Invoke(x)
            | None -> ifNone.Invoke()

        [<Extension>]
        static member Action<'T>(this: 'T option, ifSome: Action<_>, ifNone: Action<_>) =
            match this with
            | Some x -> ifSome.Invoke(x)
            | None -> ifNone.Invoke()
