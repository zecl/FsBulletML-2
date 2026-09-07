// 焼いた JS の側。表の 1 件 ずつを、その target の `describe` に通して
// `<番号>\t<答え>` を出す。**番号は表の中の通し番号**で、target ごとに
// 振り直さない —— 突き合わせる側が 2 つ 以上 の target を 1 つ の並びとして数える。
//
// **答えを組み立てるのはここではない。** 組み立ては各 target の describe の
// 中に在って、.NET と共通。ここで組むと、組み方のほうが食い違って
// 「中身は同じなのに赤」「違うのに緑」になる。
//
// **引数の名前は表に在る**（targets.json の `args`）。前はここに
// `c.target === 'XmlScan' ? … : …` と書いてあり、**2 本 目 のスキャナを
// 足した瞬間に、そちらが `fn(c.id)` の側へ落ちる**形だった —— 表を 1 か所 に
// したはずが、引数の取り出しだけ 2 か所 目 の表になっていた。
//
// F# 側には振り分けが残る（静的に型が要る）。取り違えれば答えがずれるので、
// 突き合わせ自身がそこも見ている。
import { readFileSync } from 'node:fs'
import { pathToFileURL } from 'node:url'
import { join } from 'node:path'

const [casesPath, targetsPath, repoRoot] = process.argv.slice(2)
if (!casesPath || !targetsPath || !repoRoot) {
  console.error('usage: node fable-parity.mjs <cases.json> <targets.json> <repoRoot>')
  process.exit(2)
}

const cases = JSON.parse(readFileSync(casesPath, 'utf8'))
const targets = JSON.parse(readFileSync(targetsPath, 'utf8'))

const describe = {}
for (const t of targets) {
  const mod = await import(pathToFileURL(join(repoRoot, t.js)).href)
  const fn = mod[t.export]
  if (typeof fn !== 'function') {
    console.error(`${t.export} が焼いた JS に無い（${t.js}）`)
    process.exit(3)
  }
  if (!Array.isArray(t.args) || t.args.length === 0) {
    console.error(`target『${t.target}』に args が無い（targets.json）`)
    process.exit(3)
  }
  describe[t.target] = { fn, args: t.args }
}

cases.forEach((c, i) => {
  const d = describe[c.target]
  if (!d) {
    console.error(`表に知らない target が在る: ${c.target}`)
    process.exit(4)
  }
  console.log(`${i}\t${d.fn(...d.args.map((a) => c[a]))}`)
})
