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

$cases = @(
  @{ name = '全部 選ばれて全部 緑';     sel = 'success'; t = 'success';   b = 'success'; tp = $one;  bp = $one;  want = $true }
  @{ name = '試験 0 件・build だけ';    sel = 'success'; t = 'skipped';   b = 'success'; tp = '[]';  bp = $one;  want = $true }
  @{ name = 'md だけ（両方 0 件）';     sel = 'success'; t = 'skipped';   b = 'skipped'; tp = '[]';  bp = '[]';  want = $true }
  @{ name = '試験が落ちた';             sel = 'success'; t = 'failure';   b = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = 'build が落ちた';           sel = 'success'; t = 'success';   b = 'failure'; tp = $one;  bp = $one;  want = $false }
  @{ name = '選ばれたのに skip された'; sel = 'success'; t = 'skipped';   b = 'success'; tp = $one;  bp = $one;  want = $false }
  @{ name = '選ぶところが落ちた';       sel = 'failure'; t = 'skipped';   b = 'skipped'; tp = '';    bp = '';    want = $false }
  @{ name = '試験が cancel された';     sel = 'success'; t = 'cancelled'; b = 'success'; tp = $one;  bp = $one;  want = $false }
)

$fails = 0
foreach ($c in $cases) {
  $passed = $true
  try {
    & $gate -Select $c.sel -TestResult $c.t -BuildResult $c.b `
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
