# Build-DockerImage.ps1
# -----------------
# Purpose: Builds a Docker image with version information from Directory.Build.props
#
# Usage:
#   .\Build-DockerImage.ps1 [-PushToRegistry] [-Registry <registry>]
#
# Examples:
#   .\Build-DockerImage.ps1                     # Just build locally
#   .\Build-DockerImage.ps1 -PushToRegistry     # Build and push to clarusmenscr.azurecr.io
#   .\Build-DockerImage.ps1 -Registry myregistry.azurecr.io -PushToRegistry # Custom registry

param (
    [switch]$PushToRegistry,
    [string]$Registry = "clarusmenscr.azurecr.io"
)

# Get version from Directory.Build.props
$propsPath = "Directory.Build.props"
if (-not (Test-Path $propsPath)) {
    Write-Error "Could not find $propsPath. Make sure you're running the script from the solution root."
    exit 1
}

$version = ([xml](Get-Content $propsPath)).Project.PropertyGroup.Version
Write-Host "Building Docker image for version $version"

# Remove pre-release tag for image tag if present
$imageTag = $version
if ($version -match "^([\d\.]+)(?:\-([a-zA-Z0-9\.\-]+))?$") {
    # For Docker tags, we'll use v1.2.3 format and add -beta for pre-release
    $numericVersion = $Matches[1]
    $preRelease = if ($Matches.Count -gt 2) { $Matches[2] } else { "" }
    
    if ($preRelease) {
        $imageTag = "v$numericVersion-$preRelease"
    }
    else {
        $imageTag = "v$numericVersion"
    }
}

# Image name with registry and tag
$imageName = "$Registry/clarus-mens:$imageTag"

# Build the Docker image with version argument
Write-Host "Building image: $imageName"
docker build --build-arg VERSION=$version -t $imageName .

# Check if build was successful
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed!"
    exit 1
}

Write-Host "Successfully built Docker image: $imageName"

# Push to registry if requested
if ($PushToRegistry) {
    Write-Host "Pushing to registry: $Registry"
    
    # Log in to the registry (Azure Container Registry)
    if ($Registry -match "\.azurecr\.io$") {
        Write-Host "Logging in to Azure Container Registry..."
        az acr login --name $Registry.Split('.')[0]
    }
    
    # Push the image
    docker push $imageName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push Docker image to registry!"
        exit 1
    }
    
    Write-Host "Successfully pushed Docker image to registry: $imageName"
}

# Add latest tag if this is a release version (no pre-release suffix)
if ($version -notmatch "-") {
    $latestImageName = "$Registry/clarus-mens:latest"
    Write-Host "Tagging as latest: $latestImageName"
    docker tag $imageName $latestImageName
    
    if ($PushToRegistry) {
        docker push $latestImageName
        Write-Host "Pushed latest tag to registry"
    }
}

Write-Host "`nSummary:"
Write-Host "---------"
Write-Host "Image: $imageName"
Write-Host "App Version: $version"
if ($PushToRegistry) {
    Write-Host "Registry: $Registry"
} 