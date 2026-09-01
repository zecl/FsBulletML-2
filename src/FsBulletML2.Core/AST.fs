namespace FsBulletML2
open System

[<AutoOpen>]
module AST = 
  type EncodingAndDoctype = Nothing | Exist

  type Attributes = (string * string) list

  type XmlNode =
  | Element of string * Attributes * XmlNode list
  | PCData of string
