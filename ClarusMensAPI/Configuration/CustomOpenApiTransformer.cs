using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace ClarusMensAPI.Configuration;

/// <summary>
/// Custom transformer for OpenAPI configuration to enhance API documentation.
/// </summary>
public class CustomOpenApiTransformer
{
    /// <summary>
    /// Transforms OpenAPI operations to add custom metadata, examples, or other enhancements.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to transform</param>
    /// <param name="apiDescription">The API description</param>
    public void Transform(OpenApiOperation operation, ApiDescription apiDescription)
    {
        // Add custom security definitions if needed
        // operation.Security = new List<OpenApiSecurityRequirement> { ... };
        
        // Add standard response descriptions if not present
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Bad Request - The request was malformed or contained invalid parameters."
            };
        }
        
        if (!operation.Responses.ContainsKey("401"))
        {
            operation.Responses["401"] = new OpenApiResponse
            {
                Description = "Unauthorized - Authentication is required and has failed or has not been provided."
            };
        }
        
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses["500"] = new OpenApiResponse
            {
                Description = "Server Error - An unexpected server error occurred."
            };
        }
        
        // Add API version to all operations
        if (operation.Tags == null)
        {
            operation.Tags = new List<OpenApiTag>();
        }
    }
} 