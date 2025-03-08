# Update-Version.ps1
# -----------------
# Purpose: Updates version numbers in Directory.Build.props following 
#          Semantic Versioning (SemVer).
#
# Usage:
#   .\Update-Version.ps1 -VersionType major|minor|patch|release [-PreRelease <suffix>]
#
# Examples:
#   .\Update-Version.ps1 -VersionType patch          # 1.2.3 → 1.2.4
#   .\Update-Version.ps1 -VersionType minor          # 1.2.3 → 1.3.0  
#   .\Update-Version.ps1 -VersionType minor -PreRelease beta # 1.2.3 → 1.3.0-beta
#   .\Update-Version.ps1 -VersionType release        # 1.2.3-beta → 1.2.3

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("major", "minor", "patch", "release")]
    [string]$VersionType,
    
    [Parameter(Mandatory=$false)]
    [string]$PreRelease = ""
)

# Path to the properties file
$propsPath = "Directory.Build.props"

# Check if the file exists
if (-not (Test-Path $propsPath)) {
    Write-Error "Could not find $propsPath. Make sure you're running the script from the solution root."
    exit 1
}

# Load the XML file
[xml]$props = Get-Content $propsPath

# Get the current version
$currentVersionFull = $props.Project.PropertyGroup.Version
Write-Host "Current version: $currentVersionFull"

# Parse the current version with potential pre-release identifier
$preReleaseSuffix = ""
if ($currentVersionFull -match '^([\d\.]+)(?:\-([a-zA-Z0-9\.\-]+))?$') {
    $currentVersion = $Matches[1]
    if ($Matches.Count -gt 2) {
        $preReleaseSuffix = $Matches[2]
    }
}
else {
    Write-Error "Version format is not valid. It should be in SemVer format (e.g., 1.2.3 or 1.2.3-beta)."
    exit 1
}

# Split the numeric part of the version into parts
$versionParts = $currentVersion -split '\.'
if ($versionParts.Count -lt 3) {
    Write-Error "Version must be in format MAJOR.MINOR.PATCH"
    exit 1
}

# Special case for the "release" version type - just removes pre-release suffix
if ($VersionType -eq "release") {
    if ($preReleaseSuffix -eq "") {
        Write-Host "Version is already a release version without pre-release suffix."
        exit 0
    }
    # Keep the version numbers but remove the pre-release suffix
}
else {
    # Update the appropriate part based on the version type
    if ($VersionType -eq "major") {
        $versionParts[0] = [int]$versionParts[0] + 1
        $versionParts[1] = 0
        $versionParts[2] = 0
    } 
    elseif ($VersionType -eq "minor") {
        $versionParts[1] = [int]$versionParts[1] + 1
        $versionParts[2] = 0
    } 
    elseif ($VersionType -eq "patch") {
        $versionParts[2] = [int]$versionParts[2] + 1
    }
}

# Create the new version string (numeric part)
$newVersionPrefix = $versionParts -join '.'

# Add pre-release identifier if provided
$newVersion = $newVersionPrefix
if ($PreRelease -ne "") {
    $newVersion = "$newVersionPrefix-$PreRelease"
    Write-Host "Adding pre-release suffix: $PreRelease"
}

Write-Host "New version: $newVersion"

# Update version properties in Directory.Build.props
$props.Project.PropertyGroup.Version = $newVersion

# Optional: Set VersionPrefix and VersionSuffix explicitly if they exist
$propertyGroup = $props.Project.PropertyGroup
if ($null -ne $propertyGroup.VersionPrefix) {
    $propertyGroup.VersionPrefix = $newVersionPrefix
}
if ($null -ne $propertyGroup.VersionSuffix) {
    $propertyGroup.VersionSuffix = $PreRelease
}

# Update .NET specific version properties, always using 4-part version
$props.Project.PropertyGroup.AssemblyVersion = "$newVersionPrefix.0"
$props.Project.PropertyGroup.FileVersion = "$newVersionPrefix.0"

# Save the changes
$props.Save((Resolve-Path $propsPath))
Write-Host "Version updated successfully to $newVersion"
Write-Host "Assembly version updated to $newVersionPrefix.0"

# Summary of how to use the new version
Write-Host "`nSummary:"
Write-Host "---------"
Write-Host "NuGet Package Version: $newVersion"
Write-Host "Assembly Version: $newVersionPrefix.0"
if ($PreRelease -ne "") {
    Write-Host "This is a pre-release version. Use -VersionType release to create a stable release."
}