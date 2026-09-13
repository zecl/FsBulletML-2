namespace FsBulletML2.Core.Tests

open System
open System.Globalization
open System.IO
open FsBulletML2

/// **旧実装。式の字を毎回 XPath で評価する。**
///
/// エンジンはもうここを通らない。残してあるのは `ExprTests` が
/// 「木（`Expr.NumExpr`）が同じ値を返すか」を突き合わせる相手として要るから。
/// 消すと、木が正しいことを確かめる基準が無くなる。
///
/// **`Core` ではなくここに在る理由。** `System.Xml.XPath` は Fable に無く、
/// Fable は proj まるごとしか焼けないので、Core に 1 か所 でも在ると
/// Core ごと焼けなくなる。**相手は試験の側にしか要らない** ——
/// だから Core から出して、当てる先の隣へ置いた。
///
/// この実装には穴が 2 つ ある（どちらも ExprTests が名指しで固定している）。
///   - $rand / $rank が 1e-4 未満だと ToString が "1E-07" を吐き、
///     xpathNumber の空白入れがそれを割って XPathException になる
///   - 読めない式で例外になる（木のほうは NaN）
[<AutoOpen>]
module XPathOracle =

  /// 式の値は BulletML の文書と同じ書き方（小数点は . ）で持ち回る。
  /// 読む側を既定カルチャのままにすると、de-DE は "2.5" の . を桁区切りと読んで
  /// 例外なく 25 を返す。作る側はここでは元から不変（F# の string 演算子）。
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
  /// **乱数は式の中身によらず 1 回 だけ引く** —— 木の側もそう書いてある。
  let getValueByXPath (env: Domain.Env) (s: string) =
    let rand = env.Rand ()
    let rank = env.Rank
    let s = s.Replace("$rand", rand.ToString(CultureInfo.InvariantCulture))
             .Replace("$rank", rank.ToString(CultureInfo.InvariantCulture))
    // 置き換え残りの $N を 0 に潰す。\d が \$d* と書かれていて、
    // $ だけが 0 になり数字が残っていた（$1 が "01" = 1 になる）
    let s = System.Text.RegularExpressions.Regex.Replace(s, "\$\d*", "0")
    xpathEval s
