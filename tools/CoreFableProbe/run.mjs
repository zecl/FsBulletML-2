// 焼いた JS の側。**答えの形は `Probe.fs` の `line` が 1 か所 で持つ** ——
// ここで組み直すと .NET 側と食い違って「中身は同じなのに赤」になる。
//
//   node run.mjs <焼いた js の置き場>
import { pathToFileURL } from 'node:url'
import { join } from 'node:path'

const out = process.argv[2]
if (!out) {
  console.error('使い方: node run.mjs <焼いた js の置き場>')
  process.exit(2)
}
// **file URL で読む。** 焼いた JS は自分の隣の `fable_modules` を相対で引くので、
// 置き場から離して読み込むと解決できない
const { line } = await import(pathToFileURL(join(out, 'Probe.js')).href)
console.log(line())
