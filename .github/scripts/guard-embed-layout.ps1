#requires -Version 7
<#
.SYNOPSIS
  記事の枠に貼られた形（`html.embed`）が、成り立つ形で置かれているかを見る。

.DESCRIPTION
  埋め込みの体裁は 3 か所 に散っている ——

      html      消す口に `embed-hide` の印
      css       `html.embed` の規則（印を受ける側 と、面の上限を戻す側）
      Fable     `?embed=1` / `window.self !== window.top` で `html` に印を立て、
                そのとき `startEditor` を呼ばない

  **どれが欠けても走行は止まらない。** 印だけ在って規則が無ければ全部 出るし、
  規則だけ在って印が無ければ何も消えない。Fable が印を立てなければ両方 死ぬ。

  ## いちばん危ないのは順番

  `html.embed` の規則は、**送る画面用のメディアと真っ向から衝突する** ——

      @media (max-height: 560px)   .page を height: auto にして、面を 60dvh で止める
      html.embed                   .page を height: 100dvh に戻して、面の上限を親に対して持たせる

  詳細度は同じ（`html.embed .page` のほうが高いが、`#stage` の上限は
  どちらも同じ強さ）なので、**後ろに書いたほうが勝つ**。
  `html.embed` が前に来ると、**720x420 で面が 262x349 から 191x254 へ戻り、
  縦に 139px 溢れる** —— 画では「少し小さいだけ」に見えて、
  溢れた分 は枠が `scrolling="no"` なので見えない。

  だから**位置そのものを数える。**

  ## 見ないもの

  **絵の見た目は見ていない。** ここで測れるのは「印と規則と配線が揃っていて、
  規則が勝つ位置に在る」まで。実際の寸法は走らせて測る（README の表）。

  較正は guard-embed-layout.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$IndexHtml,
  [string]$Css,
  [string]$PlaygroundFs,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$pg = Join-Path $RepoRoot 'src/FsBulletML2.Playground'
if (-not $IndexHtml) { $IndexHtml = Join-Path $pg 'wwwroot/index.html' }
if (-not $Css) { $Css = Join-Path $pg 'wwwroot/css/playground.css' }
if (-not $PlaygroundFs) { $PlaygroundFs = Join-Path $pg 'fable/Playground.fs' }

foreach ($f in @($IndexHtml, $Css, $PlaygroundFs)) {
  if (-not (Test-Path -LiteralPath $f)) {
    throw "読めなかった（$f）。**0 件 は違反 0 件 と同じ顔をする**ので、ここで落とす"
  }
}

$problems = [System.Collections.Generic.List[string]]::new()

# --- html。**コメントを先に落とす** ------------------------------------------
$html = [regex]::Replace([IO.File]::ReadAllText($IndexHtml), '(?s)<!--.*?-->',
                         { param($m) $m.Value -replace '[^\r\n]', ' ' })
$marks = ([regex]::Matches($html, 'class="[^"]*\bembed-hide\b[^"]*"')).Count
if (-not $Quiet) { Write-Host "html の embed-hide  $marks 件" }
if ($marks -eq 0) {
  $problems.Add('html に embed-hide の印が 1 つ も無い（隠す口が無いのと、印を消したのは別）')
}

# --- css ---------------------------------------------------------------------
$cssText = [regex]::Replace([IO.File]::ReadAllText($Css), '(?s)/\*.*?\*/', ' ')

$embedRules = ([regex]::Matches($cssText, '(?m)^html\.embed\b')).Count
if (-not $Quiet) { Write-Host "css の html.embed   $embedRules 件" }
if ($embedRules -eq 0) {
  $problems.Add('css に html.embed の規則が 1 つ も無い（印だけ在っても何も消えない）')
}

# 印を受ける側。**これが無いと、印を付けた口が全部 枠の中 に出る**
if ($cssText -notmatch 'html\.embed\s+\.embed-hide') {
  $problems.Add('css に `html.embed .embed-hide` が無い（html の印を受ける側 が居ない）')
}

# --- 順番。**送る画面用のメディアより後ろ でなければ効かない** ----------------
$firstEmbed = [regex]::Match($cssText, '(?m)^html\.embed\b')

# **`@media` を 1 つ ずつ引いてから、条件で選り分ける。**
# 条件の字を直に探すと、`@media (max-width: 719px), (max-height: 560px)` が
# 「幅だけの並び」の網にも当たる —— そちらのほうが前に在るので、
# **本当に後ろに在るべき並びとの前後が測れなくなる**（較正が出した）
$allMedia = @([regex]::Matches($cssText, '@media(?<cond>[^{]*)\{') |
             ForEach-Object { @{ Index = $_.Index; Cond = $_.Groups['cond'].Value } })

$medias = @(
  @{ Name = '縦が足りない画面の並び（max-height: 560px）'
     Pick = { param($c) $c.Cond -match 'max-height:\s*560px' } }
  @{ Name = '幅だけの並び（max-width: 719px のみ）'
     Pick = { param($c) $c.Cond -match 'max-width:\s*719px' -and $c.Cond -notmatch 'max-height' } }
)
foreach ($m in $medias) {
  $hit = @($allMedia | Where-Object { & $m.Pick $_ } | Select-Object -First 1)
  if ($hit.Count -eq 0) {
    # 守る対象が消えたのに門だけ残る形を避ける
    $problems.Add("$($m.Name) が css に無い。**この門が守っている衝突が消えたか、書き方が変わった** —— どちらでも門を見直す")
    continue
  }
  if ($firstEmbed.Success -and $firstEmbed.Index -lt $hit[0].Index) {
    $problems.Add("html.embed が $($m.Name) より前 に在る（後ろでないと、送る画面用の指定に負けて黙って効かない）")
  }
}

# --- Fable -------------------------------------------------------------------
$fs = ([IO.File]::ReadAllText($PlaygroundFs) -split "\r?\n" |
       ForEach-Object { $_ -replace '^\s*///.*$', '' }) -join "`n"

$wants = @(
  @{ Name = '?embed=1 を見る';        Rx = "embed'\)\s*===\s*'1'" }
  @{ Name = '枠の中 かを見る';          Rx = 'window\.self\s*!==\s*window\.top' }
  @{ Name = 'html に印を立てる';        Rx = 'documentElement\.classList\.add\s*"embed"' }
  @{ Name = '埋め込みでは欄を建てない'; Rx = 'if\s+not\s+embedded\s+then\s+self\.startEditor' }
)
foreach ($w in $wants) {
  if ($fs -notmatch $w.Rx) { $problems.Add("Fable に「$($w.Name)」が無い") }
}

# --- 判定 --------------------------------------------------------------------
if ($problems.Count -gt 0) {
  throw ("埋め込みの体裁が成り立たない形になっている:`n" +
    (($problems | ForEach-Object { "    $_" }) -join "`n") +
    "`n印（html の embed-hide）・規則（css の html.embed）・配線（Playground.fs）の 3 つ を揃えること。" +
    "`n**css の html.embed は、送る画面用のメディアより後ろ に置く**（前だと黙って効かない）")
}

if (-not $Quiet) {
  Write-Host '埋め込みの印と規則と配線は揃っていて、規則は送る画面用のメディアより後ろ に在る'
}
