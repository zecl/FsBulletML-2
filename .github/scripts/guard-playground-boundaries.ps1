#requires -Version 7
<#
.SYNOPSIS
  Playground が守ると決めた線を、機械で見る。

.DESCRIPTION
  実装計画の「完成条件」のうち、目で確かめると書いてあったものを門にする。
  **どれも いまは守れている**ので、これは網ではなく現状固定 ——
  「いま守れている形を、次の PR で崩さない」ためだけに置く。
  例外は 3 つ目 で、当てる名前は DTD から毎回 引き直すので、
  そちらは要素が増えれば網も広がる。

  ## 見るもの

  1. 追跡された `.js` が Playground の下に無い
     ブラウザ側は Fable で書く。glue を手で書き始めると、そこだけ型も
     検査も掛からなくなる（v0.2 で一度 捨てている）

  2. Core が Bolero / Monaco / Blazor を参照していない
     Core は表示を知らない。参照が付くと、Unity も MonoGame も
     ブラウザの物を引きずる

  3. Fable 側に BulletML の要素名が書かれていない
     語彙は host が `Core/DTD.fs` から焼いて渡す。**当てる名前は
     `WriteStartElement(...)` から引く** —— 門の中に表を持たない

  4. `index.html` に起動のロジックが無い
     html に書くと、そこだけ型も検査も掛からない

  5. Playground の csproj で `UseSystemResourceKeys` が `false`
     読めなかった理由をそのまま人へ見せるので、例外の文面が字である必要が
     ある。Blazor WASM の SDK は Release でこれを立てるので、放っておくと
     `Xml_TagMismatchEx` のような鍵が波線に載る。**動きは変わらないので、
     外れても誰も気づかない**

  ## 0 件 を緑にしない

  当てる材料が読めなければ赤にする —— 3 つ目 の要素名、Fable のソース、
  5 つ目 の csproj。拾えなくなった状態は、「違反 0 件」と同じ顔をする。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 追跡ファイルの一覧（repo からの相対）。渡さなければ git から引く
  [string[]]$Files,
  # 以下は較正で差し替えるためだけに開けてある
  [string]$PlaygroundDir,
  [string]$CoreProj,
  [string]$DtdSource,
  [string]$IndexHtml,
  [string]$PlaygroundProj,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '\\', '/').TrimEnd('/')

if (-not $PlaygroundDir) { $PlaygroundDir = 'src/FsBulletML2.Playground' }
if (-not $CoreProj) { $CoreProj = "$RepoRoot/src/FsBulletML2.Core/FsBulletML2.Core.fsproj" }
if (-not $DtdSource) { $DtdSource = "$RepoRoot/src/FsBulletML2.Core/DTD.fs" }
if (-not $IndexHtml) { $IndexHtml = "$RepoRoot/$PlaygroundDir/wwwroot/index.html" }
if (-not $PlaygroundProj) { $PlaygroundProj = "$RepoRoot/$PlaygroundDir/FsBulletML2.Playground.fsproj" }
if (-not $PSBoundParameters.ContainsKey('Files')) {
  $Files = @(git -C $RepoRoot -c core.quotepath=false ls-files)
}

$prefix = ($PlaygroundDir -replace '\\', '/').TrimEnd('/') + '/'
$bad = [System.Collections.Generic.List[string]]::new()

# --- 1. 手書きの .js -------------------------------------------------------
$js = @($Files | Where-Object { $_.StartsWith($prefix) -and $_.ToLower().EndsWith('.js') })
if ($js.Count -gt 0) {
  $bad.Add("  追跡された .js が $($js.Count) 件:`n      " + ($js -join "`n      ") +
           "`n      ブラウザ側は Fable で書く。焼いた js は .gitignore の下に置く")
}

# --- 2. Core の参照 --------------------------------------------------------
if (Test-Path -LiteralPath $CoreProj) {
  $refs = @(Select-String -LiteralPath $CoreProj -Pattern 'Bolero|Monaco|Blazor' |
            ForEach-Object { "{0} 行  {1}" -f $_.LineNumber, $_.Line.Trim() })
  if ($refs.Count -gt 0) {
    $bad.Add("  Core が表示側を参照している:`n      " + ($refs -join "`n      "))
  }
}

# --- 3. Fable 側に要素名 ---------------------------------------------------
#
# **当てる名前は DTD から引く。** 門の中に表を持つと、要素が増えたとき
# 表だけが古びる
$names = @()
if (Test-Path -LiteralPath $DtdSource) {
  $names = @(Select-String -LiteralPath $DtdSource -Pattern 'WriteStartElement\("([^"]+)"' -AllMatches |
             ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}
if ($names.Count -eq 0) {
  throw "当てる要素名を 1 つ も引けなかった（$DtdSource）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}

# 文字列の足し算で組む。**`"$prefix`fable"` と書くと `` `f `` が改ページになり**、
# ディレクトリが見つからず「走査 0 本 で違反 0 件」になる（実際にやった）
$fableDir = Join-Path $RepoRoot ($prefix + 'fable')
$fableFiles = @()
if (Test-Path -LiteralPath $fableDir) {
  $fableFiles = @(Get-ChildItem -LiteralPath $fableDir -Recurse -Filter '*.fs' -File |
                  ForEach-Object { $_.FullName })
}
if ($fableFiles.Count -eq 0) {
  throw "Fable のソースを 1 本 も読めなかった（$fableDir）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$hardcoded = [System.Collections.Generic.List[string]]::new()
foreach ($n in $names) {
  $hits = Select-String -LiteralPath $fableFiles -Pattern ('"' + [regex]::Escape($n) + '"') -CaseSensitive
  foreach ($h in $hits) {
    $hardcoded.Add(("{0} {1} 行  {2}" -f (Split-Path $h.Path -Leaf), $h.LineNumber, $h.Line.Trim()))
  }
}
if ($hardcoded.Count -gt 0) {
  $bad.Add("  Fable 側に要素名が $($hardcoded.Count) 件:`n      " + ($hardcoded -join "`n      ") +
           "`n      語彙は host が Core/DTD.fs から焼いて渡す。ここに表を持たない")
}

# --- 4. html のロジック ----------------------------------------------------
if (Test-Path -LiteralPath $IndexHtml) {
  $logic = @(Select-String -LiteralPath $IndexHtml -Pattern 'Blazor\.start' |
             ForEach-Object { "{0} 行  {1}" -f $_.LineNumber, $_.Line.Trim() })
  if ($logic.Count -gt 0) {
    $bad.Add("  html に起動のロジックが $($logic.Count) 件:`n      " + ($logic -join "`n      ") +
             "`n      起こすのは Fable の側。html は読み込むだけ")
  }
}

# --- 5. 例外の文面 ---------------------------------------------------------
#
# **現状固定。** 外すと Release で resource key に戻り、波線に載る文面が
# 鍵になる。動きは変わらないので、外れても走行では気づけない
if (-not (Test-Path -LiteralPath $PlaygroundProj)) {
  throw "Playground の csproj を読めなかった（$PlaygroundProj）。" +
        "**違反 0 件 と同じ顔をする**ので、ここで落とす"
}
$resourceKeys = @(Select-String -LiteralPath $PlaygroundProj `
                    -Pattern '<UseSystemResourceKeys>([^<]*)</UseSystemResourceKeys>' -AllMatches |
                  ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value.Trim() })
if ($resourceKeys.Count -eq 0) {
  $bad.Add("  UseSystemResourceKeys が csproj に無い:`n      $PlaygroundProj" +
           "`n      Blazor WASM の SDK が Release で立てるので、書かないと鍵に戻る")
} elseif ($resourceKeys.Count -ne 1 -or $resourceKeys[0] -ne 'false') {
  $bad.Add("  UseSystemResourceKeys が false でない: " + ($resourceKeys -join ', ') +
           "`n      読めなかった理由をそのまま人へ見せる。文面は字で出す")
}

if (-not $Quiet) {
  Write-Host ("走査 {0} 件 / 当てる要素名 {1} 個 / Fable のファイル {2} 本" -f
              $Files.Count, $names.Count, $fableFiles.Count)
}

if ($bad.Count -gt 0) {
  throw ("Playground の線を越えているものが $($bad.Count) 形:`n" + ($bad -join "`n"))
}

if (-not $Quiet) { Write-Host 'Playground は線の内側に居る' }
