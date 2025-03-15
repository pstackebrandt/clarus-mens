using ClarusMensAPI.Extensions;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.Endpoints;

/// <summary>
/// Endpoints for API version information
/// </summary>
public static class VersionEndpoints
{
    /// <summary>
    /// Registers endpoints for API version information
    /// </summary>
    public static WebApplication MapVersionEndpoints(this WebApplication app)
    {
        /**
         * Returns detailed version information of the API.
         * Uses the safe JSON serialization approach to avoid .NET 9 serialization issues.
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

        return app;
    }
} 