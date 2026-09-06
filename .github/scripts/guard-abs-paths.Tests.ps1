#requires -Version 7
<#
.SYNOPSIS
  guard-abs-paths.ps1 の校正。

.DESCRIPTION
  通る側と落ちる側を両方 置く。**落ちる側だけ見ていると「全部 落とす」壊れ方が
  緑のまま残る** —— 相対パスしか無いコードが通ることも見る。

  但し書きそのものも 2 方向 から測る。

    渡さなければ、いま通している現物が落ちる  但し書きが仕事をしている証拠
    当たらない但し書きを渡せば落ちる          守る対象が消えたら一緒に死ぬ

  最後に repo の現物へ当てる。**校正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-abs-paths.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("guard-abs-" + [Guid]::NewGuid().ToString('N'))
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
  param([string]$Name, [string[]]$Paths, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard -RepoRoot $root -Files $Paths -Quiet } catch { $passed = $false; $msg = "$_" }

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
  Write-Host '=== 通る側'
  Check '相対パスだけ' @((Make 'ok.ps1' 'Join-Path $root "src/Foo"')) $true ''
  Check '環境変数で渡す形' @((Make 'env.cs' 'var p = Environment.GetEnvironmentVariable("UNITY_HUB_EDITOR_PATH");')) $true ''
  # URL のスキームはドライブに見えない。ここが当たると当たりだらけになる
  Check 'URL' @((Make 'url.fs' '/// https://example.com/a  と  s3://bucket/k')) $true ''
  # 記録は「そのとき その場所で こうだった」を書くもの。絶対パスが載っているのが正しい
  Check '見ない拡張子（.md）' @((Make 'note.md' 'cd C:\Code\FsBulletML-2')) $true ''

  Write-Host '=== 落ちる側'
  Check 'ドライブから始まる絶対パス' @((Make 'drive.cs' 'var r = @"C:\Code\FsBulletML-2";')) $false 'ドライブから始まる絶対パス'
  Check '前向きスラッシュのドライブ' @((Make 'drive2.fs' 'let p = "D:/work/thing"')) $false 'ドライブから始まる絶対パス'
  Check '誰かの home（macOS）' @((Make 'home1.sh' 'cd /Users/someone/proj')) $false '誰かの home'
  Check '誰かの home（Linux）' @((Make 'home2.yml' '  run: /home/someone/bin/tool')) $false '誰かの home'
  Check 'WSL のマウント' @((Make 'wsl.ps1' '$p = "/mnt/c/Code/thing"')) $false 'WSL のマウント'

  Write-Host '=== 行と、当たったところを出すこと'
  $count++
  $msg = ''
  try { & $guard -RepoRoot $root -Files @((Make 'pos.cs' "var a = 1;`nvar r = @`"C:\Code\X`";")) -Quiet } catch { $msg = "$_" }
  if ($msg -match '2 行' -and $msg -match 'C:\\Code\\X') { Write-Host '  ok   行と当たった字を出す' }
  else {
    $fails++
    Write-Host '  NG   行と当たった字を出す'
    Write-Host "         $($msg -replace "`n", ' / ')"
  }

  # --- 但し書きそのものを 2 方向 から ------------------------------------
  #
  # **通す側だけ見ていると、但し書きが仕事をしているのか、そもそも網が
  # その行に当たらないのかを見分けられない。** 但し書きを外して落ちることで、
  # 通っているのが但し書きのおかげだと分かる
  Write-Host '=== 但し書き'
  $count++
  $msg = ''
  try { & $guard -RepoRoot $root -Allow @() -Quiet } catch { $msg = "$_" }
  if ($msg -match 'その機械にしか無い絶対パス') { Write-Host '  ok   但し書きを外すと、いま通している現物が落ちる' }
  else {
    $fails++
    Write-Host '  NG   但し書きを外すと、いま通している現物が落ちる'
    Write-Host "         $($msg -replace "`n", ' / ')"
  }

  # 当たらない但し書きは、守る対象が消えた印。残すと次の穴が黙って通る
  $count++
  $msg = ''
  try { & $guard -RepoRoot $root -Allow @(@{ Path = 'no/such/file.cs'; Why = '校正' }) -Quiet } catch { $msg = "$_" }
  if ($msg -match 'もう当たらない但し書き') { Write-Host '  ok   当たらない但し書きは赤' }
  else {
    $fails++
    Write-Host '  NG   当たらない但し書きは赤'
    Write-Host "         $($msg -replace "`n", ' / ')"
  }

  # **-Files を渡さないで呼ぶ。** 渡した数点が緑になるだけでは、
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
