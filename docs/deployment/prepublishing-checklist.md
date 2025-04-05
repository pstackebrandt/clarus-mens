# Pre-Publishing Checklist (Training Project)

> **Focus**: This checklist covers critical preparation steps to complete *before* publishing to Azure or \
> any other hosting platform.
>
> **Note**: After completing this checklist, refer to our [Publishing Checklist](publishing-checklist.md) \
> for manual deployment steps or [Azure Deployment Workflow](azure-deployment-workflow.md) for our \
> automated deployment script.
>
> **Maintenance**: This document follows the checklist maintenance rules maintained by Cursor AI.
> Updates follow the standard status indicators: ✅ (completed), - [ ] (todo), 🔄 (in-progress), \
> ⚠️ (blocked).
>
> **Guidelines for maintaining this document:**
>
> - Review and update at the end of each sprint/start of merge
> - Mark items with ✅ when fully completed
> - Add new requirements as they're identified
> - Keep sections organized by logical grouping
> - Ensure priority items are at the top of their sections

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Code Preparation](#code-preparation)
- [Docker Setup](#docker-setup)
- [Documentation](#documentation)
- [Testing](#testing)
- [Configuration Management](#configuration-management)
- [Security](#security)
- [Monitoring and Health Checks](#monitoring-and-health-checks)
- [Performance](#performance)
- [Version Management](#version-management)
- [Future-Proofing](#future-proofing)
- [Detailed Security Review](#detailed-security-review)
  - [Configuration Files](#configuration-files)
  - [Source Code](#source-code)
  - [Docker Configuration](#docker-configuration)
  - [CI/CD Configuration](#cicd-configuration)
  - [Environment Variables](#environment-variables)
  - [Authentication/Authorization](#authenticationauthorization)

## Code Preparation

Essential steps to prepare your code:

✅ Update project purpose in documentation to reflect training/template focus
✅ Ensure basic API endpoint works (`/api/question`)
✅ Configure proper error handling
✅ Set up health check endpoint
✅ Ensure proper logging is configured
✅ Implement enhanced logging in Program.cs for startup/shutdown
✅ Add diagnostic endpoint for troubleshooting (`/api/diagnostics`)
✅ Implement environment-specific configuration handling

- [ ] Review and remove any hardcoded values or secrets

## Docker Setup

Docker configuration for deployment:

✅ Create multi-stage Dockerfile with build, publish, test and runtime stages
✅ Configure container health checks
✅ Set up proper base images
✅ Add `.dockerignore` file
✅ Implement security best practices (non-root user, minimal dependencies)
✅ Optimize layer caching for faster builds
✅ Fixed Dockerfile linting issues (exec form for healthcheck, layer reduction)

- [ ] Test Docker build locally
- [ ] Test Docker container locally

## Documentation

Essential documentation updates:

✅ Update README with project purpose
✅ Document API endpoints
✅ Add deployment documentation
✅ Create well-structured markdown files
✅ Document environment-specific configuration approaches
✅ Create Azure deployment walkthrough and planning guides
✅ Document lessons learned from deployment process
✅ Create comprehensive version update checklist with validation steps

- [ ] Add deployment URLs once available
- [ ] Add API versioning documentation

## Testing

Verify everything works:

✅ Unit tests passing
✅ Functional tests passing
✅ Test configuration properly separated
✅ Docker test stage available (`--target=test`)
✅ Updated HTTP request file to support multiple environments (local, Docker, Azure)

- [ ] Test Docker container locally:

```bash
docker build -t clarusmens-api .
docker run -p 5000:80 clarusmens-api
```

- [ ] Test endpoints using the HTTP request file:
  1. Open `ClarusMensAPI/api-manual-endpoint-requests.http`
  2. Change the active environment to Docker: `@ClarusMensAPI_HostAddress = {{docker}}`
  3. Send requests to test the following endpoints:
     - [ ] Health check
     - [ ] Main API endpoint
     - [ ] Diagnostics endpoint
     - [ ] Version endpoint
- [ ] Add load testing configuration

## Configuration Management

Configuration setup:

✅ Well-structured configuration files with environment-specific settings
✅ Application Insights integration properly configured
✅ Implementation of layered configuration approach (appsettings → env-specific → secrets/key vault)
✅ Configuration for different environments (development, staging, production)
✅ Implement conditional environment-based configuration in Program.cs

- [ ] Complete implementation of API contact information via environment variables or Azure Key Vault

## Security

Security measures:

✅ HTTPS redirection properly configured for non-development environments
✅ Environment-specific HTTPS redirection implemented
✅ `.dockerignore` properly excludes sensitive files
✅ `.gitignore` properly excludes secrets and environment files
✅ Non-root user configured in container
✅ User Secrets configured for local development

- [ ] Add rate limiting for API endpoints (after first publish possible)
- [ ] Implement CORS policy configuration (after first publish possible)

## Monitoring and Health Checks

Monitoring setup:

✅ Health checks implemented and exposed via endpoint
✅ Application Insights properly configured
✅ Container health check configured in Dockerfile
✅ Health check script implemented with proper permissions
✅ Diagnostic endpoint implemented for runtime troubleshooting
✅ Enhanced startup/shutdown logging for container lifecycle diagnostics

- [ ] Add custom telemetry for business-critical operations (after first publish possible)

## Performance

Performance optimizations:

✅ Invariant globalization enabled for better performance
✅ Port configuration optimized for Azure deployment

- [ ] Implement response compression (after first publish possible)
- [ ] Add caching strategy for appropriate endpoints (after first publish possible)

## Version Management

Version tracking:

✅ Version information properly tracked
✅ Add API versioning strategy (to be extended after first publish)
✅ Added version information to logs at startup
✅ Create version update script (Update-Version.ps1)
✅ Create detailed version update checklist document
✅ Implement version endpoint to verify deployed version
✅ Integration of version information into Docker image labels

🔄 Implement validation feature for configuration settings
🔄 Design version validation process for deployment

- [ ] Add staging environment deployment step

## Future-Proofing

Plan for the future:

✅ Environment-specific configuration handling implemented
✅ Version update process documented for maintainability

- [ ] Prepare database migration strategy
- [ ] Add scalability considerations documentation

## Detailed Security Review

### Configuration Files

✅ Configuration structured to avoid exposing secrets
✅ Environment-specific HTTPS redirection implemented

- [ ] Review remaining items in appsettings.json
  - [ ] Connection strings
  - [ ] API endpoints
  - [ ] Service URLs
  - [ ] Default credentials
- [ ] Review appsettings.Development.json
  - [ ] Development-specific secrets
  - [ ] Test credentials
- [ ] Check launchSettings.json for exposed values
✅ Secrets.json is properly excluded from source control

### Source Code

✅ Removed hard-coded port configurations in Program.cs

- [ ] Check Controllers/Endpoints for hardcoded values
- [ ] Review Service classes for embedded credentials
- [ ] Check initialization code in Program.cs
- [ ] Review middleware configuration for sensitive data
- [ ] Check test files for exposed test credentials

### Docker Configuration

✅ Review Dockerfile for exposed values
✅ Check .dockerignore excludes sensitive files (secrets.json, appsettings.*.json,*.pfx certificates, .env files)
✅ Docker-compose environment variables properly configured

### CI/CD Configuration

- [ ] Review GitHub Actions workflows
  - [ ] All secrets use GitHub Secrets
  - [ ] No hardcoded connection strings
  - [ ] No exposed access tokens
- [ ] Check Azure deployment settings
  - [ ] Connection strings use Azure Key Vault
  - [ ] App settings use proper configuration

### Environment Variables

✅ Environment variables set up with proper prefix (CLARUSMENS_)
✅ Added ASPNETCORE_URLS and WEBSITES_PORT environment variables for Azure
✅ Implemented environment-specific configuration in Program.cs

- [ ] Complete documentation of required environment variables
- [ ] Remove any hardcoded fallback values
- [ ] Verify development values are not used in production

### Authentication/Authorization

- [ ] Check for hardcoded API keys
- [ ] Review JWT configuration
- [ ] Check OAuth client secrets
- [ ] Verify certificate paths are configurable

[↑ Back to Code Preparation](#code-preparation)
