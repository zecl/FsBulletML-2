#requires -Version 7
<#
.SYNOPSIS
  guard-share-roundtrip.ps1 の較正。

.DESCRIPTION
  **通る側と落ちる側**を両方 当てる。落ちる側だけ見ていると「常に赤」の
  壊れ方が、通る側だけ見ていると「何も見ていない」壊れ方が、それぞれ
  緑のまま残る。

  **門の腕ごとに変異を 1 つ ずつ置く。** この門は 3 つ を見ているので、
  1 本 だけ壊して赤を見ても、ほかの腕が誰も当てていない状態は緑のまま残る ——

      往復      解く側の形式を替える。**符号化は同じまま**なので、
                字は作れて読み戻すところだけが落ちる
      読めない  版の突き合わせを外す。古いリンクが黙って読めるようになる
      数        拾う拡張子を減らす。往復は 1 件 も落ちないまま数が減る

  いちばん怖いのは**材料が読めないとき** —— 焼いた JS が無い、口が無い、
  本文が 1 本 も無い。どれも「食い違い 0 件」と同じ顔をする。

  最後に repo の現物へ当てる。**差し替えを 1 つ も渡さない経路**を
  通らないと、既定のパスを引くところが一度も走らない。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-share-roundtrip.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("share-roundtrip-" + [Guid]::NewGuid().ToString('N'))

$fails = 0
$count = 0

function Check {
  param([string]$Name, [hashtable]$Opt, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard @Opt -Quiet } catch { $passed = $false; $msg = "$_" }

  if ($passed -ne $WantPass) {
    $script:fails++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $Name,
      $(if ($WantPass) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
    if ($msg) { Write-Host "         $($msg -replace "`n", ' / ')" }
    return
  }
  if ($Expect -and $msg -notmatch [regex]::Escape($Expect)) {
    $script:fails++
    Write-Host "  NG   $Name  「$Expect」が出ていない"
    Write-Host "         $($msg -replace "`n", ' / ')"
    return
  }
  Write-Host ("  ok   {0}  -> {1}" -f $Name, $(if ($passed) { '通る' } else { '落ちる' }))
}

function With([hashtable]$Base, [hashtable]$Over) {
  $h = @{}
  foreach ($k in $Base.Keys) { $h[$k] = $Base[$k] }
  foreach ($k in $Over.Keys) { $h[$k] = $Over[$k] }
  $h
}

try {
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null

  $realJsDir = Join-Path $root 'src/FsBulletML2.LanguageService.Js/js'
  if (-not (Test-Path -LiteralPath (Join-Path $realJsDir 'Share.js'))) {
    throw "較正の材料が無い（$realJsDir/Share.js）。先に dotnet build src/FsBulletML2.Playground で焼くこと"
  }
  # **相対 import ごと写す。** 焼いた JS は `./fable_modules/` と
  # `./FsBulletML2.LanguageService/` を相対で引くので、深さを変えると
  # import が解けず「口が無い」で落ちて、狙った点と別の赤になる
  $jsDir = Join-Path $tmp 'js'
  Copy-Item -LiteralPath $realJsDir -Destination $jsDir -Recurse -Force

  $shareJs = Join-Path $jsDir 'Share.js'
  $kindJs = Join-Path $jsDir 'FsBulletML2.LanguageService/SourceKind.js'
  $linkJs = Join-Path $jsDir 'FsBulletML2.LanguageService/ShareLink.js'

  # **本文は少しでよい。** ここが見るのは門の腕であって網の広さではない。
  # repo の現物は最後に 1 回 通す
  $corpus = Join-Path $tmp 'corpus'
  New-Item -ItemType Directory -Path $corpus -Force | Out-Null
  $real = @(Get-ChildItem -LiteralPath (Join-Path $root 'tests/TestData') -Recurse -File |
            Where-Object { $_.Extension -ceq '.xml' } | Select-Object -First 6)
  if ($real.Count -eq 0) { throw '較正に使う本文が引けなかった' }
  foreach ($f in $real) { Copy-Item -LiteralPath $f.FullName -Destination $corpus -Force }
  # 拡張子が減ったことに気づくかを見るために、別の表記も 1 本 置く
  $sxml = @(Get-ChildItem -LiteralPath (Join-Path $root 'tests/TestData') -Recurse -File |
            Where-Object { $_.Extension -ceq '.sxml' } | Select-Object -First 2)
  if ($sxml.Count -eq 0) { throw '較正に使う sxml が引けなかった' }
  foreach ($f in $sxml) { Copy-Item -LiteralPath $f.FullName -Destination $corpus -Force }

  # 変異した写しを作る。**元の写しは触らない** ——
  # 書き換えて戻す形にすると、戻し忘れがそのまま次の点の素になる。
  #
  # **置くのは元と同じディレクトリ。** 焼いた JS が相対で import を引くので、
  # 深さを変えると import が解けず、狙った点と別の赤になる。
  # だから**元が repo の中に在る物は、先に写しへ持ってくること** ——
  # そうしないと変異が repo に残る（実際にやった。`finally` は $tmp しか消さない）
  function MutatedCopy([string]$Src, [string]$Suffix, [string]$From, [string]$To, [string]$Why) {
    $dir = Split-Path -Parent $Src
    if (-not $dir.StartsWith($tmp)) { throw "変異の写しが $tmp の外へ出る: $Src" }
    $out = Join-Path $dir ([IO.Path]::GetFileNameWithoutExtension($Src) + '-' + $Suffix +
                           [IO.Path]::GetExtension($Src))
    $text = [IO.File]::ReadAllText($Src)
    $patched = $text.Replace($From, $To)
    if ($patched -ceq $text) { throw "較正が当たらなかった（$Why）" }
    [IO.File]::WriteAllText($out, $patched)
    $out
  }

  $ok = @{ RepoRoot = $root; ShareJs = $shareJs; KindJs = $kindJs; CorpusDir = $corpus }

  Write-Host '=== 通る側'
  # **既定と違う場所で測る。** 既定のままだと「渡せるようにした」ことが
  # 効いている証拠にならない
  Check '写した材料で往復する' $ok $true ''

  Write-Host '=== 落ちる側（腕ごとに 1 つ ずつ壊す）'

  # 往復 —— 解く側の形式だけ替える。**作るほうは同じ**なので、
  # 「リンクは作れるが読み戻せない」形になる
  $inflateBroken = MutatedCopy $shareJs 'inflate' `
    "new DecompressionStream('deflate-raw')" "new DecompressionStream('gzip')" `
    '焼いた Share.js の中身が想定と違う'
  Check '解く側の形式が違う' (With $ok @{ ShareJs = $inflateBroken }) $false '読み戻せなかった'

  # 読めない側 —— 版の突き合わせを外す。**古いリンクが黙って読める**
  $noVersion = MutatedCopy $linkJs 'noversion' `
    'if (v !== ShareLinkModule_version) {' 'if (false) {' `
    '焼いた ShareLink.js の中身が想定と違う'
  $sharePointing = MutatedCopy $shareJs 'noversion' `
    './FsBulletML2.LanguageService/ShareLink.js' `
    ('./FsBulletML2.LanguageService/' + (Split-Path $noVersion -Leaf)) `
    'Share.js の import 先が想定と違う'
  Check '版が違うリンクを読んでしまう' (With $ok @{ ShareJs = $sharePointing }) $false '版が違う'

  # 数 —— 拾う拡張子を減らす。**往復は 1 件 も落ちない**まま数が減る
  $mjsCopy = Join-Path $tmp 'guard-share-roundtrip.mjs'
  Copy-Item -LiteralPath (Join-Path $root '.github/scripts/guard-share-roundtrip.mjs') `
    -Destination $mjsCopy -Force
  $fewerExt = MutatedCopy $mjsCopy 'fewer' `
    "'.sxml': 'sxml', " '' '走らせる口の中身が想定と違う'
  Check '拾う拡張子が減っている' (With $ok @{ MjsPath = $fewerExt }) $false '往復した数が合わない'

  Write-Host '=== 材料が読めないときも落ちる'

  Check '焼いた JS が無い' (With $ok @{ ShareJs = (Join-Path $jsDir 'Nope.js') }) $false '焼いた JS が無い'

  $noExport = MutatedCopy $shareJs 'noexport' 'export function encode' 'function encode' `
    'export の綴りが想定と違う'
  Check '焼いた JS に口が無い' (With $ok @{ ShareJs = $noExport }) $false '数を返さなかった'

  $emptyCorpus = Join-Path $tmp 'empty'
  New-Item -ItemType Directory -Path $emptyCorpus -Force | Out-Null
  Check '本文が 1 本 も無い' (With $ok @{ CorpusDir = $emptyCorpus }) $false '1 本 も無い'

  Check '本文の在り処が無い' `
    (With $ok @{ CorpusDir = (Join-Path $tmp 'nowhere') }) $false '本文の在り処が無い'

  Check '走らせる口が無い' `
    (With $ok @{ MjsPath = (Join-Path $tmp 'nope.mjs') }) $false '走らせる口が無い'

  Write-Host '=== 本番の軸'
  # 差し替えを 1 つ も渡さない。渡した数点が緑でも、既定のパスを引く経路は通らない
  $count++
  try {
    & $guard -RepoRoot $root -Quiet
    Write-Host '  ok   repo の現物  -> 通る'
  } catch {
    $fails++
    Write-Host '  NG   repo の現物  -> 落ちた'
    Write-Host "         $_"
  }
} finally {
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ''
if ($fails -gt 0) {
  Write-Host "校正 $count 点 中 $fails 点 が外れた"
  exit 1
}
Write-Host "校正 $count 点 すべて一致"
