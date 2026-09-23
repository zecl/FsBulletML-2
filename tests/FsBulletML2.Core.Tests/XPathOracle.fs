namespace FsBulletML2.Core.Tests

open System
open System.Globalization
open System.IO
open FsBulletML2

/// 旧実装。
[<AutoOpen>]
module XPathOracle =

  /// 式の値は BulletML の文書と同じ書き方（小数点は . ）で持ち回る。
  let private xpathNumber (expression: string) =
    let regx = new System.Text.RegularExpressions.Regex(@"([\+\-\*])")
    let xexpr = regx.Replace(expression, " ${1} ").Replace("/", " div ").Replace("%", " mod ")
    let doc = new System.Xml.XPath.XPathDocument(new StringReader("<r/>"))
    let nav = doc.CreateNavigator()
    Convert.ToString(nav.Evaluate(String.Format("number({0})", xexpr)), CultureInfo.InvariantCulture)

  let xpathTryEval (expression: string) =
    Single.TryParse(xpathNumber expression, NumberStyles.Float, CultureInfo.InvariantCulture)

  let xpathEval (expression: string) =
    Single.Parse(xpathNumber expression, NumberStyles.Float, CultureInfo.InvariantCulture)

  /// 走行の `getValue` と同じ引数で呼べる形。
  /// 乱数は式の中身によらず 1 回 だけ引く —— 木の側もそう書いてある。
  let getValueByXPath (env: Domain.Env) (s: string) =
    let rand = env.Rand ()
    let rank = env.Rank
    let s = s.Replace("$rand", rand.ToString(CultureInfo.InvariantCulture))
             .Replace("$rank", rank.ToString(CultureInfo.InvariantCulture))
    // 置き換え残りの $N を 0 に潰す。\d が \$d* と書かれていて、
    // $ だけが 0 になり数字が残っていた（$1 が "01" = 1 になる）
    let s = System.Text.RegularExpressions.Regex.Replace(s, "\$\d*", "0")
    xpathEval s
