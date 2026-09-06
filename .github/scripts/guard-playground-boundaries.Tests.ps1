#requires -Version 7
<#
.SYNOPSIS
  guard-playground-boundaries.ps1 の較正。

.DESCRIPTION
  線を 1 つ ずつ、**通る側と落ちる側**で当てる。
  落ちる側だけ見ていると「常に赤」の壊れ方が、通る側だけ見ていると
  「何も見ていない」壊れ方が、それぞれ緑のまま残る。

  **「材料が読めない」も落ちること**を見る。これがいちばん怖い形 ——
  当てる名前が 0 個、走査するファイルが 0 本 は、違反 0 件 と同じ顔をする。
  実際に一度 やった（`"$prefix`fable"` の `` `f `` が改ページになり、
  Fable のソースを 1 本 も読まずに緑を出した）。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-playground-boundaries.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("pg-bound-" + [Guid]::NewGuid().ToString('N'))

$fails = 0
$count = 0

function Make {
  param([string]$Rel, [string]$Text)
  $p = Join-Path $tmp $Rel
  New-Item -ItemType Directory -Path (Split-Path -Parent $p) -Force | Out-Null
  [IO.File]::WriteAllText($p, $Text)
  $p
}

# 既定に上書きを重ねた新しいハッシュ。**+ は重複キーで落ちる**
function With {
  param([hashtable]$Base, [hashtable]$Over)
  $h = @{}
  foreach ($k in $Base.Keys) { $h[$k] = $Base[$k] }
  foreach ($k in $Over.Keys) { $h[$k] = $Over[$k] }
  $h
}

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
  # 差し替え用の材料。既定はどれも「線の内側」
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null
  $dtd = Make 'core/DTD.fs' @'
writer.WriteStartElement("bulletml")
writer.WriteStartElement("action")
writer.WriteStartElement("vanish")
'@
  $proj = Make 'core/Core.fsproj' '<Project><ItemGroup><Compile Include="DTD.fs" /></ItemGroup></Project>'
  $html = Make 'pg/wwwroot/index.html' '<script type="module">import "./js/Playground.js";</script>'
  Make 'pg/fable/Playground.fs' 'let el (id: string) = document.getElementById id' | Out-Null
  $pgProj = Make 'pg/Playground.fsproj' `
    '<Project><PropertyGroup><UseSystemResourceKeys>false</UseSystemResourceKeys></PropertyGroup></Project>'

  $ok = @{ RepoRoot = $tmp; PlaygroundDir = 'pg'; CoreProj = $proj; DtdSource = $dtd; IndexHtml = $html
           PlaygroundProj = $pgProj; Files = @('pg/fable/Playground.fs') }

  Write-Host '=== 通る側'
  Check '線の内側' $ok $true ''

  Write-Host '=== 落ちる側'

  Check '追跡された .js が在る' (With $ok @{ Files = @('pg/wwwroot/js/glue.js') }) $false '追跡された .js'

  $badProj = Make 'core/Bad.fsproj' '<Project><ItemGroup><PackageReference Include="Bolero" /></ItemGroup></Project>'
  Check 'Core が Bolero を参照している' (With $ok @{ CoreProj = $badProj }) $false 'Core が表示側を参照している'

  # **要素名は DTD から引く。** ここでは vanish を Fable 側に書いてみる
  $badFable = Join-Path $tmp 'bad/fable'
  Make 'bad/fable/Xml.fs' 'let root = "vanish"' | Out-Null
  Make 'bad/wwwroot/index.html' '<script type="module">import "./js/Playground.js";</script>' | Out-Null
  Check 'Fable 側に要素名が在る' `
    (With $ok @{ PlaygroundDir = 'bad'; IndexHtml = (Join-Path $tmp 'bad/wwwroot/index.html'); Files = @() }) `
    $false 'Fable 側に要素名'

  $badHtml = Make 'pg/wwwroot/bad.html' '<script>Blazor.start().catch(function (e) {});</script>'
  Check 'html に起動のロジックが在る' (With $ok @{ IndexHtml = $badHtml }) $false 'html に起動のロジック'

  # **書き忘れと、書いてあるが逆を、別々に当てる。** 既定は SDK 側が立てるので、
  # 「無い」は「true と書いた」と同じ結果になる —— 門の側では別の壊れ方
  $noKeys = Make 'pg/NoKeys.fsproj' '<Project><PropertyGroup /></Project>'
  Check 'UseSystemResourceKeys が無い' (With $ok @{ PlaygroundProj = $noKeys }) `
    $false 'UseSystemResourceKeys が csproj に無い'

  $trueKeys = Make 'pg/TrueKeys.fsproj' `
    '<Project><PropertyGroup><UseSystemResourceKeys>true</UseSystemResourceKeys></PropertyGroup></Project>'
  Check 'UseSystemResourceKeys が true' (With $ok @{ PlaygroundProj = $trueKeys }) `
    $false 'UseSystemResourceKeys が false でない'

  Write-Host '=== 材料が読めないときも落ちる'
  # **ここが本体。** 0 件 は違反 0 件 と同じ顔をする
  Check '当てる要素名が 0 個' (With $ok @{ DtdSource = (Make 'core/Empty.fs' 'なにも書いていない') }) `
    $false '当てる要素名を 1 つ も引けなかった'
  Check 'Fable のソースが 0 本' (With $ok @{ PlaygroundDir = 'nowhere' }) `
    $false 'Fable のソースを 1 本 も読めなかった'
  Check 'csproj が読めない' (With $ok @{ PlaygroundProj = (Join-Path $tmp 'pg/Nope.fsproj') }) `
    $false 'Playground の csproj を読めなかった'

  Write-Host '=== 本番の軸'
  # 差し替えを 1 つ も渡さないで呼ぶ。渡した数点が緑でも、
  # git から一覧を引く経路（本番で通る側）を通らない
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
