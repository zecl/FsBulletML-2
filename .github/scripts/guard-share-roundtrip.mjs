// 共有リンクの往復を、**ブラウザ側の実装のまま**走らせる。
//
// 組み立てと base64url は器（`ShareLink`）に在り、そちらは .NET でも走るので
// `guard-fable-parity.ps1` と `Parser.Tests` が当てている。
// **ここが見るのは、圧縮を通した往復が本文を 1 文字 も変えないこと** ——
// `CompressionStream` は .NET に無いので、走らせないと当たらない。
//
// 本文は `tests/TestData` から引く。**表を持たない** —— 弾幕が増えれば網も広がる。
// 読めない弾幕（`failure` の下）も混ぜる。**共有は本文を読まない**ので、
// そこが通るのが正しい。
//
// 数える先は 3 つ ——
//
//   往復      本文が 1 文字 も変わらないこと。表記も戻ること
//   大きさ    人が貼るものの大きさは決められない。塊で詰める形が要る
//   読めない  版が違う / 字が壊れている。**黙って空にしない**
import { readdirSync, readFileSync, statSync } from 'node:fs'
import { pathToFileURL } from 'node:url'
import { join, extname } from 'node:path'

const [shareJs, kindJs, corpusDir] = process.argv.slice(2)
if (!shareJs || !kindJs || !corpusDir) {
  console.error('usage: node guard-share-roundtrip.mjs <Share.js> <SourceKind.js> <corpusDir>')
  process.exit(2)
}

const mod = await import(pathToFileURL(shareJs).href)
for (const name of ['encode', 'decode']) {
  if (typeof mod[name] !== 'function') {
    console.error(`${name} が焼いた JS に無い（${shareJs}）`)
    process.exit(3)
  }
}

// **表記は器から作る。** ここで `{ tag: 0 }` のような物を組むと、腕の並びが
// 2 か所 目 の表になり、並びを変えたときそちらだけ古びる
const kinds = await import(pathToFileURL(kindJs).href)
if (typeof kinds.SourceKindModule_tryParse !== 'function') {
  console.error(`SourceKindModule_tryParse が焼いた JS に無い（${kindJs}）`)
  process.exit(3)
}
function kindOf(id) {
  const k = kinds.SourceKindModule_tryParse(id)
  if (k == null) {
    console.error(`器が知らない表記: ${id}`)
    process.exit(3)
  }
  return k
}

// 拡張子から表記を決める。**器の `FileExtension` と同じ対応** ——
// ずれれば下の「表記も戻る」で落ちる
const byExt = { '.xml': 'xml', '.sxml': 'sxml', '.fsb': 'fsb', '.fsx': 'fsharp' }

function walk(dir) {
  const out = []
  for (const name of readdirSync(dir)) {
    const p = join(dir, name)
    if (statSync(p).isDirectory()) out.push(...walk(p))
    else if (byExt[extname(name).toLowerCase()]) out.push(p)
  }
  return out
}

const bad = []
let files = 0
let maxLink = 0
let maxLinkAt = ''
let rawBytes = 0
let linkChars = 0

for (const path of walk(corpusDir)) {
  const text = readFileSync(path, 'utf8')
  const kindId = byExt[extname(path).toLowerCase()]
  const link = await mod.encode(kindOf(kindId), 50, 7, text).catch((e) => `THREW:${e}`)
  if (typeof link !== 'string' || link.startsWith('THREW:')) {
    bad.push(`${path}: リンクを作れなかった（${link}）`)
    continue
  }
  files++
  linkChars += link.length
  rawBytes += Buffer.byteLength(text, 'utf8')
  if (link.length > maxLink) {
    maxLink = link.length
    maxLinkAt = path
  }
  // **頭 の # を付けて戻す。** `location.hash` が返すのはその形
  const back = await mod.decode('#' + link).catch((e) => ({ Ok: false, Message: `THREW:${e}` }))
  if (!back.Ok) {
    bad.push(`${path}: 読み戻せなかった（${back.Message}）`)
  } else if (back.Text !== text) {
    bad.push(`${path}: 本文が変わった（素 ${text.length} 字 -> 戻り ${back.Text.length} 字）`)
  } else if (back.KindId !== kindId) {
    bad.push(`${path}: 表記が変わった（${kindId} -> ${back.KindId}）`)
  }
}

async function roundTrip(name, text) {
  const link = await mod.encode(kindOf('xml'), 50, 7, text).catch((e) => `THREW:${e}`)
  if (typeof link !== 'string' || link.startsWith('THREW:')) {
    bad.push(`${name}: リンクを作れなかった（${link}）`)
    return null
  }
  const back = await mod.decode(link).catch((e) => ({ Ok: false, Message: `THREW:${e}` }))
  if (!back.Ok) bad.push(`${name}: 読み戻せなかった（${back.Message}）`)
  else if (back.Text !== text) bad.push(`${name}: 本文が変わった`)
  return link
}

async function expectNg(name, fragment) {
  const r = await mod.decode(fragment).catch((e) => ({ Ok: false, Message: `THREW:${e}` }))
  if (r.Ok) bad.push(`${name}: 読めてしまった`)
  else if (!r.Message) bad.push(`${name}: 落ちたのに理由が空`)
}

// **人が貼るものの大きさは決められない。** 塊で詰めない書き方だと
// ここで `Maximum call stack size exceeded` が出る
await roundTrip('大きい本文', 'あ<wait>$rand</wait>\n'.repeat(50000))
// 空の本文。**往復しない形なら、ここで分かる**
await roundTrip('空の本文', '')

// **貼るときに切れたリンクを、中身の化けた弾幕にしない**
const sample = await roundTrip('素の本文', '<bulletml/>')
if (sample) {
  await expectNg('版が違う', '1' + sample.slice(1))
  await expectNg('字が壊れている', sample.slice(0, -1) + '*')
  await expectNg('途中で切れている', sample.slice(0, sample.length - 4))
}
await expectNg('区切りが足りない', '2.xml')
  await expectNg('走らせ方が数でない', '2.xml.x.7.AQID')
await expectNg('空', '')

for (const line of bad) console.log(`NG\t${line}`)
console.log(`SUM\t${files}\t${maxLink}\t${maxLinkAt}\t${rawBytes}\t${linkChars}`)
if (bad.length > 0) process.exit(1)
