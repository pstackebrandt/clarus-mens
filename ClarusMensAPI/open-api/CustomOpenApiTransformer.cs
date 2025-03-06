using Microsoft.OpenApi.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClarusMensAPI.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;


/*
This is a custom OpenAPI transformer that sets the title and version of the API.
It gets the version from the VersionService.
*/
internal sealed class CustomOpenApiTransformer : IOpenApiDocumentTransformer
{
    private readonly VersionService _versionService;

    public CustomOpenApiTransformer(VersionService versionService)
    {
        _versionService = versionService;
    }

    /*
    This method is called when the OpenAPI document is being transformed.
    It sets the title and version of the API.
    */
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {   
            Title = "Clarus Mens API",
            Version = _versionService.GetVersionString(),
            Description = "API for Clarus Mens"
        };
        return Task.CompletedTask;
    }
}
