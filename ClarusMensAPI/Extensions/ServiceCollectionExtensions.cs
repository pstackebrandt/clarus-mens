using ClarusMensAPI.Configuration;
using ClarusMensAPI.Services;
using ClarusMensAPI.Services.Interfaces;
using Microsoft.AspNetCore.HttpsPolicy;
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
        // Make sure it's registered before Swagger config
        services.AddSingleton<VersionService>();
        
        // Register question services
        services.AddSingleton<IQuestionService, SimpleQuestionService>();
        
        return services;
    }
    
    /// <summary>
    /// Configures OpenAPI and Swagger for the application
    /// </summary>
    /// <remarks>
    /// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    /// </remarks>
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services, string apiVersion = "v0")
    {
        // Add Open API services
        // Using the simpler approach without options
        services.AddOpenApi();
        services.AddSingleton<CustomOpenApiTransformer>();
        
        // Configure Swagger through the SwaggerVersionSetup class which uses the IConfigureOptions pattern
        // This properly integrates with ASP.NET Core's configuration system and runs once at startup
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerVersionSetup>();
        services.AddSwaggerGen();
        
        return services;
    }
    
    /// <summary>
    /// Configures HTTPS redirection for the application
    /// </summary>
    public static IServiceCollection AddHttpsRedirection(this IServiceCollection services, int httpsPort = 7043)
    {
        // Configure HTTPS redirection
        // This explicitly sets the HTTPS port to prevent the warning:
        // "Failed to determine the https port for redirect"
        services.Configure<HttpsRedirectionOptions>(options =>
        {
            options.HttpsPort = httpsPort;
        });
        
        return services;
    }
} 