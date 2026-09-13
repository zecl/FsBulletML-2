#requires -Version 7
<#
  guard-core-fable.ps1 の校正。

  **落ちるところを見てから、本番の軸で回す。**

  当てる枝は 4 つ ——

      焼けない（Core に System.Xml が入った）  この門が在る理由そのもの
      答えが食い違う                          焼けただけで緑にしない
      撃った数が 0                            一致するが何も確かめていない
      材料が無い                              0 件 は違反 0 件 と同じ顔をする

  F# の reflection（`UnionCaseInfo.DeclaringType`）も同じ「焼けない」枝を通る。
  v5.5 で `System.Xml` を外したら**次の段としてそれが出た** —— 枝は同じなので
  校正は 1 本 で足りる。
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$guard = Join-Path $PSScriptRoot 'guard-core-fable.ps1'
$repo = (git rev-parse --show-toplevel)
$src = Join-Path $repo 'tools/CoreFableProbe'

$ok = 0
$ng = 0
function Case([string]$name, [bool]$wantPass, [scriptblock]$run) {
  $out = & $run 2>&1
  $passed = ($LASTEXITCODE -eq 0)
  if ($passed -eq $wantPass) {
    $script:ok++
    Write-Host ("  ok   {0}  -> {1}" -f $name, $(if ($wantPass) { '通る' } else { '落ちる' }))
  } else {
    $script:ng++
    Write-Host ("  NG   {0}  期待 {1} 実際 {2}" -f $name, $(if ($wantPass) { '通る' } else { '落ちる' }), $(if ($passed) { '通る' } else { '落ちる' }))
    $out | Select-Object -Last 8 | ForEach-Object { Write-Host "         $_" }
  }
}

$tmp = Join-Path ([IO.Path]::GetTempPath()) ('cf-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp -Force | Out-Null
$enc = New-Object System.Text.UTF8Encoding($false)

# 写しを作る。**`ProjectReference` の相対を絶対へ直す** ——
# 写した先から `..\..\src` は届かない（写しは temp に在る）
function New-Probe([string]$name) {
  $dst = Join-Path $tmp $name
  Copy-Item -LiteralPath $src -Destination $dst -Recurse -Force
  $proj = Join-Path $dst 'CoreFableProbe.fsproj'
  $t = [IO.File]::ReadAllText($proj)
  $t = $t.Replace('..\..\src\', ((Join-Path $repo 'src') -replace '/', '\') + '\')
  [IO.File]::WriteAllText($proj, $t, $enc)
  $dst
}

Write-Host 'Core に System.Xml が入ると焼けない。**この門が在る理由**'
$dirty = New-Probe 'xml'
$fs = Join-Path $dirty 'Probe.fs'
[IO.File]::WriteAllText($fs,
  ([IO.File]::ReadAllText($fs) + "`n// 校正: Fable に無いものを 1 か所 足す`n" +
   "let private _xml = System.Xml.XmlWriter.Create(""x"")`n"), $enc)
Case 'System.Xml が入る' $false { & $guard -RepoRoot $repo -ProbeRoot $dirty -Quiet }

Write-Host ''
Write-Host '焼けても、答えが食い違えば赤。**「焼けた」と「同じ答えを出す」は別**'
$skew = New-Probe 'skew'
$mjs = Join-Path $skew 'run.mjs'
[IO.File]::WriteAllText($mjs,
  ([IO.File]::ReadAllText($mjs) -replace 'console\.log\(line\(\)\)',
   "console.log(line().replace(/spawned=\d+/, 'spawned=99'))"), $enc)
Case '答えが食い違う' $false { & $guard -RepoRoot $repo -ProbeRoot $skew -Quiet }

Write-Host ''
Write-Host '撃った数が 0。**両方 一致するのに、何も確かめていない**'
$zero = New-Probe 'zero'
$fs = Join-Path $zero 'Probe.fs'
[IO.File]::WriteAllText($fs,
  ([IO.File]::ReadAllText($fs) -replace 'for _ in 1 \.\. 60 do', 'for _ in 1 .. 0 do'), $enc)
Case '撃った数が 0' $false { & $guard -RepoRoot $repo -ProbeRoot $zero -Quiet }

Write-Host ''
Write-Host '材料が無いとき。**0 件 は違反 0 件 と同じ顔をする**'
$none = Join-Path $tmp 'none'
New-Item -ItemType Directory -Path $none -Force | Out-Null
Case '材料が無い' $false { & $guard -RepoRoot $repo -ProbeRoot $none -Quiet }

Write-Host ''
Write-Host '本番の軸で走らせる。**校正が緑でも、現物に当たらないなら意味が無い**'
Case 'repo の Core' $true { & $guard -RepoRoot $repo -Quiet }

Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
Write-Host "校正 $ok 点 / 外れ $ng 点"
if ($ng -gt 0) { exit 1 }
exit 0
