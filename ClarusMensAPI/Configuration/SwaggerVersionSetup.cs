using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ClarusMensAPI.Services;

namespace ClarusMensAPI.Configuration;

/// <summary>
/// Configures Swagger documentation at application startup using the IConfigureOptions pattern.
/// This is the recommended way to configure Swagger in ASP.NET Core.
/// </summary>
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