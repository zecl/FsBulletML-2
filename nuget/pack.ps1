#requires -Version 7
<#
.SYNOPSIS
  配るパッケージを焼いて、表と 1 対 1 で一致するかを見る。

.DESCRIPTION
  slnx を丸ごと pack しない。下の表に並べた fsproj を 1 本 ずつ pack する ——
  表に無いものは出ない。

  出たものを数えて、表と 1 対 1 で一致しなければ落とす（多い も 少ない も）。
  同じ PackageId が 2 つ 出たときも落とす。

  版は Directory.Build.props の <Version> 1 か所。ここでは決めない。

.PARAMETER Output
  焼き先。既定は artifacts/packages。走らせるたびに空にする。

.PARAMETER Configuration
  既定は Release。

.EXAMPLE
  ./nuget/pack.ps1
#>
[CmdletBinding()]
param(
  [string]$Output,
  [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
if (-not $Output) { $Output = Join-Path $root 'artifacts/packages' }

# 配るもの。足すときは fsproj に IsPackable=true / PackageId / Description も書く
$Packages = @(
  @{ Id = 'FsBulletML2.Core';   Project = 'src/FsBulletML2.Core/FsBulletML2.Core.fsproj' }
  @{ Id = 'FsBulletML2.Parser'; Project = 'src/FsBulletML2.Parser/FsBulletML2.Parser.fsproj' }
  @{ Id = 'FsBulletML2.Front';  Project = 'src/FsBulletML2.Front/FsBulletML2.Front.fsproj' }
  @{ Id = 'FsBulletML2.Dsl';    Project = 'src/FsBulletML2.Dsl/FsBulletML2.Dsl.fsproj' }
  @{ Id = 'FsBulletML2.TypeProviders'; Project = 'src/FsBulletML2.TypeProviders/FsBulletML2.TypeProviders.fsproj' }
)

if (Test-Path -LiteralPath $Output) { Remove-Item -LiteralPath $Output -Recurse -Force }
New-Item -ItemType Directory -Path $Output | Out-Null

foreach ($p in $Packages) {
  Write-Host "== $($p.Id)"
  dotnet pack (Join-Path $root $p.Project) -c $Configuration -o $Output -nologo
  if ($LASTEXITCODE -ne 0) { throw "$($p.Id) の pack が落ちた（exit $LASTEXITCODE）" }
}

# 版は props から読む。nupkg の名前は <Id>.<Version>.nupkg
[xml]$props = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw
$version = @($props.Project.PropertyGroup | ForEach-Object { $_.Version } | Where-Object { $_ })[0]
if (-not $version) { throw 'Directory.Build.props に <Version> が無い' }

$expected = $Packages | ForEach-Object { "$($_.Id).$version" } | Sort-Object
$found = @(Get-ChildItem -LiteralPath $Output -Filter '*.nupkg' | ForEach-Object { $_.BaseName } | Sort-Object)
$symbols = @(Get-ChildItem -LiteralPath $Output -Filter '*.snupkg' | ForEach-Object { $_.BaseName } | Sort-Object)

$fails = @()
$missing = @($expected | Where-Object { $_ -notin $found })
$extra = @($found | Where-Object { $_ -notin $expected })
$noSymbols = @($expected | Where-Object { $_ -notin $symbols })
if ($missing.Count -gt 0) { $fails += "出なかった: $($missing -join ', ')" }
if ($extra.Count -gt 0) { $fails += "表に無いものが出た: $($extra -join ', ')" }
if ($noSymbols.Count -gt 0) { $fails += ".snupkg が無い: $($noSymbols -join ', ')" }
$dupIds = @($Packages | Group-Object { $_.Id } | Where-Object Count -gt 1 | ForEach-Object Name)
if ($dupIds.Count -gt 0) { $fails += "表に同じ Id が 2 つ: $($dupIds -join ', ')" }

Write-Host ''
Write-Host "版        $version"
Write-Host "出たもの  $($found.Count) 本 / 表 $($expected.Count) 本"
foreach ($f in $found) { Write-Host "  $f" }

if ($fails.Count -gt 0) {
  foreach ($f in $fails) { Write-Host "NG  $f" }
  throw '表と一致しない'
}
Write-Host '表と 1 対 1 で一致'
