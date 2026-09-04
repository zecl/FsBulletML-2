namespace FsBulletML2

open System
open System.IO 
open System.Text 
open System.Xml
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection 

[<AutoOpen>]
module DTD =
  let stringifyFullName (discriminatedUnion:'T) = 
    if box discriminatedUnion = null  then
      nullArg  "discriminatedUnion"  
    if FSharpType.IsUnion(typeof<'T>)|> not then
      invalidArg "discriminatedUnion" (sprintf "not DU:%s" typeof<'T>.FullName)
 
    let info, objects = FSharpValue.GetUnionFields(discriminatedUnion, typeof<'T>)
    let typeName = 
      if info.DeclaringType.IsGenericType then
        info.DeclaringType.Name.Substring(0, info.DeclaringType.Name.LastIndexOf("`"))  + "." + info.Name
      else
        info.DeclaringType.Name + "." + info.Name
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

  /// 文字列を数値式として読む。XML の #PCDATA や内部 DSL で書いた文字列を
  /// DU へ入れる入口はここ 1 本 にする。
  ///
  /// 木は Expr.NumExpr が持つ（もとの文字列と、読んだ木と、$rand / $rank を
  /// 使うかの旗）。走行中に読み直さないので、読むのはこの入口を通る 1 回だけ。
  /// 実引数の置き換えだけは文字でやる必要があるので、そこは
  /// Expr.NumExpr.mapSource が「置き換えてから読み直す」形で通る
  let numExpr (s: string) : Expr.NumExpr = Expr.NumExpr.ofString s

  /// BulletML DTD
  /// <!ELEMENT vertical (#PCDATA)>
  /// <!ATTLIST vertical type (absolute|relative|sequence) "absolute">
  type Vertical =
  | Vertical of VerticalAttrs option * Expr.NumExpr 
  and VerticalAttrs = { verticalType : VerticalType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]VerticalType = 
  | Absolute 
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

    /// 実引数を式へ入れる。**文字で置き換えてから読み直す。**
    ///
    /// 木の節として差し込むと優先順位が変わる。実引数 "1+2" を仮引数
    /// "$1*3" へ入れると、文字なら 1+2*3 = 7、節なら (1+2)*3 = 9 になる。
    /// 同梱 227 本 のうち 50 本 が、トップレベルに二項の + / - を持つ
    /// 実引数を含んでいるので、ここを節に変えるとその 50 本 の軌跡が動く
    let replaceIn (param: Map<string, string>) (e: Expr.NumExpr) : Expr.NumExpr =
      Expr.NumExpr.mapSource (fun t -> replace t param) e

  /// BulletML DTD
  /// <!ELEMENT speed (#PCDATA)>
  /// <!ATTLIST speed type (absolute|relative|sequence) "absolute">
  type Speed =
  | Speed of SpeedAttrs option * Expr.NumExpr
  and SpeedAttrs = { speedType : SpeedType }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]SpeedType = 
  | Absolute 
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
  | Aim | Absolute | Relative | Sequence
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
  | Absolute // Default
  | Relative
  | Sequence
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 

  type BulletmlAttrs = { bulletmlXmlns : string option; bulletmlType : ShootingDirection option; bulletmlName : string option; bulletmlDescription : string option }
  and [<StructuredFormatDisplay("{ToStructuredDisplay}")>]ShootingDirection =
  | BulletNone // Default
  | BulletVertical
  | BulletHorizontal
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t

  type BulletType =
    | Enemy
    | Player

  /// 要素の名前。**定義する側と参照する側で同じ型にしてある。**
  ///
  ///   <action label="top">  と  <actionRef label="top">  は同じ ActionLabel
  ///
  /// actionRef が指す先は action なので、同じ型で持つのが自然で、
  /// 「fire の名前で action を探す」が型で組めなくなる。以前は
  /// どれも素の string で、種別は refKey が "action:" のような接頭辞を
  /// 文字で足して区別していた（IntermediateParser）。その足し算が要らなくなる。
  ///
  /// 3 つを 1 つの型に kind フィールドで畳まないのは、畳むと
  /// 「kind が違うものを渡した」が実行時にしか分からなくなるため
  type ActionLabel = ActionLabel of string
  type FireLabel = FireLabel of string
  type BulletLabel = BulletLabel of string

  module ActionLabel =
    let text (ActionLabel s) = s
  module FireLabel =
    let text (FireLabel s) = s
  module BulletLabel =
    let text (BulletLabel s) = s

  /// 展開中の参照。種別と名前の組で、以前は refKey が
  /// "action:" + label のように文字で作っていたもの。
  ///
  /// 型にすると、種別を書き忘れて別の種別と衝突する形が組めない。
  /// F# の DU は構造で比較・整列できるので Set にそのまま入る
  type RefKey =
    | ActionKey of ActionLabel
    | FireKey of FireLabel
    | BulletKey of BulletLabel

  module RefKey =
    /// 人へ見せる形。以前 refKey が作っていた "action:top" と同じ書き方に
    /// してある（輪を見つけたときの例外文がこの形で出ていた）
    let text = function
      | ActionKey l -> "action:" + ActionLabel.text l
      | FireKey l -> "fire:" + FireLabel.text l
      | BulletKey l -> "bullet:" + BulletLabel.text l

  type ActionAttrs = { actionLabel : ActionLabel option }
  type ActionRefAttrs = { actionRefLabel : ActionLabel }
  type FireAttrs = { fireLabel : FireLabel option }
  type FireRefAttrs = { fireRefLabel : FireLabel }
  type BulletAttrs = { bulletLabel : BulletLabel option }
  type BulletRefAttrs = { bulletRefLabel : BulletLabel }

  /// Innternal DSL
  ///
  /// **根。腕は bulletml 1 つ だけ。** 子は位置ごとの型（BulletmlElm /
  /// Action / ActionElm / BulletElm、下）に分かれている。
  ///
  /// **エンジンが歩くのもこの木。** 以前は Rec* という別の 5 つ が並んで
  /// いたが、腕まで同じだったので畳んだ（IntermediateParser の但し書き）。
  ///
  /// 以前はここに 13 腕 あった —— action / wait / fire / repeat …と、
  /// **根になれないものまで根の型に並んでいた。** XML を読む段が、どの位置の
  /// 子もいったんこの平らな型で返し、親が自分の位置の型へ入れ直していたため。
  /// 読む段を位置ごとに割ったので、誰も作らなくなった。
  ///
  /// **根が 1 腕 になると、`| _ -> raise` が 2 か所 消える** ——
  /// Type / Name / Description の `| _ -> None` と、
  /// foldConstants の「木の根は bulletml でなければならない」。
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
    /// description は BulletML公式の属性ではない。BulletMLの名前/説明文を格納するための属性として追加した。
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
  | ChangeDirection of Direction * Term
  | Accel of Horizontal option * Vertical option * Term
  | Vanish 
  | ChangeSpeed of Speed * Term
  | Repeat of Times * ActionElm 
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

  /// BulletML を XML に書き戻す。
  ///
  /// **以前は走らせる木の型の member だった。** その型が公開の Bulletml と
  /// 同じになったので member のままだと、Parser の Bulletml.ToXmlString（定数を
  /// 畳んでから書く口）と名前がぶつかって自分を呼ぶ。関数に出した。
  ///
  /// ToString の override も落とした。**公開の Bulletml.ToString は
  /// DU の中身を出す**（stringifyFullName）ので、XML を出す override が
  /// 同じ型に 2 つ 付くことになる。XML が要る側は toXmlString を呼ぶ。
  module internal BulletmlXml =
    /// BulletML 書き込み
    let writeContentTo (writer: XmlWriter) (this: Bulletml) =
      // 型が位置ごとに分かれたので、走査も位置ごとに分ける。
      // 各腕の中身は分ける前と同じ順で書く（往復の試験が順序まで見ている）。
      //
      // direction / speed / term / param は 4 か所 ずつ同じものを書いていたので
      // 関数に出した。出す順は変えていない
      let writeDirection (d: Direction) =
        writer.WriteStartElement("direction")
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
            writer.WriteAttributeString("type", t)
          | None -> ()
          writer.WriteString(Expr.NumExpr.text s)
        writer.WriteEndElement()

      let writeSpeed (sp: Speed) =
        writer.WriteStartElement("speed")
        match sp with
        | Speed (attrs, s) ->
          match attrs with
          | Some attrs ->
            let t =
              attrs.speedType |> function
              | SpeedType.Absolute -> "absolute"
              | SpeedType.Relative -> "relative"
              | SpeedType.Sequence -> "sequence"
            writer.WriteAttributeString("type", t)
          | None -> ()
          writer.WriteString(Expr.NumExpr.text s)
        writer.WriteEndElement()

      let writeTerm (Term s) =
        writer.WriteStartElement("term")
        writer.WriteString(Expr.NumExpr.text s)
        writer.WriteEndElement()

      let writeParams (prams: Params) =
        prams |> Seq.iter (fun s ->
          writer.WriteStartElement("param")
          writer.WriteString(s)
          writer.WriteEndElement())

      let writeBulletBody (attrs: BulletAttrs) direction speed writeChildren =
        writer.WriteStartElement("bullet")
        match attrs.bulletLabel with
        | Some v -> writer.WriteAttributeString("label", BulletLabel.text v)
        | None -> ()
        direction |> Option.iter writeDirection
        speed |> Option.iter writeSpeed
        writeChildren ()
        writer.WriteEndElement()

      let writeFireBody (attrs: FireAttrs) direction speed writeChild =
        writer.WriteStartElement("fire")
        match attrs.fireLabel with
        | Some v -> writer.WriteAttributeString("label", FireLabel.text v)
        | None -> ()
        direction |> Option.iter writeDirection
        speed |> Option.iter writeSpeed
        writeChild ()
        writer.WriteEndElement()

      let writeActionBody (attrs: ActionAttrs) writeChildren =
        writer.WriteStartElement("action")
        match attrs.actionLabel with
        | Some v -> writer.WriteAttributeString("label", ActionLabel.text v)
        | None -> ()
        writeChildren ()
        writer.WriteEndElement()

      let rec writeCommand (c: Action) =
        match c with
        | Action.ChangeDirection (direction, term) ->
          writer.WriteStartElement("changeDirection")
          writeDirection direction
          writeTerm term
          writer.WriteEndElement()
        | Action.ChangeSpeed (speed, term) ->
          writer.WriteStartElement("changeSpeed")
          writeSpeed speed
          writeTerm term
          writer.WriteEndElement()
        | Action.Accel (horizontal, vertical, term) ->
          writer.WriteStartElement("accel")
          match horizontal with
          | Some (Horizontal.Horizontal(attrs, s)) ->
            writer.WriteStartElement("horizontal")
            match attrs with
            | Some attrs ->
              let t =
                attrs.horizontalType |> function
                | HorizontalType.Absolute -> "absolute"
                | HorizontalType.Relative -> "relative"
                | HorizontalType.Sequence -> "sequence"
              writer.WriteAttributeString("type", t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | _ -> ()
          match vertical with
          | Some (Vertical.Vertical(attrs, s)) ->
            writer.WriteStartElement("vertical")
            match attrs with
            | Some attrs ->
              let t =
                attrs.verticalType |> function
                | VerticalType.Absolute -> "absolute"
                | VerticalType.Relative -> "relative"
                | VerticalType.Sequence -> "sequence"
              writer.WriteAttributeString("type", t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | _ -> ()
          writeTerm term
          writer.WriteEndElement()
        | Action.Wait s ->
          writer.WriteStartElement("wait")
          writer.WriteString(Expr.NumExpr.text s)
          writer.WriteEndElement()
        | Action.Vanish ->
          writer.WriteStartElement("vanish")
          writer.WriteEndElement()
        | Action.Repeat (times, child) ->
          writer.WriteStartElement("repeat")
          match times with
          | Times s ->
            writer.WriteStartElement("times")
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          writeActionElm child
          writer.WriteEndElement()
        | Action.Fire (attrs, direction, speed, child) ->
          writeFireBody attrs direction speed (fun () -> writeBulletElm child)
        | Action.FireRef (attrs, prams) ->
          writer.WriteStartElement("fireRef")
          writer.WriteAttributeString("label", FireLabel.text attrs.fireRefLabel)
          writeParams prams
          writer.WriteEndElement()
        | Action.Action (attrs, children) ->
          writeActionBody attrs (fun () -> children |> Seq.iter writeCommand)
        | Action.ActionRef (attrs, prams) ->
          writer.WriteStartElement("actionRef")
          writer.WriteAttributeString("label", ActionLabel.text attrs.actionRefLabel)
          writeParams prams
          writer.WriteEndElement()

      and writeActionElm (a: ActionElm) =
        match a with
        | ActionElm.Action (attrs, children) ->
          writeActionBody attrs (fun () -> children |> Seq.iter writeCommand)
        | ActionElm.ActionRef (attrs, prams) ->
          writer.WriteStartElement("actionRef")
          writer.WriteAttributeString("label", ActionLabel.text attrs.actionRefLabel)
          writeParams prams
          writer.WriteEndElement()

      and writeBulletElm (b: BulletElm) =
        match b with
        | BulletElm.Bullet (attrs, direction, speed, children) ->
          writeBulletBody attrs direction speed (fun () -> children |> Seq.iter writeActionElm)
        | BulletElm.BulletRef (attrs, prams) ->
          writer.WriteStartElement("bulletRef")
          writer.WriteAttributeString("label", BulletLabel.text attrs.bulletRefLabel)
          writeParams prams
          writer.WriteEndElement()

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
        writer.WriteStartElement("bulletml")
        match attrs.bulletmlXmlns with
        | Some v -> writer.WriteAttributeString("xmlns", v)
        | None -> ()

        match attrs.bulletmlType with
        | Some typeName ->
          let t =
            typeName |> function
            | ShootingDirection.BulletNone -> "none"
            | ShootingDirection.BulletHorizontal -> "horizontal"
            | ShootingDirection.BulletVertical   -> "vertical"
          writer.WriteAttributeString("type", t)
        | None -> ()

        // parser が読む属性は writer も書く。書かないと往復で消える。
        // 同梱の弾幕も type のうしろに name を置いている
        match attrs.bulletmlName with
        | Some v -> writer.WriteAttributeString("name", v)
        | None -> ()

        match attrs.bulletmlDescription with
        | Some v -> writer.WriteAttributeString("description", v)
        | None -> ()

        children |> Seq.iter writeTopElm
        writer.WriteEndElement()

    let getXmlString formatting (encdoc: EncodingAndDoctype) indentation (this: Bulletml) =
      let output = new StringBuilder()             
      let sw =
        { new StringWriter(output) with
          override this.Encoding with get () = Encoding.UTF8 }
      sw.NewLine <- "\r\n"

      use writer = new XmlTextWriter(sw, Formatting=formatting, Indentation = indentation)
      encdoc |> function
      | Nothing -> ()
      | Exist -> writer.WriteStartDocument()
                 let docType = "bulletml"
                 let sysid = "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd"
                 writer.WriteDocType(docType, null, sysid, null)

      writeContentTo writer this
      output.ToString()

    let toXmlString (encodingAndDoctype: EncodingAndDoctype) (this: Bulletml) =
      getXmlString Formatting.None encodingAndDoctype 0 this

    let toIndentedXmlString (indentation: int) (encodingAndDoctype: EncodingAndDoctype) (this: Bulletml) =
      getXmlString Formatting.Indented encodingAndDoctype indentation this
