namespace FsBulletML2

open System
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection 

[<AutoOpen>]
module DTD =
  /// DU を「型名.腕名 中身」の字にする。`ToString` の override 専用 ——
  /// 走行も trace も通らない。
  ///
  /// `inline` なのも `typeof<'T>` なのも Fable の都合（外すと Core ごと焼けなくなる）
  let inline stringifyFullName (discriminatedUnion:'T) =
    if box discriminatedUnion = null  then
      nullArg  "discriminatedUnion"
    if FSharpType.IsUnion(typeof<'T>)|> not then
      invalidArg "discriminatedUnion" (sprintf "not DU:%s" typeof<'T>.FullName)

    let info, objects = FSharpValue.GetUnionFields(discriminatedUnion, typeof<'T>)
    let duType = typeof<'T>
    let typeName =
      if duType.IsGenericType then
        duType.Name.Substring(0, duType.Name.LastIndexOf("`"))  + "." + info.Name
      else
        duType.Name + "." + info.Name
    match objects  with
    | [||] -> typeName
    | elements ->
      let fields = info.GetFields()
      if fields.Length = 1 then
        sprintf "%s %A" typeName elements.[0]
      else
        let tupleType = 
          fields
          |> Array.map( fun pi -> pi.PropertyType )
          |> FSharpType.MakeTupleType
        let tuple = FSharpValue.MakeTuple(elements, tupleType)
        sprintf "%s %A" typeName tuple

  /// 文字列を数値式として読む。DU へ入れる入口はここ 1 本。
  ///
  /// 走行中に読み直さない —— 読むのはこの入口を通る 1 回だけ。
  let numExpr (s: string) : Expr.NumExpr = Expr.NumExpr.ofString s

  /// 属性値の並びのうち、その属性を書かなかったときに走る腕に付ける。
  ///
  /// 置き場が型しか無い（AST には何も残らないので、字のコメントで書くと
  /// どこからも引けず、実装がずれても赤くならない）。
  ///
  /// 1 つ の並びに付くのは 0 個 か 1 個。読むのは
  /// `UnionCaseInfo.GetCustomAttributes`。
  [<AttributeUsage(AttributeTargets.All, AllowMultiple = false)>]
  type BulletmlDefaultAttribute() =
    inherit Attribute()

  /// BulletML DTD
  /// <!ELEMENT vertical (#PCDATA)>
  /// <!ATTLIST vertical type (absolute|relative|sequence) "absolute">
  type Vertical =
  | Vertical of VerticalAttrs option * Expr.NumExpr 
  and VerticalAttrs = { verticalType : VerticalType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]VerticalType =
  | [<BulletmlDefault>] Absolute
  | Relative
  | Sequence
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  /// BulletML DTD
  /// <!ELEMENT param (#PCDATA)>  
  type Params = string list

  module Param =
    /// List to Params
    let ofList source =
      source |> List.mapi (fun i x -> ("$" + (i + 1).ToString(), x)) |> Map.ofList 

    let replace s param =
      let result = ref s
      let f x = 
        let x = "$" + string x
        match Map.tryFind x (param:Map<string, string>) with
        | Some v ->
          result := Regex.Replace(!result, "\\" + x , v)
        | None -> new BulletmlDTDViolationException(sprintf "not found parameter:[%s]" x) |> raise
      List.iter f [1..param.Count]
      !result

    /// 実引数を式へ入れる。文字で置き換えてから読み直す。
    ///
    /// 木の節として差し込むと優先順位が変わる（`"1+2"` を `"$1*3"` へ入れると
    /// 文字なら 7、節なら 9）。同梱 227 本 のうち 50 本 が当たる。
    let replaceIn (param: Map<string, string>) (e: Expr.NumExpr) : Expr.NumExpr =
      Expr.NumExpr.mapSource (fun t -> replace t param) e

  /// BulletML DTD
  /// <!ELEMENT speed (#PCDATA)>
  /// <!ATTLIST speed type (absolute|relative|sequence) "absolute">
  type Speed =
  | Speed of SpeedAttrs option * Expr.NumExpr
  and SpeedAttrs = { speedType : SpeedType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]SpeedType =
  | [<BulletmlDefault>] Absolute
  | Relative
  | Sequence
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  /// BulletML DTD
  /// <!ELEMENT direction (#PCDATA)>
  /// <!ATTLIST direction type (aim|absolute|relative|sequence) "aim">
  type Direction = 
  | Direction of DirectionAttrs option * Expr.NumExpr
  and DirectionAttrs = { directionType : DirectionType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]DirectionType =
  | [<BulletmlDefault>] Aim
  | Absolute
  | Relative
  | Sequence
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  /// BulletML DTD
  /// <!ELEMENT term (#PCDATA)>
  type Term = Term of Expr.NumExpr

  /// BulletML DTD
  /// <!ELEMENT times (#PCDATA)>
  type Times = Times of Expr.NumExpr

  /// BulletML DTD
  /// <!ELEMENT horizontal (#PCDATA)>
  /// <!ATTLIST horizontal type (absolute|relative|sequence) "absolute">
  type Horizontal = 
  | Horizontal of HorizontalAttrs option * Expr.NumExpr
  and HorizontalAttrs = { horizontalType : HorizontalType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]HorizontalType =
  | [<BulletmlDefault>] Absolute
  | Relative
  | Sequence
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  type BulletmlAttrs = { bulletmlXmlns : string option; bulletmlType : ShootingDirection option; bulletmlName : string option; bulletmlDescription : string option }
  /// 省いたときに走るのは vertical。 属性が無いとき Api が
  /// BulletVertical を返す。
  ///
  /// `[<BulletmlDefault>]` が指すのは「省いたときに走る値」で、
  /// DTD の既定値（`"none"`）ではない —— `Parser.Tests/AttributeDefaults.fs` が
  /// 走りで固定している。札を動かすとあの点が赤くなる。
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]ShootingDirection =
  | BulletNone
  | [<BulletmlDefault>] BulletVertical
  | BulletHorizontal
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t

  type BulletType =
    | Enemy
    | Player

  /// 要素の名前。定義する側と参照する側で同じ型にしてある。
  ///
  ///   <action label="top">  と  <actionRef label="top">  は同じ ActionLabel
  ///
  /// 「fire の名前で action を探す」が型で組めなくなる。
  /// 3 つ を 1 つ の型に `kind` フィールドで畳まないのも同じ筋 ——
  /// 畳むと種別違いが実行時にしか分からない。
  type ActionLabel = ActionLabel of string
  type FireLabel = FireLabel of string
  type BulletLabel = BulletLabel of string

  module ActionLabel =
    let text (ActionLabel s) = s
  module FireLabel =
    let text (FireLabel s) = s
  module BulletLabel =
    let text (BulletLabel s) = s

  /// 展開中の参照。種別と名前の組。
  ///
  /// 型にすると、種別を書き忘れて別の種別と衝突する形が組めない。
  /// F# の DU は構造で比較・整列できるので `Set` にそのまま入る。
  type RefKey =
    | ActionKey of ActionLabel
    | FireKey of FireLabel
    | BulletKey of BulletLabel

  module RefKey =
    /// 人へ見せる形。`"action:top"` の書き方は変えない
    /// （輪を見つけたときの例外文がこの形で出ていた）
    let text = function
      | ActionKey l -> "action:" + ActionLabel.text l
      | FireKey l -> "fire:" + FireLabel.text l
      | BulletKey l -> "bullet:" + BulletLabel.text l

  /// BulletML DTD
  /// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
  /// <!ATTLIST action label CDATA #IMPLIED>
  type ActionAttrs = { actionLabel : ActionLabel option }
  /// BulletML DTD
  /// <!ELEMENT actionRef (param*)>
  /// <!ATTLIST actionRef label CDATA #REQUIRED>
  type ActionRefAttrs = { actionRefLabel : ActionLabel }
  /// BulletML DTD
  /// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
  /// <!ATTLIST fire label CDATA #IMPLIED>
  type FireAttrs = { fireLabel : FireLabel option }
  /// BulletML DTD
  /// <!ELEMENT fireRef (param*)>
  /// <!ATTLIST fireRef label CDATA #REQUIRED>
  type FireRefAttrs = { fireRefLabel : FireLabel }
  /// BulletML DTD
  /// <!ELEMENT bullet (direction?, speed?, (action | actionRef)*)>
  /// <!ATTLIST bullet label CDATA #IMPLIED>
  type BulletAttrs = { bulletLabel : BulletLabel option }
  /// BulletML DTD
  /// <!ELEMENT bulletRef (param*)>
  /// <!ATTLIST bulletRef label CDATA #REQUIRED>
  type BulletRefAttrs = { bulletRefLabel : BulletLabel }

  /// Innternal DSL
  ///
  /// 根。腕は bulletml 1 つ だけ。 子は位置ごとの型（BulletmlElm /
  /// Action / ActionElm / BulletElm、下）に分かれている。
  ///
  /// エンジンが歩くのもこの木（`Rec*` という別の 5 つ は畳んだ）。
  [<StructuredFormatDisplay("{ToStructuredDisplay}")>]
  type Bulletml =
/// BulletML DTD
/// <!ELEMENT bulletml (bullet | fire | action)*>
/// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
/// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
  | Bulletml of BulletmlAttrs * BulletmlElm list
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t
    member this.ToNodeString() =
      this.ToString().Replace("null","None")
    member this.Type
        with get() =
            match this with
            | Bulletml (x,_) -> x.bulletmlType
    member this.Name
        with get() =
            match this with
            | Bulletml (x,_) -> x.bulletmlName
    /// description は BulletML 公式の属性ではない。 名前 / 説明文を入れるために足した
    member this.Description
        with get() =
            match this with
            | Bulletml (x,_) -> x.bulletmlDescription

  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]BulletmlElm =
  | Bullet of BulletAttrs * Direction option * Speed option * ActionElm list 
  | Fire of FireAttrs * Direction option * Speed option * BulletElm 
  | Action of ActionAttrs * Action list 
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]Action =
  /// <!ELEMENT changeDirection (direction, term)>
  | ChangeDirection of Direction * Term
  /// <!ELEMENT accel (horizontal?, vertical?, term)>
  | Accel of Horizontal option * Vertical option * Term
  /// <!ELEMENT vanish (#PCDATA)>
  | Vanish
  /// <!ELEMENT changeSpeed (speed, term)>
  | ChangeSpeed of Speed * Term
  /// <!ELEMENT repeat (times, (action | actionRef))>
  | Repeat of Times * ActionElm
  /// <!ELEMENT wait (#PCDATA)>
  | Wait of Expr.NumExpr
  | Fire of FireAttrs * Direction option * Speed option * BulletElm 
  | FireRef of FireRefAttrs * Params
  | Action of ActionAttrs * Action list 
  | ActionRef of ActionRefAttrs * Params
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]BulletElm =
  | Bullet of BulletAttrs * Direction option * Speed option * ActionElm list 
  | BulletRef of BulletRefAttrs * Params
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]ActionElm =
  | Action of ActionAttrs * Action list 
  | ActionRef of ActionRefAttrs * Params
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t

  /// 弾幕を字にするときの受け口。
  ///
  /// 木を歩くのは 1 本（`BulletmlWriter.writeTo`）で、歩きは表記を知らない。
  ///
  /// その表記で書ける字かどうかは、受け口が知っている —— だから受け口は
  /// その文法を読むパーサと同じところに置く（片方 だけ直すのを防ぐ）。
  ///
  /// 深さは受け口が自分で数える（fsb の字下げに要る）。
  type IBulletmlSink =
    /// 要素を開く
    abstract Start: name: string -> unit
    /// 属性 1 つ。開いた直後にだけ来る
    abstract Attr: name: string * value: string -> unit
    /// 本文（#PCDATA）
    abstract Text: value: string -> unit
    /// いま開いている要素を閉じる
    abstract End: unit -> unit
  /// BulletML を XML に書き戻す。member ではなく関数
  /// （member だと Parser の `Bulletml.ToXmlString` と名前がぶつかって自分を呼ぶ）。
  module internal BulletmlXml =
    /// BulletML 書き込み
    let writeContentTo (sink: IBulletmlSink) (this: Bulletml) =
      // 各腕の中身は位置ごとに分ける前と同じ順で書く
      // （往復の試験が順序まで見ている）
      let writeDirection (d: Direction) =
        sink.Start("direction")
        match d with
        | Direction (attrs, s) ->
          match attrs with
          | Some attrs ->
            let t =
              attrs.directionType |> function
              | DirectionType.Aim      -> "aim"
              | DirectionType.Absolute -> "absolute"
              | DirectionType.Relative -> "relative"
              | DirectionType.Sequence -> "sequence"
            sink.Attr("type", t)
          | None -> ()
          sink.Text(Expr.NumExpr.text s)
        sink.End()

      let writeSpeed (sp: Speed) =
        sink.Start("speed")
        match sp with
        | Speed (attrs, s) ->
          match attrs with
          | Some attrs ->
            let t =
              attrs.speedType |> function
              | SpeedType.Absolute -> "absolute"
              | SpeedType.Relative -> "relative"
              | SpeedType.Sequence -> "sequence"
            sink.Attr("type", t)
          | None -> ()
          sink.Text(Expr.NumExpr.text s)
        sink.End()

      let writeTerm (Term s) =
        sink.Start("term")
        sink.Text(Expr.NumExpr.text s)
        sink.End()

      let writeParams (prams: Params) =
        prams |> Seq.iter (fun s ->
          sink.Start("param")
          sink.Text(s)
          sink.End())

      let writeBulletBody (attrs: BulletAttrs) direction speed writeChildren =
        sink.Start("bullet")
        match attrs.bulletLabel with
        | Some v -> sink.Attr("label", BulletLabel.text v)
        | None -> ()
        direction |> Option.iter writeDirection
        speed |> Option.iter writeSpeed
        writeChildren ()
        sink.End()

      let writeFireBody (attrs: FireAttrs) direction speed writeChild =
        sink.Start("fire")
        match attrs.fireLabel with
        | Some v -> sink.Attr("label", FireLabel.text v)
        | None -> ()
        direction |> Option.iter writeDirection
        speed |> Option.iter writeSpeed
        writeChild ()
        sink.End()

      let writeActionBody (attrs: ActionAttrs) writeChildren =
        sink.Start("action")
        match attrs.actionLabel with
        | Some v -> sink.Attr("label", ActionLabel.text v)
        | None -> ()
        writeChildren ()
        sink.End()

      let rec writeCommand (c: Action) =
        match c with
        | Action.ChangeDirection (direction, term) ->
          sink.Start("changeDirection")
          writeDirection direction
          writeTerm term
          sink.End()
        | Action.ChangeSpeed (speed, term) ->
          sink.Start("changeSpeed")
          writeSpeed speed
          writeTerm term
          sink.End()
        | Action.Accel (horizontal, vertical, term) ->
          sink.Start("accel")
          match horizontal with
          | Some (Horizontal.Horizontal(attrs, s)) ->
            sink.Start("horizontal")
            match attrs with
            | Some attrs ->
              let t =
                attrs.horizontalType |> function
                | HorizontalType.Absolute -> "absolute"
                | HorizontalType.Relative -> "relative"
                | HorizontalType.Sequence -> "sequence"
              sink.Attr("type", t)
            | None -> ()
            sink.Text(Expr.NumExpr.text s)
            sink.End()
          | _ -> ()
          match vertical with
          | Some (Vertical.Vertical(attrs, s)) ->
            sink.Start("vertical")
            match attrs with
            | Some attrs ->
              let t =
                attrs.verticalType |> function
                | VerticalType.Absolute -> "absolute"
                | VerticalType.Relative -> "relative"
                | VerticalType.Sequence -> "sequence"
              sink.Attr("type", t)
            | None -> ()
            sink.Text(Expr.NumExpr.text s)
            sink.End()
          | _ -> ()
          writeTerm term
          sink.End()
        | Action.Wait s ->
          sink.Start("wait")
          sink.Text(Expr.NumExpr.text s)
          sink.End()
        | Action.Vanish ->
          sink.Start("vanish")
          sink.End()
        | Action.Repeat (times, child) ->
          sink.Start("repeat")
          match times with
          | Times s ->
            sink.Start("times")
            sink.Text(Expr.NumExpr.text s)
            sink.End()
          writeActionElm child
          sink.End()
        | Action.Fire (attrs, direction, speed, child) ->
          writeFireBody attrs direction speed (fun () -> writeBulletElm child)
        | Action.FireRef (attrs, prams) ->
          sink.Start("fireRef")
          sink.Attr("label", FireLabel.text attrs.fireRefLabel)
          writeParams prams
          sink.End()
        | Action.Action (attrs, children) ->
          writeActionBody attrs (fun () -> children |> Seq.iter writeCommand)
        | Action.ActionRef (attrs, prams) ->
          sink.Start("actionRef")
          sink.Attr("label", ActionLabel.text attrs.actionRefLabel)
          writeParams prams
          sink.End()

      and writeActionElm (a: ActionElm) =
        match a with
        | ActionElm.Action (attrs, children) ->
          writeActionBody attrs (fun () -> children |> Seq.iter writeCommand)
        | ActionElm.ActionRef (attrs, prams) ->
          sink.Start("actionRef")
          sink.Attr("label", ActionLabel.text attrs.actionRefLabel)
          writeParams prams
          sink.End()

      and writeBulletElm (b: BulletElm) =
        match b with
        | BulletElm.Bullet (attrs, direction, speed, children) ->
          writeBulletBody attrs direction speed (fun () -> children |> Seq.iter writeActionElm)
        | BulletElm.BulletRef (attrs, prams) ->
          sink.Start("bulletRef")
          sink.Attr("label", BulletLabel.text attrs.bulletRefLabel)
          writeParams prams
          sink.End()

      let writeTopElm (t: BulletmlElm) =
        match t with
        | BulletmlElm.Bullet (attrs, direction, speed, children) ->
          writeBulletBody attrs direction speed (fun () -> children |> Seq.iter writeActionElm)
        | BulletmlElm.Fire (attrs, direction, speed, child) ->
          writeFireBody attrs direction speed (fun () -> writeBulletElm child)
        | BulletmlElm.Action (attrs, children) ->
          writeActionBody attrs (fun () -> children |> Seq.iter writeCommand)

      match this with
      | Bulletml.Bulletml (attrs, children) ->
        sink.Start("bulletml")
        match attrs.bulletmlXmlns with
        | Some v -> sink.Attr("xmlns", v)
        | None -> ()

        match attrs.bulletmlType with
        | Some typeName ->
          let t =
            typeName |> function
            | ShootingDirection.BulletNone -> "none"
            | ShootingDirection.BulletHorizontal -> "horizontal"
            | ShootingDirection.BulletVertical   -> "vertical"
          sink.Attr("type", t)
        | None -> ()

        // parser が読む属性は writer も書く（書かないと往復で消える）
        match attrs.bulletmlName with
        | Some v -> sink.Attr("name", v)
        | None -> ()

        match attrs.bulletmlDescription with
        | Some v -> sink.Attr("description", v)
        | None -> ()

        children |> Seq.iter writeTopElm
        sink.End()

  // XML の受け口（`XmlSink`）はここに無い（`FsBulletML2.Parser` の `XmlWrite.fs`）。
  // 歩き（`writeContentTo`）だけがここに残る —— 表記を知らないから。
