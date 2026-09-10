#requires -Version 7
<#
.SYNOPSIS
  guard-kept-controls.ps1 の校正。

.DESCRIPTION
  通る側と落ちる側を両方 置く。**落ちる側だけ見ていると「全部 落とす」壊れ方が
  緑のまま残る。**

  落とす形は 3 つ ——

      配列に在って html に無い
      checked で読む口が checkbox でない
      keptChecks に在って keptControls に無い

  **コメントの中の口は数えない**ことも見る。この repo の html はコメントが濃く、
  例示として `<input …>` が書いてあることが在る ——
  コメントを落とし忘れると、**在るはずの無い口が在ることになって**
  「html に無い」が黙って通る。

  最後に repo の現物へ当てる。**校正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-kept-controls.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("guard-kept-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null

$fails = 0
$count = 0

function Make {
  param([string]$Name, [string]$Text)
  $p = Join-Path $tmp $Name
  [IO.File]::WriteAllText($p, $Text)
  $p
}

function Check {
  param([string]$Name, [string]$Src, [string]$Html, [bool]$WantPass)
  $script:count++
  $passed = $true
  $msg = ''
  try { & $guard -RepoRoot $root -SourcePath $Src -HtmlPath $Html -Quiet }
  catch { $passed = $false; $msg = "$_" }
  if ($passed -ne $WantPass) {
    $script:fails++
    Write-Host ("  NG  {0}  （通るはず={1} / 実際={2}） {3}" -f $Name, $WantPass, $passed, $msg)
  } else {
    Write-Host ("  ok  {0}" -f $Name)
  }
}

$srcOk = Make 'ok.fs' @'
let private keptControls =
  [| "theme"; "trail"; "marks" |]

let private keptChecks = [| "trail"; "marks" |]
'@

$htmlOk = Make 'ok.html' @'
<select id="theme"></select>
<input id="trail" type="checkbox">
<input id="marks" type="checkbox" checked>
'@

Check '素の形は通る' $srcOk $htmlOk $true

# --- 1 配列に在って html に無い
$htmlMissing = Make 'missing.html' @'
<select id="theme"></select>
<input id="trail" type="checkbox">
'@
Check '覚える口が html に無いと落ちる' $srcOk $htmlMissing $false

# --- 2 checked で読む口が checkbox でない
$htmlWrongType = Make 'wrongtype.html' @'
<select id="theme"></select>
<input id="trail" type="checkbox">
<input id="marks" type="range">
'@
Check 'checked で読む口が checkbox でないと落ちる' $srcOk $htmlWrongType $false

# --- 3 keptChecks に在って keptControls に無い
$srcStray = Make 'stray.fs' @'
let private keptControls =
  [| "theme"; "trail" |]

let private keptChecks = [| "trail"; "marks" |]
'@
Check 'keptChecks が keptControls からはみ出すと落ちる' $srcStray $htmlOk $false

# --- 4 コメントの中の口は数えない
#     **落ちる側で測る。** コメントを落とし忘れると、この口が「在る」ことになり
#     上の 1 が通ってしまう
$htmlCommented = Make 'commented.html' @'
<select id="theme"></select>
<input id="trail" type="checkbox">
<!-- 例: <input id="marks" type="checkbox"> と書く -->
'@
Check 'コメントの中の口は在るうちに入らない' $srcOk $htmlCommented $false

# --- 5 配列が読めないなら黙って通さない
$srcBroken = Make 'broken.fs' @'
let private somethingElse = [| "theme" |]
'@
Check '配列が読めなければ落ちる' $srcBroken $htmlOk $false

# --- 6 本番の軸で走る
$script:count++
$passed = $true
try { & $guard -RepoRoot $root -Quiet } catch { $passed = $false; Write-Host "  $_" }
if (-not $passed) { $script:fails++; Write-Host '  NG  repo の現物が通らない' }
else { Write-Host '  ok  repo の現物が通る' }

Remove-Item -LiteralPath $tmp -Recurse -Force

Write-Host ''
Write-Host ("校正 {0} 点 / 外れ {1} 点" -f $count, $fails)
if ($fails -gt 0) { throw "校正が通らない: $fails 点" }
