/*
 * Program.cs
 * 
 * Entry point and configuration for the Clarus Mens API.
 * This file:
 * - Initializes the web application
 * - Configures services, middleware, and endpoints
 * - Sets up OpenAPI/Swagger documentation
 * - Defines application lifecycle events
 * - Contains a non-static Program class to support WebApplicationFactory testing
 */

using ClarusMensAPI.Services;
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Endpoints;
using ClarusMensAPI.Configuration;
using System.Runtime.InteropServices;

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// Configure application configuration sources
ApiConfiguration.ConfigureAppConfiguration(builder);

// Service Registration
builder.Services.AddApplicationServices();
builder.Services.AddOpenApiServices(ApiContractVersion);
// Only configure HTTPS redirection in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(7043);
}
builder.Services.AddHealthChecks();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Validate configuration
ApiConfiguration.ValidateConfiguration(app.Configuration, app.Environment);

// Application Lifecycle Events
app.Lifetime.ApplicationStarted.Register(() =>
{
    var versionService = app.Services.GetRequiredService<VersionService>();
    app.Logger.LogInformation("Application started successfully. Version: {Version}", versionService.GetDisplayVersion());
    app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    app.Logger.LogInformation("ASPNETCORE_URLS: {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
    app.Logger.LogInformation("Operating System: {OS}", Environment.OSVersion);
    app.Logger.LogInformation("Framework: {Framework}", RuntimeInformation.FrameworkDescription);
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogWarning("Application is stopping!");
});

// Middleware Configuration
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// OpenAPI/Swagger Configuration
// Swagger will be active in production so interviewers will be able to test the API.
app.MapOpenApi(); // Makes JSON spec available at /openapi
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{ApiContractVersion}/swagger.json", $"Clarus Mens API {app.Services.GetRequiredService<VersionService>().GetDisplayVersion()}");
    options.RoutePrefix = "swagger";
});

// Add diagnostic endpoint
app.MapGet("/api/diagnostics", () =>
{
    return new
    {
        Environment = app.Environment.EnvironmentName,
        Time = DateTime.UtcNow,
        OsVersion = Environment.OSVersion.ToString(),
        ProcessorCount = Environment.ProcessorCount,
        FrameworkDescription = RuntimeInformation.FrameworkDescription,
        AspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS"),
        WorkingDirectory = Environment.CurrentDirectory,
        AvailableMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024
    };
});

// Endpoint Registration
app.MapApplicationEndpoints();

await app.RunAsync();

// Program class must NOT be static to support WebApplicationFactory<Program> in tests.
// Static classes cannot be used as generic type parameters (CS0718 error).
// While top-level statements in .NET 6+ typically use a static Program class,
// for testability we need a non-static class that testing frameworks can instantiate.
public partial class Program
{
    // Protected constructor allows WebApplicationFactory to create instances
    // while preventing direct instantiation elsewhere
    protected Program() { }
}