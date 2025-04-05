# Deployment Documentation

This directory contains comprehensive documentation for deploying the Clarus Mens API to Azure App Service.

## Getting Started

For deploying the application, we recommend the following workflow:

1. Complete the [Pre-Publishing Checklist](prepublishing-checklist.md) to prepare your application
2. Follow the [Azure Deployment Workflow](azure-deployment-workflow.md) to deploy using our automated script

## Document Categories

### Core Deployment Documents

| Document                                                  | Description                                                           |
| --------------------------------------------------------- | --------------------------------------------------------------------- |
| [Azure Deployment Workflow](azure-deployment-workflow.md) | **Current recommended process** using our automated deployment script |
| [Pre-Publishing Checklist](prepublishing-checklist.md)    | Critical preparation steps before deployment                          |
| [Publishing Checklist](publishing-checklist.md)           | Manual deployment steps (for reference)                               |
| [Version Update Checklist](version-update-checklist.md)   | Complete process for version updates and deployment validation        |

### Reference Documentation

| Document                                                                                    | Description                                                         |
| ------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| [Azure Configuration Reference](azure-deployment-workflow.md#azure-configuration-reference) | Comprehensive list of environment variables and Azure configuration |

### Background & Context

| Document                                                              | Description                                        |
| --------------------------------------------------------------------- | -------------------------------------------------- |
| [Azure Deployment Walkthrough](azure-deployment-walkthrough.md)       | Historical deployment process with lessons learned |
| [Azure Deployment Planning Guide](azure-deployment-planning-guide.md) | Theoretical background on Azure deployment         |
| [Docker Build Documentation](docker-build-documentation.md)           | Details about Docker image building process        |

### Alternative Approaches

| Document                                                        | Description                                                    |
| --------------------------------------------------------------- | -------------------------------------------------------------- |
| [GitHub Deployment Transition](github-deployment-transition.md) | Guide for transitioning to GitHub-based deployment             |
| [ASP.NET Hosting Options](asp-net-hosting-options.md)           | Overview of different hosting options for ASP.NET applications |

## Deployment Process Evolution

Our deployment process has evolved through several phases:

1. **Initial Manual Deployment** - Documented in [Azure Deployment Walkthrough](azure-deployment-walkthrough.md)
2. **Scripted Deployment** - Current approach using [Deploy-ToAzure.ps1](azure-deployment-workflow.md)
3. **GitHub-based Deployment** - Future direction outlined in [GitHub Deployment Transition](github-deployment-transition.md)

## Future Enhancements

See the `future-reference` directory for planned improvements to our deployment process.
