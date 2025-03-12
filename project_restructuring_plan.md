# ClarusMensAPI Code Structure Refactoring Plan

## Overview

This document outlines the plan to refactor the ClarusMensAPI from a single Program.cs file to a structured, maintainable project organization. The refactoring will be implemented incrementally to ensure functionality is preserved at each step.

## Target Structure

```plaintext
ClarusMensAPI/
├── Controllers/              # For any controller-based endpoints if added later
├── Configuration/            # Configuration-related classes
│   ├── OpenApiConfiguration.cs
│   └── SwaggerVersionSetup.cs
├── Extensions/               # Extension methods
│   ├── ResultsExtensions.cs  # Your JsonSafeOk extension methods
│   └── ServiceCollectionExtensions.cs
├── Middleware/               # Custom middleware components
├── Models/                   # Data models
│   ├── Requests/             # Input models
│   └── Responses/            # Response models like RootResponse
│       ├── RootResponse.cs
│       └── QuestionAnswerResponse.cs
├── Services/                 # Service implementations
│   ├── Interfaces/           # Service interfaces
│   │   └── IQuestionService.cs
│   ├── VersionService.cs  
│   └── SimpleQuestionService.cs
├── Endpoints/                # Grouped endpoint definitions
│   ├── QuestionEndpoints.cs
│   └── VersionEndpoints.cs
├── Validators/               # Input validation
├── Program.cs                # Main entry point (leaner version)
├── appsettings.json          # Configuration
└── appsettings.Development.json
```

## Implementation Plan

### Phase 1: Create Base Structure and Extract Models

1. Create the folder structure
2. Extract response models to dedicated files
3. Verify application still works

### Phase 2: Extract Services

1. Move service interfaces to Services/Interfaces
2. Move service implementations to Services
3. Verify application still works

### Phase 3: Extract Configuration and Extensions

1. Move SwaggerVersionSetup to Configuration folder
2. Create Extension methods for results handling
3. Create ServiceCollectionExtensions for registration
4. Verify application still works

### Phase 4: Implement Endpoint Organization

1. Create endpoint extension methods in the Endpoints folder
2. Update Program.cs to use the extension methods
3. Verify application still works

### Phase 5: Clean Up and Documentation

1. Clean up Program.cs to its final form
2. Add XML documentation to all public APIs
3. Update README with structure explanation

## Detailed Implementation Guide

### Phase 1 Implementation: Create Base Structure and Extract Models

#### Step 1.1: Create folder structure

```powershell
# Create folders
New-Item -Path "ClarusMensAPI/Controllers" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Configuration" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Extensions" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Middleware" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Models/Requests" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Models/Responses" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Services/Interfaces" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Endpoints" -ItemType Directory -Force
New-Item -Path "ClarusMensAPI/Validators" -ItemType Directory -Force
```

#### Step 1.2: Extract response models

Create `Models/Responses/RootResponse.cs`:

```csharp
using System.Text.Json.Serialization;

namespace ClarusMensAPI.Models.Responses;

public class RootResponse
{
    public string Status { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public LicenseInfo License { get; set; } = new LicenseInfo();
    public LinksInfo Links { get; set; } = new LinksInfo();
}

public class LicenseInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class LinksInfo
{
    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
    
    [JsonPropertyName("openapi_spec")]
    public string OpenApiSpec { get; set; } = string.Empty;
    
    [JsonPropertyName("health")]
    public string Health { get; set; } = string.Empty;
    
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}
```

Create `Models/Responses/QuestionAnswerResponse.cs`:

```csharp
namespace ClarusMensAPI.Models.Responses;

public record QuestionAnswerResponse
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}
```

### Phase 2 Implementation: Extract Services

#### Step 2.1: Extract service interfaces

Create `Services/Interfaces/IQuestionService.cs`:

```csharp
namespace ClarusMensAPI.Services.Interfaces;

public interface IQuestionService
{
    Task<string> GetAnswerAsync(string question);
}
```

#### Step 2.2: Extract service implementations

Create `Services/SimpleQuestionService.cs`:

```csharp
using ClarusMensAPI.Services.Interfaces;

namespace ClarusMensAPI.Services;

public class SimpleQuestionService : IQuestionService
{
    // For the MVP, this could be a simple implementation
    // Later, you can replace this with actual AI integration
    public Task<string> GetAnswerAsync(string question)
    {
        // Simple mapping of questions to answers
        // In a real implementation, this would call an AI service
        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "hello", "Hello there! How can I help you?" },
            { "what is your name", "I am Clarus Mens, an AI assistant." },
            { "what time is it", "I don't have real-time capabilities, but you can check your device's clock." },
            { "how does this work", "You ask a question, and I provide an answer using my AI capabilities." }
        };

        // Check if we have a direct match
        foreach (var key in answers.Keys)
        {
            if (question.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(answers[key]);
            }
        }

        // Default response
        return Task.FromResult("I don't have an answer for that question yet. As we grow, I'll learn to answer more questions.");
    }
}
```

### Phase 3 Implementation: Extract Configuration and Extensions

#### Step 3.1: Extract Swagger configuration

Create `Configuration/SwaggerVersionSetup.cs`:

```csharp
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.Configuration;

// Configuration pattern for Swagger documentation at application startup
public class SwaggerVersionSetup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly VersionService _versionService;
    private readonly string _apiVersion;
    private readonly IConfiguration _config;

    public SwaggerVersionSetup(VersionService versionService, IConfiguration config)
    {
        _versionService = versionService;
        _config = config;
        _apiVersion = config["ApiVersion"] ?? "v0"; // Get from config or use default
    }

    public void Configure(SwaggerGenOptions options)
    {
        var apiName = _config["ApiInfo:Name"] ?? "Clarus Mens API";
        var apiDescription = _config["ApiInfo:Description"] ?? "API for Clarus Mens question answering service";
        
        options.SwaggerDoc(_apiVersion, new OpenApiInfo
        {
            Title = apiName,
            Version = _versionService.GetDisplayVersion(),
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
            },
            TermsOfService = !string.IsNullOrEmpty(_config["ApiInfo:TermsOfService"]) 
                ? new Uri(_config["ApiInfo:TermsOfService"]!) 
                : null
        });
    }
}
```

#### Step 3.2: Create results extensions

Create `Extensions/ResultsExtensions.cs`:

```csharp
using System.Text.Json;

namespace ClarusMensAPI.Extensions;

/// <summary>
/// Extension methods for safely serializing JSON responses in .NET 9
/// to work around the PipeWriter.UnflushedBytes issue.
/// </summary>
public static class ResultsExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    /// <summary>
    /// Safely serializes an object to JSON and returns it with a 200 OK status
    /// </summary>
    public static IResult JsonSafeOk<T>(this T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return Results.Text(json, "application/json", System.Net.HttpStatusCode.OK);
    }
    
    /// <summary>
    /// Safely serializes an object to JSON and returns it with the specified status code
    /// </summary>
    public static IResult JsonSafeWithStatus<T>(this T value, int statusCode)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return Results.Text(json, "application/json", (System.Net.HttpStatusCode)statusCode);
    }
}
```

#### Step 3.3: Create service collection extensions

Create `Extensions/ServiceCollectionExtensions.cs`:

```csharp
using ClarusMensAPI.Configuration;
using ClarusMensAPI.Services;
using ClarusMensAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ClarusMensAPI.Extensions;

/// <summary>
/// Extension methods for configuring services in the application
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the service collection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register version service
        services.AddSingleton<VersionService>();
        
        // Register question services
        services.AddSingleton<IQuestionService, SimpleQuestionService>();
        
        return services;
    }
    
    /// <summary>
    /// Configures Swagger and OpenAPI for the application
    /// </summary>
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        // Add Open API services
        services.AddOpenApi();
        services.AddSingleton<CustomOpenApiTransformer>();
        
        // Configure Swagger
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerVersionSetup>();
        services.AddSwaggerGen();
        
        return services;
    }
}
```

### Phase 4 Implementation: Implement Endpoint Organization

#### Step 4.1: Extract endpoints

Create `Endpoints/QuestionEndpoints.cs`:

```csharp
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Models.Responses;
using ClarusMensAPI.Services.Interfaces;

namespace ClarusMensAPI.Endpoints;

public static class QuestionEndpoints
{
    public static WebApplication MapQuestionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/question", async (string query, IQuestionService questionService) =>
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(query))
            {
                return new { error = "Question cannot be empty" }.JsonSafeWithStatus(400);
            }
            
            if (query.Length > 500)
            {
                return new { error = "Question is too long. Maximum length is 500 characters." }.JsonSafeWithStatus(400);
            }

            try
            {
                // Process the question and get an answer
                var answer = await questionService.GetAnswerAsync(query);
                
                // Return the answer using safe serialization
                return new QuestionAnswerResponse
                {
                    Question = query,
                    Answer = answer,
                    ProcessedAt = DateTime.UtcNow
                }.JsonSafeOk();
            }
            catch (Exception)
            {
                // No exception details needed
                return new
                {
                    title = "Error processing question",
                    detail = "An unexpected error occurred while processing your question."
                }.JsonSafeWithStatus(500);
            }
        })
        .WithName("GetAnswer")
        .WithOpenApi(operation => {
            operation.Summary = "Get an answer to a question";
            operation.Description = "Provides a short answer to a user's question";
            operation.Parameters[0].Description = "The question to be answered";

            operation.Parameters[0].Example = new Microsoft.OpenApi.Any.OpenApiString("What is your name?");
            return operation;
        });
        
        return app;
    }
}
```

Create `Endpoints/VersionEndpoints.cs`:

```csharp
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.Endpoints;

public static class VersionEndpoints
{
    public static WebApplication MapVersionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/version", (VersionService versionService) =>
        {
            var semVersion = versionService.GetSemVersion();
            var dotNetVersion = versionService.GetDotNetVersion();
            
            var response = new 
            { 
                version = versionService.GetVersionString(),
                semVer = new
                {
                    major = semVersion.Major,
                    minor = semVersion.Minor,
                    patch = semVersion.Patch,
                    preRelease = semVersion.PreRelease ?? string.Empty,
                    buildMetadata = semVersion.BuildMetadata ?? string.Empty,
                    isPreRelease = semVersion.IsPreRelease
                },
                assemblyVersion = $"{dotNetVersion.Major}.{dotNetVersion.Minor}.{dotNetVersion.Build}.{dotNetVersion.Revision}"
            };
            
            return response.JsonSafeOk();
        })
        .WithName("GetVersion")
        .WithOpenApi(operation => {
            operation.Summary = "Get API version information";
            operation.Description = "Returns detailed version information of the API following SemVer 2.0.0 specification";
            return operation;
        });
        
        return app;
    }
}
```

Create `Endpoints/RootEndpoint.cs`:

```csharp
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Models.Responses;
using ClarusMensAPI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClarusMensAPI.Endpoints;

public static class RootEndpoint
{
    public static WebApplication MapRootEndpoint(this WebApplication app)
    {
        app.MapGet("/", async (
            IWebHostEnvironment env, 
            VersionService versionService,
            IConfiguration config,
            HealthCheckService healthCheck) => 
        {
            // Get health status asynchronously
            var health = await healthCheck.CheckHealthAsync();
            var status = health.Status == HealthStatus.Healthy ? "operational" : "degraded";
            
            // Get API info from configuration
            var apiName = config["ApiInfo:Name"] ?? "Clarus Mens API";
            
            var response = new RootResponse
            {
                Status = status,
                Name = apiName,
                Version = versionService.GetDisplayVersion(),
                Environment = env.EnvironmentName,
                License = new LicenseInfo
                {
                    Name = config["ApiInfo:License:Name"] ?? "Apache License 2.0",
                    Url = config["ApiInfo:License:Url"] ?? "https://www.apache.org/licenses/LICENSE-2.0"
                },
                Links = new LinksInfo
                {
                    Documentation = "/swagger",
                    OpenApiSpec = "/openapi",
                    Health = "/health",
                    Source = "https://github.com/pstackebrandt/clarus-mens"
                }
            };
            
            // Use the safe JSON serialization extension method
            return response.JsonSafeOk();
        });
        
        return app;
    }
}
```

### Phase 5 Implementation: Update Program.cs

Final version of `Program.cs`:

```csharp
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Endpoints;
using Microsoft.AspNetCore.HttpsPolicy;

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddOpenApiServices();

// Add health checks
builder.Services.AddHealthChecks();

// Configure HTTPS redirection
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 7043; // Or whatever your HTTPS port should be
});

var app = builder.Build();

// Application Lifecycle Events
app.Lifetime.ApplicationStarted.Register(() =>
{
    var versionService = app.Services.GetRequiredService<VersionService>();
    app.Logger.LogInformation("Application started. Version: {Version}", versionService.GetDisplayVersion());
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Always make OpenAPI available regardless of environment
app.MapOpenApi(); // Makes JSON spec available at /openapi

// Configure Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{ApiContractVersion}/swagger.json", $"Clarus Mens API {app.Services.GetRequiredService<VersionService>().GetDisplayVersion()}");
    options.RoutePrefix = "swagger";
});

// Map health checks endpoint
app.MapHealthChecks("/health");

// Map API endpoints
app.MapRootEndpoint();
app.MapQuestionEndpoints();
app.MapVersionEndpoints();

app.Run();

// Added for test accessibility
public partial class Program { }
```

## Migration Verification

After each phase:

1. Build the project to check for compilation errors
2. Run the application to verify functionality
3. Test all endpoints to ensure they still work as expected
4. Check Swagger documentation to ensure it's correctly configured

## Benefits of New Structure

1. **Maintainability**: Code is organized by responsibility, making it easier to maintain
2. **Scalability**: Easy to add new features without cluttering Program.cs
3. **Testability**: Components are isolated, making them easier to test
4. **Readability**: Smaller, focused files improve code readability
5. **Onboarding**: Clear structure helps new developers understand the application

## Next Steps

- Implement input validation with FluentValidation
- Add more comprehensive unit tests
- Consider implementing a proper repository pattern if database access is needed
- Implement proper authentication and authorization
