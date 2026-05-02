$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "..\_Shared\RimWorldModTools.ps1"
. $toolPath

$repoRoot = $PSScriptRoot
$projectPath = Join-Path $repoRoot "Source\OutfitStandsPlus\OutfitStandsPlus.csproj"
$targetRoot = Get-RimWorldModTargetPath -ModName "Outfit Stands Plus Forked"

Invoke-RimWorldDotNetBuild -ProjectPath $projectPath -Configuration "Release" -DotNetHome (Join-Path $repoRoot ".dotnet")
Sync-RimWorldModFolders -SourceRoot $repoRoot -TargetRoot $targetRoot -Folders @("About", "Defs", "Languages", "Patches") -RemoveFiles @("Assemblies\OutfitStandsPlus.dll", "Assemblies\OutfitStandsPlus.pdb")

Write-Host "Deployed Outfit Stands Plus Forked to $targetRoot"
