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
using ClarusMensAPI.Endpoints;

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

// TEMPLATE: Endpoint Registration - Register all endpoints using the extension methods
app.MapApplicationEndpoints();

app.Run();

// Added for test accessibility
public partial class Program { }