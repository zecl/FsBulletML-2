namespace FsBulletML2

open System
open System.Diagnostics 
open System.IO 
open System.Text 
open System.Xml
open System.Text.RegularExpressions
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
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

  //[<DebuggerDisplay("BulletML = { this.ToXmlString() }")>]
  [<RequireQualifiedAccess>]
  type internal RecBulletml =
    internal
  /// BulletML DTD
  /// <!ELEMENT bulletml (bullet | fire | action)*>
  /// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
  /// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
    | Bulletml of BulletmlAttrs * RecBulletml list 
  /// BulletML DTD
  /// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
  /// <!ATTLIST action label CDATA #IMPLIED>
    | Action of ActionAttrs * RecBulletml list 
  /// BulletML DTD
  /// <!ELEMENT actionRef (param* )>
  /// <!ATTLIST actionRef label CDATA #REQUIRED>
    | ActionRef of ActionRefAttrs * Params
  /// BulletML DTD
  /// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
  /// <!ATTLIST fire label CDATA #IMPLIED>
    | Fire of FireAttrs * Direction option * Speed option * RecBulletml  
  /// BulletML DTD
  /// <!ELEMENT fireRef (param* )>
  /// <!ATTLIST fireRef label CDATA #REQUIRED>
    | FireRef of FireRefAttrs * Params
  /// BulletML DTD
  /// <!ELEMENT wait (#PCDATA)>
    | Wait of Expr.NumExpr
  /// BulletML DTD
  /// <!ELEMENT vanish (#PCDATA)>
    | Vanish 
  /// BulletML DTD
  /// <!ELEMENT changeSpeed (speed, term)>
    | ChangeSpeed of Speed * Term
  /// BulletML DTD
  /// <!ELEMENT changeDirection (direction, term)>
    | ChangeDirection of Direction * Term
  /// BulletML DTD
  /// <!ELEMENT accel (horizontal?, vertical?, term)>  
    | Accel of Horizontal option * Vertical option * Term
  /// BulletML DTD
  /// <!ELEMENT bullet (direction?, speed?, (action | actionRef)* )>
  /// <!ATTLIST bullet label CDATA #IMPLIED>
    | Bullet of BulletAttrs * Direction option * Speed option * RecBulletml list 
  /// BulletML DTD
  /// <!ELEMENT bulletRef (param* )>
  /// <!ATTLIST bulletRef label CDATA #REQUIRED>
    | BulletRef of BulletRefAttrs * Params
  /// BulletML DTD
  /// <!ELEMENT repeat (times, (action | actionRef))>
    | Repeat of Times * RecBulletml 
    | NotCommand

    /// BulletML 書き込み
    member private this.WriteContentTo(writer:XmlWriter) =
      let rec write element =
        match element with
        | RecBulletml.Bulletml (attrs, children) ->
          writer.WriteStartElement("bulletml")
          let localName,xmlnsName = "xmlns", attrs.bulletmlXmlns 
          xmlnsName |> function 
          | Some v -> 
            writer.WriteAttributeString(localName, v)
          | None -> ()

          let localName,typeName = "type", attrs.bulletmlType 
          match typeName with
          | Some typeName ->
            let t = 
              typeName |> function 
              | ShootingDirection.BulletNone -> "none"
              | ShootingDirection.BulletHorizontal -> "horizontal"
              | ShootingDirection.BulletVertical   -> "vertical"
            writer.WriteAttributeString(localName, t )
          | None -> ()

          // parser が読む属性は writer も書く。書かないと往復で消える。
          // 同梱の弾幕も type のうしろに name を置いている
          match attrs.bulletmlName with
          | Some v -> writer.WriteAttributeString("name", v)
          | None -> ()

          match attrs.bulletmlDescription with
          | Some v -> writer.WriteAttributeString("description", v)
          | None -> ()

          children |> Seq.iter (fun child -> write child)
          writer.WriteEndElement()
        | RecBulletml.Action (attrs, children) -> 
          writer.WriteStartElement("action")
          let localName,labelName = "label", attrs.actionLabel 
          labelName |> function 
          | Some v -> 
            writer.WriteAttributeString(localName, ActionLabel.text v)
          | None -> ()
          children |> Seq.iter (fun child -> write child)
          writer.WriteEndElement()
        | RecBulletml.ActionRef (attrs, prams) ->
          writer.WriteStartElement("actionRef")
          
          let localName,labelName = "label", attrs.actionRefLabel
          writer.WriteAttributeString(localName, ActionLabel.text labelName)

          prams |> Seq.iter(fun s -> 
            writer.WriteStartElement("param")
            writer.WriteString(s)
            writer.WriteEndElement())

          writer.WriteEndElement()
        | RecBulletml.Repeat (times , child) ->
          writer.WriteStartElement("repeat")
          
          match times with
          | Times s ->
            writer.WriteStartElement("times")
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()

          write child
          writer.WriteEndElement()
        | RecBulletml.Fire (attrs, direction, speed, child) ->
          writer.WriteStartElement("fire")
          
          let localName,labelName = "label", attrs.fireLabel
          labelName |> function 
          | Some v -> 
            writer.WriteAttributeString(localName, FireLabel.text v)
          | None -> ()
          direction |> function
          | Some d ->
            writer.WriteStartElement("direction")
            match d with
            | Direction (attrs, s) ->
              match attrs  with
              | Some attrs ->
                let localName, typeName = "type", attrs.directionType
                let t = 
                  typeName |> function 
                  | DirectionType.Aim      -> "aim"
                  | DirectionType.Absolute -> "absolute"
                  | DirectionType.Relative -> "relative"
                  | DirectionType.Sequence -> "sequence"
                writer.WriteAttributeString(localName, t)
              | None -> ()
              writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | None -> ()

          speed |> function
          | Some d ->
            writer.WriteStartElement("speed")
            match d with
            | Speed (attrs, s) ->
              match attrs  with
              | Some attrs ->
                let localName, typeName = "type", attrs.speedType
                let t = 
                  typeName |> function 
                  | SpeedType.Absolute -> "absolute"
                  | SpeedType.Relative -> "relative"
                  | SpeedType.Sequence -> "sequence"
                writer.WriteAttributeString(localName, t)
              | None -> ()
              writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | None -> ()
          write child
          writer.WriteEndElement()
        | RecBulletml.FireRef (attrs, prams) ->
          writer.WriteStartElement("fireRef")
          let localName,labelName = "label", attrs.fireRefLabel
          writer.WriteAttributeString(localName, FireLabel.text labelName)
          prams |> Seq.iter(fun s -> 
            writer.WriteStartElement("param")
            writer.WriteString(s)
            writer.WriteEndElement())
          writer.WriteEndElement()
        | RecBulletml.Bullet (attrs, direction, speed, children) ->
          writer.WriteStartElement("bullet")
          let localName,labelName = "label", attrs.bulletLabel
          labelName |> function 
          | Some v -> 
            writer.WriteAttributeString(localName, BulletLabel.text v)
          | None -> ()
          direction |> function
          | Some d ->
            writer.WriteStartElement("direction")
            match d with
            | Direction (attrs, s) ->
              match attrs  with
              | Some attrs ->
                let localName, typeName = "type", attrs.directionType
                let t = 
                  typeName |> function 
                  | DirectionType.Aim      -> "aim"
                  | DirectionType.Absolute -> "absolute"
                  | DirectionType.Relative -> "relative"
                  | DirectionType.Sequence -> "sequence"
                writer.WriteAttributeString(localName, t)
              | None -> ()
              writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | None -> ()
          speed |> function
          | Some speed ->
            writer.WriteStartElement("speed")
            match speed with
            | Speed (attrs, s) ->
              match attrs  with
              | Some attrs ->
                let localName, typeName = "type", attrs.speedType
                let t = 
                  typeName |> function 
                  | SpeedType.Absolute -> "absolute"
                  | SpeedType.Relative -> "relative"
                  | SpeedType.Sequence -> "sequence"
                writer.WriteAttributeString(localName, t)
              | None -> ()
              writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | None -> ()
          children |> Seq.iter (fun child -> write child)
          writer.WriteEndElement()
        | RecBulletml.BulletRef (attrs, prams) ->
          writer.WriteStartElement("bulletRef")
          let localName,labelName = "label", attrs.bulletRefLabel
          writer.WriteAttributeString(localName, BulletLabel.text labelName)
          prams |> Seq.iter(fun s -> 
            writer.WriteStartElement("param")
            writer.WriteString(s)
            writer.WriteEndElement())
          writer.WriteEndElement()
        | RecBulletml.ChangeDirection (direction, term) ->
          writer.WriteStartElement("changeDirection")
          writer.WriteStartElement("direction")
          match direction with
          | Direction (attrs, s) ->
            match attrs  with
            | Some attrs ->
              let localName, typeName = "type", attrs.directionType
              let t = 
                typeName |> function 
                | DirectionType.Aim      -> "aim"
                | DirectionType.Absolute -> "absolute"
                | DirectionType.Relative -> "relative"
                | DirectionType.Sequence -> "sequence"
              writer.WriteAttributeString(localName, t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
          writer.WriteEndElement()
          match term with
          | Term s ->
            writer.WriteStartElement("term")
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          writer.WriteEndElement()
        | RecBulletml.ChangeSpeed (speed, term) ->
          writer.WriteStartElement("changeSpeed")
          writer.WriteStartElement("speed")
          match speed with
          | Speed (attrs, s) ->
            match attrs  with
            | Some attrs ->
              let localName, typeName = "type", attrs.speedType
              let t = 
                typeName |> function 
                | SpeedType.Absolute -> "absolute"
                | SpeedType.Relative -> "relative"
                | SpeedType.Sequence -> "sequence"
              writer.WriteAttributeString(localName, t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
          writer.WriteEndElement()
          match term with
          | Term s ->
            writer.WriteStartElement("term")
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          writer.WriteEndElement()
        | RecBulletml.Accel (horizontal, vertical, term) ->
          writer.WriteStartElement("accel")
          match horizontal with
          | Some (Horizontal.Horizontal(attrs,s)) ->
            writer.WriteStartElement("horizontal")
            match attrs with
            | Some attrs ->
              let localName,typeName = "type", attrs.horizontalType 
              let t = 
                typeName |> function 
                | HorizontalType.Absolute -> "absolute"
                | HorizontalType.Relative -> "relative"
                | HorizontalType.Sequence -> "sequence"
              writer.WriteAttributeString(localName, t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | _ -> ()
          match vertical with
          | Some (Vertical.Vertical(attrs,s)) ->
            writer.WriteStartElement("vertical")
            match attrs with
            | Some attrs ->
              let localName,typeName = "type", attrs.verticalType  
              let t = 
                typeName |> function 
                | VerticalType.Absolute -> "absolute"
                | VerticalType.Relative -> "relative"
                | VerticalType.Sequence -> "sequence"
              writer.WriteAttributeString(localName, t)
            | None -> ()
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          | _ -> ()
          match term with
          | Term s ->
            writer.WriteStartElement("term")
            writer.WriteString(Expr.NumExpr.text s)
            writer.WriteEndElement()
          writer.WriteEndElement()
        | RecBulletml.Wait (s) ->
          writer.WriteStartElement("wait")
          writer.WriteString(Expr.NumExpr.text s)
          writer.WriteEndElement()
        | RecBulletml.Vanish ->
          writer.WriteStartElement("vanish")
          writer.WriteEndElement()
        | RecBulletml.NotCommand -> ()
      write this

    member private this.GetXmlString formatting (encdoc:EncodingAndDoctype) indentation = 
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

      this.WriteContentTo(writer)
      output.ToString()

    override this.ToString() =
      this.GetXmlString Formatting.None EncodingAndDoctype.Nothing 0

    member this.ToXmlString(?encodingAndDoctype) = 
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing 
      this.GetXmlString Formatting.None encodingAndDoctype 0

    member this.ToIndentedXmlString([<Optional; DefaultParameterValue(4)>]?indentation : int, ?encodingAndDoctype) =
      let indentation = defaultArg indentation 4
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing
      this.GetXmlString Formatting.Indented encodingAndDoctype indentation

  /// Innternal DSL
  [<StructuredFormatDisplay("{ToStructuredDisplay}")>]
  type Bulletml =
/// BulletML DTD
/// <!ELEMENT bulletml (bullet | fire | action)*>
/// <!ATTLIST bulletml xmlns CDATA #IMPLIED>
/// <!ATTLIST bulletml type (none|vertical|horizontal) "none">
  | Bulletml of BulletmlAttrs * BulletmlElm list 
/// BulletML DTD
/// <!ELEMENT action (changeDirection | accel | vanish | changeSpeed | repeat | wait | (fire | fireRef) | (action | actionRef))*>
/// <!ATTLIST action label CDATA #IMPLIED>
  | Action of ActionAttrs * Action list 
/// BulletML DTD
/// <!ELEMENT actionRef (param* )>
/// <!ATTLIST actionRef label CDATA #REQUIRED>
  | ActionRef of ActionRefAttrs * Params
/// BulletML DTD
/// <!ELEMENT fire (direction?, speed?, (bullet | bulletRef))>
/// <!ATTLIST fire label CDATA #IMPLIED>
  | Fire of FireAttrs * Direction option * Speed option * BulletElm  
/// BulletML DTD
/// <!ELEMENT fireRef (param* )>
/// <!ATTLIST fireRef label CDATA #REQUIRED>
  | FireRef of FireRefAttrs * Params
/// BulletML DTD
/// <!ELEMENT wait (#PCDATA)>
  | Wait of Expr.NumExpr
/// BulletML DTD
/// <!ELEMENT vanish (#PCDATA)>
  | Vanish 
/// BulletML DTD
/// <!ELEMENT changeSpeed (speed, term)>
  | ChangeSpeed of Speed * Term
/// BulletML DTD
/// <!ELEMENT changeDirection (direction, term)>
  | ChangeDirection of Direction * Term
/// BulletML DTD
/// <!ELEMENT accel (horizontal?, vertical?, term)>  
  | Accel of Horizontal option * Vertical option * Term
/// BulletML DTD
/// <!ELEMENT bullet (direction?, speed?, (action | actionRef)* )>
/// <!ATTLIST bullet label CDATA #IMPLIED>
  | Bullet of BulletAttrs * Direction option * Speed option * ActionElm list 
/// BulletML DTD
/// <!ELEMENT bulletRef (param* )>
/// <!ATTLIST bulletRef label CDATA #REQUIRED>
  | BulletRef of BulletRefAttrs * Params
/// BulletML DTD
/// <!ELEMENT repeat (times, (action | actionRef))>
  | Repeat of Times * ActionElm 
  | NotCommand
    member private t.ToStructuredDisplay = t.ToString()
    override t.ToString () = stringifyFullName t 
    member this.ToNodeString() = 
      this.ToString().Replace("null","None")
    member this.Type
        with get() = 
            match this with
            | Bulletml (x,_) -> x.bulletmlType
            | _ -> None
    member this.Name
        with get() = 
            match this with
            | Bulletml (x,_) -> x.bulletmlName   
            | _ -> None
    /// description は BulletML公式の属性ではない。BulletMLの名前/説明文を格納するための属性として追加した。
    member this.Description
        with get() =
            match this with
            | Bulletml (x,_) -> x.bulletmlDescription
            | _ -> None

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
