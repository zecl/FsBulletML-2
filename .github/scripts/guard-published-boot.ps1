#requires -Version 7
<#
.SYNOPSIS
  publish した Playground が、インライン script 抜きで起動できるかを見る。

.DESCRIPTION
  **build が通ることと、publish した物が起動することは別だった。**

  SDK は publish のとき資産に指紋を付け（`dotnet.<hash>.js`）、素の名前との
  対応を**インラインの `<script type="importmap">`** で html に入れる。
  この app の CSP は `script-src` に `'unsafe-inline'` を出していないので、
  **その importmap は効かない** —— boot が `_framework/dotnet.js` を素の名前で
  取りに行って落ち、`#loop-error` が「起動待ち」のまま止まる。

  **どこにも出なかった。**

      dev server                   指紋を付けないので importmap が空のまま効く
      CI の Build · Playground     build するだけ。publish しない
      guard-playground-boundaries  **ソースの** index.html を見ていて、
                                   しかも空の importmap は数えないと決めてある

  人が書く script は見ていたが、**SDK が後から入れる script は見ていなかった。**

  ## 見るもの

  1. CSP が在り、`script-src` に `'unsafe-inline'` が無い
     下の 2 つ はこれが前提。緩めたなら**この門ごと考え直す合図**なので、
     緩んだこと自体を赤にする

  2. importmap の対応が、効かなくても困らない形になっている
     対応表の key のうち、**その名前でファイルが無く、html の `<script src>` が
     指紋つきの側を直に指してもいないもの**が在れば赤 ——
     それは「map が効かないと解決しない名前」で、この CSP では効かない

  3. html の `<script src>` の指す先が実在する（CDN は除く）
     指紋の埋め込み（`OverrideHtmlAssetPlaceholders`）が外れると、
     素の名前のまま publish されて 404 になる

  ## 0 件 を緑にしない

  publish 成果物が読めなければ落とす。読めない状態は「違反 0 件」と同じ顔をする。
#>
[CmdletBinding()]
param(
  # publish の出力（`dotnet publish -o` に渡した場所）
  [Parameter(Mandatory = $true)][string]$PublishDir,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$wwwroot = Join-Path $PublishDir 'wwwroot'
if (-not (Test-Path -LiteralPath $wwwroot)) {
  throw "publish した wwwroot が無い（$wwwroot）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$indexPath = Join-Path $wwwroot 'index.html'
if (-not (Test-Path -LiteralPath $indexPath)) {
  throw "publish した index.html が無い（$indexPath）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$framework = Join-Path $wwwroot '_framework'
if (-not (Test-Path -LiteralPath $framework)) {
  throw "publish した _framework が無い（$framework）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}

# **コメントを外してから当てる。** 外さないと、但し書きに書いた例文が
# そのまま違反として出る（`guard-playground-boundaries.ps1` が実際に踏んだ）。
# 外すのは中身だけで改行は残す —— 残さないと行番号がずれる
$raw = [IO.File]::ReadAllText($indexPath)
$html = [regex]::Replace($raw, '<!--[\s\S]*?-->',
                         { param($m) $m.Value -replace '[^\r\n]', '' })
$bad = [System.Collections.Generic.List[string]]::new()

# --- 1. CSP がインライン script を許していないこと -------------------------
#
# **緩んだら赤にする。** 下の 2 つ は「map が効かない」ことを前提に置いた門で、
# 前提が消えたら守っているものが変わる
$csp = [regex]::Match($html, 'http-equiv="Content-Security-Policy"\s+content="(?<c>[^"]*)"')
if (-not $csp.Success) {
  $bad.Add("  publish した index.html に CSP が無い`n      " +
           "この門は「インライン script が効かない」を前提にしている")
} else {
  $scriptSrc = [regex]::Match($csp.Groups['c'].Value, 'script-src[^;]*')
  if (-not $scriptSrc.Success) {
    $bad.Add("  CSP に script-src が無い`n      インライン script が素通りする")
  } elseif ($scriptSrc.Value -match "'unsafe-inline'") {
    $bad.Add("  script-src に 'unsafe-inline' が出ている: $($scriptSrc.Value.Trim())" +
             "`n      インライン script が効くようになったので、下の 2 つ の前提が消えた。" +
             "この門ごと考え直すこと")
  }
}

# --- html が直に指している script の src ------------------------------------
$srcs = @([regex]::Matches($html, '<script[^>]*\ssrc="(?<s>[^"]+)"') |
          ForEach-Object { $_.Groups['s'].Value })
$localSrcs = @($srcs | Where-Object { $_ -notmatch '^https?://' })

# --- 2. importmap が効かなくても解決するか ----------------------------------
#
# **対応表の key を 1 つ ずつ当てる。** key の名前でファイルが在れば map は
# 要らないし、html が指紋つきの側を直に指していても要らない。
# どちらでもないものが「map 頼み」で、この CSP では解決しない
$map = [regex]::Match($html, '<script type="importmap">(?<b>[\s\S]*?)</script>')
if ($map.Success -and $map.Groups['b'].Value.Trim() -ne '') {
  $imports = $null
  try {
    $imports = ($map.Groups['b'].Value | ConvertFrom-Json).imports
  } catch {
    $bad.Add("  importmap を読めなかった: $_")
  }
  if ($null -ne $imports) {
    foreach ($p in $imports.PSObject.Properties) {
      $key = $p.Name -replace '^\./', ''
      $value = ([string]$p.Value) -replace '^\./', ''
      $keyFile = Join-Path $wwwroot ($key -replace '/', [IO.Path]::DirectorySeparatorChar)
      $namedInHtml = @($localSrcs | Where-Object { ($_ -replace '^\./', '') -eq $value }).Count -gt 0
      if (-not (Test-Path -LiteralPath $keyFile) -and -not $namedInHtml) {
        $bad.Add("  importmap 頼みの名前が在る: $key -> $value" +
                 "`n      その名前のファイルは publish 成果物に無く、html も指紋つきの側を指していない。" +
                 "`n      CSP がインライン script を止めるので **この対応は効かない** —— 起動待ちで止まる")
      }
    }
  }
}

# --- 3. html の script src が実在する ---------------------------------------
foreach ($s in $localSrcs) {
  $rel = ($s -replace '^\./', '') -replace '/', [IO.Path]::DirectorySeparatorChar
  if (-not (Test-Path -LiteralPath (Join-Path $wwwroot $rel))) {
    $bad.Add("  html が指す script が publish 成果物に無い: $s" +
             "`n      指紋の埋め込み（OverrideHtmlAssetPlaceholders）が外れると、素の名前のまま出る")
  }
}

if (-not $Quiet) {
  Write-Host ("publish を見た: script src {0} 本（外 {1} 本）/ importmap {2}" -f
              $localSrcs.Count, ($srcs.Count - $localSrcs.Count),
              $(if ($map.Success -and $map.Groups['b'].Value.Trim() -ne '') { '在り' } else { '無し' }))
}

if ($bad.Count -gt 0) {
  throw ("publish した Playground は起動しない形になっている（$($bad.Count) 形）:`n" + ($bad -join "`n"))
}

if (-not $Quiet) { Write-Host 'publish した Playground は、インライン script 抜きで起動できる形' }
