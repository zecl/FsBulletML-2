#requires -Version 7
<#
.SYNOPSIS
  共有リンクの往復を、焼いた JS のまま走らせる。

.DESCRIPTION
  リンクの中身は 2 つ に割れている ——

      組み立てと base64url   器の `ShareLink`。**.NET でも走る**
      圧縮                   `fable/Share.fs`。**ブラウザにしか無い**

  前半は `guard-fable-parity.ps1` と `Parser.Tests` が当てている。
  **後半はここでしか当たらない** —— `CompressionStream` は .NET に無いので、
  `dotnet test` を何本 増やしても 1 度 も通らない。

  ## 見るもの

  1. `tests/TestData` の本文が、リンクにして戻すと **1 文字 も変わらない**
     表記も戻る（拡張子から決めたものと、戻ってきたものを突き合わせる）
  2. **大きい本文でも落ちない**（塊で詰めない書き方はここで stack を割る）
  3. 版が違う / 字が壊れている / 途中で切れている リンクは
     **「読めない」と言う**（黙って空にしない）

  ## 長さは数えるが、点をつけない

  いちばん長いリンクの字数と、素との合計は出す。**が、そこに線を引かない** ——

      上限の字を書くと、弾幕が長くなったときに門のほうが古びる
      「圧縮が効いているか」を長さで見ようとすると、当てる先が
      **作る側と解く側を両方 替えたとき**しかない（片方 だけなら往復が落ちる）。
      しかも符号がコーパスの中身で動く —— 小さい本文ばかりになると、
      圧縮は効いているのに赤が出る

  数は人が読む。**門は往復と「読めない」の 2 つ だけ見る。**

  ## 0 件 を緑にしない

  焼いた JS が無い / コーパスに 1 本 も無い / 走行が数を返さないのは全部 赤。
  どれも「食い違い 0 件」と同じ顔をする。

  焼くのは `dotnet build src/FsBulletML2.Playground`（BeforeBuild で fable が走る）。
  **焼き直す前にここを回すと、ソースだけ直して JS が古い状態が赤で出る。**
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 以下は較正で差し替えるためだけに開けてある
  [string]$ShareJs,
  [string]$KindJs,
  [string]$CorpusDir,
  # 走らせる口。**較正が拡張子の対応を落とした写しへ向ける** ——
  # 拾えなくなった状態を「往復 0 件 で緑」にしないところを見るため
  [string]$MjsPath,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path

$jsDir = Join-Path $RepoRoot 'src/FsBulletML2.Playground/wwwroot/js'
if (-not $ShareJs) { $ShareJs = Join-Path $jsDir 'Share.js' }
if (-not $KindJs) { $KindJs = Join-Path $jsDir 'FsBulletML2.LanguageService/SourceKind.js' }
if (-not $CorpusDir) { $CorpusDir = Join-Path $RepoRoot 'tests/TestData' }
if (-not $MjsPath) { $MjsPath = Join-Path $RepoRoot '.github/scripts/guard-share-roundtrip.mjs' }

if (-not (Test-Path -LiteralPath $MjsPath)) { throw "走らせる口が無い（$MjsPath）" }
foreach ($p in @($ShareJs, $KindJs)) {
  if (-not (Test-Path -LiteralPath $p)) {
    throw "焼いた JS が無い（$p）。**違反 0 件 と同じ顔をする**ので、ここで落とす。" +
          " dotnet build src/FsBulletML2.Playground で焼ける"
  }
}
if (-not (Test-Path -LiteralPath $CorpusDir)) { throw "本文の在り処が無い（$CorpusDir）" }

# **こちらでも数える。** 走らせた側の数だけを見ると、拡張子の対応が
# ずれて 1 本 も拾わなかった状態が「往復 0 件 で緑」になる
$corpus = @(Get-ChildItem -LiteralPath $CorpusDir -Recurse -File |
            Where-Object { $_.Extension.ToLower() -in @('.xml', '.sxml', '.fsb', '.fsx') })
if ($corpus.Count -eq 0) {
  throw "本文が 1 本 も無い（$CorpusDir）。**往復 0 件 と同じ顔をする**ので、ここで落とす"
}

$raw = & node $MjsPath $ShareJs $KindJs $CorpusDir 2>&1 | ForEach-Object { [string]$_ }
$ng = @($raw | Where-Object { $_ -match "^NG`t" } | ForEach-Object { '  ' + ($_ -replace "^NG`t", '') })
$sum = @($raw | Where-Object { $_ -match "^SUM`t" })

if ($ng.Count -gt 0) {
  throw ("共有リンクの往復が $($ng.Count) 件 外れた:`n" + ($ng -join "`n"))
}
if ($sum.Count -ne 1) {
  $head = ($raw | Select-Object -First 12) -join "`n"
  throw "走行が数を返さなかった（SUM が $($sum.Count) 行）。`n$head"
}

$parts = $sum[0] -split "`t"
if ($parts.Count -ne 6) { throw "走行が返した数の形が違う: $($sum[0])" }
$files = [int]$parts[1]
$maxLink = [int]$parts[2]
$maxLinkAt = $parts[3]
$rawBytes = [long]$parts[4]
$linkChars = [long]$parts[5]

if ($files -eq 0) {
  throw "往復した本文が 0 件。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
if ($files -ne $corpus.Count) {
  throw "往復した数が合わない（走行 $files 件 / 在るのは $($corpus.Count) 件）。" +
        "拾えていない本文が在る"
}
if (-not $Quiet) {
  Write-Host ("$files 件 を往復して食い違い 0 件 / いちばん長いリンク $maxLink 字（$maxLinkAt）" +
              " / 素 $rawBytes バイト -> リンク $linkChars 字")
}
