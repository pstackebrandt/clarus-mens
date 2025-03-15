namespace ClarusMensAPI.Endpoints;

/// <summary>
/// Endpoint registration extensions for the application
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all application endpoints
    /// </summary>
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Register the root endpoint
        app.MapRootEndpoint();
        
        // Register the question endpoints
        app.MapQuestionEndpoints();
        
        // Register the version endpoints
        app.MapVersionEndpoints();
        
        // Register health check endpoint
        app.MapHealthChecks("/health");
        
        return app;
    }
} 