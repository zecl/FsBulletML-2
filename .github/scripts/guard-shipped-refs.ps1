#requires -Version 7
<#
.SYNOPSIS
  同梱している dll が、どのアセンブリから型を引いているかを見る。

.DESCRIPTION
  Unity のサンプルは `Assets/FsBulletML2/` に置いた dll を読む。その dll は
  Unity を持っていない機械でも建つように `src/*.Stub` を参照して焼く。
  **stub のアセンブリ名が本物とずれていると、そのずれがそのまま dll の
  参照として焼き込まれる。**

  実際に 1 度 やっている。ECS と URP の型が `AssemblyName=UnityEngine` の
  stub に同居していて、焼いた dll が「`Unity.Entities.Entity` は UnityEngine
  に在る」と主張したまま Unity へ渡った。本物の `UnityEngine.dll` は
  型フォワードだけの facade なので、`Transform` や `Vector3` は通り、
  **ECS と URP を触った所だけ** CS7069 で落ちた。

  `-p:UseShippedDlls=true` の build も stub を参照するので、**そちらは
  最後まで緑だった。** この門はそこを見る。

  ## 何と突き合わせるか

  対応表は手で書かない。`src/*.Stub` の各プロジェクトから

      AssemblyName（本物と同じ名前を名乗っているはず）
      その中の .cs が宣言している namespace

  を読んで表にする。stub を足したり割ったりすれば表も動く。

  ## この門が見ないこと

  **stub の名乗り自体が本物と違う場合は当たらない**（表も一緒に間違う）。
  そこは Unity での実行と `-p:UseRealUnity=true` の build が見る。
#>
[CmdletBinding()]
param(
  [string]$RepoRoot,
  # 同梱 dll の在り処を差し替える（較正で、古い dll を当てるのに使う）
  [string[]]$ShippedDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.Reflection.Metadata

if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel) }
$RepoRoot = ($RepoRoot -replace '/', '\').TrimEnd('\')

# --- 表を作る: 型 -> 名乗るべきアセンブリ名 -----------------------------
#
# **namespace 単位では引けない。** `Unity.Collections` は本物でも 2 つ に
# 分かれていて、`Allocator` と `NativeArray<>` は UnityEngine.CoreModule、
# `AllocatorManager` はパッケージのほうに在る。型 1 つ ずつ引く。
$table = @{}
$stubDlls = Get-ChildItem -Path (Join-Path $RepoRoot 'src') -Directory |
  Where-Object { $_.Name -like '*.Stub' } |
  ForEach-Object {
    Get-ChildItem -Path $_.FullName -Filter '*.dll' -File -Recurse -ErrorAction SilentlyContinue |
      Where-Object { $_.FullName -match '\\bin\\Release\\' -and $_.BaseName -eq ($_.Name -replace '\.dll$', '') } |
      Where-Object { $_.BaseName -eq ($_.Directory.Parent.Parent.Parent.Name -replace '\.Stub$', '') } |
      Select-Object -First 1
  }

function Get-Decls ($path) {
  $fs = [IO.File]::OpenRead($path)
  try {
    $pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
    try {
      $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      $asm = $md.GetString($md.GetAssemblyDefinition().Name)
      $out = @()
      foreach ($h in $md.TypeDefinitions) {
        $td = $md.GetTypeDefinition($h)
        $ns = $md.GetString($td.Namespace)
        if (-not $ns) { continue }
        # コンパイラが各アセンブリに生やす印。どの stub にも同じ名前で出る
        if ($ns -eq 'Microsoft.CodeAnalysis' -or $ns -eq 'System.Runtime.CompilerServices') { continue }
        $out += [pscustomobject]@{ Asm = $asm; Ns = $ns; Name = $md.GetString($td.Name) }
      }
      $out
    } finally { $pe.Dispose() }
  } finally { $fs.Dispose() }
}

foreach ($dll in $stubDlls) {
  if (-not $dll) { continue }
  foreach ($d in (Get-Decls $dll.FullName)) {
    $key = $d.Ns + '.' + $d.Name
    if ($table.ContainsKey($key) -and $table[$key] -ne $d.Asm) {
      throw "型 $key が 2 つ の stub に在る: $($table[$key]) / $($d.Asm)"
    }
    $table[$key] = $d.Asm
  }
}
if ($table.Count -eq 0) {
  Write-Host '表が空。src/*.Stub の dll が 1 つ も読めていない（先に build すること）'
  exit 1
}

# --- 同梱 dll を読む -----------------------------------------------------
if (-not $ShippedDir) {
  $ShippedDir = Get-ChildItem -Path (Join-Path $RepoRoot 'samples') -Directory -Recurse |
    Where-Object { $_.FullName -match '\\Assets\\FsBulletML2$' } |
    ForEach-Object { $_.FullName }
}
if (-not $ShippedDir) { Write-Host '同梱 dll の置き場が 1 つ も無い'; exit 1 }

function Get-TypeRefs ($path) {
  $fs = [IO.File]::OpenRead($path)
  try {
    $pe = New-Object System.Reflection.PortableExecutable.PEReader($fs)
    try {
      $md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
      $out = @()
      foreach ($h in $md.TypeReferences) {
        $tr = $md.GetTypeReference($h)
        $scope = $tr.ResolutionScope
        if ($scope.Kind -ne 'AssemblyReference') { continue }
        $ar = $md.GetAssemblyReference([System.Reflection.Metadata.AssemblyReferenceHandle]::op_Explicit($scope))
        $out += [pscustomobject]@{
          Asm  = $md.GetString($ar.Name)
          Ns   = $md.GetString($tr.Namespace)
          Name = $md.GetString($tr.Name)
        }
      }
      $out
    } finally { $pe.Dispose() }
  } finally { $fs.Dispose() }
}

$dlls = @()
foreach ($d in $ShippedDir) { $dlls += Get-ChildItem -Path $d -Filter '*.dll' -File }

$bad = @()
foreach ($dll in $dlls) {
  foreach ($r in (Get-TypeRefs $dll.FullName)) {
    $key = $r.Ns + '.' + $r.Name
    if (-not $table.ContainsKey($key)) { continue }
    if ($r.Asm -ne $table[$key]) {
      $bad += [pscustomobject]@{
        Dll = $dll.Name; Ns = $r.Ns; Type = $r.Name; From = $r.Asm; Want = $table[$key]
      }
    }
  }
}

'走査 {0} 本 / 表 {1} 並び / 置き場 {2} か所' -f $dlls.Count, $table.Count, @($ShippedDir).Count
if ($bad.Count -eq 0) {
  '同梱 dll は、どの型も名乗りどおりのアセンブリから引いている'
  exit 0
}

'** 名乗りと違うアセンブリから引いている型が {0} 件 **' -f $bad.Count
$bad | Group-Object Dll | ForEach-Object {
  '  ' + $_.Name
  $_.Group | Group-Object Ns, From, Want | ForEach-Object {
    $g = $_.Group[0]
    '      {0}  <- {1}  （本物は {2}）' -f $g.Ns, $g.From, $g.Want
    '          ' + (($_.Group | ForEach-Object { $_.Type } | Sort-Object -Unique) -join ', ')
  }
}
''
'焼き直すか、src/*.Stub のアセンブリ名を本物に合わせること'
exit 1
