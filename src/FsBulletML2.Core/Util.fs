namespace FsBulletML2

open System
open System.Globalization
open System.IO
open System.Runtime.CompilerServices

// BinaryFormatter は戻すな。深い複製が要る型が出たら、型の付いた clone を足す。

[<RequireQualifiedAccess>]
module internal TryParse =
    let tryParseWith tryParseFunc =
        tryParseFunc
        >> function
            | true, v -> Some v
            | false, _ -> None

    let parseDate = tryParseWith (fun (s: string) -> System.DateTime.TryParse(s))
    let parseInt32 = tryParseWith (fun (s: string) -> System.Int32.TryParse(s))
    let parseInt64 = tryParseWith (fun (s: string) -> System.Int64.TryParse(s))
    let parseSingle = tryParseWith (fun (s: string) -> System.Single.TryParse(s))
    let parseDouble = tryParseWith (fun (s: string) -> System.Double.TryParse(s))
    let parseDecimal = tryParseWith (fun (s: string) -> System.Decimal.TryParse(s))

[<AutoOpen>]
module TryParseActivePattern =
    let (|Date|_|) = TryParse.parseDate
    let (|Int32|_|) = TryParse.parseInt32
    let (|Int64|_|) = TryParse.parseInt64
    let (|Single|_|) = TryParse.parseSingle
    let (|Double|_|) = TryParse.parseDouble
    let (|Decimal|_|) = TryParse.parseDecimal

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
