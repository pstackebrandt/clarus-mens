using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.HttpsPolicy;

namespace ClarusMensAPI.FunctionalTests;

// TEMPLATE: This TestWebApplicationFactory is a cornerstone of the testing infrastructure.
// When using this project as a template, keep this class and its patterns intact.
// You should only need to modify the configuration values and service replacements
// to match your specific API requirements, but maintain the overall approach.

/// <summary>
/// Custom WebApplicationFactory for functional testing of the API.
/// 
/// This factory provides an isolated test environment that:
/// 1. Uses dynamic port allocation to avoid conflicts with running applications
/// 2. Configures a test-specific environment with custom settings
/// 3. Allows service replacement for more isolated testing
/// 
/// This approach follows ASP.NET Core testing best practices by creating a fully
/// isolated test environment that doesn't interfere with development instances.
/// </summary>
/// <typeparam name="TProgram">The entry point class of the application</typeparam>
public class TestWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    /// <summary>
    /// Configures the web host used for testing.
    /// </summary>
    /// <param name="builder">The IWebHostBuilder to configure</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // TEMPLATE: Dynamic port allocation - keep this pattern to avoid port conflicts
        // Use a random available port for the tests to avoid conflicts
        // with other running applications (like development instances)
        var port = GetAvailablePort();
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // TEMPLATE: Test-specific configuration - customize values but keep the approach
            // Add test-specific configuration that overrides application defaults
            // This ensures the test environment is isolated and predictable
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Override the URLs to use our dynamic port
                ["Urls"] = $"http://localhost:{port}",
                
                // Flag that identifies this as a test environment
                // Can be used in the application to adjust behavior for tests
                ["TestSettings:IsTestEnvironment"] = "true"
            });
        });
        
        // Use the "Development" environment to disable HTTPS redirection
        // This prevents tests from having to handle redirects
        builder.UseEnvironment("Development");
        
        builder.ConfigureServices(services =>
        {
            // TEMPLATE: Service replacement for testing - customize with your own services
            // This section allows replacing real services with test doubles
            // Examples of services you might want to replace for testing:
            // - External API clients
            // - Database contexts with in-memory versions
            // - Authentication services
            
            // Example of replacing a service (commented out):
            // services.Remove(services.Single(d => d.ServiceType == typeof(IMyService)));
            // services.AddScoped<IMyService, MyServiceTestDouble>();
            
            // Disable HTTPS redirection to prevent redirect responses in tests
            services.Configure<HttpsRedirectionOptions>(options =>
            {
                options.HttpsPort = null;  // Disables HTTPS redirection
            });
        });
    }
    
    // TEMPLATE: Port allocation helper - keep this utility method as-is
    /// <summary>
    /// Finds an available TCP port that can be used for the test server.
    /// This prevents port conflicts when running tests concurrently or
    /// when the development server is already running.
    /// </summary>
    /// <returns>An available port number</returns>
    /// <exception cref="Exception">Thrown when no available ports are found</exception>
    private int GetAvailablePort()
    {
        // Get a random port between 10000 and 50000
        // This range typically has available ports and avoids well-known ports
        var random = new Random();
        var startingPort = random.Next(10000, 50000);
        
        // Check up to 100 ports from the starting point
        // This provides a good balance between performance and finding an available port
        for (int port = startingPort; port < startingPort + 100; port++)
        {
            bool isPortAvailable = true;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            
            // Check TCP listeners (servers that are listening on ports)
            var tcpListeners = properties.GetActiveTcpListeners();
            if (tcpListeners.Any(l => l.Port == port))
            {
                isPortAvailable = false;
                continue;
            }
            
            // Check TCP connections (active connections that might be using ports)
            var tcpConnections = properties.GetActiveTcpConnections();
            if (tcpConnections.Any(c => c.LocalEndPoint.Port == port))
            {
                isPortAvailable = false;
                continue;
            }
            
            if (isPortAvailable)
            {
                return port;
            }
        }
        
        throw new Exception("No available ports found");
    }
} 