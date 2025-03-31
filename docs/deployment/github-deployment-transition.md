# Transitioning to GitHub-Based Azure Deployment

> This document outlines the process of transitioning from direct Azure CLI deployment to
> automated GitHub Actions deployment for the Clarus Mens API project.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Benefits and Use Cases](#benefits-and-use-cases)
- [Prerequisites](#prerequisites)
- [Architecture Changes](#architecture-changes)
- [Implementation Steps](#implementation-steps)
- [Required Azure Resources](#required-azure-resources)
- [GitHub Repository Configuration](#github-repository-configuration)
- [Security Considerations](#security-considerations)
- [Maintenance and Operations](#maintenance-and-operations)
- [Workflow Example](#workflow-example)

## Benefits and Use Cases

GitHub-based deployment provides several advantages over direct deployment:

- **Automation**: Eliminates manual deployment steps
- **Consistency**: Same process every time, reducing human error
- **Auditability**: Complete history of all deployments
- **Collaboration**: Built-in approvals and reviews
- **Multi-environment**: Simplified promotion between environments

This approach is most beneficial when:

- Multiple developers contribute to the codebase
- Frequent deployments are needed
- Multiple environments must be maintained
- Regulatory requirements demand process controls

## Prerequisites

Before transitioning to GitHub-based deployment:

- Active GitHub repository with your code
- GitHub account with appropriate permissions
- Azure subscription
- Existing Azure resources (already configured)
- Basic familiarity with YAML syntax

## Architecture Changes

Transitioning to GitHub-based deployment introduces these architectural changes:

1. **Source of Truth**: Deployment configuration moves from CLI commands to workflow files
2. **Trigger Mechanism**: Deployments triggered by GitHub events instead of manual commands
3. **Authentication Flow**: Service Principal authentication replaces interactive login
4. **Execution Environment**: Commands run in GitHub's hosted runners instead of local machine

The deployed resources themselves remain identical.

## Implementation Steps

The high-level process for transitioning includes:

1. Create a GitHub Actions workflow file
2. Set up Azure service principal for CI/CD
3. Configure GitHub repository secrets
4. Test the initial workflow
5. Extend the workflow with additional environments if needed

The core Azure resources and application configuration remain unchanged.

## Required Azure Resources

Transitioning requires creating one new Azure resource:

- **Service Principal**: An identity for GitHub Actions to use when interacting with Azure

All other Azure resources (ACR, App Service, etc.) remain the same as in direct deployment.

## GitHub Repository Configuration

New GitHub repository elements needed:

1. **Workflow File**: `.github/workflows/azure-deploy.yml`
2. **Repository Secrets**:
   - `AZURE_CREDENTIALS`: Service principal credentials JSON
   - `REGISTRY_URL`: ACR URL (clarusmenscr.azurecr.io)
   - `REGISTRY_USERNAME`: ACR admin username
   - `REGISTRY_PASSWORD`: ACR admin password

## Security Considerations

When transitioning to GitHub-based deployment:

- Service principal should use the principle of least privilege
- Credentials should never appear in workflow files directly
- Consider adding approval workflows for production deployments
- Regularly rotate service principal credentials
- Audit GitHub repository access permissions

## Maintenance and Operations

After transitioning, these operational changes apply:

- Monitor workflow runs in GitHub Actions dashboard
- Update workflow files when deployment needs change
- Manage secrets when credentials expire
- Consider implementing environment-specific configurations
- Set up branch protection rules for production deployments

The direct deployment commands documented in the deployment walkthrough become reference
material but are no longer needed for routine operations.

## Workflow Example

Below is a simple GitHub Actions workflow file that implements the deployment process
using the resources and configuration mentioned above:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:  # Allows manual triggering

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
        tags: ${{ secrets.REGISTRY_URL }}/clarus-mens:${{ github.sha }}
        
    - name: Login to Azure
      uses: azure/login@v2
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
        
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v3
      with:
        app-name: 'clarus-mens-app'
        images: ${{ secrets.REGISTRY_URL }}/clarus-mens:${{ github.sha }}
        
    - name: Azure logout
      run: az logout
      if: always()
```

This workflow will:

1. Trigger on pushes to main, pull requests to main, or manual activation
2. Check out your repository code
3. Set up Docker Buildx for multi-platform builds
4. Log in to your Azure Container Registry
5. Build and push your Docker image with a SHA-based tag
6. Log in to Azure using the service principal credentials
7. Deploy the newly built image to your App Service
8. Log out of Azure when complete
