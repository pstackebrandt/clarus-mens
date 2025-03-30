using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace ClarusMensAPI.Configuration;

/// <summary>
/// Handles configuration setup for the API, including environment-specific settings
/// and secrets management.
/// </summary>
public static class ApiConfiguration
{
    /// <summary>
    /// Configures the application's configuration sources based on the environment.
    /// </summary>
    public static void ConfigureAppConfiguration(WebApplicationBuilder builder)
    {
        var environment = builder.Environment;
        var configuration = builder.Configuration;

        // Base configuration (already loaded by default)
        // - appsettings.json
        // - appsettings.{environment}.json
        // - User secrets (in Development)
        // - Environment variables

        if (environment.IsDevelopment())
        {
            // Enable user secrets in development
            builder.Configuration.AddUserSecrets<Program>();
        }
        else if (environment.IsProduction())
        {
            // In production, use Azure Key Vault
            var keyVaultUrl = configuration["KeyVault:Url"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                configuration.AddAzureKeyVault(
                    new Uri(keyVaultUrl),
                    new DefaultAzureCredential());
            }
        }

        // Add environment variables with prefix
        configuration.AddEnvironmentVariables("CLARUSMENS_");
    }

    /// <summary>
    /// Validates required configuration settings based on the environment.
    /// </summary>
    public static void ValidateConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Validate API contact information is available
        var contactName = configuration["ApiInfo:Contact:Name"];
        var contactEmail = configuration["ApiInfo:Contact:Email"];

        if (string.IsNullOrEmpty(contactName) || string.IsNullOrEmpty(contactEmail))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "API contact information is not configured. In production, these must be set via Key Vault or environment variables.");
            }
            else
            {
                // In development, we'll use defaults from appsettings.json
                // Log a warning that we're using defaults
                Console.WriteLine("Warning: Using default API contact information from appsettings.json");
            }
        }
    }
}