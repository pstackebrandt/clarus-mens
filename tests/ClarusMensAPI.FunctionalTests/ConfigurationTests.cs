using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClarusMensAPI.FunctionalTests;

/// <summary>
/// Tests that verify configuration inheritance and overriding works as expected
/// </summary>
[TestClass]
public class ConfigurationTests : FunctionalTestBase
{
    [TestMethod]
    public void Configuration_EnvironmentSpecificValues_OverrideBaseValues()
    {
        // Arrange
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        
        // Act & Assert
        
        // 1. Check overridden values (should come from environment-specific file)
        var apiName = configuration["ApiInfo:Name"];
        Assert.AreEqual("Clarus Mens API (Development)", apiName);
        
        var apiDescription = configuration["ApiInfo:Description"];
        Assert.AreEqual("Development instance of the Clarus Mens question answering service", apiDescription);
        
        // 2. Check inherited values (should come from base appsettings.json)
        var contactName = configuration["ApiInfo:Contact:Name"];
        Assert.AreEqual("Peter Stackebrandt", contactName);
        
        var licenseName = configuration["ApiInfo:License:Name"];
        Assert.AreEqual("Apache License 2.0", licenseName);
        
        // 3. Verify nested object inheritance works
        var licenseUrl = configuration["ApiInfo:License:Url"];
        Assert.IsNotNull(licenseUrl);
        Assert.AreEqual("https://github.com/pstackebrandt/clarus-mens/blob/main/LICENSE", licenseUrl);
    }
    
    [TestMethod]
    public void Configuration_DefaultValuesInCode_ApplyWhenNotInSettings()
    {
        // Arrange
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        
        // Act
        // Get a value that doesn't exist in either settings file
        var nonExistentValue = configuration["ApiInfo:NonExistentSetting"];
        
        // Assert
        Assert.IsNull(nonExistentValue);
        
        // Note: If we want to test actual default values applied in code (like in the root endpoint),
        // we would need to make a request to the API and check the response.
    }
    
    [TestMethod]
    public async Task RootEndpoint_ReturnsExpectedConfigurationValues()
    {
        // Arrange - Already set up in FunctionalTestBase
        
        // Act - Call the root endpoint
        var response = await Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var rootInfo = JsonSerializer.Deserialize<RootResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        // Assert
        Assert.IsNotNull(rootInfo);
        
        // Verify development-specific overridden values
        Assert.AreEqual("Clarus Mens API (Development)", rootInfo.Name);
        
        // Verify inherited values from base appsettings.json
        Assert.IsNotNull(rootInfo.License);
        Assert.AreEqual("Apache License 2.0", rootInfo.License.Name);
        Assert.AreEqual("https://github.com/pstackebrandt/clarus-mens/blob/main/LICENSE", rootInfo.License.Url);
        
        // Verify environment is correctly set
        Assert.AreEqual("Development", rootInfo.Environment);
        
        // Verify links (configured in code)
        Assert.IsNotNull(rootInfo.Links);
        Assert.AreEqual("/swagger", rootInfo.Links.Documentation);
        Assert.AreEqual("/health", rootInfo.Links.Health);
    }
    
    // Helper class for deserializing the root endpoint response
    private class RootResponse
    {
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? Environment { get; set; }
        public LicenseInfo? License { get; set; }
        public LinksInfo? Links { get; set; }
        
        public class LicenseInfo
        {
            public string? Name { get; set; }
            public string? Url { get; set; }
        }
        
        public class LinksInfo
        {
            [JsonPropertyName("documentation")]
            public string? Documentation { get; set; }
            
            [JsonPropertyName("openapi_spec")]
            public string? OpenApiSpec { get; set; }
            
            [JsonPropertyName("health")]
            public string? Health { get; set; }
            
            [JsonPropertyName("source")]
            public string? Source { get; set; }
        }
    }
} 