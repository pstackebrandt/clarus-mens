# Docker + Azure App Service Deployment Guide

This document provides a comprehensive guide for containerizing an ASP.NET Core application with Docker
and deploying it to Azure App Service.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Benefits of This Approach](#benefits-of-this-approach)
- [Prerequisites](#prerequisites)
- [Azure Account Setup](#azure-account-setup)
  - [Account Requirements](#account-requirements)
  - [Registration Process](#registration-process)
  - [Regional Considerations](#regional-considerations)
- [Step 1: Containerize Your ASP.NET Core Application](#step-1-containerize-your-aspnet-core-application)
  - [Create a Dockerfile](#create-a-dockerfile)
  - [Create .dockerignore](#create-dockerignore)
  - [Build and Test Locally](#build-and-test-locally)
  - [Building for Different Purposes](#building-for-different-purposes)
    - [Production Build (Default)](#production-build-default)
    - [Testing Build](#testing-build)
- [Step 2: Set Up Azure Resources](#step-2-set-up-azure-resources)
  - [Create Azure App Service Plan and Web App](#create-azure-app-service-plan-and-web-app)
- [Step 3: Set Up GitHub Actions for CI/CD](#step-3-set-up-github-actions-for-cicd)
- [Step 4: Configure Azure Container Registry](#step-4-configure-azure-container-registry)
- [Step 5: Create Azure Service Principal for GitHub Actions](#step-5-create-azure-service-principal-for-github-actions)
- [Step 6: Configure Application Settings](#step-6-configure-application-settings)
- [Future Database Integration](#future-database-integration)
- [Troubleshooting](#troubleshooting)
- [Resources](#resources)

## Benefits of This Approach

- **Containerization**: Learn Docker, an essential skill for modern development
- **Portability**: Deploy the same container to any environment
- **CI/CD Integration**: Automate builds and deployments with GitHub Actions
- **Azure Ecosystem**: Learn Microsoft's cloud platform (valuable for .NET developers)
- **Cost-Effective**: Use Azure's free tier for learning
- **Database Integration**: Easy connection to Azure SQL Database
- **Scalability**: Simple path to scale as your application grows

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed
- [.NET SDK](https://dotnet.microsoft.com/download) (matching your project version)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- An [Azure account](https://azure.microsoft.com/free/) (free tier available)
- [GitHub account](https://github.com/) for source control and GitHub Actions

## Azure Account Setup

### Account Requirements

To set up an Azure account, you will need:

- A valid email address
- A phone number for verification
- A credit or debit card (for identity verification only, no charges unless you upgrade)
- Your address details

The free tier includes 12 months of popular services plus $200 credit for 30 days, which is sufficient
for a training project.

### Registration Process

1. Visit [Azure's free account page](https://azure.microsoft.com/free/)
2. Click the "Start free" button
3. Sign in with your Microsoft account or create a new one
4. Complete the identity verification process:
   - Verify your email address
   - Verify your phone number via text or call
   - Enter your payment card details (for verification only)
5. Accept the subscription agreement and privacy statement
6. Your free Azure subscription will be automatically created

After registration, you can access the Azure Portal at [portal.azure.com](https://portal.azure.com) to
manage your resources.

### Regional Considerations

**For German Users**:

- A German-specific registration page is available at [azure.microsoft.com/de-de/free/](https://azure.microsoft.com/de-de/free/)
- This provides the same services but with:
  - German language interface
  - Pricing in Euros
  - EU data center options for GDPR compliance
  - German-specific support resources
  - Documentation in German at [docs.microsoft.com/de-de/azure](https://docs.microsoft.com/de-de/azure)

## Step 1: Containerize Your ASP.NET Core Application

### Create a Dockerfile

Create a `Dockerfile` in your project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.sln .
COPY ClarusMensAPI/*.csproj ./ClarusMensAPI/
RUN dotnet restore

# Copy all files and build the app
COPY . .
WORKDIR /src/ClarusMensAPI
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Create non-root user for security
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

EXPOSE 80
ENTRYPOINT ["dotnet", "ClarusMensAPI.dll"]
```

### Create .dockerignore

Create a `.dockerignore` file to exclude unnecessary files:

```dockerignore
**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
```

### Build and Test Locally

```powershell
docker build -t clarusmens-api .
docker run -p 5000:80 clarusmens-api
```

Visit `http://localhost:5000/swagger` to verify the application works.

### Building for Different Purposes

The Dockerfile supports conditional inclusion of test projects using build arguments:

#### Production Build (Default)

```powershell
docker build -t clarusmens-api .
```

This builds a production-optimized image without test projects.

#### Testing Build

```powershell
docker build -t clarusmens-api-test --build-arg INCLUDE_TESTS=true .
```

This includes test projects in the image, useful for:

- Running tests in containerized environments
- Testing container-specific behaviors
- CI/CD pipelines that require containerized testing

## Step 2: Set Up Azure Resources

### Create Azure App Service Plan and Web App

```powershell
# Login to Azure
az login

# Create a resource group
az group create --name ClarusMensRG --location eastus

# Create an App Service plan (Free tier)
az appservice plan create --name ClarusMensPlan --resource-group ClarusMensRG --sku F1 --is-linux

# Create a Web App configured for container deployment
az webapp create --resource-group ClarusMensRG --plan ClarusMensPlan --name clarusmens-api --deployment-container-image-name mcr.microsoft.com/appsvc/staticsite:latest
```

## Step 3: Set Up GitHub Actions for CI/CD

Create a GitHub Actions workflow file at `.github/workflows/azure-deploy.yml`:

```yaml
name: Build and Deploy to Azure

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Login to Azure Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ secrets.REGISTRY_URL }}
        username: ${{ secrets.REGISTRY_USERNAME }}
        password: ${{ secrets.REGISTRY_PASSWORD }}
        
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ secrets.REGISTRY_URL }}/clarusmens-api:${{ github.sha }}
        
    - name: Login to Azure
      uses: azure/login@v2
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
        
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v3
      with:
        app-name: 'clarusmens-api'
        images: ${{ secrets.REGISTRY_URL }}/clarusmens-api:${{ github.sha }}
```

## Step 4: Configure Azure Container Registry

```powershell
# Create Azure Container Registry
az acr create --resource-group ClarusMensRG --name clarusmensregistry --sku Basic

# Enable admin user for the registry
az acr update --name clarusmensregistry --admin-enabled true

# Get the credentials
az acr credential show --name clarusmensregistry
```

Save the username and password as GitHub repository secrets:

- `REGISTRY_URL` = clarusmensregistry.azurecr.io
- `REGISTRY_USERNAME` = (admin username)
- `REGISTRY_PASSWORD` = (admin password)

## Step 5: Create Azure Service Principal for GitHub Actions

```powershell
# Create a service principal and get credentials
az ad sp create-for-rbac --name "ClarusMensGitHubAction" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/ClarusMensRG --sdk-auth
```

Save the JSON output as the `AZURE_CREDENTIALS` secret in your GitHub repository.

## Step 6: Configure Application Settings

```powershell
# Set application settings (example for database connection)
az webapp config appsettings set --resource-group ClarusMensRG --name clarusmens-api --settings CONNECTION_STRING="your-connection-string"
```

## Future Database Integration

When you're ready to add a database:

1. Create an Azure SQL Database:

   ```powershell
   az sql server create --name clarusmens-sql --resource-group ClarusMensRG --location eastus --admin-user adminuser --admin-password "ComplexPassword123!"
   
   az sql db create --resource-group ClarusMensRG --server clarusmens-sql --name ClarusMensDB --service-objective Basic
   ```

2. Configure firewall rules:

   ```powershell
   # Allow Azure services
   az sql server firewall-rule create --resource-group ClarusMensRG --server clarusmens-sql --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
   ```

3. Add the connection string to your Web App:

   ```powershell
   az webapp config connection-string set --resource-group ClarusMensRG --name clarusmens-api --connection-string-type SQLAzure --settings DefaultConnection="Server=tcp:clarusmens-sql.database.windows.net,1433;Database=ClarusMensDB;User ID=adminuser;Password=ComplexPassword123!;Encrypt=true;Connection Timeout=30;"
   ```

## Troubleshooting

- **Container fails to start**: Check the logs with `az webapp log tail --name clarusmens-api --resource-group ClarusMensRG`
- **Database connection issues**: Verify firewall rules and connection strings
- **GitHub Actions failures**: Check the workflow run logs in GitHub

## Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Docker Documentation](https://docs.docker.com/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [.NET Containerization Guide](https://docs.microsoft.com/en-us/dotnet/core/docker/build-container)
