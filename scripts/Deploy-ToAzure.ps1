# Deploy-ToAzure.ps1
# -----------------
# Purpose: Deploys the Clarus Mens application to Azure App Service, handling all necessary
#          configuration and validation steps based on our deployment experience.
#
# Usage:
#   .\Deploy-ToAzure.ps1 [-ResourceGroup <name>] [-AppName <name>] [-BuildImage] [-UseStaging] [-ImageTag <tag>] [-SwapAfterDeployment] [-DryRun]
#
# Examples:
#   .\Deploy-ToAzure.ps1                                  # Deploy current version to production
#   .\Deploy-ToAzure.ps1 -BuildImage                      # Build image, then deploy to production
#   .\Deploy-ToAzure.ps1 -UseStaging -SwapAfterDeployment # Deploy to staging slot then swap
#   .\Deploy-ToAzure.ps1 -ImageTag v0.8.0                 # Deploy specific version
#   .\Deploy-ToAzure.ps1 -BuildImage -DryRun              # Simulate deployment without making changes

# There are some linter warnings about "restricted functions" related to the use of Invoke-Expression, which
# is used to execute dynamic command strings. This approach was chosen to avoid PowerShell escaping issues
# that were documented in the lessons learned.
# This has been fixed by replacing Invoke-Expression with the & call operator and proper argument arrays.

param (
    [string]$ResourceGroup = "clarus-mens-rg",
    [string]$AppName = "clarus-mens-app",
    [string]$SlotName = "",
    [switch]$BuildImage,
    [switch]$UseStaging,
    [string]$ImageTag = "",
    [switch]$SwapAfterDeployment,
    [switch]$Verbose,
    [switch]$DryRun # New parameter for dry run mode
)

#region Setup and Validation

# Set strict error handling
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue" # Speeds up Invoke-WebRequest

# Create timestamp for logging
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logFile = "deploy_log_$timestamp.txt"

function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $logMessage = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') [$Level] $Message"
    
    # Always write to console
    if ($Level -eq "ERROR") {
        Write-Host $logMessage -ForegroundColor Red
    }
    elseif ($Level -eq "WARNING") {
        Write-Host $logMessage -ForegroundColor Yellow
    }
    elseif ($Level -eq "SUCCESS") {
        Write-Host $logMessage -ForegroundColor Green
    }
    else {
        Write-Host $logMessage
    }
    
    # Write to log file
    $logMessage | Out-File -Append -FilePath $logFile
}

function Test-CommandExists {
    param (
        [string]$Command
    )
    
    $exists = $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
    return $exists
}

# Add a function to execute or simulate commands
function Invoke-CommandWithDryRun {
    param (
        [string]$Command,
        [array]$Arguments,
        [string]$Description = ""
    )
    
    $commandLine = "$Command $($Arguments -join ' ')"
    $message = if ($Description) { $Description } else { "Command" }
    
    if ($DryRun) {
        Write-Log "[DRY RUN] Would execute: $message - $commandLine" "WARNING"
        return 0  # Simulate success
    }
    else {
        Write-Log "Executing: $message - $commandLine"
        & $Command $Arguments
        return $LASTEXITCODE
    }
}

# Early in the script, add dry run notification
if ($DryRun) {
    Write-Log "=== DRY RUN MODE ENABLED - NO CHANGES WILL BE MADE ===" "WARNING"
    Write-Log "This will show all commands that would be executed without actually running them" "WARNING"
}

# Check prerequisites
Write-Log "Checking prerequisites..."

# Verify Azure CLI is installed
if (-not (Test-CommandExists "az")) {
    Write-Log "Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" "ERROR"
    exit 1
}

# Verify Docker is installed if building image
if ($BuildImage -and -not (Test-CommandExists "docker")) {
    Write-Log "Docker is not installed, but -BuildImage was specified. Please install Docker Desktop." "ERROR"
    exit 1
}

# Check Azure CLI login status
$azLoginCheck = az account show --query name -o tsv 2>$null
if (-not $azLoginCheck) {
    Write-Log "Not logged into Azure. Attempting to log in..." "WARNING"
    if (-not $DryRun) {
        az login
        
        # Check again
        $azLoginCheck = az account show --query name -o tsv 2>$null
        if (-not $azLoginCheck) {
            Write-Log "Failed to log in to Azure. Please log in manually and try again." "ERROR"
            exit 1
        }
    }
    else {
        Write-Log "[DRY RUN] Would attempt to log in to Azure" "WARNING"
    }
}
Write-Log "Logged in to Azure as: $azLoginCheck" "SUCCESS"

# Verify resource group exists
if (-not $DryRun) {
    $resourceGroupExists = az group exists --name $ResourceGroup -o tsv
    if ($resourceGroupExists -ne "true") {
        Write-Log "Resource group '$ResourceGroup' does not exist." "ERROR"
        Write-Log "Create it first with: az group create --name $ResourceGroup --location westeurope" "ERROR"
        exit 1
    }

    # Verify web app exists
    $webAppExists = az webapp show --name $AppName --resource-group $ResourceGroup --query name -o tsv 2>$null
    if (-not $webAppExists) {
        Write-Log "Web app '$AppName' does not exist in resource group '$ResourceGroup'." "ERROR"
        exit 1
    }

    # Verify ACR exists and we can access it
    $registryName = "clarusmenscr"
    $registryExists = az acr show --name $registryName --query name -o tsv 2>$null
    if (-not $registryExists) {
        Write-Log "Azure Container Registry '$registryName' does not exist or cannot be accessed." "ERROR"
        exit 1
    }
}
else {
    Write-Log "[DRY RUN] Would verify resource group: $ResourceGroup" "WARNING"
    Write-Log "[DRY RUN] Would verify web app: $AppName" "WARNING"
    Write-Log "[DRY RUN] Would verify ACR: clarusmenscr" "WARNING"
}

# Determine deployment slot
$deploySlot = ""
if ($UseStaging) {
    $deploySlot = "staging"
    
    # Check if staging slot exists, create if it doesn't
    if (-not $DryRun) {
        $slotExists = az webapp deployment slot list --name $AppName --resource-group $ResourceGroup --query "[?name=='$deploySlot'].name" -o tsv 2>$null
        if (-not $slotExists) {
            Write-Log "Staging slot does not exist. Creating it..." "WARNING"
            $slotArgs = @(
                'webapp', 'deployment', 'slot', 'create',
                '--name', $AppName,
                '--resource-group', $ResourceGroup,
                '--slot', $deploySlot
            )
            $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $slotArgs -Description "Create staging slot"
            if ($exitCode -ne 0) {
                Write-Log "Failed to create staging slot." "ERROR"
                exit 1
            }
            Write-Log "Created staging slot successfully." "SUCCESS"
        }
    }
    else {
        Write-Log "[DRY RUN] Would check if staging slot exists and create if needed" "WARNING"
    }
}

#endregion

#region Version Determination

# Determine version to deploy
$registry = "clarusmenscr.azurecr.io"
$imageName = "clarus-mens"
$fullImageName = ""

if (-not $ImageTag) {
    # No tag specified, read from Directory.Build.props
    if (-not (Test-Path "Directory.Build.props")) {
        Write-Log "Directory.Build.props not found. Please run from the solution root directory." "ERROR"
        exit 1
    }
    
    # Parse version from Directory.Build.props
    [xml]$props = Get-Content "Directory.Build.props"
    $version = $props.Project.PropertyGroup.Version
    
    if (-not $version) {
        Write-Log "Could not determine version from Directory.Build.props" "ERROR"
        exit 1
    }
    
    # Format as v0.9.0
    $tagVersion = "v$version"
    Write-Log "Using version from Directory.Build.props: $tagVersion"
}
else {
    $tagVersion = $ImageTag
    if (-not $tagVersion.StartsWith("v")) {
        $tagVersion = "v$tagVersion"
    }
    Write-Log "Using specified image tag: $tagVersion"
}

$fullImageName = "$registry/$imageName`:$tagVersion"
Write-Log "Full image name: $fullImageName"

#endregion

#region Image Building (if requested)

if ($BuildImage) {
    Write-Log "Building Docker image..."
    
    # Login to ACR
    Write-Log "Logging in to Azure Container Registry..."
    if (-not $DryRun) {
        $acrLoginArgs = @('acr', 'login', '--name', $registryName)
        $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $acrLoginArgs -Description "Log in to ACR"
        if ($exitCode -ne 0) {
            Write-Log "Failed to log in to ACR." "ERROR"
            exit 1
        }
    }
    else {
        Write-Log "[DRY RUN] Would log in to ACR: az acr login --name $registryName" "WARNING"
    }
    
    # Build and push the image using our script
    Write-Log "Building and pushing Docker image with tag $tagVersion..."
    
    # Use our existing script for building
    if (Test-Path ".\scripts\Build-DockerImage.ps1") {
        $scriptPath = ".\scripts\Build-DockerImage.ps1"
        $scriptArgs = @()
        if ($true) { $scriptArgs += "-PushToRegistry" }
        
        $exitCode = Invoke-CommandWithDryRun -Command $scriptPath -Arguments $scriptArgs -Description "Build and push Docker image"
        if ($exitCode -ne 0) {
            Write-Log "Failed to build and push Docker image." "ERROR"
            exit 1
        }
    }
    else {
        Write-Log "Build-DockerImage.ps1 script not found. Building manually..." "WARNING"
        
        # Build the image
        $dockerBuildArgs = @('build', '-t', $fullImageName, '.')
        $exitCode = Invoke-CommandWithDryRun -Command "docker" -Arguments $dockerBuildArgs -Description "Build Docker image"
        if ($exitCode -ne 0) {
            Write-Log "Failed to build Docker image." "ERROR"
            exit 1
        }
        
        # Push the image
        $dockerPushArgs = @('push', $fullImageName)
        $exitCode = Invoke-CommandWithDryRun -Command "docker" -Arguments $dockerPushArgs -Description "Push Docker image to registry"
        if ($exitCode -ne 0) {
            Write-Log "Failed to push Docker image to registry." "ERROR"
            exit 1
        }
    }
    
    Write-Log "Successfully built and pushed Docker image." "SUCCESS"
}

#endregion

#region Deployment

# Use slot parameter for Azure CLI commands
Write-Log "Deploying to $(if ($deploySlot) { "slot: $deploySlot" } else { "production" })..."

# Update container settings using individual commands for better reliability
Write-Log "Setting container image: $fullImageName"

# Use a try-catch block for safer execution
try {
    # Set container image
    $azArgs = @('webapp', 'config', 'container', 'set', 
        '--name', $AppName, 
        '--resource-group', $ResourceGroup)
    if ($deploySlot) { $azArgs += @('--slot', $deploySlot) }
    $azArgs += @('--docker-custom-image-name', $fullImageName)
    
    $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $azArgs -Description "Set container image"
    if ($exitCode -ne 0) {
        throw "Failed to set container image"
    }
    
    # Set critical environment variables - ensure ports are configured correctly
    Write-Log "Setting critical application settings..."
    
    # Set each setting individually for better reliability - lesson learned from our deployment experience
    $appSettings = @(
        @{ name = "WEBSITES_PORT"; value = "80" },
        # HTTP is intentional here - Azure App Service handles TLS termination at the edge
        # while containers must listen on HTTP internally
        # DevSkim: ignore DS137138
        @{ name = "ASPNETCORE_URLS"; value = "http://+:80" },
        @{ name = "ASPNETCORE_ENVIRONMENT"; value = "Production" },
        @{ name = "DOCKER_CUSTOM_IMAGE_NAME"; value = $fullImageName }
    )
    
    foreach ($setting in $appSettings) {
        $azArgs = @('webapp', 'config', 'appsettings', 'set',
            '--name', $AppName,
            '--resource-group', $ResourceGroup)
        if ($deploySlot) { $azArgs += @('--slot', $deploySlot) }
        $azArgs += @('--settings', "$($setting.name)=$($setting.value)")
        
        Write-Log "Setting $($setting.name)=$($setting.value)"
        $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $azArgs -Description "Set app setting: $($setting.name)"
        if ($exitCode -ne 0) {
            throw "Failed to set application setting: $($setting.name)"
        }
    }
    
    # Restart the app to apply all settings
    Write-Log "Restarting web app to apply settings..."
    $azArgs = @('webapp', 'restart',
        '--name', $AppName,
        '--resource-group', $ResourceGroup)
    if ($deploySlot) { $azArgs += @('--slot', $deploySlot) }
    
    $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $azArgs -Description "Restart web app"
    if ($exitCode -ne 0) {
        throw "Failed to restart web app"
    }
    
    # Wait for the app to restart
    if ($DryRun) {
        Write-Log "[DRY RUN] Would wait for app to restart" "WARNING"
    }
    else {
        Write-Log "Waiting for the application to start..."
        Start-Sleep -Seconds 10
    }
} 
catch {
    Write-Log "Deployment error: $_" "ERROR"
    Write-Log "Command that failed: $($_.InvocationInfo.Line)" "ERROR"
    exit 1
}

#endregion

#region Deployment Verification

if (-not $DryRun) {
    # Get the app URL
    $appUrl = if ($deploySlot) {
        "https://$AppName-$deploySlot.azurewebsites.net"
    }
    else {
        "https://$AppName.azurewebsites.net"
    }

    Write-Log "Verifying deployment at: $appUrl"

    # Check if the app is running by querying the health endpoint
    try {
        Write-Log "Checking application health..."
        $healthUrl = "$appUrl/health"
        
        # Retry a few times in case the application is still starting
        $maxRetries = 5
        $retryCount = 0
        $success = $false
        
        while (-not $success -and $retryCount -lt $maxRetries) {
            try {
                $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 30
                if ($response.StatusCode -eq 200) {
                    $success = $true
                    Write-Log "Health check successful (HTTP $($response.StatusCode))" "SUCCESS"
                }
                else {
                    Write-Log "Health check returned status code: $($response.StatusCode)" "WARNING"
                }
            } 
            catch {
                $retryCount++
                if ($retryCount -lt $maxRetries) {
                    Write-Log "Health check failed. Retrying in 10 seconds... (Attempt $retryCount of $maxRetries)" "WARNING"
                    Start-Sleep -Seconds 10
                }
                else {
                    Write-Log "Health check failed after $maxRetries attempts." "ERROR"
                    Write-Log "Error: $_" "ERROR"
                }
            }
        }
        
        # Try to get diagnostic info if available
        try {
            $diagnosticsUrl = "$appUrl/api/diagnostics"
            $diagResponse = Invoke-WebRequest -Uri $diagnosticsUrl -UseBasicParsing -TimeoutSec 10
            if ($diagResponse.StatusCode -eq 200) {
                Write-Log "Diagnostic endpoint accessible. Application is running." "SUCCESS"
                if ($Verbose) {
                    Write-Log "Diagnostic information: $($diagResponse.Content)"
                }
            }
        } 
        catch {
            Write-Log "Could not access diagnostic endpoint. Application may have issues." "WARNING"
        }
    } 
    catch {
        Write-Log "Error during verification: $_" "ERROR"
    }
}
else {
    Write-Log "[DRY RUN] Would verify deployment at: https://$AppName$(if ($deploySlot) { "-$deploySlot" }).azurewebsites.net" "WARNING"
    Write-Log "[DRY RUN] Would check health endpoint and diagnostic endpoint" "WARNING"
}

#endregion

#region Slot Swapping (if requested)

if ($UseStaging -and $SwapAfterDeployment) {
    Write-Log "Preparing to swap staging slot to production..."
    
    if ($DryRun) {
        Write-Log "[DRY RUN] Would swap staging slot to production" "WARNING"
    }
    else {
        # Prompt for confirmation before swap
        $confirmation = Read-Host "Are you sure you want to swap staging to production? (y/n)"
        if ($confirmation -eq "y") {
            Write-Log "Swapping staging slot to production..."
            
            $azArgs = @('webapp', 'deployment', 'slot', 'swap',
                '--name', $AppName,
                '--resource-group', $ResourceGroup,
                '--slot', 'staging',
                '--target-slot', 'production')
            
            $exitCode = Invoke-CommandWithDryRun -Command "az" -Arguments $azArgs -Description "Swap slots"
            if ($exitCode -eq 0) {
                Write-Log "Successfully swapped staging to production." "SUCCESS"
            }
            else {
                Write-Log "Failed to swap staging to production." "ERROR"
                exit 1
            }
        }
        else {
            Write-Log "Slot swap cancelled by user." "WARNING"
        }
    }
}

#endregion

#region Summary

if ($DryRun) {
    Write-Log "=== DRY RUN SUMMARY - NO ACTUAL DEPLOYMENT PERFORMED ===" "WARNING"
}
else {
    Write-Log "Deployment Summary:" "SUCCESS"
}
Write-Log "--------------------"
Write-Log "Resource Group: $ResourceGroup"
Write-Log "App Name: $AppName"
Write-Log "Image: $fullImageName"
Write-Log "Environment: $(if ($deploySlot) { $deploySlot } else { "production" })"
$appUrl = if ($deploySlot) { "https://$AppName-$deploySlot.azurewebsites.net" } else { "https://$AppName.azurewebsites.net" }
Write-Log "Application URL: $appUrl"
Write-Log "Health URL: $appUrl/health"
Write-Log "Swagger URL: $appUrl/swagger"
Write-Log "Diagnostics URL: $appUrl/api/diagnostics"
Write-Log "Log Command: az webapp log tail --name $AppName --resource-group $ResourceGroup $(if ($deploySlot) { "--slot $deploySlot" })"
Write-Log "--------------------"
Write-Log "Deployment log saved to: $logFile"

# Open the URL in the default browser
if (-not $DryRun) {
    Write-Log "Opening application URL in browser..."
    Start-Process $appUrl
    Write-Log "Deployment completed successfully!" "SUCCESS"
}
else {
    Write-Log "[DRY RUN] Would open application URL in browser" "WARNING"
    Write-Log "Dry run completed successfully!" "SUCCESS"
}

#endregion 