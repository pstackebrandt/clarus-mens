using ClarusMensAPI.Services;
using ClarusMensAPI.Extensions;
using ClarusMensAPI.Endpoints;

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// Service Registration
builder.Services.AddApplicationServices();
builder.Services.AddOpenApiServices(ApiContractVersion);
builder.Services.AddHttpsRedirection(7043);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Application Lifecycle Events
app.Lifetime.ApplicationStarted.Register(() =>
{
    var versionService = app.Services.GetRequiredService<VersionService>();
    app.Logger.LogInformation("Application started. Version: {Version}", versionService.GetDisplayVersion());
});

// Middleware Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// OpenAPI/Swagger Configuration
// I make OpenAPI available regardless of environment.
app.MapOpenApi(); // Makes JSON spec available at /openapi
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{ApiContractVersion}/swagger.json", $"Clarus Mens API {app.Services.GetRequiredService<VersionService>().GetDisplayVersion()}");
    options.RoutePrefix = "swagger";
});

// Endpoint Registration
app.MapApplicationEndpoints();

await app.RunAsync();

// Added for test accessibility
public static partial class Program { }