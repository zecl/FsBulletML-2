#requires -Version 7
<#
.SYNOPSIS
  guard-text.ps1 の校正。

.DESCRIPTION
  材料はバイト列で組む。**制御文字を字で書かない** —— この校正そのものが
  検査したい相手を持ち込む形になるので（それが元の不具合の入口だった）。

  通る側と落ちる側を両方 置く。許す 3 文字（TAB / LF / CR）が通ることも見る ——
  「全部 落とす」壊れ方は、落ちる側だけ見ていると緑のまま残る。

  最後に repo の現物へ当てる。**校正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-text.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("guard-text-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null

$fails = 0
$count = 0

function Make {
  param([string]$Name, [byte[]]$Bytes)
  $p = Join-Path $tmp $Name
  [IO.File]::WriteAllBytes($p, $Bytes)
  $p
}

function Check {
  param([string]$Name, [string[]]$Paths, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard -RepoRoot $root -Files $Paths -Quiet } catch { $passed = $false; $msg = "$_" }

  if ($passed -ne $WantPass) {
    $script:fails++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $Name,
      $(if ($WantPass) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
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

# 'ab' + LF + 'cd'
$clean = [byte[]]@(0x61, 0x62, 0x0a, 0x63, 0x64)
# TAB と CR も混ぜたもの
$allowed = [byte[]]@(0x61, 0x09, 0x62, 0x0d, 0x0a, 0x63)
# 2 行目 の 2 桁目 に NUL
$withNul = [byte[]]@(0x61, 0x62, 0x0a, 0x63, 0x00, 0x64)
# DEL
$withDel = [byte[]]@(0x61, 0x7f, 0x62)
# BEL（PowerShell の二重引用符でバッククォート ＋ a を書くと入る側）
$withBel = [byte[]]@(0x61, 0x07, 0x62)

try {
  Write-Host '=== 通る側'
  Check '素のテキスト' @((Make 'clean.ps1' $clean)) $true ''
  Check '許す 3 文字（TAB / LF / CR）は通る' @((Make 'allowed.fs' $allowed)) $true ''
  Check 'binary の拡張子は見ない' @((Make 'art.png' $withNul)) $true ''

  Write-Host '=== 落ちる側'
  Check 'NUL' @((Make 'nul.ps1' $withNul)) $false 'NUL（U+0000）'
  Check 'DEL' @((Make 'del.fs' $withDel)) $false 'DEL（U+007F）'
  Check 'BEL' @((Make 'bel.md' $withBel)) $false 'BEL（U+0007）'

  Write-Host '=== 位置と、字をそのまま出さないこと'
  $count++
  $msg = ''
  try { & $guard -RepoRoot $root -Files @((Make 'pos.ps1' $withNul)) -Quiet } catch { $msg = "$_" }
  $okPos = $msg -match '2 行 2 桁'
  # 報告の中に生の制御文字が残っていたら、ログのほうが壊れる
  $okSafe = -not ([regex]::IsMatch($msg, '[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]'))
  if ($okPos -and $okSafe) { Write-Host '  ok   行と桁を出し、字は出さない' }
  else {
    $fails++
    Write-Host '  NG   行と桁を出し、字は出さない'
    if (-not $okPos) { Write-Host '         2 行 2 桁 が出ていない' }
    if (-not $okSafe) { Write-Host '         報告の中に生の制御文字が残っている' }
  }

  # **-Files を渡さないで呼ぶ。** 空配列を渡すと 0 件 を走査して緑になるだけで、
  # git から一覧を引く経路（本番で通る側）を一度も通らない
  Write-Host '=== 本番の軸'
  $count++
  try {
    & $guard -RepoRoot $root -Quiet
    Write-Host '  ok   repo の追跡ファイル全部  -> 通る'
  } catch {
    $fails++
    Write-Host '  NG   repo の追跡ファイル全部  -> 落ちた'
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
