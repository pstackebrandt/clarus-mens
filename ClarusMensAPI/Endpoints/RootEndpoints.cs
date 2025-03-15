using ClarusMensAPI.Extensions;
using ClarusMensAPI.Models.Responses;
using ClarusMensAPI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClarusMensAPI.Endpoints;

/// <summary>
/// Root endpoint providing basic API information
/// </summary>
public static class RootEndpoints
{
    /// <summary>
    /// Registers the root endpoint that provides essential API information
    /// </summary>
    public static WebApplication MapRootEndpoint(this WebApplication app)
    {
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

        return app;
    }
} 