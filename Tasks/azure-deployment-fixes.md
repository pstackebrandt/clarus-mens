# Azure Deployment Tasks

## Overview

This file tracks the current tasks for fixing and improving the Azure deployment process for the ClarusMens API.
The main issue is that the dockerized application stops after starting in Azure App Service.

## Current Tasks

### Immediate Tasks

🔄 Test current deployment state

- [ ] Log in to Azure: `az login`
- [ ] Build the Docker image locally: `.\scripts\Build-DockerImage.ps1`
- [ ] Test container health locally: `.\scripts\Test-DockerImage.ps1 -TestType HealthCheck`
- [ ] Push container to Azure Container Registry: `.\scripts\Build-DockerImage.ps1 -PushToRegistry`
- [ ] Deploy to Azure App Service: `.\scripts\Deploy-ToAzure.ps1`
- [ ] Monitor startup logs: `az webapp log tail --name clarus-mens-app --resource-group clarus-mens-rg`
- [ ] Test endpoints after deployment:
  - [ ] Health check: <https://clarus-mens-app.azurewebsites.net/health>
  - [ ] API version: <https://clarus-mens-app.azurewebsites.net/api/version>

### Diagnostic Tasks

- [ ] Verify Azure configuration settings:

  ```powershell
  az webapp config appsettings list --name clarus-mens-app --resource-group clarus-mens-rg
  az webapp config container show --name clarus-mens-app --resource-group clarus-mens-rg
  ```

- [ ] Check if the following critical settings are correct:
  - [ ] WEBSITES_PORT=80
  - [ ] ASPNETCORE_URLS=http://+:80
  - [ ] DOCKER_CUSTOM_IMAGE_NAME is set correctly
  - [ ] ASPNETCORE_ENVIRONMENT=Production

### Fix Implementation

- [ ] Adjust health check configuration in Dockerfile if necessary
- [ ] Fix any port configuration mismatches
- [ ] Address environment variable issues
- [ ] Consider disabling health check temporarily for testing:

  ```powershell
  az webapp config appsettings set --name clarus-mens-app --resource-group clarus-mens-rg --settings WEBSITES_DISABLE_CONTAINER_HEALTHCHECK=true
  ```

### Documentation Updates

- [ ] Update deployment documentation with findings
- [ ] Document the solution to prevent future issues
- [ ] Add troubleshooting steps to deployment guide

## References

- [Azure Container Troubleshooting Guide](../docs/deployment/azure-container-troubleshooting.md)
- [Publishing Checklist](../docs/deployment/publishing-checklist.md)
- [Deploy-ToAzure.ps1 Script](../scripts/Deploy-ToAzure.ps1)
- [Test-DockerImage.ps1 Script](../scripts/Test-DockerImage.ps1)

## Notes

Remember to:

1. Test changes locally before deploying to Azure
2. Check logs frequently to identify issues
3. Validate all endpoint functionality after fixing issues
