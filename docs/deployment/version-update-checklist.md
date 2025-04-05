# Version Update Checklist

**Focus**: This document provides a step-by-step process for version updates in the Clarus Mens project, \
including deployment to Azure and validation of all required settings.

**Maintenance**: This document follows the checklist maintenance rules maintained by Cursor AI.
Updates follow the standard status indicators: ✅ (completed), - [ ] (todo), 🔄 (in-progress), ⚠️ (blocked).

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Pre-Update Planning](#pre-update-planning)
- [Version Number Update](#version-number-update)
- [Docker Image Building](#docker-image-building)
- [Azure Deployment](#azure-deployment)
- [Azure Configuration Validation](#azure-configuration-validation)
- [Post-Deployment Verification](#post-deployment-verification)
- [Automated Validation](#automated-validation)

## Pre-Update Planning

Before starting the version update process:

- [ ] Review changes since the last version to determine appropriate increment type:
  - **PATCH** for bug fixes (1.2.3 → 1.2.4)
  - **MINOR** for new features (1.2.3 → 1.3.0)
  - **MAJOR** for breaking changes (1.2.3 → 2.0.0)
- [ ] Ensure all tests pass locally
- [ ] Ensure all required changes are committed to the repository
- [ ] Create a new branch for the version update (e.g., `version/v0.9.1`)

## Version Number Update

Update the version number in the project:

✅ Create Update-Version.ps1 script for automated version updates  
✅ Configure semantic versioning in Directory.Build.props  

- [ ] Run the Update-Version.ps1 script with the appropriate version type:

```powershell
# For bug fixes
.\scripts\Update-Version.ps1 -VersionType patch

# For new features
.\scripts\Update-Version.ps1 -VersionType minor

# For breaking changes
.\scripts\Update-Version.ps1 -VersionType major
```

- [ ] Verify the version was updated in `Directory.Build.props`:
  - `<Version>` property should reflect the new version
  - `<AssemblyVersion>` and `<FileVersion>` should be updated accordingly
- [ ] Commit the version change:

```powershell
git add Directory.Build.props
git commit -m "Bump version to vX.Y.Z"
```

## Docker Image Building

Build and push the Docker image with the new version:

✅ Create Build-DockerImage.ps1 script with automatic versioning  
✅ Configure automated tagging based on semantic version  

- [ ] Run the Build-DockerImage.ps1 script to build the image:

  ```powershell
  .\scripts\Build-DockerImage.ps1 -PushToRegistry
  ```

- [ ] Verify the image was pushed to Azure Container Registry:

  ```powershell
  az acr repository show-tags --name clarusmenscr --repository clarus-mens
  ```

- [ ] Verify the image has the correct version label:

  ```powershell
  docker pull clarusmenscr.azurecr.io/clarus-mens:vX.Y.Z
  docker inspect clarusmenscr.azurecr.io/clarus-mens:vX.Y.Z --format '{{.Config.Labels}}'
  ```

- [ ] Test the Docker image locally:

  ```powershell
  docker run -p 5000:80 clarusmenscr.azurecr.io/clarus-mens:vX.Y.Z
  ```

- [ ] Test endpoints using the HTTP request file:
  1. Open `ClarusMensAPI/api-manual-endpoint-requests.http`
  2. Change the active environment to Docker: `@ClarusMensAPI_HostAddress = {{docker}}`
  3. Send requests to verify all endpoints are working correctly

## Azure Deployment

Deploy the new version to Azure:

✅ Create Deploy-ToAzure.ps1 script for automated deployment  
✅ Configure script to handle resource group and app name parameters  

- [ ] Deploy using the Deploy-ToAzure.ps1 script:

  ```powershell
  # For direct production deployment
  .\scripts\Deploy-ToAzure.ps1
  
  # For deployment to staging slot
  .\scripts\Deploy-ToAzure.ps1 -UseStaging
  ```

- [ ] If using staging slot, verify the application works correctly in staging
- [ ] Swap staging to production if needed:

  ```powershell
  .\scripts\Deploy-ToAzure.ps1 -UseStaging -SwapAfterDeployment
  # Or manually:
  az webapp deployment slot swap --name clarus-mens-app --resource-group clarus-mens-rg \
  --slot staging --target-slot production
  ```

## Azure Configuration Validation

Validate all Azure settings are correctly configured:

✅ Identify critical app settings for validation  
✅ Document expected values for container registry configuration  

- [ ] Verify application settings match expected values:

  ```powershell
  az webapp config appsettings list --name clarus-mens-app --resource-group clarus-mens-rg
  ```

- [ ] Check the following critical settings:
  - [ ] WEBSITES_PORT = 80
  - [ ] ASPNETCORE_URLS = http://+:80
  - [ ] ASPNETCORE_ENVIRONMENT = Production
  - [ ] DOCKER_CUSTOM_IMAGE_NAME = clarusmenscr.azurecr.io/clarus-mens:vX.Y.Z (matching the new version)
- [ ] Verify container registry configuration:

  ```powershell
  az webapp config container show --name clarus-mens-app --resource-group clarus-mens-rg
  ```

- [ ] Check the following registry settings:
  - [ ] DOCKER_REGISTRY_SERVER_URL = [https://clarusmenscr.azurecr.io](https://clarusmenscr.azurecr.io)clarusmenscr.azurecr.io)
  - [ ] DOCKER_REGISTRY_SERVER_USERNAME = clarusmenscr
  - [ ] DOCKER_REGISTRY_SERVER_PASSWORD is set (value will be hidden)
- [ ] Check the diagnostic endpoint for confirmation:

```url
https://clarus-mens-app.azurewebsites.net/api/diagnostics
```

- [ ] Verify returned values match expected configuration

## Post-Deployment Verification

Complete final verification of the deployed version:

✅ Create diagnostic endpoint for runtime verification  
✅ Implement version endpoint to verify deployed version  

- [ ] Check the version endpoint to confirm the deployed version:

  ```url
  https://clarus-mens-app.azurewebsites.net/api/version
  ```

- [ ] Test endpoints using the HTTP request file:
  1. Open `ClarusMensAPI/api-manual-endpoint-requests.http`
  2. Change the active environment to Azure: `@ClarusMensAPI_HostAddress = {{azure}}`
  3. Send requests to test all endpoints
- [ ] Verify key functionality works in the application
- [ ] Check application logs for any issues:

  ```powershell
  az webapp log tail --name clarus-mens-app --resource-group clarus-mens-rg
  ```

- [ ] Create a Git tag for the release:

  ```powershell
  git tag -a vX.Y.Z -m "Version X.Y.Z: [Brief description]"
  git push origin vX.Y.Z
  ```

- [ ] If applicable, update release notes or changelog

## Automated Validation

To automate the validation of Azure settings:

🔄 Implement -Validate parameter in Deploy-ToAzure.ps1 script  
🔄 Create validation function in deployment script  

- [ ] Run validation independently or after deployment:

  ```powershell
  .\scripts\Deploy-ToAzure.ps1 -Validate
  ```

This would verify all critical settings are correctly configured and match expected values, including:

1. Comparing actual settings to expected settings
2. Verifying the deployed container image matches the version in Directory.Build.props
3. Confirming environment variables are correctly set
4. Producing a validation report showing pass/fail status for each setting

**Implementation Proposal**: Update the Deploy-ToAzure.ps1 script to add a validation function that:

- Pulls all app settings and container settings via Azure CLI
- Compares against expected values for each critical setting
- Uses /api/diagnostics endpoint to validate runtime settings
- Reports results in a structured format
- Returns non-zero exit code if validation fails
