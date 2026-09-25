#requires -Version 7
<#
.SYNOPSIS
  焼いたパッケージを dotnet-validate に当てて、見るべき 2 つ だけで合否を決める。

.DESCRIPTION
  dotnet-validate は 3 つ 見る。ここで落とすのは上の 2 つ。

      Source Link     ✅ でなければ落とす
      Deterministic   ✅ でなければ落とす（CI と同じく ContinuousIntegrationBuild で焼くこと）
      Compiler Flags  見ない

  Compiler Flags は、コンパイラの設定を PDB に書き込む C# の機能を見ている。
  F# のコンパイラは書き込まないので、F# のパッケージは設定では ✅ にならない。
  dotnet-validate はこの 1 つ で exit 1 を返すので、終了コードでは合否を決められない。

.PARAMETER Path
  .nupkg の置き場。既定は artifacts/packages。

.EXAMPLE
  $env:GITHUB_ACTIONS = 'true'; ./nuget/pack.ps1; ./nuget/validate.ps1
#>
[CmdletBinding()]
param([string]$Path)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
if (-not $Path) { $Path = Join-Path $root 'artifacts/packages' }

$packages = @(Get-ChildItem -LiteralPath $Path -Filter '*.nupkg')
if ($packages.Count -eq 0) { throw "$Path に .nupkg が無い（先に nuget/pack.ps1）" }

$fails = 0
foreach ($p in $packages) {
  $out = dotnet tool run dotnet-validate package local $p.FullName 2>&1 | Out-String
  $sourceLink = $out -match '•\s*Source Link:\s*✅'
  $deterministic = $out -match '•\s*Deterministic \(dll/exe\):\s*✅'
  $mark = { param($ok) if ($ok) { 'ok' } else { 'NG' } }
  Write-Host ("{0,-44} SourceLink {1}  Deterministic {2}" -f $p.Name, (& $mark $sourceLink), (& $mark $deterministic))
  if (-not ($sourceLink -and $deterministic)) {
    $fails++
    Write-Host $out
  }
}

if ($fails -gt 0) { throw "$fails 本 が通らなかった" }
Write-Host "$($packages.Count) 本 とも通った"
