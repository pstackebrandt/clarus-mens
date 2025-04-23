[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Validate", "HealthCheck", "API", "All")]
    [string]$TestType = "All",
    
    [Parameter()]
    [string]$ImageName = "clarus-mens",
    
    [Parameter()]
    [switch]$SkipBuild,
    
    [Parameter()]
    [int]$ApiPort = 8080,
    
    [Parameter()]
    [switch]$KeepContainer,
    
    [Parameter()]
    [switch]$Detailed
)

# Output formatting
function Write-Step {
    param([string]$Message)
    Write-Host "`n==== $Message ====" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Failure {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Extract version from Directory.Build.props
function Get-ProjectVersion {
    try {
        $propsPath = "Directory.Build.props"
        if (-not (Test-Path $propsPath)) {
            Write-Warning "Directory.Build.props not found in current directory"
            return "0.0.0"
        }
        
        $xml = [xml](Get-Content $propsPath)
        $version = $xml.Project.PropertyGroup.Version
        if (-not $version) {
            Write-Warning "Version not found in Directory.Build.props"
            return "0.0.0"
        }
        
        return $version
    }
    catch {
        Write-Failure "Error extracting version: $_"
        return "0.0.0"
    }
}

# Validate Dockerfile syntax and build process
function Test-DockerfileValidation {
    Write-Step "Testing Dockerfile Validation"
    
    $validateTag = "$ImageName-validate"
    
    try {
        Write-Host "Building with buildx to validate Dockerfile..." -ForegroundColor Gray
        docker buildx build --progress=plain -t $validateTag . | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Dockerfile validation successful"
            return $true
        }
        else {
            Write-Failure "Dockerfile validation failed"
            return $false
        }
    }
    catch {
        Write-Failure "Error during Dockerfile validation: $_"
        return $false
    }
    finally {
        # Clean up validation image
        docker rmi $validateTag -f 2>$null | Out-Null
    }
}

# Build the Docker image with version
function Build-DockerImage {
    param([string]$Version)
    
    Write-Step "Building Docker Image"
    Write-Host "Building $ImageName with version $Version..." -ForegroundColor Gray
    
    docker build --build-arg VERSION=$Version -t $ImageName .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Docker image built successfully: $ImageName"
        return $true
    }
    else {
        Write-Failure "Docker image build failed"
        return $false
    }
}

# Test container health
function Test-ContainerHealth {
    param([string]$ContainerName)
    
    Write-Step "Testing Container Health"
    
    $maxAttempts = 5
    $attempt = 0
    $healthy = $false
    
    Write-Host "Waiting for container to initialize..." -ForegroundColor Gray
    Start-Sleep -Seconds 3
    
    while ($attempt -lt $maxAttempts) {
        $attempt++
        $status = docker inspect --format='{{.State.Health.Status}}' $ContainerName 2>$null
        
        if ($status -eq "healthy") {
            $healthy = $true
            break
        }
        
        if ($Detailed) {
            Write-Host "Health check attempt $attempt/${maxAttempts}: $status" -ForegroundColor Gray
        }
        
        Start-Sleep -Seconds 2
    }
    
    if ($healthy) {
        Write-Success "Container health check passed"
        
        if ($Detailed) {
            # Show container logs for detailed mode
            Write-Host "`nContainer logs:" -ForegroundColor Gray
            docker logs $ContainerName
        }
        
        return $true
    }
    else {
        Write-Failure "Container health check failed after $maxAttempts attempts"
        Write-Host "`nContainer logs:" -ForegroundColor Gray
        docker logs $ContainerName
        return $false
    }
}

# Test API endpoints
function Test-ApiEndpoints {
    param(
        [string]$ContainerName,
        [int]$Port,
        [string]$ExpectedVersion
    )
    
    Write-Step "Testing API Endpoints"
    
    $baseUrl = "http://localhost:$Port"
    $endpoints = @(
        @{Path = "/health"; ExpectJsonContent = $false; Name = "Health Check" },
        @{Path = "/api/version"; ExpectJsonContent = $true; Name = "Version API" }
    )
    
    $successes = 0
    
    foreach ($endpoint in $endpoints) {
        $url = "$baseUrl$($endpoint.Path)"
        
        try {
            Write-Host "Testing endpoint: $($endpoint.Name) ($url)" -ForegroundColor Gray
            
            # Use Invoke-WebRequest to test the endpoint
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10
            
            # Check status code
            if ($response.StatusCode -eq 200) {
                Write-Success "$($endpoint.Name) responded with status 200 OK"
                $successes++
                
                # For version endpoint, verify version matches expected
                if ($endpoint.Path -eq "/api/version" -and $endpoint.ExpectJsonContent) {
                    $content = $response.Content | ConvertFrom-Json
                    
                    if ($content.version -eq $ExpectedVersion) {
                        Write-Success "Version matches expected: $ExpectedVersion"
                    }
                    else {
                        Write-Failure "Version mismatch: Expected $ExpectedVersion, got $($content.version)"
                    }
                    
                    if ($Detailed) {
                        # Show full version response
                        Write-Host "Full version response:" -ForegroundColor Gray
                        $response.Content
                    }
                }
            }
            else {
                Write-Failure "$($endpoint.Name) failed with status $($response.StatusCode)"
            }
        }
        catch {
            Write-Failure "Error testing $($endpoint.Name): $_"
        }
    }
    
    # Check container labels
    try {
        $labelVersion = docker inspect --format='{{index .Config.Labels "org.clarus-mens.version"}}' $ImageName
        
        if ($labelVersion -eq $ExpectedVersion) {
            Write-Success "Docker image label version matches expected: $ExpectedVersion"
            $successes++
        }
        else {
            Write-Failure "Docker image label version mismatch: Expected $ExpectedVersion, got $labelVersion"
        }
    }
    catch {
        Write-Failure "Error checking Docker image labels: $_"
    }
    
    # Return true if all checks passed
    return $successes -eq ($endpoints.Count + 1)
}

# Clean up resources
function Clean-Resources {
    param([string]$ContainerName)
    
    if (-not $KeepContainer) {
        Write-Host "Cleaning up resources..." -ForegroundColor Gray
        docker stop $ContainerName 2>$null | Out-Null
        docker rm $ContainerName 2>$null | Out-Null
    }
    else {
        Write-Host "Container $ContainerName left running on port $ApiPort" -ForegroundColor Yellow
    }
}

# Main execution
$version = Get-ProjectVersion
$containerName = "$ImageName-test"
$success = $true

# Track overall test success
$testResults = @{
    Validation = $false
    Build      = $false
    Health     = $false
    API        = $false
}

# Perform validation test
if ($TestType -eq "Validate" -or $TestType -eq "All") {
    $testResults.Validation = Test-DockerfileValidation
    $success = $success -and $testResults.Validation
}

# Build the image if needed
if (-not $SkipBuild -and ($TestType -eq "HealthCheck" -or $TestType -eq "API" -or $TestType -eq "All")) {
    $testResults.Build = Build-DockerImage -Version $version
    $success = $success -and $testResults.Build
}
else {
    $testResults.Build = $true
}

# Run container and test health
if (($TestType -eq "HealthCheck" -or $TestType -eq "API" -or $TestType -eq "All") -and $testResults.Build) {
    try {
        # Run container
        Write-Step "Starting Container"
        docker run -d --name $containerName -p "$($ApiPort):80" $ImageName | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            # Health check
            if ($TestType -eq "HealthCheck" -or $TestType -eq "All") {
                $testResults.Health = Test-ContainerHealth -ContainerName $containerName
                $success = $success -and $testResults.Health
            }
            else {
                $testResults.Health = $true
            }
            
            # API tests
            if (($TestType -eq "API" -or $TestType -eq "All") -and $testResults.Health) {
                $testResults.API = Test-ApiEndpoints -ContainerName $containerName -Port $ApiPort -ExpectedVersion $version
                $success = $success -and $testResults.API
            }
        }
        else {
            Write-Failure "Failed to start container"
            $success = $false
        }
    }
    finally {
        Clean-Resources -ContainerName $containerName
    }
}

# Display summary
Write-Step "Test Summary"

if ($TestType -eq "Validate" -or $TestType -eq "All") {
    if ($testResults.Validation) { Write-Success "Dockerfile Validation: Passed" } else { Write-Failure "Dockerfile Validation: Failed" }
}

if (($TestType -eq "HealthCheck" -or $TestType -eq "API" -or $TestType -eq "All") -and -not $SkipBuild) {
    if ($testResults.Build) { Write-Success "Docker Build: Passed" } else { Write-Failure "Docker Build: Failed" }
}

if ($TestType -eq "HealthCheck" -or $TestType -eq "All") {
    if ($testResults.Health) { Write-Success "Container Health Check: Passed" } else { Write-Failure "Container Health Check: Failed" }
}

if ($TestType -eq "API" -or $TestType -eq "All") {
    if ($testResults.API) { Write-Success "API Endpoints: Passed" } else { Write-Failure "API Endpoints: Failed" }
}

# Set exit code based on overall success
exit [int](-not $success) 