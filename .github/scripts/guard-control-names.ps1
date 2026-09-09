#requires -Version 7
<#
.SYNOPSIS
  Playground の操作の口が、機械に届く名前を持っているかを見る。

.DESCRIPTION
  キーだけで回っている人には、フォーカスが当たった口の名前しか手がかりが無い。
  画面に字が書いてあっても、**その字が口に結びついていなければ届かない**。

  名前は 3 通り の付き方をする ——

      包む label    <label class="mode">配色<select id="theme"></select></label>
      aria-label    <select id="pattern" aria-label="弾幕">
      口の中の字    <button type="button" id="play">Play</button>

  **`label[for=]` だけを探すと、包む形を全部 見落とす。** 一度 それで
  「名前が届かない口が 11 / 23」と数えた。正しくは 1 / 23 だった。

  **行では読まない。** `<label>` の開きと中の口が別の行に在ることがある
  （`#rate` `#player` `#rank` がそう）。`<label` と `</label>` を数えて
  区間を作り、その中に居るかで見る。開いたまま閉じない label が在れば落とす ——
  区間がずれたまま「名前が在る」と読むほうが危ない。

  **コメントを先に落とす。** この repo の html はコメントが濃く、その中に
  例示として `<select …>` が書いてある。最初に書いたとき、実際に例示の口を
  「id が無い口」として拾った。

  **button の中身は同じ行に在る前提で読む。** またがっている button が
  在れば落とす。前提が崩れたことを、判定より先に出す。

  **またぎは 2 か所 で起きる。** 中身（`</button>` が次の行）だけでなく、
  **開き札の `>` が次の行に在る**ときも読めない —— そちらは網に 1 度 も
  当たらないので、口が数から消えて**違反 0 件 と同じ顔をする**。
  属性を改行して並べたときに踏んだ。

  較正は guard-control-names.Tests.ps1。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string]$IndexHtml,
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
if (-not $IndexHtml) { $IndexHtml = Join-Path $RepoRoot 'src/FsBulletML2.Playground/wwwroot/index.html' }

if (-not (Test-Path -LiteralPath $IndexHtml)) {
  throw "index.html を読めなかった（$IndexHtml）。**口 0 個 は違反 0 件 と同じ顔をする**ので、ここで落とす"
}

# 名前を持たなくてよい口。理由を必ず添える。
$OutOfScope = @{
  # 画面に出ない。Open が JS から click するだけで、人はこれを触らない
  'open-file' = 'type="file" hidden。Open から呼ぶだけで画面に出ない'
}

# **コメントを先に落とす。** 字数と行を保ったまま空白へ潰す ——
# 行番号がずれると、報告が指す先が変わる
$stripped = [regex]::Replace(
  [IO.File]::ReadAllText($IndexHtml),
  '(?s)<!--.*?-->',
  { param($m) $m.Value -replace '[^\r\n]', ' ' })
$lines = $stripped -split "\r?\n"

# --- label の区間を作る ------------------------------------------------------
# 開きと閉じを数えるだけ。**属性の値に "<label" は現れない**ので素朴でよい
$depth = 0
$labelDepth = New-Object int[] $lines.Count
for ($i = 0; $i -lt $lines.Count; $i++) {
  $open = ([regex]::Matches($lines[$i], '<label\b')).Count
  $close = ([regex]::Matches($lines[$i], '</label>')).Count
  # その行に在る口は、行の頭の深さか、その行で開いた label の中に居る
  $labelDepth[$i] = $depth + $open
  $depth = $depth + $open - $close
  if ($depth -lt 0) {
    throw "$($i + 1) 行目 で </label> が開きより多い。区間が数えられないので、名前の在り無しは読めない"
  }
}
if ($depth -ne 0) {
  throw "<label> が $depth 個 閉じていない。区間が数えられないので、名前の在り無しは読めない"
}

# --- 口を集めて、名前が在るかを見る ------------------------------------------
$named = [System.Collections.Generic.List[string]]::new()
$unnamed = [System.Collections.Generic.List[string]]::new()
$skipped = [System.Collections.Generic.List[string]]::new()
$broken = [System.Collections.Generic.List[string]]::new()

for ($i = 0; $i -lt $lines.Count; $i++) {
  $line = $lines[$i]

  # **開き札そのものが行をまたいでいないか。** 下の網は 1 行 に閉じた札しか
  # 拾わないので、`<button` の `>` が次の行に在ると**その口は数から消える** ——
  # 違反 0 件 と同じ顔をする。中身のまたぎ（下の `</button>` の検査）とは
  # 別の穴で、属性を改行して並べたときに踏んだ
  foreach ($o in [regex]::Matches($line, '<(button|select|input)\b')) {
    if (-not [regex]::IsMatch($line.Substring($o.Index), '^<(button|select|input)\b[^>]*>')) {
      $broken.Add("$($i + 1) 行目 $($o.Groups[1].Value) の開き札が行をまたいでいる")
    }
  }

  foreach ($m in [regex]::Matches($line, '<(button|select|input)\b[^>]*>')) {
    $tag = $m.Groups[1].Value
    $idm = [regex]::Match($m.Value, 'id="([^"]+)"')
    if (-not $idm.Success) {
      $broken.Add("$($i + 1) 行目 $tag に id が無い")
      continue
    }
    $id = $idm.Groups[1].Value

    if ($OutOfScope.ContainsKey($id)) { $skipped.Add("$id — $($OutOfScope[$id])"); continue }

    $how = $null
    if ($m.Value -match 'aria-label="[^"]+"') { $how = 'aria-label' }
    elseif ($labelDepth[$i] -gt 0) { $how = '包む label' }
    elseif ($tag -eq 'button') {
      # **中身は同じ行に在る前提。** またがっていたら判定せず落とす
      $rest = $line.Substring($m.Index + $m.Length)
      $endm = [regex]::Match($rest, '^(.*?)</button>')
      if (-not $endm.Success) {
        $broken.Add("$($i + 1) 行目 button#$id が行をまたいでいる")
        continue
      }
      $text = ($endm.Groups[1].Value -replace '<[^>]*>', '').Trim()
      if ($text) { $how = '口の中の字' }
    }

    if ($how) { $named.Add("$id ($how)") } else { $unnamed.Add("$id ($tag / $($i + 1) 行目)") }
  }
}

$total = $named.Count + $unnamed.Count

if (-not $Quiet) {
  Write-Host "口 $total 個 / 名前が要らない $($skipped.Count) 個"
  foreach ($s in $skipped) { Write-Host "    のけた  $s" }
}

# **読めない口が先。** 読めない口は $total に入らないので、後ろに置くと
# 「口を 1 つ も引けなかった」が先に出て、本当の理由が隠れる（較正が出した）
if ($broken.Count -gt 0) {
  throw ("この門が読める形になっていない。名前の在り無しより先に、ここを直す:`n" +
    (($broken | ForEach-Object { "    $_" }) -join "`n"))
}

if ($total -eq 0) {
  throw "操作の口を 1 つ も引けなかった（$IndexHtml）。**違反 0 件 と同じ顔をする**ので、ここで落とす"
}

if ($unnamed.Count -gt 0) {
  throw ("フォーカスしても名前が読めない口が $($unnamed.Count) 個 在る。キーだけで回る人には手がかりが無い:`n" +
    (($unnamed | ForEach-Object { "    $_" }) -join "`n") +
    "`n包む <label> を足すか、aria-label を足すこと。" +
    "`n画面にラベルを置く場所が無いときだけ aria-label（#pattern がそう）")
}

if (-not $Quiet) { Write-Host '操作の口は、どれも名前が機械に届く形になっている' }
