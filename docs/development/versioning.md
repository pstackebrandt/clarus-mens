# Version Management

This document describes the version management approach used in the Clarus Mens project.

## Versioning System

We follow [Semantic Versioning 2.0.0](https://semver.org/) (`MAJOR.MINOR.PATCH`):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backward-compatible functionality additions
- **PATCH** version for backward-compatible bug fixes

## Current Setup

Version information is centralized in the `Directory.Build.props` file in the solution root. This approach ensures that all projects in the solution share the same version number.

### Version Properties

The `Directory.Build.props` file deliberately includes multiple versioning properties:

```xml
<PropertyGroup>
    <!-- Package version using SemVer (can include pre-release tags like -beta) -->
    <Version>0.5.0</Version>
    
    <!-- .NET requires 4-part versioning for assembly -->
    <AssemblyVersion>0.5.0.0</AssemblyVersion>
    <FileVersion>0.5.0.0</FileVersion>
    
    <!-- Optional: Explicit pre-release label for NuGet -->
    <VersionPrefix>0.5.0</VersionPrefix>
    <VersionSuffix></VersionSuffix>
</PropertyGroup>
```

**Why both `<Version>` and `<VersionPrefix>`/`<VersionSuffix>`?**

While these properties may seem redundant, they serve different purposes:

1. **`<Version>`** - Used directly by NuGet packaging and is the most straightforward property for version control
2. **`<VersionPrefix>`/`<VersionSuffix>`** - Provides more flexibility:
   - Allows CI/CD pipelines to inject version suffixes at build time without modifying source files
   - Enables more granular control in specific build scenarios
   - Makes transitioning to pre-release versions easier in the future

The `Update-Version.ps1` script maintains consistency across all these properties.

### Key Files

- `Directory.Build.props` - Contains centralized version information
- `Update-Version.ps1` - PowerShell script for bumping version numbers

## How to Update Versions

### Using the PowerShell Script

To update the version number, run the `Update-Version.ps1` script from the solution root:

```powershell
# Bump patch version (e.g., 0.5.0 → 0.5.1)
.\Update-Version.ps1 -VersionType patch

# Bump minor version (e.g., 0.5.0 → 0.6.0)
.\Update-Version.ps1 -VersionType minor

# Bump major version (e.g., 0.5.0 → 1.0.0)
.\Update-Version.ps1 -VersionType major
```

### Using VS Code Tasks

For convenience, VS Code tasks have been configured to run the script:

1. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on macOS)
2. Type "Tasks: Run Task"
3. Select one of:
   - "Bump Patch Version"
   - "Bump Minor Version"
   - "Bump Major Version"

## When to Update Versions

- **PATCH** version: When making backward-compatible bug fixes
- **MINOR** version: When adding functionality in a backward-compatible manner
- **MAJOR** version: When making incompatible changes to the API

## Version Release Process

1. Determine the appropriate version increment (major, minor, patch)
2. Run the version update script
3. Commit the updated `Directory.Build.props` file
4. Create a tag for the release with a descriptive message: `git tag -a v{VERSION} -m "Version {VERSION}: [Brief description of changes]"`
   - Include a short summary of key changes, new features, or bug fixes
   - Example: `git tag -a v0.5.1 -m "Version 0.5.1: Fixed API response formatting and improved error handling"`
     - Including the version number in both the tag and message ensures consistency and makes it easier to identify the version when viewing git logs or browsing repository interfaces
5. Push the changes and tags: `git push && git push --tags`

## Viewing Version Information

The current version information can be found in:

- `Directory.Build.props` file
- Project assembly metadata
- API documentation via Swagger/OpenAPI
- Application logs (configured at startup)
- API endpoint at `/api/version`

## Accessing Version Information in the Application

### Version Information in API

The application exposes version information through a dedicated endpoint:

```json
GET /api/version

{
    "version": "0.5.0",
    "major": 0,
    "minor": 5,
    "build": 0,
    "revision": 0
}
```

Note: In .NET, the `System.Version` class uses four components (Major.Minor.Build.Revision), where the SemVer "patch" component maps to .NET's "build" component.

### Display Versions and Pre-release Identifiers

The `VersionService` provides methods to generate different version formats:

- `GetVersionString()` - Returns the three-part version number (e.g., "0.5.0")
- `GetDisplayVersion(suffix)` - Returns a display-friendly version with optional suffix and environment name (e.g., "0.5.0-beta (Development)")

## Adding Version Display to Your Application

To make the current version more visible, consider adding version display to:

1. API responses
2. Logs
3. Health check endpoints

## Relationship Between Directory.Build.props and Runtime Version

The version defined in `Directory.Build.props` is used during build time to set:

- `Version` (e.g., "0.5.0") - Used for package versioning
- `AssemblyVersion` (e.g., "0.5.0.0") - Used by .NET for assembly identity
- `FileVersion` (e.g., "0.5.0.0") - Used by Windows for file properties

The `Update-Version.ps1` script sets AssemblyVersion and FileVersion to "$newVersion.0", adding a fourth component (revision) set to 0.

At runtime, the application accesses this information through `Assembly.GetName().Version` or via the custom `VersionService`.

## Fixing Versioning with SemVer in .NET

Yes, Semantic Versioning (SemVer) is very useful for your .NET project and I recommend implementing it more consistently. SemVer provides clear communication about compatibility and is widely used in the .NET ecosystem, especially for NuGet packages.

## Current Issues

The current versioning approach has some inconsistencies:

1. You're using both SemVer's 3-part versioning and .NET's 4-part versioning
2. Your script appends `.0` to create 4-part versions for `AssemblyVersion` and `FileVersion`
3. There's confusion between SemVer's "patch" and .NET's "build" components
4. Pre-release identifiers aren't consistently handled

## Recommendations for .NET SemVer Implementation

### 1. Consistent Version Properties

```xml
<PropertyGroup>
    <!-- Package version using SemVer (can include pre-release tags like -beta) -->
    <Version>0.5.0</Version>
    
    <!-- .NET requires 4-part versioning for assembly -->
    <AssemblyVersion>0.5.0.0</AssemblyVersion>
    <FileVersion>0.5.0.0</FileVersion>
    
    <!-- Optional: Explicit pre-release label for NuGet -->
    <VersionPrefix>0.5.0</VersionPrefix>
    <VersionSuffix></VersionSuffix>
</PropertyGroup>
```

### 2. Update the Versioning Script

Modify your `Update-Version.ps1` script to:

1. Support pre-release identifiers
2. Handle version transitions between pre-release and stable
3. Properly map SemVer to .NET versioning

```powershell
param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("major", "minor", "patch")]
    [string]$VersionType,
    
    [Parameter(Mandatory=$false)]
    [string]$PreRelease = ""
)

# Current logic...

# Create version strings
$versionPrefix = $versionParts -join '.'
$version = if ($PreRelease) { "$versionPrefix-$PreRelease" } else { $versionPrefix }

# Update properties
$props.Project.PropertyGroup.Version = $version
$props.Project.PropertyGroup.VersionPrefix = $versionPrefix
$props.Project.PropertyGroup.VersionSuffix = $PreRelease
$props.Project.PropertyGroup.AssemblyVersion = "$versionPrefix.0"
$props.Project.PropertyGroup.FileVersion = "$versionPrefix.0"
```

### 3. Align API Response with SemVer

Update your `/api/version` endpoint to clearly show SemVer components:

```json
{
    "version": "0.5.0",
    "semVer": {
        "major": 0,
        "minor": 5,
        "patch": 0,
        "preRelease": "",
        "buildMetadata": ""
    },
    "assemblyVersion": "0.5.0.0"
}
```

### 4. Pre-release Version Handling

Add proper support for pre-release versions:

```powershell
# Examples:
.\Update-Version.ps1 -VersionType minor -PreRelease "beta1"  # 0.5.0 → 0.6.0-beta1
.\Update-Version.ps1 -VersionType patch                      # 0.6.0-beta1 → 0.6.0
```

## Benefits of Consistent SemVer in .NET

1. **Package Management**: Better NuGet package versioning
2. **Dependency Management**: Clearer compatibility expectations
3. **Release Management**: Easier to automate and understand release processes
4. **Industry Standards**: Following best practices used across .NET ecosystem

## Implementation Plan

1. Update `Directory.Build.props` to use the recommended version properties
2. Enhance `Update-Version.ps1` to support pre-release identifiers
3. Modify `VersionService` to better align with SemVer
4. Update documentation to clearly explain the SemVer approach

Would you like me to create specific code examples for any of these components to implement a more consistent SemVer approach?
