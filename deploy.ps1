$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "..\_Shared\RimWorldModTools.ps1"
. $toolPath

Invoke-RimWorldModDeploy `
    -ModName "Outfit Stands Plus Forked" `
    -SourceRoot $PSScriptRoot `
    -BuildPath (Join-Path $PSScriptRoot "Source\OutfitStandsPlus\OutfitStandsPlus.csproj") `
    -Configuration "Release" `
    -DotNetHome (Join-Path $PSScriptRoot ".dotnet") `
    -Folders @("About", "Assemblies", "Defs", "Languages", "NoOdyssey", "Patches") `
    -Files @("LoadFolders.xml") `
    -RemoveFilePatterns @("*.pdb")
