param(
    [string]$Configuration = "Release",
    [string]$SteamModPath = "E:\Steam\steamapps\common\RimWorld\Mods\Outfit Stands Plus Forked"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $repoRoot "Source\OutfitStandsPlus\OutfitStandsPlus.csproj"
$assemblyPath = Join-Path $repoRoot "Assemblies\OutfitStandsPlus.dll"
$dotnetHome = Join-Path $repoRoot ".dotnet"
$runtimeDirs = @("About", "Defs", "Languages", "Patches", "Assemblies")

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Project file not found: $projectPath"
}

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_HOME = $dotnetHome

dotnet build $projectPath -c $Configuration /p:UseSharedCompilation=false
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Expected assembly was not produced: $assemblyPath"
}

New-Item -ItemType Directory -Force -Path $SteamModPath | Out-Null

foreach ($dir in $runtimeDirs) {
    $target = Join-Path $SteamModPath $dir
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Recurse -Force
    }
}

foreach ($dir in $runtimeDirs) {
    $source = Join-Path $repoRoot $dir
    $target = Join-Path $SteamModPath $dir
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Runtime source directory not found: $source"
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Recurse -Force
}

$deployedPdb = Join-Path $SteamModPath "Assemblies\OutfitStandsPlus.pdb"
if (Test-Path -LiteralPath $deployedPdb) {
    Remove-Item -LiteralPath $deployedPdb -Force
}

Write-Host "Deployed Outfit Stands Plus Forked runtime files to $SteamModPath"
