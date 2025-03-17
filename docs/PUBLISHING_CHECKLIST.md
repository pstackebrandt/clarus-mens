# Publishing Process Checklist

This checklist guides you through the actual process of publishing your ASP.NET Core application to Azure. Complete the
[Pre-Publishing Checklist](PREPUBLISHING_CHECKLIST.md) before starting this process.

## Local Environment Setup

- [ ] Install required tools:
  - [ ] [Docker Desktop](https://www.docker.com/products/docker-desktop/)
  - [ ] [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
  - [ ] [.NET SDK 9.0](https://dotnet.microsoft.com/download)

- [ ] Docker configuration:
  - [ ] Start Docker Desktop
  - [ ] Verify Docker is running: `docker --version`
  - [ ] Clean up any old containers/images if needed

## Local Testing

- [ ] Build and test Docker image locally:

  ```powershell
  docker build -t clarusmens-api .
  docker run -p 5000:80 clarusmens-api
  ```

- [ ] Verify endpoints:
  - [ ] Check health endpoint: <http://localhost:5000/health>
  - [ ] Access Swagger UI: <http://localhost:5000/swagger>
  - [ ] Test API endpoints using Swagger UI
  - [ ] Verify OpenAPI spec: <http://localhost:5000/openapi>

- [ ] Run automated tests:

  ```powershell
  dotnet test -c Testing
  ```

## Azure Setup

- [ ] Azure account preparation:
  - [ ] Create Azure account if needed
  - [ ] Install Azure CLI
  - [ ] Login to Azure: `az login`
  - [ ] Select appropriate subscription:

    ```powershell
    az account list
    az account set --subscription <subscription-id>
    ```

- [ ] Resource creation:
  - [ ] Create resource group:

    ```powershell
    az group create --name ClarusMensRG --location eastus
    ```

  - [ ] Create App Service plan:

    ```powershell
    az appservice plan create --name ClarusMensPlan --resource-group ClarusMensRG --sku F1 --is-linux
    ```

  - [ ] Create Web App:

    ```powershell
    az webapp create --resource-group ClarusMensRG --plan ClarusMensPlan --name clarusmens-api --deployment-container-image-name mcr.microsoft.com/appsvc/staticsite:latest
    ```

## Container Registry Setup

- [ ] Create and configure Azure Container Registry:

  ```powershell
  az acr create --resource-group ClarusMensRG --name clarusmensregistry --sku Basic
  az acr update --name clarusmensregistry --admin-enabled true
  ```

- [ ] Get registry credentials:

  ```powershell
  az acr credential show --name clarusmensregistry
  ```

## GitHub Configuration

- [ ] Repository setup:
  - [ ] Push code to GitHub if not already done
  - [ ] Configure GitHub repository secrets:
    - [ ] `REGISTRY_URL`
    - [ ] `REGISTRY_USERNAME`
    - [ ] `REGISTRY_PASSWORD`
    - [ ] `AZURE_CREDENTIALS`

- [ ] Create service principal for GitHub Actions:

  ```powershell
  az ad sp create-for-rbac --name "ClarusMensGitHubAction" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/ClarusMensRG --sdk-auth
  ```

## Application Configuration

- [ ] Configure application settings:

  ```powershell
  az webapp config appsettings set --resource-group ClarusMensRG --name clarusmens-api --settings WEBSITES_PORT=80
  ```

- [ ] Set up Application Insights:

  ```powershell
  az monitor app-insights component create --app ClarusMensInsights --location eastus --resource-group ClarusMensRG --application-type web
  ```

## Deployment Verification

- [ ] Monitor deployment:
  - [ ] Check GitHub Actions workflow progress
  - [ ] Monitor container startup in Azure portal
  - [ ] Check application logs:

    ```powershell
    az webapp log tail --name clarusmens-api --resource-group ClarusMensRG
    ```

- [ ] Verify production deployment:
  - [ ] Check health endpoint
  - [ ] Verify Swagger UI access
  - [ ] Test all API endpoints
  - [ ] Monitor Application Insights for any issues

## Post-Deployment

- [ ] Set up monitoring:
  - [ ] Configure Azure Monitor alerts
  - [ ] Set up email notifications for critical issues
  - [ ] Review Application Insights data

- [ ] Documentation:
  - [ ] Update README with production URL
  - [ ] Document any deployment-specific configurations
  - [ ] Record any issues and solutions encountered

## Rollback Plan

- [ ] Document rollback procedure:

  ```powershell
  # Example: Revert to previous deployment
  az webapp deployment slot swap --resource-group ClarusMensRG --name clarusmens-api --slot staging --target-slot production
  ```

- [ ] Test rollback procedure in staging environment

## Final Checks

- [ ] Security:
  - [ ] Verify HTTPS is enforced
  - [ ] Check all endpoints require appropriate authentication
  - [ ] Verify no sensitive data in logs

- [ ] Performance:
  - [ ] Check response times
  - [ ] Monitor resource usage
  - [ ] Verify auto-scaling settings (if configured)

Remember to maintain this checklist for future deployments and update it based on lessons learned from each deployment.
