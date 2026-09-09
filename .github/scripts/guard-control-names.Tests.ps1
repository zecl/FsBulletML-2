#requires -Version 7
<#
.SYNOPSIS
  guard-control-names.ps1 の較正。

.DESCRIPTION
  名前の付き方が 3 通り あるので、**3 通り とも「在れば通る / 消せば落ちる」**
  を当てる。片方 だけ見ていると、「常に赤」と「何も見ていない」がそれぞれ
  緑のまま残る。

  **材料が読めないときに落ちること**も見る。口 0 個 は違反 0 件 と同じ顔をする。

  **区間の数え方**も当てる。この門がいちばん壊れやすいのはそこで、
  `<label>` の開き閉じが釣り合わないまま「名前が在る」と読むと、
  読んだ結果が全部 嘘になる。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-control-names.ps1'
$root = (git -C $here rev-parse --show-toplevel)
$tmp = Join-Path ([IO.Path]::GetTempPath()) ("pg-names-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp -Force | Out-Null

$fails = 0
$count = 0

function Make {
  param([string]$Body)
  $p = Join-Path $tmp ([Guid]::NewGuid().ToString('N') + '.html')
  [IO.File]::WriteAllText($p, "<!doctype html>`n<body>`n$Body`n</body>")
  $p
}

function Check {
  param([string]$Name, [string]$Body, [bool]$WantPass, [string]$Expect)
  $script:count++
  $msg = ''
  $passed = $true
  try { & $guard -IndexHtml (Make $Body) -Quiet } catch { $passed = $false; $msg = "$_" }

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

Write-Host '名前の付き方 3 通り。**在れば通り、消せば落ちる**'
Check '口の中の字' '<button type="button" id="play">Play</button>' $true ''
Check '  それを消す' '<button type="button" id="play"></button>' $false '名前が読めない口が 1 個'
Check 'aria-label' '<select id="pattern" aria-label="弾幕"></select>' $true ''
Check '  それを消す' '<select id="pattern"></select>' $false '名前が読めない口が 1 個'
Check '包む label' '<label>配色<select id="theme"></select></label>' $true ''
Check '  それを外す' '配色<select id="theme"></select>' $false '名前が読めない口が 1 個'

Write-Host ''
Write-Host '**行では読まない。** label の開きと口が別の行に在る形（repo の #rate がこれ）'
Check '2 行 にまたがる label' "<label>速さ`n  <select id=`"rate`"></select>`n</label>" $true ''
Check '  閉じを消す' "<label>速さ`n  <select id=`"rate`"></select>" $false '閉じていない'
Check '  開きを消す' "速さ`n  <select id=`"rate`"></select>`n</label>" $false '開きより多い'

Write-Host ''
Write-Host 'コメントの中の例示を口として数えない（最初に書いたとき数えた）'
Check 'コメントの中の裸の口' @'
<!-- <select id="example"> のように書く -->
<button type="button" id="play">Play</button>
'@ $true ''
Check '  複数行のコメント' @'
<!--
  <label>配色<select id="theme"></select></label>
  のように包む
-->
<button type="button" id="play">Play</button>
'@ $true ''

Write-Host ''
Write-Host 'この門が読める形になっていないときは、名前の在り無しより先に落ちる'
Check 'id が無い口' '<button type="button">Play</button>' $false 'id が無い'
Check 'button の中身が行をまたぐ' "<button type=`"button`" id=`"play`">`n  Play`n</button>" $false '行をまたいでいる'
# **開き札のまたぎは別の穴。** 中身のまたぎと違って網に 1 度 も当たらないので、
# 見ていないと「口が消えた」ことに気づけない
Check 'button の開き札が行をまたぐ' "<button type=`"button`"`n        id=`"play`">Play</button>" $false '開き札が行をまたいでいる'
Check 'select の開き札が行をまたぐ' "<select`n  id=`"theme`" aria-label=`"配色`"></select>" $false '開き札が行をまたいでいる'
# **ほかの口が揃っていても落ちること。** 落ちないと「揃っている」と読んで、
# 消えた 1 個 に気づかない
Check '  ほかの口が揃っていても落ちる' "<button type=`"button`" id=`"play`">Play</button>`n<button type=`"button`"`n        id=`"pause`">Pause</button>" $false '開き札が行をまたいでいる'

Write-Host ''
Write-Host '材料が読めないとき。**口 0 個 は違反 0 件 と同じ顔をする**'
Check '口が 1 つ も無い' '<p>字だけ</p>' $false '1 つ も引けなかった'
$script:count++
$missing = Join-Path $tmp 'no-such-file.html'
try { & $guard -IndexHtml $missing -Quiet; $script:fails++; Write-Host '  NG   ファイルが無い  期待 落ちる 実際 通る' }
catch { Write-Host '  ok   ファイルが無い  -> 落ちる' }

Write-Host ''
Write-Host 'のけている口は、名前が無くても通る'
Check 'open-file（画面に出ない）' '<input id="open-file" type="file" hidden>' $false '1 つ も引けなかった'
Check '  ほかの口と一緒なら通る' '<button type="button" id="play">Play</button><input id="open-file" type="file" hidden>' $true ''

Write-Host ''
Write-Host '本番の軸で走らせる。**較正が緑でも、現物に当たらないなら意味が無い**'
$script:count++
try {
  & $guard -RepoRoot $root -Quiet
  Write-Host '  ok   repo の index.html  -> 通る'
} catch {
  $script:fails++
  Write-Host "  NG   repo の index.html  -> 落ちた: $($_ -replace "`n", ' / ')"
}

Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ''
Write-Host "較正 $count 件 / 食い違い $fails 件"
if ($fails -gt 0) { exit 1 }
