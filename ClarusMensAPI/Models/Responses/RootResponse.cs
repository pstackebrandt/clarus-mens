using System.Text.Json.Serialization;

namespace ClarusMensAPI.Models.Responses;

public class RootResponse
{
    public string Status { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public LicenseInfo License { get; set; } = new LicenseInfo();
    public LinksInfo Links { get; set; } = new LinksInfo();
}

public class LicenseInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class LinksInfo
{
    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
    
    [JsonPropertyName("openapi_spec")]
    public string OpenApiSpec { get; set; } = string.Empty;
    
    [JsonPropertyName("health")]
    public string Health { get; set; } = string.Empty;
    
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
} 