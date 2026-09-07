#requires -Version 7
<#
.SYNOPSIS
  guard-published-boot.ps1 の較正。

.DESCRIPTION
  **通る側と落ちる側**を両方 当てる。落ちる側だけ見ていると「常に赤」の
  壊れ方が、通る側だけ見ていると「何も見ていない」壊れ方が、それぞれ
  緑のまま残る。

  **publish は 1 回 しかしない。** 落ちる側は、その成果物の写しを変異させて作る
  —— publish は 2 分 かかるので、点の数だけ回すと較正が回らなくなる。

  変異は「実際に踏んだ形」から取る ——
  指紋が付いて importmap 頼みになる、CSP が消える、`'unsafe-inline'` が
  出てくる、指紋の埋め込みが外れて素の名前が残る。
#>
[CmdletBinding()]
param(
  # publish の出力。渡さなければここで publish する
  [string]$PublishDir
)

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-published-boot.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("published-boot-" + [Guid]::NewGuid().ToString('N'))

$fails = 0
$count = 0

function Check {
  param([string]$Name, [string]$Dir, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard -PublishDir $Dir -Quiet } catch { $passed = $false; $msg = "$_" }

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
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null

  if (-not $PublishDir) {
    $PublishDir = Join-Path $tmp 'publish'
    Write-Host '=== publish する（渡されなかったので）'
    & dotnet publish (Join-Path $root 'src/FsBulletML2.Playground/FsBulletML2.Playground.fsproj') `
        -c Release -o $PublishDir --nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "publish が落ちた" }
  }
  if (-not (Test-Path -LiteralPath (Join-Path $PublishDir 'wwwroot/index.html'))) {
    throw "較正の材料が無い（$PublishDir）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
  }

  # **写しを作って index.html だけ変異させる。** 元は触らない ——
  # 書き換えて戻す形にすると、戻し忘れがそのまま次の点の素になる
  function Copyed([string]$Suffix) {
    $d = Join-Path $tmp $Suffix
    New-Item -ItemType Directory -Path $d -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $PublishDir 'wwwroot') -Destination (Join-Path $d 'wwwroot') -Recurse -Force
    $d
  }
  function Mutate([string]$Dir, [string]$Pattern, [string]$To, [string]$Why) {
    $p = Join-Path $Dir 'wwwroot/index.html'
    $text = [IO.File]::ReadAllText($p)
    $patched = [regex]::Replace($text, $Pattern, $To)
    if ($patched -ceq $text) { throw "較正が当たらなかった（$Why）" }
    [IO.File]::WriteAllText($p, $patched)
  }

  Write-Host '=== 通る側'
  Check 'publish した現物' $PublishDir $true ''

  Write-Host '=== 落ちる側'

  # **実際に踏んだ形。** 指紋が付くと SDK が対応を importmap に入れ、
  # CSP がそれを止めるので boot が素の名前を取りに行って落ちる。
  #
  # **html だけ変異させても当たらない** —— 素の名前のファイルが在るうちは
  # 「map は要らない」が正しい。指紋つきを真似るには**ファイルも動かす**
  # （最初これを忘れて、この点だけ緑のまま通った）
  $d = Copyed 'fingerprinted'
  Move-Item -LiteralPath (Join-Path $d 'wwwroot/_framework/dotnet.js') `
            -Destination (Join-Path $d 'wwwroot/_framework/dotnet.abc123.js')
  Mutate $d '<script type="importmap">[\s\S]*?</script>' `
    ('<script type="importmap">{"imports":{"./_framework/dotnet.js":"./_framework/dotnet.abc123.js"},' +
     '"scopes":{},"integrity":{}}</script>') `
    'importmap が想定と違う'
  Check '指紋つきで importmap 頼みになる' $d $false 'importmap 頼みの名前が在る'

  $d = Copyed 'nocsp'
  Mutate $d '<meta http-equiv="Content-Security-Policy"[\s\S]*?>' '' 'CSP の meta が想定と違う'
  Check 'CSP が消えた' $d $false 'CSP が無い'

  $d = Copyed 'unsafe-inline'
  Mutate $d "script-src 'self'" "script-src 'self' 'unsafe-inline'" 'script-src が想定と違う'
  Check "script-src に 'unsafe-inline' が出てきた" $d $false "'unsafe-inline' が出ている"

  # 指紋を止め忘れると、html は素の名前のまま publish 成果物には
  # 指紋つきしか出ない —— html の側が実在しない名前を指す形になる
  $d = Copyed 'missing-src'
  Mutate $d '(<script src="_framework/blazor\.webassembly)([^"]*)"' '$1.nope"' `
    '起動の script src が想定と違う'
  Check '指す先が publish 成果物に無い' $d $false 'publish 成果物に無い'

  Write-Host '=== 材料が読めないときも落ちる'

  $d = Join-Path $tmp 'empty'
  New-Item -ItemType Directory -Path $d -Force | Out-Null
  Check 'wwwroot が無い' $d $false 'wwwroot が無い'

  $d = Copyed 'no-index'
  Remove-Item -LiteralPath (Join-Path $d 'wwwroot/index.html') -Force
  Check 'index.html が無い' $d $false 'index.html が無い'

  $d = Copyed 'no-framework'
  Remove-Item -LiteralPath (Join-Path $d 'wwwroot/_framework') -Recurse -Force
  Check '_framework が無い' $d $false '_framework が無い'
} finally {
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ''
if ($fails -gt 0) {
  Write-Host "校正 $count 点 中 $fails 点 が外れた"
  exit 1
}
Write-Host "校正 $count 点 すべて一致"
