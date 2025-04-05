# Publishing Checklist (Training Project)

**Focus**: Simple steps to publish the training/template API project using Docker and Azure App Service.
This checklist covers the actual implementation process used in our deployment.

**Note**: For a more automated deployment process, see our new
[Azure Deployment Workflow](azure-deployment-workflow.md) which uses the `Deploy-ToAzure.ps1` script to
streamline the deployment process.

For version updates and validation of Azure settings, refer to the
[Version Update Checklist](version-update-checklist.md) which provides a comprehensive process for
managing versions and validating deployments.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Prerequisites](#prerequisites)
- [Local Docker Testing](#local-docker-testing)
- [Azure Resource Setup](#azure-resource-setup)
- [Configuration Updates](#configuration-updates)
- [Deployment](#deployment)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

## Prerequisites

Before starting deployment:

- [x] Complete critical items in [Pre-Publishing Checklist](prepublishing-checklist.md)
- [x] Install Docker Desktop
- [x] Install and configure Azure CLI
- [x] Create an Azure account (free tier is sufficient)
- [x] Authenticate with Azure: `az login`

## Local Docker Testing

Verify everything works locally:

- [x] Build Docker image:

```bash
docker build -t clarusmens-api:v2 .
```

- [x] Run container:

```bash
docker run -p 5000:80 clarusmens-api:v2
```

- [ ] Test endpoints using the HTTP request file:
  1. Open `ClarusMensAPI/api-manual-endpoint-requests.http`
  2. Change the active environment to Docker: `@ClarusMensAPI_HostAddress = {{docker}}`
  3. Send requests to test the following endpoints:
     - [ ] Health check: `/health`
     - [ ] Swagger: `/swagger`
     - [ ] Diagnostics: `/api/diagnostics`
     - [ ] Version: `/api/version`

## Azure Resource Setup

Follow these steps to create required Azure resources:

1. [x] Create a resource group:

   ```bash
   az group create --name clarus-mens-rg --location westeurope
   ```

2. [x] Register the Microsoft.Web provider (if needed):

   ```bash
   az provider register --namespace Microsoft.Web
   az provider show -n Microsoft.Web --query registrationState
   ```

3. [x] Create Azure Container Registry (ACR):

   ```bash
   az acr create --resource-group clarus-mens-rg --name clarusmenscr --sku Basic --admin-enabled true
   ```

4. [x] Get ACR credentials:

   ```bash
   az acr credential show --name clarusmenscr
   ```

5. [x] Create App Service Plan:

   ```bash
   az appservice plan create --name clarus-mens-plan --resource-group clarus-mens-rg --sku B1 --is-linux
   ```

6. [x] Create Web App:

   ```bash
   az webapp create --resource-group clarus-mens-rg --plan clarus-mens-plan --name clarus-mens-app
   ```

## Configuration Updates

Make these code/configuration updates before deploying:

1. [x] Update Program.cs with environment-specific configurations:
   - [x] Make HTTPS redirection conditional based on environment
   - [x] Add enhanced startup/shutdown logging
   - [x] Add diagnostic endpoint

2. [x] Configure App Service environment variables through Azure Portal:
   - [x] WEBSITES_PORT = 80
   - [x] ASPNETCORE_URLS = http://+:80
   - [x] ASPNETCORE_ENVIRONMENT = Production

3. [x] Configure container settings in Azure Portal:
   - [x] DOCKER_REGISTRY_SERVER_URL = <https://clarusmenscr.azurecr.io>
   - [x] DOCKER_REGISTRY_SERVER_USERNAME = clarusmenscr
   - [x] DOCKER_REGISTRY_SERVER_PASSWORD = [registry password]
   - [x] DOCKER_CUSTOM_IMAGE_NAME = clarusmenscr.azurecr.io/clarus-mens:v2
   - [x] WEBSITES_ENABLE_APP_SERVICE_STORAGE = true

## Deployment

Deploy the container image to Azure:

**Note on Versioning**: We've transitioned from simple sequential versioning (v1, v2) to semantic versioning \
that matches our application version. Use the Build-DockerImage.ps1 script for consistent versioning.

**Process Automation**: For a more comprehensive version update process, refer to the \
[Version Update Checklist](version-update-checklist.md) document.

1. [x] Log in to ACR:

   ```bash
   az acr login --name clarusmenscr
   ```

2. [x] Build Docker image with version from Directory.Build.props:

   ```powershell
   # Recommended approach - handles versioning automatically
   .\scripts\Build-DockerImage.ps1
   
   # Previous approach (for reference only)
   # docker build -t clarusmenscr.azurecr.io/clarus-mens:v2 .
   ```

3. [x] Push image to ACR:

   ```powershell
   # Recommended approach - builds and pushes in one step
   .\scripts\Build-DockerImage.ps1 -PushToRegistry
   
   # Previous approach (for reference only)
   # docker push clarusmenscr.azurecr.io/clarus-mens:v2
   ```

4. [x] Update Web App container image (replace 0.9.0 with your current version):

   ```powershell
   az webapp config container set --name clarus-mens-app --resource-group clarus-mens-rg \
   --docker-custom-image-name clarusmenscr.azurecr.io/clarus-mens:v0.9.0
   ```

5. [x] Restart the Web App:

   ```bash
   az webapp restart --name clarus-mens-app --resource-group clarus-mens-rg
   ```

## Verification

After deployment, verify everything works:

**Note on Validation**: For automated validation of Azure settings, use the new `-Validate` parameter
with the Deploy-ToAzure.ps1 script:

```powershell
.\scripts\Deploy-ToAzure.ps1 -Validate
```

- [x] Check app availability at <https://clarus-mens-app.azurewebsites.net>
- [ ] Verify these endpoints:
  - [ ] Health check: <https://clarus-mens-app.azurewebsites.net/health>
  - [ ] Swagger: <https://clarus-mens-app.azurewebsites.net/swagger>
  - [ ] Diagnostics: <https://clarus-mens-app.azurewebsites.net/api/diagnostics>
  - [ ] Version: <https://clarus-mens-app.azurewebsites.net/api/version>
- [ ] Validate Azure configuration settings:

  ```powershell
  az webapp config appsettings list --name clarus-mens-app --resource-group clarus-mens-rg
  az webapp config container show --name clarus-mens-app --resource-group clarus-mens-rg
  ```

- [ ] Check logs and diagnostics:

  ```bash
  az webapp log tail --name clarus-mens-app --resource-group clarus-mens-rg
  ```

## Troubleshooting

Common issues and solutions:

1. [x] **Container startup issues**:
   - Ensure port configuration is correct (WEBSITES_PORT=80, ASPNETCORE_URLS=http://+:80)
   - Make HTTPS redirection conditional based on environment
   - Check startup logs for errors

2. [x] **Configuration issues**:
   - Use Azure Portal for configuration rather than CLI when dealing with complex values
   - Set one setting at a time if you encounter issues
   - Escape special characters properly in PowerShell
   - Use the new validation feature to verify settings: `.\scripts\Deploy-ToAzure.ps1 -Validate`

3. [x] **Authentication issues**:
   - Re-authenticate with `az login` if session expires
   - Verify container registry credentials are correct
   - Check ACR firewall settings

4. [ ] **Application errors**:
   - Use diagnostic endpoint to verify runtime environment
   - Check application logs for detailed error information
   - Temporarily set ASPNETCORE_ENVIRONMENT=Development for more detailed errors
