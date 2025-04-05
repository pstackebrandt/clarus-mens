# Azure Training Guide

> This document recommends specific Microsoft Azure training resources that are most relevant
> for deploying the Clarus Mens API project to Azure App Service using Docker containers.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Recommended Training Resources](#recommended-training-resources)
  - [Essential Training (2-3 hours)](#essential-training-2-3-hours)
  - [Quick Start Option (30-45 minutes)](#quick-start-option-30-45-minutes)
- [Why These Resources](#why-these-resources)
- [Additional Learning Paths](#additional-learning-paths)

## Recommended Training Resources

### Essential Training (2-3 hours)

**"Deploy and manage containers with Azure App Service"**  
[Deploy and manage containers][deploy-manage-containers]

This learning path covers:

- Deploying containerized applications to Azure App Service
- Configuring continuous deployment from GitHub and container registries
- Managing application settings and configuration
- Scaling and monitoring container deployments
- Implementing staging environments with deployment slots

### Quick Start Option (30-45 minutes)

If you need just the essentials to get started quickly:

**"Deploy a container instance in Azure"**  
[Deploy container instance][deploy-container]

This focused module covers:

- Basic container deployment to App Service
- Command-line deployment using Azure CLI
- Essential configuration options

## Why These Resources

These specific training resources were selected because they:

1. **Directly align with our deployment approach** using Docker containers and Azure App Service
2. **Focus on practical implementation** rather than theoretical concepts
3. **Include hands-on exercises** that mirror our actual deployment steps
4. **Are officially maintained by Microsoft** and kept up-to-date
5. **Can be completed in a short timeframe** (under 3 hours)

The deployment process in these trainings closely matches our
[Azure Deployment Guide](../deployment/azure-deployment-guide.md), making them
ideal preparation before performing the actual deployment.

## Additional Learning Paths

For team members who want to deepen their Azure knowledge further:

- **"Implement containerized solutions"** (6-8 hours)  
  [Implement containerized solutions][containerized-solutions]

- **"Implement continuous integration and continuous delivery"** (4-5 hours)  
  [Implement CI/CD][implement-ci-cd]

- **"AZ-204: Developing Solutions for Microsoft Azure"** (Certification preparation)  
  [Create Azure App Service web apps][create-web-apps]

These additional resources go beyond the immediate needs of our project but provide valuable
knowledge for future Azure-based development.

[deploy-manage-containers]: https://learn.microsoft.com/en-us/training/paths/deploy-manage-containers-app-service/
[deploy-container]: https://learn.microsoft.com/en-us/training/modules/deploy-run-container-app-service/
[containerized-solutions]: https://learn.microsoft.com/en-us/training/paths/implement-containerized-solutions/
[implement-ci-cd]: https://learn.microsoft.com/en-us/training/paths/implement-ci-cd-azure-devops/
[create-web-apps]: https://learn.microsoft.com/en-us/training/paths/create-azure-app-service-web-apps/
