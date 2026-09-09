#requires -Version 7
<#
.SYNOPSIS
  guard-help-chapters.ps1 の較正。

.DESCRIPTION
  この門が守っているのは「3 か所 に散った章が揃っている」こと。
  **揃っていない形を 1 つ ずつ作って、落ちるところを見る。**

  当てるのは 4 種類 ——

      対          札・節・並びのどれかが欠ける
      順          集合は同じで並びだけ違う（ここを見ていないと目次と切り替えがずれる）
      初期状態    開いた瞬間 に出る節と、印の付いた札がちょうど 1 つ ずつで、同じ章
      絵          参照と実物のずれを**両方向 で**

  **通る側 も当てる。** 落ちるほうだけ見ていると「常に赤」の門が緑に見える。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-help-chapters.ps1'
$root = (git -C $here rev-parse --show-toplevel)

$fails = 0
$count = 0

# 素の材料。**ここを 1 か所 ずつ壊す**
$chapters = @('start', 'write', 'run')

function Base-Html {
  param([string[]]$Tabs = $chapters, [string[]]$Sections = $chapters,
        [string]$OpenSection = 'start', [switch]$AllOpen,
        [string[]]$CurrentTabs = @('start'),
        [string[]]$ImgRefs = @('img/help/a.png'))
  $navLines = $Tabs | ForEach-Object {
    $cur = if ($_ -in $CurrentTabs) { ' aria-current="true"' } else { '' }
    "      <button type=`"button`" id=`"help-tab-$_`" class=`"help-tab`"$cur>$_</button>"
  }
  $secLines = $Sections | ForEach-Object {
    $hidden = if ($AllOpen -or $_ -eq $OpenSection) { '' } else { ' hidden' }
    $imgs = if ($_ -eq $Sections[0]) {
      ($ImgRefs | ForEach-Object { "        <img src=`"$_`" width=`"10`" height=`"10`" alt=`"x`">" }) -join "`n"
    } else { '' }
    "      <section id=`"help-ch-$_`" class=`"help-ch`"$hidden>`n        <h3>$_</h3>`n$imgs`n      </section>"
  }
  @"
<!doctype html>
<body>
  <dialog id="help-dialog" class="help">
    <nav class="help-nav">
$($navLines -join "`n")
    </nav>
    <div id="help-body">
$($secLines -join "`n")
    </div>
  </dialog>
</body>
"@
}

function Base-Fs {
  param([string[]]$Chapters = $chapters)
  $q = ($Chapters | ForEach-Object { "`"$_`"" }) -join '; '
  @"
module Playground

/// 章。**この字は但し書きで、数えられてはいけない** ——
/// help-tab-nisemono / helpChapters = [ "uso" ]
let private helpChapters = [ $q ]
"@
}

# 素の css。**`display` は `[open]` の側**（ここを崩す点も下に在る）
$baseCss = @'
.help { width: 40em; overflow: hidden; }
.help[open] { display: flex; }
.help figure { display: block; }
.help-tab { display: inline-block; }
@media (max-width: 719px) {
  .help-grid { display: grid; }
}
'@

function Check {
  param([string]$Name, [string]$Html, [string]$Fs, [string[]]$Images,
        [bool]$WantPass, [string]$Expect, [string]$CssText = $baseCss,
        [switch]$NoCss)
  $script:count++
  $tmp = Join-Path ([IO.Path]::GetTempPath()) ("help-ch-" + [Guid]::NewGuid().ToString('N'))
  $www = Join-Path $tmp 'wwwroot'
  New-Item -ItemType Directory -Path (Join-Path $www 'img/help') -Force | Out-Null
  $indexPath = Join-Path $www 'index.html'
  $fsPath = Join-Path $tmp 'Playground.fs'
  $cssPath = Join-Path $tmp 'playground.css'
  [IO.File]::WriteAllText($indexPath, $Html)
  [IO.File]::WriteAllText($fsPath, $Fs)
  if (-not $NoCss) { [IO.File]::WriteAllText($cssPath, $CssText) }
  foreach ($img in $Images) {
    [IO.File]::WriteAllBytes((Join-Path $www ('img/help/' + $img)), [byte[]]@(0))
  }

  $msg = ''
  $passed = $true
  try { & $guard -IndexHtml $indexPath -PlaygroundFs $fsPath -WwwRoot $www -Css $cssPath -Quiet }
  catch { $passed = $false; $msg = "$_" }
  Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue

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

Write-Host '揃っていれば通る。**ここが赤いと、以下 の落ちは「常に赤」の可能性が在る**'
Check '3 か所 が揃っている' (Base-Html) (Base-Fs) @('a.png') $true ''

Write-Host ''
Write-Host '対。**どれか 1 つ が欠けたら落ちる**'
Check '目次の札 が 1 つ 少ない' (Base-Html -Tabs @('start', 'write')) (Base-Fs) @('a.png') $false '目次の札 に無い章: run'
Check '本文の節 が 1 つ 少ない' (Base-Html -Sections @('start', 'write')) (Base-Fs) @('a.png') $false '本文の節 に無い章: run'
Check 'Fable の並び が 1 つ 少ない' (Base-Html) (Base-Fs -Chapters @('start', 'write')) @('a.png') $false 'にだけ在る章: run'
Check 'html にだけ章が在る' (Base-Html -Tabs @('start', 'write', 'run', 'extra') -Sections @('start', 'write', 'run', 'extra')) (Base-Fs) @('a.png') $false 'にだけ在る章: extra'

Write-Host ''
Write-Host '順。**集合が同じで並びだけ違う形** —— ここを見ていないと目次と切り替えがずれる'
Check '目次の札 の並びが違う' (Base-Html -Tabs @('start', 'run', 'write')) (Base-Fs) @('a.png') $false '並びが違う'
Check '本文の節 の並びが違う' (Base-Html -Sections @('start', 'run', 'write')) (Base-Fs) @('a.png') $false '並びが違う'

Write-Host ''
Write-Host '初期状態。**開いた瞬間 に出る節と、印の付いた札が 1 つ ずつで、同じ章**'
Check 'hidden が全部 外れている' (Base-Html -AllOpen) (Base-Fs) @('a.png') $false '出る節が 3 個'
Check 'hidden が全部 付いている' (Base-Html -OpenSection '') (Base-Fs) @('a.png') $false '出る節が 0 個'
Check '印の付いた札が無い' (Base-Html -CurrentTabs @()) (Base-Fs) @('a.png') $false 'aria-current の付いた札が 0 個'
Check '印の付いた札が 2 つ' (Base-Html -CurrentTabs @('start', 'write')) (Base-Fs) @('a.png') $false 'aria-current の付いた札が 2 個'
Check '出ている節と印がずれる' (Base-Html -OpenSection 'write' -CurrentTabs @('start')) (Base-Fs) @('a.png') $false '印が付いている札は start'

Write-Host ''
Write-Host '絵。**両方向 で数える** —— 片方向 だけだと消し忘れが残り続ける'
Check '参照している絵が無い' (Base-Html) (Base-Fs) @() $false '参照している絵が無い: img/help/a.png'
Check '誰も参照していない絵' (Base-Html) (Base-Fs) @('a.png', 'nokori.png') $false '誰も参照していない絵が在る: img/help/nokori.png'
Check '絵が 2 枚 とも参照されている' (Base-Html -ImgRefs @('img/help/a.png', 'img/help/b.png')) (Base-Fs) @('a.png', 'b.png') $true ''

Write-Host ''
Write-Host '閉じた窓が生き残らないか。**素の `.help` に display を書くと、閉じても消えない**'
Check '素の .help に display' (Base-Html) (Base-Fs) @('a.png') $false '素の .help に display' `
  -CssText ".help { display: flex; width: 40em; }`n"
Check '  dialog にも当てる' (Base-Html) (Base-Fs) @('a.png') $false '素の dialog に display' `
  -CssText "dialog { display: flex; }`n.help[open] { display: flex; }`n"
Check '  並びの 2 つ目 でも当てる' (Base-Html) (Base-Fs) @('a.png') $false '素の .help に display' `
  -CssText ".picker,`n.help {`n  display: flex;`n}`n"
Check '  media の中でも当てる' (Base-Html) (Base-Fs) @('a.png') $false '素の .help に display' `
  -CssText "@media (max-width: 719px) {`n  .help { display: block; }`n}`n"
# **通る側 も当てる** —— 落ちるほうだけだと「常に赤」の見張りが緑に見える
Check '  [open] の側 なら通る' (Base-Html) (Base-Fs) @('a.png') $true '' `
  -CssText ".help[open] { display: flex; }`n"
Check '  子孫や別の名前は通る' (Base-Html) (Base-Fs) @('a.png') $true '' `
  -CssText ".help figure { display: block; }`n.help-tab { display: inline-block; }`n.help-grid { display: grid; }`n"
Check '  display 以外 は通る' (Base-Html) (Base-Fs) @('a.png') $true '' `
  -CssText ".help { width: 40em; overflow: hidden; }`n"
Check 'css が読めない' (Base-Html) (Base-Fs) @('a.png') $false 'css を読めなかった' -NoCss
Check 'css に規則が無い' (Base-Html) (Base-Fs) @('a.png') $false '規則を 1 つ も引けなかった' -CssText "/* 空 */`n"

Write-Host ''
Write-Host '材料が読めないとき。**0 件 は違反 0 件 と同じ顔をする**'
Check 'helpChapters が無い' (Base-Html) "module Playground`nlet x = 1`n" @('a.png') $false 'helpChapters の並びが見つからない'
Check 'helpChapters が空' (Base-Html) "module Playground`nlet private helpChapters = [ ]`n" @('a.png') $false '1 つ も引けなかった'
Check '目次の札 が 1 つ も無い' (Base-Html -Tabs @()) (Base-Fs) @('a.png') $false '目次の札を 1 つ も引けなかった'
$script:count++
$missing = Join-Path ([IO.Path]::GetTempPath()) 'no-such-index.html'
try { & $guard -IndexHtml $missing -Quiet; $script:fails++; Write-Host '  NG   ファイルが無い  期待 落ちる 実際 通る' }
catch { Write-Host '  ok   ファイルが無い  -> 落ちる' }

Write-Host ''
Write-Host 'コメントの中の字を数えない（`help-tab-` は但し書きにも書いてある）'
$withComment = (Base-Html) -replace '<nav class="help-nav">', @'
<!-- <button id="help-tab-nisemono"> と <section id="help-ch-nisemono"> のように書く -->
    <nav class="help-nav">
'@
Check 'コメントの中の偽の章' $withComment (Base-Fs) @('a.png') $true ''

Write-Host ''
Write-Host '本番の軸で走らせる。**較正が緑でも、現物に当たらないなら意味が無い**'
$script:count++
try {
  & $guard -RepoRoot $root -Quiet
  Write-Host '  ok   repo の現物  -> 通る'
} catch {
  $script:fails++
  Write-Host "  NG   repo の現物  -> 落ちた: $($_ -replace "`n", ' / ')"
}

Write-Host ''
Write-Host "較正 $count 件 / 食い違い $fails 件"
if ($fails -gt 0) { exit 1 }
