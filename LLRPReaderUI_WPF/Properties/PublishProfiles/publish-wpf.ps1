param(
    [string]$Project = ".\LLRPReaderUI_WPF.csproj",
    [string]$PubCfg = "FolderProfile",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$fixedProjectRoot = "F:\Projects\LLRP\LlrpSDK-参考OctaneSdk\LLRPReaderUI_WPF"
if (-not (Test-Path $fixedProjectRoot)) {
    throw "Fixed project path not found: $fixedProjectRoot"
}

Set-Location $fixedProjectRoot

if (-not (Test-Path $Project)) {
    throw "Project file not found: $Project"
}

$projectFullPath = (Resolve-Path $Project).Path
$projectDir = Split-Path $projectFullPath -Parent

[xml]$xml = Get-Content -Path $projectFullPath -Encoding UTF8
$propertyGroups = @($xml.Project.PropertyGroup)

$version = $null
$versionPrefix = $null
$versionSuffix = $null

foreach ($group in $propertyGroups) {
    if (-not $version -and $group.Version) { $version = $group.Version }
    if (-not $versionPrefix -and $group.VersionPrefix) { $versionPrefix = $group.VersionPrefix }
    if (-not $versionSuffix -and $group.VersionSuffix) { $versionSuffix = $group.VersionSuffix }
}

if ([string]::IsNullOrWhiteSpace($version)) {
    if ([string]::IsNullOrWhiteSpace($versionPrefix)) {
        $version = "dev"
    }
    elseif ([string]::IsNullOrWhiteSpace($versionSuffix)) {
        $version = $versionPrefix
    }
    else {
        $version = "$versionPrefix-$versionSuffix"
    }
}

$publishDir = Join-Path $projectDir "bin\publish\$version\"

Write-Host "Project      : $projectFullPath"
Write-Host "Profile      : $PubCfg"
Write-Host "Configuration: $Configuration"
Write-Host "Version      : $version"
Write-Host "PublishDir   : $publishDir"

$arguments = @(
    "publish",
    $projectFullPath,
    "-c", $Configuration,
    "/p:PublishProfile=$PubCfg",
    "/p:PublishDir=$publishDir",
    "/p:PublishUrl=$publishDir"
)

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Publish completed: $publishDir"
