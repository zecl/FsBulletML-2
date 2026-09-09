#requires -Version 7
<#
.SYNOPSIS
  guard-embed-layout.ps1 の較正。

.DESCRIPTION
  この門が守っているのは 3 つ ——

      印        html の `embed-hide`
      規則      css の `html.embed`（印を受ける側 と、面の上限を戻す側）
      配線      Fable の判定・印・欄を建てない側

  **いちばん当てたいのは順番。** 規則が在っても、送る画面用のメディアより
  前に在れば負けて黙って効かない —— **画では「少し小さいだけ」に見える**ので、
  目で見ても出ない。だから**前に置いた形を実際に作って、落ちるところを見る。**

  **通る側 も当てる。** 落ちるほうだけ見ていると「常に赤」の門が緑に見える。

  最後に repo の現物へ当てる。**較正が緑でも、本番の軸で走らないなら意味が無い。**
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $PSCommandPath
$guard = Join-Path $here 'guard-embed-layout.ps1'
$root = (git -C $here rev-parse --show-toplevel)

$fails = 0
$count = 0

$goodHtml = @'
<!doctype html>
<body>
  <!-- コメントの中の embed-hide は数えない -->
  <div class="picker">
    <select id="pattern" aria-label="弾幕"></select>
    <label class="mode embed-hide">表記<select id="mode"></select></label>
  </div>
  <div class="bar">
    <button type="button" id="play">Play</button>
    <button class="embed-hide" type="button" id="share">Share</button>
  </div>
</body>
'@

# **送る画面用のメディアが先、html.embed が後ろ**（これが正しい並び）
$goodCss = @'
.page { height: 100dvh; }
@media (max-width: 719px), (max-height: 560px) {
  .page { height: auto; min-height: 100dvh; }
  #stage { max-height: min(60dvh, 640px); }
}
@media (max-width: 719px) {
  #stage { max-height: 40dvh; }
}
html.embed .page { height: 100dvh; min-height: 0; }
html.embed .embed-hide { display: none; }
html.embed #stage { max-height: min(100%, 640px); }
'@

$goodFs = @'
module Playground
[<Emit("(new URLSearchParams(location.search).get('embed') === '1') || (window.self !== window.top)")>]
let private isEmbedded () : bool = jsNative
let private embedded =
  let e = isEmbedded ()
  if e then document.documentElement.classList.add "embed"
  e
// 呼ぶ側
let go (self: obj) = if not embedded then self.startEditor ()
'@

function Check {
  param([string]$Name, [string]$Html, [string]$Css, [string]$Fs,
        [bool]$WantPass, [string]$Expect)
  $script:count++
  $tmp = Join-Path ([IO.Path]::GetTempPath()) ("embed-" + [Guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Path $tmp -Force | Out-Null
  $hp = Join-Path $tmp 'index.html'; $cp = Join-Path $tmp 'playground.css'; $fp = Join-Path $tmp 'Playground.fs'
  [IO.File]::WriteAllText($hp, $Html)
  [IO.File]::WriteAllText($cp, $Css)
  [IO.File]::WriteAllText($fp, $Fs)

  $msg = ''
  $passed = $true
  try { & $guard -IndexHtml $hp -Css $cp -PlaygroundFs $fp -Quiet }
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
Check '印と規則と配線が揃っている' $goodHtml $goodCss $goodFs $true ''

Write-Host ''
Write-Host '順番。**規則が在っても、前に置けば負けて黙って効かない**'
$beforeCss = @'
html.embed .page { height: 100dvh; min-height: 0; }
html.embed .embed-hide { display: none; }
html.embed #stage { max-height: min(100%, 640px); }
@media (max-width: 719px), (max-height: 560px) {
  .page { height: auto; }
  #stage { max-height: min(60dvh, 640px); }
}
@media (max-width: 719px) {
  #stage { max-height: 40dvh; }
}
'@
Check 'html.embed が両方 のメディアより前' $goodHtml $beforeCss $goodFs $false 'より前 に在る'

# 片方 だけ前 でも落ちること。**両方 見ていないと、片方 の衝突が残る**
$halfCss = @'
@media (max-width: 719px), (max-height: 560px) {
  .page { height: auto; }
  #stage { max-height: min(60dvh, 640px); }
}
html.embed .page { height: 100dvh; }
html.embed .embed-hide { display: none; }
@media (max-width: 719px) {
  #stage { max-height: 40dvh; }
}
'@
Check '  幅だけの並びが後ろ に在る' $goodHtml $halfCss $goodFs $false '幅だけの並び'

Write-Host ''
Write-Host '守る対象が消えたら落とす。**当たらなくなった門を緑のまま残さない**'
$noMediaCss = @'
html.embed .page { height: 100dvh; }
html.embed .embed-hide { display: none; }
'@
Check 'メディアが css から消えた' $goodHtml $noMediaCss $goodFs $false 'この門が守っている衝突が消えた'

Write-Host ''
Write-Host '印と規則。**片方 だけでは何も起きない**'
Check '印が 1 つ も無い' ($goodHtml -replace 'embed-hide', '') $goodCss $goodFs $false '印が 1 つ も無い'
Check '規則が 1 つ も無い' $goodHtml ($goodCss -replace '(?m)^html\.embed.*$', '') $goodFs $false 'html.embed の規則が 1 つ も無い'
Check '印を受ける側 が無い' $goodHtml ($goodCss -replace '(?m)^html\.embed \.embed-hide.*$', '') $goodFs $false '印を受ける側 が居ない'
# **コメントの中の印を数えない**（html はコメントが濃い）
Check 'コメントの中の印だけ' ($goodHtml -replace 'class="mode embed-hide"', 'class="mode"' -replace 'class="embed-hide" ', '') $goodCss $goodFs $false '印が 1 つ も無い'

Write-Host ''
Write-Host '配線。**4 つ とも要る** —— 1 つ 欠けると印が立たないか、Monaco が読まれる'
Check '?embed=1 を見ていない' $goodHtml $goodCss ($goodFs -replace "get\('embed'\) === '1'", "get('x') === '1'") $false '?embed=1 を見る'
Check '枠の中 を見ていない' $goodHtml $goodCss ($goodFs -replace 'window\.self !== window\.top', 'false') $false '枠の中 かを見る'
Check '印を立てていない' $goodHtml $goodCss ($goodFs -replace 'classList\.add "embed"', 'classList.add "x"') $false 'html に印を立てる'
Check '欄を建ててしまう' $goodHtml $goodCss ($goodFs -replace 'if not embedded then self\.startEditor \(\)', 'self.startEditor ()') $false '欄を建てない'
# **但し書きの中の字を数えない**（Fable 側 も但し書きが濃い）
Check '但し書きにだけ在る' $goodHtml $goodCss "module Playground`n/// if not embedded then self.startEditor ()`n$($goodFs -replace 'if not embedded then self\.startEditor \(\)', '')" $false '欄を建てない'

Write-Host ''
Write-Host '材料が読めないとき。**0 件 は違反 0 件 と同じ顔をする**'
$script:count++
$missing = Join-Path ([IO.Path]::GetTempPath()) 'no-such-index.html'
try { & $guard -IndexHtml $missing -Quiet; $script:fails++; Write-Host '  NG   ファイルが無い  期待 落ちる 実際 通る' }
catch { Write-Host '  ok   ファイルが無い  -> 落ちる' }

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
