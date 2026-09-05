#requires -Version 7
<#
.SYNOPSIS
  affected.ps1 の校正。

.DESCRIPTION
  **この repo の実在するパスで目盛りを合わせる。** 作り物のパスで通しても、
  本番で動く軸（.slnx と各 proj の参照）の上を通っていないので校正にならない。

  合わせる点は 3 種。

    多いほう  Core / TestData / global.json を触ると全部 走る
    少ないほう Dsl だけ・MonoGame だけを触ると 1 本 しか走らない
    0 件      md だけなら走らない。ただし bench だけなら build が 1 本 残る

  少ないほうが要る。多いほうだけだと「常に全部 返す」壊れ方が緑のまま通る。

  期待は集合の一致で見る（含むではなく）。**選びすぎも落ちる。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$here = Split-Path -Parent $PSCommandPath
$affected = Join-Path $here 'affected.ps1'
$root = (git -C $here rev-parse --show-toplevel)

$fails = 0
$count = 0

function Check {
  param(
    [string]$Name,
    [string[]]$Files,
    [string[]]$Tests,
    [string[]]$Builds
  )
  $script:count++
  $r = & $affected -ChangedFiles $Files -RepoRoot $root -Quiet
  $gotT = @($r.Tests | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_) } | Sort-Object)
  $gotB = @($r.Builds | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_) } | Sort-Object)
  $wantT = @($Tests | Sort-Object)
  $wantB = @($Builds | Sort-Object)
  $okT = ($gotT -join ',') -eq ($wantT -join ',')
  $okB = ($gotB -join ',') -eq ($wantB -join ',')
  if ($okT -and $okB) {
    Write-Host "  ok   $Name"
  } else {
    $script:fails++
    Write-Host "  NG   $Name"
    if (-not $okT) { Write-Host "         試験  期待 [$($wantT -join ', ')]  実際 [$($gotT -join ', ')]" }
    if (-not $okB) { Write-Host "         build 期待 [$($wantB -join ', ')]  実際 [$($gotB -join ', ')]" }
  }
}

function CheckThrows {
  param([string]$Name, [string[]]$Files)
  $script:count++
  try {
    & $affected -ChangedFiles $Files -RepoRoot $root -Quiet | Out-Null
    $script:fails++
    Write-Host "  NG   $Name  （落ちるはずが通った）"
  } catch {
    Write-Host "  ok   $Name"
  }
}

$allTests = @(
'FsBulletML2.Core.Tests', 'FsBulletML2.Dsl.Tests', 'FsBulletML2.Front.Tests',
'FsBulletML2.MonoGame.Tests', 'FsBulletML2.Parser.Tests', 'FsBulletML2.TypeProviders.Tests',
'FsBulletML2.Unity2D.Tests')
$allBuilds = @(
  'FsBulletML2.Benchmarks',
  'FsBulletML2.Sample.MonoGame.CSharp', 'FsBulletML2.Sample.MonoGame.FSharp',
  'FsBulletML2.Sample.TypeProviders.Debug',
  'FsBulletML2.Sample.Unity2D.CSharp.Compile', 'FsBulletML2.Sample.Unity2D.FSharp')

Write-Host '=== 多いほう'

Check 'Core を触ると全部' `
  @('src/FsBulletML2.Core/Step.fs') $allTests $allBuilds

Check 'TestData は proj の外なので全部' `
  @('tests/TestData/xml/accel/elements/success/accel-horizontal-exist.xml') $allTests $allBuilds

Check 'global.json は全部' @('global.json') $allTests $allBuilds

Check 'slnx を触ると全部' @('FsBulletML2.slnx') $allTests $allBuilds

# Unity の C# は Assets に在り、その中に csproj は無い（Unity が生成する側は
# 追跡していない）。compile するのは隣の `.Compile` で、あちらは
# `<Compile Include="..\...\Assets\Scripts\**\*.cs" />` で引いている。
# **割り当ては場所で決めるので、この変更は不明に落ちて全部 走る** ——
# `.Compile` もその「全部」に入るので、compile はされる。
Check 'Unity の Assets は割り当て不明。全部 走るので .Compile も入る' `
  @('samples/FsBulletML2.Sample.Unity2D.CSharp/Assets/Scripts/FrontEnv.cs') $allTests $allBuilds

# **.Compile 自身を触ったら、それ 1 本 だけ。** 何も参照していないので
Check '.Compile 自身なら 1 本 だけ' `
  @('samples/FsBulletML2.Sample.Unity2D.CSharp.Compile/R3UnityShim.cs') `
  @() @('FsBulletML2.Sample.Unity2D.CSharp.Compile')

Write-Host '=== 少ないほう'

# Dsl は samples/Bullets.Dsl 経由でサンプル 4 つ に届く。**試験は 1 本 だが
# build は 4 本 残る** —— 弾幕 DSL を変えると弾幕定義のほうが先に壊れるので
Check 'Dsl だけなら試験は Dsl.Tests だけ、build は弾幕を使うサンプル 4 つ' `
  @('src/FsBulletML2.Dsl/BulletDsl.fs') `
  @('FsBulletML2.Dsl.Tests') `
  @('FsBulletML2.Sample.MonoGame.CSharp', 'FsBulletML2.Sample.MonoGame.FSharp',
    'FsBulletML2.Sample.Unity2D.CSharp.Compile', 'FsBulletML2.Sample.Unity2D.FSharp')

Check 'MonoGame だけなら MonoGame.Tests と、それが build しないサンプル 2 つ' `
  @('src/FsBulletML2.MonoGame/Manager.fs') `
  @('FsBulletML2.MonoGame.Tests') `
  @('FsBulletML2.Sample.MonoGame.CSharp', 'FsBulletML2.Sample.MonoGame.FSharp')

#
# **MonoGame.Tests は Parser を引くようになった。** フロントを実際に回す門
# （FrontRun.fs）が弾幕を XML から読むため —— 本番のサンプルと同じ口。
# 以前はここも「走らない」側だった。**この 1 行 が赤くなって気づいた。**
# **ここが本体。** Parser を触っても Dsl.Tests は走らない。
Check 'Parser だけなら Dsl.Tests は走らない' `
  @('src/FsBulletML2.Parser/Sxml.fs') `
  @('FsBulletML2.Core.Tests', 'FsBulletML2.Front.Tests', 'FsBulletML2.MonoGame.Tests', 'FsBulletML2.Parser.Tests', 'FsBulletML2.TypeProviders.Tests', 'FsBulletML2.Unity2D.Tests') `
  @('FsBulletML2.Benchmarks', 'FsBulletML2.Sample.MonoGame.CSharp',
    'FsBulletML2.Sample.MonoGame.FSharp', 'FsBulletML2.Sample.TypeProviders.Debug')

Check 'TypeProviders だけなら 1 本' `
  @('src/FsBulletML2.TypeProviders/Impl.fs') `
  @('FsBulletML2.TypeProviders.Tests') `
  @('FsBulletML2.Sample.TypeProviders.Debug')

Check '試験そのものを触ったらその試験だけ' `
  @('tests/FsBulletML2.Parser.Tests/ReadEntryPoints.fs') @('FsBulletML2.Parser.Tests') @()

Check 'サンプルの弾幕は Dsl.Tests が見ている' `
  @('samples/FsBulletML2.Bullets/Bullets.fs') @('FsBulletML2.Dsl.Tests') @()

Check '弾幕 DSL 版はサンプル 4 つ にも届く' `
  @('samples/FsBulletML2.Bullets.Dsl/Bullets.fs') `
  @('FsBulletML2.Dsl.Tests') `
  @('FsBulletML2.Sample.MonoGame.CSharp', 'FsBulletML2.Sample.MonoGame.FSharp',
    'FsBulletML2.Sample.Unity2D.CSharp.Compile', 'FsBulletML2.Sample.Unity2D.FSharp')

Write-Host '=== git が返す名前'

# 弾幕の XML は日本語名。git は既定（core.quotepath）で
# `"samples/.../\343\202\257.xml"` に化かすので、化けたまま渡ると
# どのプロジェクトの前置きにも当たらず「全部 走らせる」に落ちる。
# **化けても答えは出る（安全側）ので、選べていないことに気づけない。**
$count++
$jpAll = @(git -C $root -c core.quotepath=false ls-files 'samples/FsBulletML2.Sample.MonoGame.CSharp/Content/xml/*')
$jp = @($jpAll | Where-Object { $_ -match '[^\x00-\x7F]' })
$quoted = @($jp | Where-Object { $_.StartsWith('"') })
if ($jp.Count -eq 0) {
  $fails++
  Write-Host '  NG   日本語名の弾幕 XML が 1 つ も見つからない（校正の材料が消えた）'
} elseif ($quoted.Count -gt 0) {
  $fails++
  Write-Host "  NG   git が $($quoted.Count) 件 を化かしたまま返した"
} else {
  Write-Host "  ok   日本語名 $($jp.Count) 件 が化けずに返る"
}

if ($jp.Count -gt 0) {
  Check '日本語名のサンプル素材はそのサンプルに割り当たる' `
    @($jp[0]) @() @('FsBulletML2.Sample.MonoGame.CSharp')
}

Write-Host '=== 0 件 のあつかい'

Check 'md だけなら何も走らない' @('README.md', 'tests/README.md') @() @()

Check 'ライセンス本文・出力済みドキュメント・パッケージの bat は走らせない' `
  @('license/bulletml/readme.txt', 'docs/templates/template-file.html',
    'nuget/FsBulletML2.Core.package.bat') @() @()

Check 'bench だけなら試験 0・build 1' `
  @('bench/FsBulletML2.Benchmarks/Program.fs') @() @('FsBulletML2.Benchmarks')

CheckThrows '変更 0 件 は落ちる（起点の取り違え）' @()

Write-Host '=== matrix に渡る形'

# **数だけ見ても足りない。** `[[{..}]]` は 1 要素 の配列として数えられるが、
# matrix に渡すと project.name が空になって、名前の無い job が黙って通る。
# 中身が name / path を持つオブジェクトであることまで見る。
function CheckShape {
  param([string]$Name, [string[]]$Files, [int]$Tests, [int]$Builds)
  $script:count++
  $tmp = [IO.Path]::GetTempFileName()
  try {
    $env:GITHUB_OUTPUT = $tmp
    & $affected -ChangedFiles $Files -RepoRoot $root -Quiet | Out-Null
  } finally { $env:GITHUB_OUTPUT = $null }

  $bad = [System.Collections.Generic.List[string]]::new()
  $want = @{ tests = $Tests; builds = $Builds }
  foreach ($line in (Get-Content -LiteralPath $tmp)) {
    $k, $v = $line -split '=', 2
    if ($k -notin 'tests', 'builds') { continue }
    # **-NoEnumerate が要る。** 付けないと外側の配列が代入でほどけて、
    # `[[{..}]]` が `[{..}]` と同じ形に見える —— 見分けたい相手が消える
    $parsed = $null
    try { $parsed = ConvertFrom-Json $v -NoEnumerate } catch { $bad.Add("$k が JSON として読めない: $v"); continue }
    if ($parsed -isnot [array]) { $bad.Add("$k の外側が配列でない: $v"); continue }
    $items = $parsed
    if ($items.Count -ne $want[$k]) { $bad.Add("$k の数 期待 $($want[$k]) 実際 $($items.Count)") }
    foreach ($it in $items) {
      if ($it -isnot [pscustomobject]) { $bad.Add("$k の要素がオブジェクトでない: $($it.GetType().Name)"); continue }
      foreach ($f in 'name', 'path') {
        if (-not $it.PSObject.Properties.Name.Contains($f)) { $bad.Add("$k の要素に $f が無い") }
        elseif ([string]::IsNullOrWhiteSpace($it.$f)) { $bad.Add("$k の要素の $f が空") }
      }
    }
  }
  Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue

  if ($bad.Count -eq 0) { Write-Host "  ok   $Name" }
  else {
    $script:fails++
    Write-Host "  NG   $Name"
    foreach ($b in $bad) { Write-Host "         $b" }
  }
}

CheckShape '0 件 でも空配列' @('README.md') 0 0
CheckShape '1 件 が配列のまま出る' @('tests/FsBulletML2.Parser.Tests/ReadEntryPoints.fs') 1 0
CheckShape '複数' @('src/FsBulletML2.Parser/Sxml.fs') 6 4

Write-Host ''
if ($fails -gt 0) {
  Write-Host "校正 $count 点 中 $fails 点 が外れた"
  exit 1
}
Write-Host "校正 $count 点 すべて一致"
