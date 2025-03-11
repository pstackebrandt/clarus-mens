using Microsoft.AspNetCore.HttpsPolicy;
using ClarusMensAPI.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using System.Text.Json;

// TEMPLATE: This file demonstrates the recommended API setup pattern for .NET 9 minimal APIs.
// When using this project as a template, preserve the overall structure while customizing specific
// service registrations, middleware configuration, and endpoints for your domain.

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// TEMPLATE: Service Registration Section - customize services but maintain organizational structure
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Using the simpler approach without options
builder.Services.AddSingleton<CustomOpenApiTransformer>();

// Register version service (make sure it's registered before Swagger config)
builder.Services.AddSingleton<VersionService>();

// Configure Swagger through the SwaggerVersionSetup class which uses the IConfigureOptions pattern
// This properly integrates with ASP.NET Core's configuration system and runs once at startup
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerVersionSetup>();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Register services for question-answer functionality
builder.Services.AddSingleton<IQuestionService, SimpleQuestionService>();

// Configure HTTPS redirection
// This explicitly sets the HTTPS port to prevent the warning:
// "Failed to determine the https port for redirect"
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 7043; // Or whatever your HTTPS port should be
});

var app = builder.Build();

// TEMPLATE: Application Lifecycle Events - preserve this pattern for logging and startup tasks
// Add version information to logs
app.Lifetime.ApplicationStarted.Register(() =>
{
    var versionService = app.Services.GetRequiredService<VersionService>();
    app.Logger.LogInformation("Application started. Version: {Version}", versionService.GetDisplayVersion());
});

// TEMPLATE: Middleware Configuration Section - maintain this order while customizing for your needs
// Configure the HTTP request pipeline.
// Only use HTTPS redirection in non-development environments
// This prevents certificate issues during local development
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

// No UI configuration for .NET 9 SimpleAPI - UI is automatically 
// available at /openapi/ui when using MapOpenApi()

// Map health checks endpoint
app.MapHealthChecks("/health");

// TEMPLATE: Root Endpoint Pattern - Customize content but preserve the approach
/**
 * Root API endpoint providing essential API information
 * 
 * IMPORTANT: .NET 9 Serialization Issue Workaround
 * ----------------------------------------------
 * There is a known issue in .NET 9 where the ResponseBodyPipeWriter doesn't 
 * implement the PipeWriter.UnflushedBytes property that System.Text.Json
 * expects. This causes failures with Results.Json() and even Results.Ok() with 
 * serialized objects.
 * 
 * The error appears as:
 * System.InvalidOperationException: The PipeWriter 'ResponseBodyPipeWriter' 
 * does not implement PipeWriter.UnflushedBytes
 * 
 * Solution:
 * We use the JsonSafeOk extension method from ResultsExtensions.cs that handles
 * the serialization safely by converting to string first and using Results.Text().
 * See docs/TROUBLESHOOTING.md for more details on this issue.
 */
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

// TEMPLATE: API Endpoint Pattern - Replace with your own endpoints but follow this structure
// Add the question-answer endpoint
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

/**
    Returns detailed version information of the API.
    Uses the safe JSON serialization approach to avoid .NET 9 serialization issues.
*/
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

app.Run();

// TEMPLATE: Response Models - Replace with your own domain models but maintain the pattern of
// clear separation between API response types and internal domain models
// Response models
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

// Response model
record QuestionAnswerResponse
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}

// TEMPLATE: Service interfaces and implementations - Replace with your domain services
// but maintain this separation of concerns pattern
// Service interfaces and implementations
public interface IQuestionService
{
    Task<string> GetAnswerAsync(string question);
}

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

// TEMPLATE: Configuration Pattern - This demonstrates the IConfigureOptions pattern,
// which is a recommended way to handle complex configuration in ASP.NET Core
// SwaggerVersionSetup configures Swagger documentation at application startup
// This is the recommended way to configure Swagger in ASP.NET Core
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

// Added for test accessibility
public partial class Program { }