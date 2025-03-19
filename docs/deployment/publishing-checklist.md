# Publishing Checklist (Training Project)

> **Focus**: Simple steps to publish the training/template API project using GitHub and Azure.
> This checklist focuses on the essential deployment steps for demonstration purposes.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Prerequisites](#prerequisites)
- [Local Testing](#local-testing)
- [GitHub Setup](#github-setup)
- [Azure Setup](#azure-setup)
- [Deployment](#deployment)
- [Verification](#verification)

## Prerequisites

Before starting deployment:

- [ ] Complete all items in [Pre-Publishing Checklist](prepublishing-checklist.md)
- [ ] Install Docker Desktop
- [ ] Create a GitHub account (if not already done)
- [ ] Create an Azure account (free tier is sufficient)

## Local Testing

Verify everything works locally:

- [ ] Build Docker image:

  ```bash
  docker build -t clarusmens-api .
  ```

- [ ] Run container:

  ```bash
  docker run -p 5000:80 clarusmens-api
  ```

- [ ] Test endpoints:
  - [ ] <http://localhost:5000/health>
  - [ ] <http://localhost:5000/swagger>
  - [ ] Test main API endpoint

## GitHub Setup

1. [ ] Push your code to GitHub:

   ```bash
   git push origin main
   ```

2. [ ] Create `.github/workflows` directory:

   ```bash
   mkdir -p .github/workflows
   ```

3. [ ] Create deployment workflow file (will be provided in next step)

## Azure Setup

1. [ ] Sign up for Azure (free tier):
   - Go to <https://azure.microsoft.com/free>
   - Create account with Microsoft credentials
   - No credit card required for free tier

2. [ ] Install Azure CLI:
   - Download from: <https://docs.microsoft.com/cli/azure/install-azure-cli>
   - Verify installation: `az --version`

3. [ ] Login to Azure:

   ```bash
   az login
   ```

## Deployment

1. [ ] Create Azure Web App (through portal):
   - Choose "Create a resource"
   - Select "Web App"
   - Choose Free tier (F1)
   - Enable Docker

2. [ ] Configure GitHub Actions:
   - In your GitHub repository:
     - Go to "Settings" > "Secrets"
     - Add Azure deployment credentials (will be provided)
   - Add workflow file (template will be provided)

## Verification

After deployment:

- [ ] Check deployment status in GitHub Actions
- [ ] Verify app is running:
  - [ ] Health endpoint
  - [ ] Swagger UI
  - [ ] Test main API endpoint
- [ ] Update documentation with production URLs

Note: Detailed Azure setup and GitHub Actions workflow files will be provided in separate guides.
