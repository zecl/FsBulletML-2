// 焼いた JS の側。`XmlScan_describe` を 1 件 ずつ呼んで、`<番号>\t<答え>` を出す。
//
// **答えを組み立てるのはここではない。** 組み立ては XmlScan.describe の中に
// 在って、.NET と共通。ここで組むと、組み方のほうが食い違って
// 「中身は同じなのに赤」「違うのに緑」になる。
import { readFileSync } from 'node:fs'
import { pathToFileURL } from 'node:url'

const [casesPath, jsPath] = process.argv.slice(2)
if (!casesPath || !jsPath) {
  console.error('usage: node fable-parity.mjs <cases.json> <XmlScan.js>')
  process.exit(2)
}

const cases = JSON.parse(readFileSync(casesPath, 'utf8'))
const mod = await import(pathToFileURL(jsPath).href)
const describe = mod.XmlScan_describe
if (typeof describe !== 'function') {
  console.error('XmlScan_describe が焼いた JS に無い')
  process.exit(3)
}

cases.forEach((c, i) => {
  console.log(`${i}\t${describe(c.src, c.cursor)}`)
})
