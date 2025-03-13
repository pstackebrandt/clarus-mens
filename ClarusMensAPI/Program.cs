using Microsoft.AspNetCore.HttpsPolicy;
using ClarusMensAPI.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using System.Text.Json;
using ClarusMensAPI.Models.Responses;
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Services.Interfaces;
using ClarusMensAPI.Configuration;

// TEMPLATE: This file demonstrates the recommended API setup pattern for .NET 9 minimal APIs.
// When using this project as a template, preserve the overall structure while customizing specific
// service registrations, middleware configuration, and endpoints for your domain.

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// TEMPLATE: Service Registration Section - customize services but maintain organizational structure
// Register all application services using extension methods
builder.Services.AddApplicationServices();
builder.Services.AddOpenApiServices(ApiContractVersion);
builder.Services.AddHttpsRedirection(7043);

// Add health checks
builder.Services.AddHealthChecks();

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

// Added for test accessibility
public partial class Program { }