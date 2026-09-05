#requires -Version 7
<#
.SYNOPSIS
  枝分かれした job をまとめて 1 つ の合否にする。

.DESCRIPTION
  必須チェックにするのはこれ 1 つ。matrix の job は選ばれた数だけ増減するので、
  名前を固定できない。

  **skipped を一律に成功として数えない。** skip には 2 通り あって

    選ばれなかったから skip   通してよい
    手前が落ちて飛ばされた     通してはいけない

  見分けは「選ばれたか」でつける。選ぶところが通っていなければ、その先の
  結果は全部 意味を持たないので即 落とす。
#>
[CmdletBinding()]
param(
  [string]$Select = $env:SELECT,
  [string]$TestResult = $env:TEST,
  [string]$BuildResult = $env:BUILD,
  [string]$ShippedResult = $env:SHIPPED,
  [string]$PickedTests = $env:TESTS,
  [string]$PickedBuilds = $env:BUILDS,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

if (-not $Quiet) {
  Write-Host "選ぶ     $Select"
  Write-Host "試験     $TestResult   （選ばれた: $PickedTests）"
  Write-Host "build    $BuildResult  （選ばれた: $PickedBuilds）"
  Write-Host "同梱 dll $ShippedResult  （選ばずに毎回 走る）"
}

if ($Select -ne 'success') { throw "選ぶところが $Select。先の結果は読めない" }

# **これは選ばれる側ではない。** 毎回 走るので、success 以外は全部 誤り
if ($ShippedResult -ne 'success') { throw "同梱 dll が $ShippedResult" }

foreach ($p in @(
    @{ Name = '試験';  Result = $TestResult;  Picked = $PickedTests },
    @{ Name = 'build'; Result = $BuildResult; Picked = $PickedBuilds })) {
  $wanted = $p.Picked -ne '[]' -and $p.Picked -ne ''
  if ($p.Result -eq 'success') { continue }
  if (-not $wanted -and $p.Result -eq 'skipped') { continue }
  throw "$($p.Name) が $($p.Result)（選ばれた: $($p.Picked)）"
}

if (-not $Quiet) { Write-Host '通った' }
