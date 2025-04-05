# Azure Deployment Validation Feature

This document provides a specification for the `-Validate` parameter to be added to the `Deploy-ToAzure.ps1` script.

## Overview

The validation feature will provide a way to verify that Azure App Service settings match expected values \
after deployment. This helps ensure that the application is properly configured and reduces the risk of \
configuration-related issues.

## Implementation Specification

### Parameter Definition

```powershell
[Parameter()]
[switch]$Validate
```

### Validation Function

```powershell
function Validate-AzureSettings {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$ResourceGroup,
        
        [Parameter(Mandatory = $true)]
        [string]$AppName,
        
        [Parameter(Mandatory = $false)]
        [string]$ExpectedImageTag,
        
        [Parameter(Mandatory = $false)]
        [switch]$Verbose
    )
    
    # Implementation details below
}
```

### What Will Be Validated

1. **App Settings Validation**
   - Critical environment variables:
     - WEBSITES_PORT
     - ASPNETCORE_URLS
     - ASPNETCORE_ENVIRONMENT
     - DOCKER_CUSTOM_IMAGE_NAME
   - Optional environment variables:
     - APPLICATION_INSIGHTS_CONNECTION_STRING
     - WEBSITE_TIME_ZONE
     - ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS

2. **Container Registry Configuration**
   - DOCKER_REGISTRY_SERVER_URL
   - DOCKER_REGISTRY_SERVER_USERNAME
   - Existence of DOCKER_REGISTRY_SERVER_PASSWORD (value can't be verified)

3. **Runtime Configuration**
   - Hit the `/api/diagnostics` endpoint
   - Verify returned environment variables match expected values
   - Verify application is responding correctly

4. **Version Validation**
   - Verify the version from `/api/version` matches expected version
   - Confirm Docker image tag matches version in Directory.Build.props

### Validation Process

1. **Get expected values**
   - Read version from Directory.Build.props
   - Determine expected container image name
   - Define expected values for all critical settings

2. **Retrieve actual values**
   - Use Azure CLI to get app settings
   - Use Azure CLI to get container settings
   - Make HTTP request to diagnostic endpoint

3. **Compare values**
   - Match each expected value with actual value
   - Generate pass/fail status for each setting
   - Calculate overall validation score

4. **Generate report**
   - Create formatted table of results
   - Highlight any failed validations
   - Provide suggestions for fixing issues

### Example Output

```console
============== Azure Configuration Validation Report ==============

App: clarus-mens-app (Resource Group: clarus-mens-rg)
Version: 0.9.1
Timestamp: 2023-04-01 15:30:45

----- App Settings -----
[✓] WEBSITES_PORT: 80
[✓] ASPNETCORE_URLS: http://+:80
[✓] ASPNETCORE_ENVIRONMENT: Production
[✓] DOCKER_CUSTOM_IMAGE_NAME: clarusmenscr.azurecr.io/clarus-mens:v0.9.1
[!] APPLICATION_INSIGHTS_CONNECTION_STRING: Missing (Optional)

----- Container Registry -----
[✓] DOCKER_REGISTRY_SERVER_URL: https://clarusmenscr.azurecr.io
[✓] DOCKER_REGISTRY_SERVER_USERNAME: clarusmenscr
[✓] DOCKER_REGISTRY_SERVER_PASSWORD: Set (value hidden)

----- Runtime Configuration -----
[✓] API Diagnostics: Accessible
[✓] Environment Variables: Match configuration
[✓] Version API: Returns v0.9.1

----- Summary -----
Total Checks: 10
Passed: 9
Failed: 0
Warnings: 1

Validation Result: PASSED

Notes:
- APPLICATION_INSIGHTS_CONNECTION_STRING is not set but is optional
- All critical settings validated successfully
```

### Integration with Deploy Script

The validation feature will be integrated into the main script flow:

1. **Standalone validation**:

   ```powershell
   .\Deploy-ToAzure.ps1 -Validate
   ```

   This will only perform validation without any deployment.

2. **Post-deployment validation**:

   ```powershell
   .\Deploy-ToAzure.ps1 -BuildImage -Validate
   ```

   This will perform deployment and then validate the configuration.

### Error Handling

- If validation fails, the script will output detailed information about which checks failed
- The script will return a non-zero exit code if validation fails
- For CI/CD pipelines, this allows automated detection of configuration issues

### Advantages

1. Reduces manual verification steps
2. Ensures consistent configuration across environments
3. Catches configuration issues early
4. Provides clear documentation of expected settings
5. Facilitates automated deployment validation in CI/CD pipelines
