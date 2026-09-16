#requires -Version 7
<#
.SYNOPSIS
  追跡されているコードと script に、その機械にしか無い絶対パスが
  焼き込まれていないかを見る。

.DESCRIPTION
  `tools/StubShapeCheck` が、repo の置き場と Unity の置き場を
  書いた人の機械のまま持っていた。
  **書いた人の手元では完全に緑になる。** repo を別の場所に置いた人、
  Unity を別のドライブに入れた人のところでだけ落ちる。

  build も試験もこれを見ない —— 走る機械が 1 台 しか無いから。
  目でも出ない。パスは字面が自然で、読み流せてしまう。

  ## 何に当てるか

  $Ext の拡張子で、追跡されているもの。**`.md` は入れない** ——
  記録は「そのとき その場所で こうだった」を書くものなので、
  絶対パスが載っているのが正しい。

  ## 但し書き

  $Allow に置く。**当たらなくなった但し書きは赤にする** ——
  守る対象が消えたのに但し書きだけ残ると、次に同じ穴が開いたときに
  黙って通る。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  [string[]]$Files,
  # 通す先と、通す理由。**理由が書けないものは通さない**。
  # 既定がこの repo の本番の並びで、渡せるのは校正のため
  [hashtable[]]$Allow = @(
    @{ Path = 'src/FsBulletML2.Docs/*'
       Why  = 'slnx の外。元の repo から持ち越したまま建てていない' }
    @{ Path = 'src/FsBulletML2.TypeProviders/ProvidedTypes.fs'
       Why  = 'よそから貰った現物。こちらで直さない' }
    @{ Path = '.github/scripts/guard-abs-paths.Tests.ps1'
       Why  = 'この門の校正の材料。落ちる側を字で置いてある' }
  ),
  [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '\\', '/').TrimEnd('/')

$Ext = @('.ps1', '.psm1', '.cs', '.fs', '.fsx', '.fsi', '.csproj', '.fsproj',
         '.props', '.targets', '.slnx', '.sln', '.yml', '.yaml', '.sh', '.cmd', '.bat')

# 名前で出す。「何が当たったか」だけだと、直し方が分からない
$Patterns = @(
  @{ Name = 'ドライブから始まる絶対パス'; Rx = '(?<![A-Za-z0-9_])[A-Za-z]:[\\/]' }
  @{ Name = '誰かの home';                Rx = '(?<![A-Za-z0-9_])/(Users|home)/[A-Za-z0-9_.-]' }
  @{ Name = 'WSL のマウント';             Rx = '(?<![A-Za-z0-9_])/mnt/[a-z]/' }
)

$whole = -not $PSBoundParameters.ContainsKey('Files')
if ($whole) {
  # 非 ASCII のパスが化けると開けなくなるので quotepath を切る
  $Files = @(git -C $RepoRoot -c core.quotepath=false ls-files |
             ForEach-Object { "$RepoRoot/$_" })
}

$scanned = 0
$allowed = 0
$hitAllow = @{}
$bad = [System.Collections.Generic.List[string]]::new()

foreach ($f in $Files) {
  if ($Ext -notcontains [IO.Path]::GetExtension($f).ToLower()) { continue }
  if (-not (Test-Path -LiteralPath $f)) { continue }

  $rel = ($f -replace '\\', '/').Replace("$RepoRoot/", '')
  $skip = $Allow | Where-Object { $rel -like $_.Path } | Select-Object -First 1

  $scanned++
  $found = [System.Collections.Generic.List[string]]::new()
  $n = 0
  foreach ($line in [IO.File]::ReadAllLines($f)) {
    $n++
    foreach ($p in $Patterns) {
      $m = [regex]::Match($line, $p.Rx)
      if (-not $m.Success) { continue }
      # 当たったところの前後を少しだけ出す。行が長いと読めない
      $from = [Math]::Max(0, $m.Index - 8)
      $len = [Math]::Min(64, $line.Length - $from)
      $found.Add(("{0} 行  {1}  …{2}…" -f $n, $p.Name, $line.Substring($from, $len).Trim()))
      break
    }
  }
  if ($found.Count -eq 0) { continue }

  if ($skip) {
    $allowed += $found.Count
    $hitAllow[$skip.Path] = $true
    continue
  }
  $bad.Add(("  {0}`n      {1}" -f $rel, ($found -join "`n      ")))
}

if (-not $Quiet) {
  Write-Host "走査 $scanned 件 / 但し書きで通した $allowed 件"
}

# **守る対象が消えたら、但し書きも一緒に消す。**
# 残しておくと、次に同じところへ絶対パスが入ったとき黙って通る
if ($whole) {
  $dead = @($Allow | Where-Object { -not $hitAllow.ContainsKey($_.Path) })
  if ($dead.Count -gt 0) {
    throw ("もう当たらない但し書きが $($dead.Count) 件:`n" +
           (($dead | ForEach-Object { "  $($_.Path)" }) -join "`n") +
           "`n`n`$Allow から消すこと")
  }
}

if ($bad.Count -gt 0) {
  throw ("その機械にしか無い絶対パスが $($bad.Count) 件:`n" + ($bad -join "`n") +
         "`n`n引数か環境変数で渡すか、自分の居場所から探すこと。" +
         "書いた人の手元では緑のまま、他の機械でだけ落ちる")
}

if (-not $Quiet) { Write-Host '機械に貼り付いた絶対パスは無い' }
