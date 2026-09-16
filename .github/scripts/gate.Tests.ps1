#requires -Version 7
<#
.SYNOPSIS
  gate.ps1 の校正。

.DESCRIPTION
  **通る側と落ちる側を両方 置く。** 通ることだけ見ても、落ちるべきときに
  落ちるかは分からない —— 「いつも通る」壊れ方が緑のまま残る。

  見分けたい相手は skipped の 2 通り。選ばれなかったから skip（通す）と、
  手前が落ちて飛ばされた skip（通さない）が、結果の文字列としては同じ。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$gate = Join-Path (Split-Path -Parent $PSCommandPath) 'gate.ps1'
$one = '[{"name":"a","path":"x"}]'

# sh は「同梱 dll」、pa は「焼いた JS の突き合わせ」、ty は「型プロバイダを走らせる」。
# **どれも選ばれる側ではなく毎回 走る**ので、success 以外は誤り
$cases = @(
  @{ name = '全部 選ばれて全部 緑';     sel = 'success'; t = 'success';   b = 'success'; sh = 'success'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $true }
  @{ name = '試験 0 件・build だけ';    sel = 'success'; t = 'skipped';   b = 'success'; sh = 'success'; pa = 'success'; ty = 'success'; tp = '[]';  bp = $one;  want = $true }
  @{ name = 'md だけ（両方 0 件）';     sel = 'success'; t = 'skipped';   b = 'skipped'; sh = 'success'; pa = 'success'; ty = 'success'; tp = '[]';  bp = '[]';  want = $true }
  @{ name = '試験が落ちた';             sel = 'success'; t = 'failure';   b = 'success'; sh = 'success'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = 'build が落ちた';           sel = 'success'; t = 'success';   b = 'failure'; sh = 'success'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '選ばれたのに skip された'; sel = 'success'; t = 'skipped';   b = 'success'; sh = 'success'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '選ぶところが落ちた';       sel = 'failure'; t = 'skipped';   b = 'skipped'; sh = 'skipped'; pa = 'skipped'; ty = 'skipped'; tp = '';    bp = '';    want = $false }
  @{ name = '試験が cancel された';     sel = 'success'; t = 'cancelled'; b = 'success'; sh = 'success'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '同梱 dll が落ちた';        sel = 'success'; t = 'success';   b = 'success'; sh = 'failure'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '同梱 dll が skip された';  sel = 'success'; t = 'success';   b = 'success'; sh = 'skipped'; pa = 'success'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = 'md だけでも同梱 dll は走る'; sel = 'success'; t = 'skipped'; b = 'skipped'; sh = 'failure'; pa = 'success'; ty = 'success'; tp = '[]';  bp = '[]';  want = $false }
  @{ name = '突き合わせが落ちた';       sel = 'success'; t = 'success';   b = 'success'; sh = 'success'; pa = 'failure'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '突き合わせが skip された'; sel = 'success'; t = 'success';   b = 'success'; sh = 'success'; pa = 'skipped'; ty = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = 'md だけでも突き合わせは走る'; sel = 'success'; t = 'skipped'; b = 'skipped'; sh = 'success'; pa = 'failure'; ty = 'success'; tp = '[]'; bp = '[]'; want = $false }
  @{ name = '型プロバイダが落ちた';     sel = 'success'; t = 'success';   b = 'success'; sh = 'success'; pa = 'success'; ty = 'failure'; tp = $one;  bp = $one;  want = $false }
  @{ name = '型プロバイダが skip された'; sel = 'success'; t = 'success'; b = 'success'; sh = 'success'; pa = 'success'; ty = 'skipped'; tp = $one;  bp = $one;  want = $false }
  @{ name = 'md だけでも型プロバイダは走る'; sel = 'success'; t = 'skipped'; b = 'skipped'; sh = 'success'; pa = 'success'; ty = 'failure'; tp = '[]'; bp = '[]'; want = $false }
)

$fails = 0
foreach ($c in $cases) {
  $passed = $true
  try {
    & $gate -Select $c.sel -TestResult $c.t -BuildResult $c.b -ShippedResult $c.sh `
            -ParityResult $c.pa -TypeProvidersResult $c.ty `
            -PickedTests $c.tp -PickedBuilds $c.bp -Quiet
  } catch { $passed = $false }
  if ($passed -eq $c.want) {
    Write-Host ("  ok   {0}  -> {1}" -f $c.name, $(if ($passed) { '通る' } else { '落ちる' }))
  } else {
    $fails++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $c.name,
      $(if ($c.want) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
  }
}

Write-Host ''
if ($fails -gt 0) {
  Write-Host "校正 $($cases.Count) 通り 中 $fails 通り が外れた"
  exit 1
}
Write-Host "校正 $($cases.Count) 通り すべて一致"
