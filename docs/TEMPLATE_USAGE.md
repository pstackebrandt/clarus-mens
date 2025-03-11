# Using ClarusMens as a Template

This document provides guidance on how to use the ClarusMens API project as a template for creating new .NET API projects. The template includes best practices, reusable patterns, and solutions to common issues that will accelerate your development process.

## Why Use This Template?

ClarusMens provides:

- **Production-ready architecture** following modern .NET API best practices
- **Comprehensive test infrastructure** with unit, integration, and functional tests
- **Documentation patterns** that ensure maintainability
- **Solutions for common .NET 9 issues** (such as serialization workarounds)
- **CI/CD pipeline integration** ready for enterprise development
- **Clean separation of concerns** and modular design

## Step-by-Step Guide to Creating a New Project

### 1. Clone the Template Repository

```powershell
# Clone the repository
git clone https://github.com/pstackebrandt/clarus-mens.git new-project-name

# Remove the Git history to start fresh
cd new-project-name
rm -r -fo .git
git init
```

### 2. Rename the Solution and Projects

```powershell
# Rename files
Move-Item clarus-mens.sln new-project-name.sln
Move-Item ClarusMensAPI NewProjectAPI
Move-Item tests/ClarusMensAPI.UnitTests tests/NewProjectAPI.UnitTests
Move-Item tests/ClarusMensAPI.IntegrationTests tests/NewProjectAPI.IntegrationTests
Move-Item tests/ClarusMensAPI.FunctionalTests tests/NewProjectAPI.FunctionalTests
```

### 3. Update Project References and Namespaces

1. Open the solution in Visual Studio/Visual Studio Code/Cursor
2. Update project references in the .csproj files
3. Perform a solution-wide rename of:
   - "ClarusMens" → "NewProject"
   - "clarus-mens" → "new-project-name"
4. Update namespaces throughout the codebase

### 4. Update API Information

1. Update API information in `Program.cs`:
   - API name
   - Version
   - License information
   - Repository URL

2. Update configuration in `appsettings.json` files

### 5. Customize for Your Specific Requirements

1. Remove or replace the existing endpoints
2. Update models and services
3. Add your business logic while maintaining the architectural patterns

## What to Keep vs. Customize

### Keep (with minor customization)

- **Project Structure**: The layered architecture works for most API projects
- **Test Setup**: The test projects and base classes provide a solid foundation
- **Documentation Structure**: README, TROUBLESHOOTING, configuration documents
- **Helper Extensions**: Utilities like `JsonSafeOk` solve common problems
- **CI/CD Pipeline Configuration**: The workflow patterns are broadly applicable

### Customize

- **Endpoints and Models**: These are specific to your application domain
- **Business Logic and Services**: Implement your own application requirements
- **Database Integration**: Add your own database context and entities
- **Authentication**: Configure to match your organization's requirements
- **Logging and Monitoring**: Adjust to fit your operational needs

## Adapting the Testing Infrastructure

The testing infrastructure is designed to be reusable across projects:

1. Keep the test project structure and base classes
2. Replace test implementations with ones specific to your domain
3. Maintain the naming conventions and patterns
4. Update any hardcoded values in test configurations
5. Add new test categories as needed for your domain

## Documentation Guidelines

When adapting documentation:

1. Maintain the same level of detail and clarity
2. Update project-specific information
3. Keep the troubleshooting guide and expand it with issues you encounter
4. Update the examples in code snippets to match your domain
5. Maintain the README sections that describe how to run, test, and deploy

## Common Customization Scenarios

### Adding Entity Framework Core

The template doesn't include a database by default. To add Entity Framework Core:

```powershell
dotnet add NewProjectAPI package Microsoft.EntityFrameworkCore.SqlServer
dotnet add NewProjectAPI package Microsoft.EntityFrameworkCore.Design
```

Add a DbContext and register it in Program.cs:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Adding Authentication

To add JWT Bearer authentication:

```powershell
dotnet add NewProjectAPI package Microsoft.AspNetCore.Authentication.JwtBearer
```

Register the authentication services:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Configure per your requirements
        };
    });
```

### Adding Health Checks

The template includes basic health checks, but you can extend them:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck("ExternalService", () => {
        // Check external service availability
        return HealthCheckResult.Healthy();
    });
```

## Troubleshooting Template Usage

### Issue: Namespace conflicts after renaming

If you encounter compiler errors after renaming:

1. Clean the solution: `dotnet clean`
2. Delete bin and obj folders
3. Restore packages: `dotnet restore`
4. Rebuild: `dotnet build`

### Issue: Tests failing after customization

Check:
1. Whether test configuration still matches your updated API
2. If test dependencies are properly registered
3. If any hardcoded values in tests need updating

## Template Versioning

This template is based on:
- .NET 9.0
- ASP.NET Core 9.0
- MSTest 3.x

When .NET or its packages receive major updates, consider updating the template to benefit from new features and patterns. 