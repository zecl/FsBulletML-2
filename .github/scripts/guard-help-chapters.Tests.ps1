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
      寸          書いてある width / height と、実物の比（縦横 で同じか）

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
        [string[]]$ImgRefs = @('img/help/a.png'),
        # **寸を書かない `<img>`** も作れるようにする（門が数える点の 1 つ）
        [switch]$NoImgSize,
        [int]$ImgW = 10, [int]$ImgH = 10)
  $navLines = $Tabs | ForEach-Object {
    $cur = if ($_ -in $CurrentTabs) { ' aria-current="true"' } else { '' }
    "      <button type=`"button`" id=`"help-tab-$_`" class=`"help-tab`"$cur>$_</button>"
  }
  $secLines = $Sections | ForEach-Object {
    $hidden = if ($AllOpen -or $_ -eq $OpenSection) { '' } else { ' hidden' }
    $imgs = if ($_ -eq $Sections[0]) {
      $sizeAttr = if ($NoImgSize) { '' } else { " width=`"$ImgW`" height=`"$ImgH`"" }
      ($ImgRefs | ForEach-Object { "        <img src=`"$_`"$sizeAttr alt=`"x`">" }) -join "`n"
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

# **頭だけ本物の PNG。** 門は IHDR しか読まないので、画は要らない
function Png-Bytes {
  param([int]$W, [int]$H)
  $b = [System.Collections.Generic.List[byte]]::new()
  $b.AddRange([byte[]]@(137, 80, 78, 71, 13, 10, 26, 10))      # 署名
  $b.AddRange([byte[]]@(0, 0, 0, 13))                          # IHDR の長さ
  $b.AddRange([Text.Encoding]::ASCII.GetBytes('IHDR'))
  foreach ($v in @($W, $H)) {
    # **括弧で 1 つ ずつ包む。** コンマは -band より先に結ぶので、包まないと
    # 0xFF から後ろ が配列になって落ちる
    $b.AddRange([byte[]]@(((($v -shr 24) -band 0xFF)), ((($v -shr 16) -band 0xFF)),
                          ((($v -shr 8) -band 0xFF)), (($v -band 0xFF))))
  }
  $b.AddRange([byte[]]@(8, 6, 0, 0, 0))                        # 深さ・色・その他
  $b.AddRange([byte[]]@(0, 0, 0, 0))                           # crc（門は見ない）
  return $b.ToArray()
}

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
    # `名前` か `名前:WxH`。**既定は 10x10** —— Base-Html が width="10" height="10" と書く。
    #
    # **`$name` という名前は使えない。** PowerShell の変数は大小を区別しないので、
    # 引数の `$Name` を黙って潰す（試験の名前が全部 絵の名前になった）
    $file = $img
    $iw = 10; $ih = 10
    $bare = $false
    if ($img -match '^(?<n>[^:]+):(?<w>\d+)x(?<h>\d+)$') {
      $file = $Matches['n']; $iw = [int]$Matches['w']; $ih = [int]$Matches['h']
    } elseif ($img -match '^(?<n>[^:]+):なま$') {
      # **PNG でないもの。** 頭が壊れている絵を置いたときに落ちるか
      $file = $Matches['n']; $bare = $true
    }
    $bytes = if ($bare) { [byte[]]@(0) } else { Png-Bytes -W $iw -H $ih }
    [IO.File]::WriteAllBytes((Join-Path $www ('img/help/' + $file)), $bytes)
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
Write-Host '寸。**書いてある大きさと実物の比が、縦横 で同じか** —— 比が 1 とは限らない'
Check '実物と書いた寸が同じ' (Base-Html) (Base-Fs) @('a.png:10x10') $true ''
Check '  2 倍 で焼いた絵は通る' (Base-Html) (Base-Fs) @('a.png:20x20') $true ''
Check '  1.5 倍 でも通る' (Base-Html -ImgW 100 -ImgH 40) (Base-Fs) @('a.png:150x60') $true ''
Check '  縦横 の比が違う' (Base-Html) (Base-Fs) @('a.png:20x10') $false '縦横 の比が違う'
Check '  実物のほうが小さい' (Base-Html) (Base-Fs) @('a.png:5x5') $false '実物が小さい'
Check '  寸を書いていない' (Base-Html -NoImgSize) (Base-Fs) @('a.png:10x10') $false 'width / height が書いていない'
Check '  PNG として読めない' (Base-Html) (Base-Fs) @('a.png:なま') $false 'PNG として読めない'

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
