# Azure Container Startup Troubleshooting Guide

This guide outlines steps to diagnose and fix issues with Docker containers that stop after starting in Azure App Service.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Issue Description](#issue-description)
- [Diagnostic Steps](#diagnostic-steps)
  - [1. Check Container Logs](#1-check-container-logs)
  - [2. Validate App Service Configuration](#2-validate-app-service-configuration)
  - [3. Test Container Health Locally](#3-test-container-health-locally)
  - [4. Verify Dockerfile Configuration](#4-verify-dockerfile-configuration)
- [Common Issues and Solutions](#common-issues-and-solutions)
  - [Health Check Failures](#health-check-failures)
  - [Port Configuration Mismatches](#port-configuration-mismatches)
  - [Environment Variable Issues](#environment-variable-issues)
  - [Application Startup Timeout](#application-startup-timeout)
- [Fix Implementation Checklist](#fix-implementation-checklist)
- [Testing the Fix](#testing-the-fix)
- [Preventative Measures](#preventative-measures)

## Issue Description

The Docker container in Azure App Service starts but then stops shortly after. This can be caused by various issues:

1. Failed health checks
2. Port configuration mismatches
3. Environment variable issues
4. Application startup timeouts
5. Dependency issues in the application

## Diagnostic Steps

### 1. Check Container Logs

Azure App Service logs contain crucial information about container startup issues:

```powershell
# Stream logs in real-time
az webapp log tail --name clarus-mens-app --resource-group clarus-mens-rg
```

Look for:

- Error messages during startup
- Health check failures
- Application exceptions
- Network or port binding errors

### 2. Validate App Service Configuration

Check if the Azure configuration matches what the container expects:

```powershell
# Check app settings
az webapp config appsettings list --name clarus-mens-app --resource-group clarus-mens-rg

# Check container configuration
az webapp config container show --name clarus-mens-app --resource-group clarus-mens-rg
```

Verify these critical settings:

- `WEBSITES_PORT`: Should match the exposed port in the Dockerfile (80)
- `ASPNETCORE_URLS`: Should be http://+:80
- `ASPNETCORE_ENVIRONMENT`: Should be Production

### 3. Test Container Health Locally

Verify container functionality in a local environment:

```powershell
# Build and run locally
docker build -t clarus-mens-local .
docker run -p 8080:80 clarus-mens-local

# Test health endpoint
Invoke-WebRequest -Uri http://localhost:8080/health
```

Alternatively, use the Test-DockerImage.ps1 script:

```powershell
.\scripts\Test-DockerImage.ps1 -TestType HealthCheck
```

### 4. Verify Dockerfile Configuration

Review the Dockerfile:

- Check the health check configuration (interval, timeout, retries)
- Verify port exposure
- Check startup command
- Review environment variables

## Common Issues and Solutions

### Health Check Failures

If health checks are failing:

1. **Adjust Health Check Configuration** - Modify in Dockerfile:

   ```dockerfile
   # Increase timeout and interval
   HEALTHCHECK --interval=30s --timeout=10s --retries=5 \
       CMD ["/usr/local/bin/healthcheck.sh"]
   ```

2. **Simplify Health Check** - Use a more basic health check in Dockerfile:

   ```dockerfile
   HEALTHCHECK --interval=30s --timeout=10s --retries=5 \
       CMD curl -f http://localhost:80/health || exit 1
   ```

3. **Disable Health Check in Azure** - As a temporary measure:

   ```powershell
   az webapp config appsettings set --name clarus-mens-app --resource-group clarus-mens-rg --settings WEBSITES_DISABLE_CONTAINER_HEALTHCHECK=true
   ```

### Port Configuration Mismatches

If ports are misconfigured:

1. **Ensure Port 80 Exposure** - Update Dockerfile:

   ```dockerfile
   EXPOSE 80
   ```

2. **Configure Azure Port Setting**:

   ```powershell
   az webapp config appsettings set --name clarus-mens-app --resource-group clarus-mens-rg --settings WEBSITES_PORT=80
   ```

3. **Update ASPNETCORE_URLS**:

   ```powershell
   az webapp config appsettings set --name clarus-mens-app --resource-group clarus-mens-rg --settings ASPNETCORE_URLS=http://+:80
   ```

### Environment Variable Issues

If there are environment variable problems:

1. **Verify Production Configuration** - Check Program.cs for environment-specific configs
2. **Check for Required Variables** - Ensure all required env vars are set in Azure
3. **Inspect Default Values** - Verify default values are appropriate

### Application Startup Timeout

If the application takes too long to start:

1. **Optimize Application Startup Time** - Review Program.cs for slow startup processes
2. **Increase Azure Timeout** - Increase startup timeout in app settings:

   ```powershell
   az webapp config appsettings set --name clarus-mens-app --resource-group clarus-mens-rg --settings WEBSITE_CONTAINER_START_TIME_LIMIT=600
   ```

## Fix Implementation Checklist

✅ Check container logs for specific errors  

- [ ] Validate all Azure environment settings  
- [ ] Test container locally with the Test-DockerImage.ps1 script  
- [ ] Review Dockerfile health check configuration  
- [ ] Check for port configuration mismatches  
- [ ] Verify environment variables are correctly set  
- [ ] Test fix locally before redeploying  
- [ ] Update documentation with the solution  

## Testing the Fix

After implementing fixes:

1. Deploy the updated container to Azure
2. Monitor logs during startup
3. Test all API endpoints:
   - Health: <https://clarus-mens-app.azurewebsites.net/health>
   - API version: <https://clarus-mens-app.azurewebsites.net/api/version>
   - Diagnostics: <https://clarus-mens-app.azurewebsites.net/api/diagnostics>
4. Use Application Insights to monitor for exceptions

## Preventative Measures

To prevent similar issues in the future:

1. **Enhanced Local Testing**:
   - Always test container health locally
   - Use the Test-DockerImage.ps1 script before deployment

2. **Deployment Validation**:
   - Add post-deployment health checks to CI/CD pipeline
   - Automate endpoint testing after deployment

3. **Documentation**:
   - Document Azure-specific configuration requirements
   - Maintain troubleshooting steps for quick reference
