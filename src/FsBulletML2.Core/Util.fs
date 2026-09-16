namespace FsBulletML2

open System
open System.Globalization
open System.IO
open System.Runtime.CompilerServices

[<AutoOpen>]
module internal Util = 
  // BinaryFormatter was removed on modern .NET (throws / does not compile on netstandard2.1).
  // The mutable ProcessableBulletml tree this used to deep-copy is gone (Task 17);
  // BulletState is an immutable record, so spawned bullets no longer need cloning.
  // TODO: do not bring BinaryFormatter back; add a typed clone if another type needs a deep copy.
  open System.Diagnostics
  let dprintfn fmt = Printf.ksprintf Debug.WriteLine fmt

[<RequireQualifiedAccess>]
module internal TryParse =
  /// 式を字のまま評価する口はここに無い。
  ///
  /// 以前は `xpathNumber`（`System.Xml.XPath.XPathDocument` に "number(...)" を
  /// 評価させる）が居て、`eval` / `tryEval` / `parseEval` がそれを包んでいた。
  /// Core が xml に縛られていたのはここを含む 3 か所 —— 落として、
  /// 式は `Expr.NumExpr` の木を評価する 1 本 に寄せた。
  ///
  /// 旧実装は tests/FsBulletML2.Core.Tests/XPathOracle.fs に在る。
  /// 木が同じ値を返すかを突き合わせる相手なので、試験の側にしか要らない。
  let tryParseWith tryParseFunc =
    tryParseFunc >> function
    | true, v    -> Some v
    | false, _   -> None

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
    static member Match<'T>(this:'T option,ifSome: Func<_,_>, ifNone: Func<_,_>) =
      match this with
      | Some x -> ifSome.Invoke(x)
      | None -> ifNone.Invoke()

    [<Extension>]
    static member Action<'T>(this:'T option,ifSome: Action<_>, ifNone: Action<_>) =
      match this with
      | Some x -> ifSome.Invoke(x)
      | None -> ifNone.Invoke()
  