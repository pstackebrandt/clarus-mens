# API Versioning Strategy

## Overview

This document outlines the API versioning strategy that will be implemented after the initial publication
of the Clarus Mens API. While the initial release includes basic version tracking and display functionality,
a more comprehensive API versioning approach will enable us to evolve the API over time while maintaining
backward compatibility.

## Table of Contents

- [Overview](#overview)
- [Table of Contents](#table-of-contents)
- [Current State](#current-state)
- [Proposed Implementation](#proposed-implementation)
- [Implementation Steps](#implementation-steps)
  - [1. Install Required Packages](#1-install-required-packages)
  - [2. Configure API Versioning Services](#2-configure-api-versioning-services)
  - [3. Register API Versioning in Program.cs](#3-register-api-versioning-in-programcs)
  - [4. Update Endpoints with Version Information](#4-update-endpoints-with-version-information)
  - [5. Configure Swagger to Support API Versions](#5-configure-swagger-to-support-api-versions)
  - [6. Update Swagger UI Configuration](#6-update-swagger-ui-configuration)
- [API Versioning Policy](#api-versioning-policy)
  - [URL Path Versioning](#url-path-versioning)
  - [Supporting Multiple Versions](#supporting-multiple-versions)
  - [Version Deprecation](#version-deprecation)
  - [Versioning Decision Guidelines](#versioning-decision-guidelines)
- [Client Migration Strategy](#client-migration-strategy)
- [Testing Strategy](#testing-strategy)
- [Future Considerations](#future-considerations)
- [Implementation Timeline](#implementation-timeline)
  - [Traditional Development (Without AI Assistance)](#traditional-development-without-ai-assistance)
  - [AI-Assisted Development (With Cursor/AI)](#ai-assisted-development-with-cursorai)

## Current State

The current implementation includes:

- Version display in API responses via `VersionService`
- Version information endpoint at `/api/version`
- Swagger/OpenAPI documentation with version information
- SemVer 2.0.0 compliant versioning

However, it lacks a formal mechanism for managing multiple API versions simultaneously, which is critical
for evolving the API without breaking existing clients.

## Proposed Implementation

After the initial publication, we will implement a comprehensive API versioning strategy using
the Microsoft.AspNetCore.Mvc.Versioning package. This approach will allow:

1. Multiple versions of API endpoints to coexist
2. Clear versioning in API URLs, headers, or query parameters
3. Proper versioning documentation in Swagger/OpenAPI

## Implementation Steps

### 1. Install Required Packages

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
```

### 2. Configure API Versioning Services

Add the following to the `ServiceCollectionExtensions.cs` file:

```csharp
/// <summary>
/// Configures API versioning services
/// </summary>
public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
{
    // Add API versioning services
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        // Configure readers for URL path segments, headers, and query strings
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version"));
    });

    // Add versioned API explorer for Swagger/OpenAPI
    services.AddVersionedApiExplorer(options =>
    {
        // Format like 'v1'
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    
    return services;
}
```

### 3. Register API Versioning in Program.cs

Update the `Program.cs` file to register the API versioning services:

```csharp
// Service Registration
builder.Services.AddApplicationServices();
builder.Services.AddApiVersioningServices(); // Add this line
builder.Services.AddOpenApiServices(ApiContractVersion);
builder.Services.AddHttpsRedirection(7043);
builder.Services.AddHealthChecks();
builder.Services.AddApplicationInsightsTelemetry();
```

### 4. Update Endpoints with Version Information

Modify the endpoint registration in each endpoint class to include version information:

```csharp
app.MapGet("/api/v{version:apiVersion}/question", async (string query, IQuestionService questionService) =>
{
    // Existing implementation
})
.WithName("GetAnswerV1")
.HasApiVersion(new ApiVersion(1, 0));
```

### 5. Configure Swagger to Support API Versions

Create a new `SwaggerVersioningSetup.cs` class that properly configures Swagger to display multiple
API versions:

```csharp
/// <summary>
/// Configures Swagger/OpenAPI documentation to support API versioning
/// </summary>
public class SwaggerVersioningSetup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly VersionService _versionService;
    private readonly IConfiguration _config;

    public SwaggerVersioningSetup(
        IApiVersionDescriptionProvider provider,
        VersionService versionService,
        IConfiguration config)
    {
        _provider = provider;
        _versionService = versionService;
        _config = config;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var apiName = _config["ApiInfo:Name"] ?? "Clarus Mens API";
        var apiDescription = _config["ApiInfo:Description"] ?? "API for Clarus Mens question answering service";
        
        // Add a swagger document for each API version
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"{apiName} {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Description = apiDescription,
                    Contact = new OpenApiContact
                    {
                        Name = _config["ApiInfo:Contact:Name"],
                        Email = _config["ApiInfo:Contact:Email"]
                    },
                    License = new OpenApiLicense
                    {
                        Name = _config["ApiInfo:License:Name"] ?? "Apache License 2.0",
                        Url = !string.IsNullOrEmpty(_config["ApiInfo:License:Url"]) 
                            ? new Uri(_config["ApiInfo:License:Url"]!) 
                            : new Uri("https://www.apache.org/licenses/LICENSE-2.0")
                    }
                });
        }
    }
}
```

### 6. Update Swagger UI Configuration

Update the Swagger UI configuration in `Program.cs`:

```csharp
// OpenAPI/Swagger Configuration
app.MapOpenApi(); 
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Build a swagger endpoint for each API version
    foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>()
        .ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"Clarus Mens API {description.GroupName}");
    }
    options.RoutePrefix = "swagger";
});
```

## API Versioning Policy

### URL Path Versioning

The primary versioning method will be URL path versioning:

- `/api/v1/question` for version 1
- `/api/v2/question` for version 2

This approach is preferred because:

- It's the most visible and client-friendly approach
- It works well with API gateways and caching
- It allows easy testing in browsers and tools

### Supporting Multiple Versions

We will support at least two versions of the API concurrently:

- The current version
- The previous version

This ensures clients have time to migrate to newer versions.

### Version Deprecation

When deprecating an API version:

1. Mark it as deprecated in Swagger documentation
2. Set a sunset date at least 6 months in the future
3. Return deprecation notices in API responses
4. Provide clear migration paths to newer versions

### Versioning Decision Guidelines

1. **Major Version Change (v1 → v2)**: Required for breaking changes:
   - Removing or renaming endpoints
   - Removing required request parameters
   - Changing response structure in incompatible ways

2. **Minor Version Change (v1.1 → v1.2)**: For non-breaking additions:
   - Adding new optional parameters
   - Adding new properties to responses
   - Adding new endpoints

## Client Migration Strategy

To facilitate client migration between API versions:

1. Provide clear documentation of changes between versions
2. Include version information in error responses
3. Maintain migration guides in API documentation
4. Monitor usage of deprecated API versions

## Testing Strategy

For each API version, we will:

1. Maintain separate integration test suites
2. Test backward compatibility explicitly
3. Include version-specific test cases
4. Test migration paths between versions

## Future Considerations

1. **API Gateway Integration**: Consider how API versioning integrates with API gateways
2. **Health Checks By Version**: Provide version-specific health checks
3. **Cross-Version Feature Flags**: Consider flags to enable/disable features across versions

## Implementation Timeline

The API versioning strategy will be implemented after the first publication, before introducing any
breaking changes to the API. Below are two separate estimations based on development approach:

### Traditional Development (Without AI Assistance)

Estimated timeline with traditional development approaches:

1. Package installation and basic setup: 1 day
2. Endpoint versioning implementation: 2 days
3. Swagger/OpenAPI integration: 1 day
4. Testing and documentation: 2 days

**Total estimated effort:** 6 working days (1.2 weeks)

### AI-Assisted Development (With Cursor/AI)

Estimated timeline with Cursor/AI assistance:

1. Package installation and basic setup: 0.5 day
2. Endpoint versioning implementation: 1 day
3. Swagger/OpenAPI integration: 0.5 day
4. Testing and documentation: 1.5 days

**Total estimated effort:** 3.5 working days (0.7 weeks)

The AI-assisted approach can significantly reduce development time, particularly for code generation
and documentation tasks. However, testing requirements remain similar as verification of functionality
still requires human validation across different scenarios.

Peter: I'm sure this isn't correct. I think the AI-assisted approach will be more like 4 - 7 hours for my training
project (including learning and documentation). Strange new dev world!
