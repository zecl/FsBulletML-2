#requires -Version 7
<#
.SYNOPSIS
  追跡されている proj が .slnx に載っているかを見る。

.DESCRIPTION
  CI の対象は .slnx から引いている。**載せ忘れたプロジェクトは、CI から見ると
  最初から存在しない** —— build もされず、試験も選ばれず、それでも緑になる。
  安全網が守る対象を落としたまま緑を出す形なので、ここで止める。

  逆向き（slnx が指す proj が無い）は affected.ps1 が読むときに落ちる。
#>
[CmdletBinding()]
param([string]$RepoRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = $RepoRoot -replace '\\', '/'

# slnx に無くてよいもの。理由を書いて、affected.ps1 の同じ並びと揃える。
$OutOfScope = @()

function NormalizePath([string]$p) {
  $out = [System.Collections.Generic.List[string]]::new()
  foreach ($seg in ($p -replace '\\', '/').Split('/')) {
    if ($seg -eq '' -or $seg -eq '.') { continue }
    if ($seg -eq '..') { if ($out.Count -gt 0) { $out.RemoveAt($out.Count - 1) }; continue }
    $out.Add($seg)
  }
  $out -join '/'
}

$inSlnx = [System.Collections.Generic.HashSet[string]]::new()
foreach ($s in (Get-ChildItem -LiteralPath $RepoRoot -Filter '*.slnx' -File)) {
  $doc = [xml](Get-Content -LiteralPath $s.FullName -Raw)
  foreach ($n in $doc.SelectNodes('//Project')) { [void]$inSlnx.Add((NormalizePath $n.GetAttribute('Path'))) }
}

$tracked = @(git -C $RepoRoot ls-files '*.fsproj' '*.csproj' | ForEach-Object { NormalizePath $_ })

$missing = @($tracked | Where-Object {
  $p = $_
  if ($inSlnx.Contains($p)) { return $false }
  foreach ($o in $OutOfScope) { if ($p.StartsWith($o)) { return $false } }
  $true
})

Write-Host "追跡 $($tracked.Count) 本 / slnx $($inSlnx.Count) 本 / 対象外 $($OutOfScope.Count) 並び"

if ($missing.Count -gt 0) {
  Write-Host ''
  Write-Host 'slnx に載っていない proj が在る。CI はこれを一度も build しない:'
  foreach ($m in $missing) { Write-Host "    $m" }
  Write-Host ''
  Write-Host '.slnx に足すか、理由を添えて guard-slnx.ps1 と affected.ps1 の OutOfScope に足すこと'
  exit 1
}

Write-Host 'slnx から漏れている proj は無い'
