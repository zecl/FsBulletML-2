#requires -Version 7
<#
.SYNOPSIS
  guard-fable-parity.ps1 の較正。

.DESCRIPTION
  **通る側と落ちる側**を両方 当てる。落ちる側だけ見ていると「常に赤」の
  壊れ方が、通る側だけ見ていると「何も見ていない」壊れ方が、それぞれ
  緑のまま残る。

  **当てる先が 2 本 に増えた**（`XmlScan` と、proj をまたぐ `SourceKind`）ので、
  変異はそれぞれに 1 つ ずつ置く。片方 だけ壊して赤を見ても、
  もう片方 が突き合わせから外れている状態は緑のまま残る。

  いちばん怖いのは**材料が読めないとき** —— 焼いた JS が無い、表が空、
  片方 の答えが 0 行、**target を表に足しただけで 1 件 も当てていない**。
  どれも「食い違い 0 件」と同じ顔をする。

  最後に repo の現物へ当てる。**差し替えを 1 つ も渡さない経路**を
  通らないと、既定のパスを引くところが一度も走らない。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-fable-parity.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("fable-parity-" + [Guid]::NewGuid().ToString('N'))

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

  $realTargets = Join-Path $root '.github/scripts/fable-parity-targets.json'
  $targets = @(Get-Content -LiteralPath $realTargets -Raw | ConvertFrom-Json)

  # 変異した写しを指す表を組む。**元の表は触らない** ——
  # 書き換えて戻す形にすると、戻し忘れがそのまま次の点の素になる
  function TargetsWith([string]$Target, [string]$Js) {
    $t = @(Get-Content -LiteralPath $realTargets -Raw | ConvertFrom-Json)
    $hit = 0
    foreach ($x in $t) { if ($x.target -ceq $Target) { $x.js = $Js; $hit++ } }
    if ($hit -ne 1) { throw "表の中で『$Target』が $hit 本 当たった（1 本 のはず）" }
    $p = Join-Path $tmp ("targets-" + $Target + "-" + [Guid]::NewGuid().ToString('N').Substring(0, 6) + ".json")
    [IO.File]::WriteAllText($p, (ConvertTo-Json -InputObject $t -Depth 5))
    $p
  }

  # 焼いた JS のほうだけ答えを変える。**.NET は元のソースを load するので
  # 変わらない** —— これが「焼き忘れ」と同じ形。
  # **大小だけの差にしない。** `-eq` も `-ne` も既定で大小を無視するので、
  # `ctx` -> `CTX` の変異は「変えたのに変わっていない」に見える（実際に踏んだ）
  function Mutate([string]$Rel, [string]$From, [string]$To, [string]$Why) {
    $p = Join-Path $tmp $Rel
    $text = [IO.File]::ReadAllText($p)
    $patched = $text.Replace($From, $To)
    if ($patched -ceq $text) { throw "較正が当たらなかった（$Why）" }
    [IO.File]::WriteAllText($p, $patched)
  }

  # **repo と同じ相対構造で写す。** 表の `js` は repo からの相対で、
  # 根だけ `-JsRoot` で差し替える。相対 import ごと写すために
  # `wwwroot/js` を丸ごと持ってくる（`fable_modules` を `./` で引いている）
  $realJsDir = Join-Path $root 'src/FsBulletML2.Playground/wwwroot/js'
  if (-not (Test-Path -LiteralPath (Join-Path $realJsDir 'XmlScan.js'))) {
    throw "較正の材料が無い。先に dotnet build src/FsBulletML2.Playground で焼くこと"
  }
  $jsParent = Join-Path $tmp 'src/FsBulletML2.Playground/wwwroot'
  New-Item -ItemType Directory -Path $jsParent -Force | Out-Null
  Copy-Item -LiteralPath $realJsDir -Destination (Join-Path $jsParent 'js') -Recurse -Force

  $realCases = Join-Path $root '.github/scripts/fable-parity-cases.json'
  $casesCopy = Join-Path $tmp 'cases.json'
  Copy-Item -LiteralPath $realCases -Destination $casesCopy -Force

  $targetsCopy = Join-Path $tmp 'targets.json'
  Copy-Item -LiteralPath $realTargets -Destination $targetsCopy -Force

  # **既定と違う場所で測る。** 既定のままだと「渡せるようにした」ことが
  # 効いている証拠にならない
  $ok = @{ RepoRoot = $root; CasesPath = $casesCopy; TargetsPath = $targetsCopy; JsRoot = $tmp }

  Write-Host '=== 通る側'
  Check '写した材料で突き合わせる' $ok $true ''

  Write-Host '=== 落ちる側（当てる先ごとに 1 つ ずつ壊す）'

  $scanStale = 'src/FsBulletML2.Playground/wwwroot/js/xmlscan-stale.js'
  Copy-Item -LiteralPath (Join-Path $tmp 'src/FsBulletML2.Playground/wwwroot/js/XmlScan.js') `
            -Destination (Join-Path $tmp $scanStale) -Force
  Mutate $scanStale 'add(" ctx=");' 'add(" where=");' '焼いた JS の中身が想定と違う'
  Check 'XmlScan の焼いた JS だけ答えが違う' `
    (With $ok @{ TargetsPath = (TargetsWith 'XmlScan' $scanStale) }) $false '違う答えを出した'

  $kindRel = 'src/FsBulletML2.Playground/wwwroot/js/FsBulletML2.LanguageService/SourceKind.js'
  $kindStale = 'src/FsBulletML2.Playground/wwwroot/js/FsBulletML2.LanguageService/sourcekind-stale.js'
  Copy-Item -LiteralPath (Join-Path $tmp $kindRel) -Destination (Join-Path $tmp $kindStale) -Force
  Mutate $kindStale '" all="' '" kinds="' 'SourceKind の焼いた JS の中身が想定と違う'
  Check 'SourceKind の焼いた JS だけ答えが違う' `
    (With $ok @{ TargetsPath = (TargetsWith 'SourceKind' $kindStale) }) $false '違う答えを出した'

  Write-Host '=== 材料が読めないときも落ちる'

  Check '焼いた JS が無い' `
    (With $ok @{ TargetsPath = (TargetsWith 'SourceKind' 'nowhere/Nope.js') }) $false '焼いた JS が無い'

  $emptyCases = Join-Path $tmp 'empty.json'
  [IO.File]::WriteAllText($emptyCases, '[]')
  Check '入力の表が空' (With $ok @{ CasesPath = $emptyCases }) $false '入力の表が空'

  Check '入力の表が無い' (With $ok @{ CasesPath = (Join-Path $tmp 'nope.json') }) $false '入力の表が無い'

  $emptyTargets = Join-Path $tmp 'empty-targets.json'
  [IO.File]::WriteAllText($emptyTargets, '[]')
  Check '当てる先の表が空' (With $ok @{ TargetsPath = $emptyTargets }) $false '当てる先の表が空'

  Check '当てる先の表が無い' `
    (With $ok @{ TargetsPath = (Join-Path $tmp 'nope-targets.json') }) $false '当てる先の表が無い'

  # **target を表に足しただけで、当てる入力が 1 件 も無い。**
  # 「引ける形にした」と「引けることを見た」は別で、前者だけで緑になるのが怖い
  $orphanPath = Join-Path $tmp 'targets-orphan.json'
  $orphan = @(Get-Content -LiteralPath $realTargets -Raw | ConvertFrom-Json)
  $orphan += [pscustomobject]@{
    target = 'Nobody'
    note   = '較正のためだけに足した、誰も当てていない target'
    js     = $targets[0].js
    export = $targets[0].export
  }
  [IO.File]::WriteAllText($orphanPath, (ConvertTo-Json -InputObject $orphan -Depth 5))
  Check 'target に当てる入力が 0 件' (With $ok @{ TargetsPath = $orphanPath }) `
    $false '当てる入力が 1 件 も無い'

  # `describe` の export を消す。**node 側が 0 行 を返す** ——
  # 突き合わせるものが無いのに緑にしてはいけない
  $noExport = 'src/FsBulletML2.Playground/wwwroot/js/FsBulletML2.LanguageService/sourcekind-noexport.js'
  Copy-Item -LiteralPath (Join-Path $tmp $kindRel) -Destination (Join-Path $tmp $noExport) -Force
  Mutate $noExport 'export function SourceKindModule_describe' 'function SourceKindModule_describe' `
    'export の綴りが想定と違う'
  Check '焼いた JS に口が無い' `
    (With $ok @{ TargetsPath = (TargetsWith 'SourceKind' $noExport) }) $false '答えが 0 行'

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
