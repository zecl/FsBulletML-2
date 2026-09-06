#requires -Version 7
<#
.SYNOPSIS
  guard-fable-parity.ps1 の較正。

.DESCRIPTION
  **通る側と落ちる側**を両方 当てる。落ちる側だけ見ていると「常に赤」の
  壊れ方が、通る側だけ見ていると「何も見ていない」壊れ方が、それぞれ
  緑のまま残る。

  いちばん怖いのは**材料が読めないとき** —— 焼いた JS が無い、表が空、
  片方 の答えが 0 行。どれも「食い違い 0 件」と同じ顔をする。

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

try {
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null

  $realJsDir = Join-Path $root 'src/FsBulletML2.Playground/wwwroot/js'
  if (-not (Test-Path -LiteralPath (Join-Path $realJsDir 'XmlScan.js'))) {
    throw "較正の材料が無い。先に dotnet build src/FsBulletML2.Playground で焼くこと"
  }
  # **相対 import ごと写す。** XmlScan.js は fable_modules を ./ で引く
  $jsCopy = Join-Path $tmp 'js'
  Copy-Item -LiteralPath $realJsDir -Destination $jsCopy -Recurse -Force
  $copiedScan = Join-Path $jsCopy 'XmlScan.js'

  $realCases = Join-Path $root '.github/scripts/fable-parity-cases.json'
  $casesCopy = Join-Path $tmp 'cases.json'
  Copy-Item -LiteralPath $realCases -Destination $casesCopy -Force

  # **既定と違う場所で測る。** 既定のままだと「渡せるようにした」ことが
  # 効いている証拠にならない
  $ok = @{ RepoRoot = $root; CasesPath = $casesCopy; JsPath = $copiedScan }

  Write-Host '=== 通る側'
  Check '写した材料で突き合わせる' $ok $true ''

  Write-Host '=== 落ちる側'

  # 焼いた JS のほうだけ答えを変える。**.NET は元のソースを load するので
  # 変わらない** —— これが「焼き忘れ」と同じ形
  # **js の並びの中に置く。** XmlScan.js は fable_modules を ./ で引くので、
  # 外へ置くと import が解けず「答えが 0 行」で落ちて、狙った点と別の赤になる
  $stale = Join-Path $jsCopy 'stale.js'
  $text = [IO.File]::ReadAllText($copiedScan)
  # **大小だけの差にしない。** `-eq` も `-ne` も既定で大小を無視するので、
  # `ctx` -> `CTX` の変異は「変えたのに変わっていない」に見える（実際に踏んだ）
  $patched = $text.Replace('add(" ctx=");', 'add(" where=");')
  if ($patched -ceq $text) { throw '較正が当たらなかった（焼いた JS の中身が想定と違う）' }
  [IO.File]::WriteAllText($stale, $patched)
  Check '焼いた JS だけ答えが違う' @{ RepoRoot = $root; CasesPath = $casesCopy; JsPath = $stale } `
    $false '違う答えを出した'

  Write-Host '=== 材料が読めないときも落ちる'

  Check '焼いた JS が無い' @{ RepoRoot = $root; CasesPath = $casesCopy; JsPath = (Join-Path $tmp 'nope.js') } `
    $false '焼いた JS が無い'

  $emptyCases = Join-Path $tmp 'empty.json'
  [IO.File]::WriteAllText($emptyCases, '[]')
  Check '表が空' @{ RepoRoot = $root; CasesPath = $emptyCases; JsPath = $copiedScan } `
    $false '入力の表が空'

  Check '表が無い' @{ RepoRoot = $root; CasesPath = (Join-Path $tmp 'nope.json'); JsPath = $copiedScan } `
    $false '入力の表が無い'

  # `XmlScan_describe` を消す。**node 側が 0 行 を返す** ——
  # 突き合わせるものが無いのに緑にしてはいけない
  $noDescribe = Join-Path $jsCopy 'nodescribe.js'
  $text2 = [IO.File]::ReadAllText($copiedScan)
  $patched2 = $text2.Replace('export function XmlScan_describe', 'function XmlScan_describe')
  if ($patched2 -ceq $text2) { throw '較正が当たらなかった（export の綴りが想定と違う）' }
  [IO.File]::WriteAllText($noDescribe, $patched2)
  Check '焼いた JS に口が無い' @{ RepoRoot = $root; CasesPath = $casesCopy; JsPath = $noDescribe } `
    $false '答えが 0 行'

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
