using Microsoft.AspNetCore.HttpsPolicy;
using ClarusMensAPI.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Using the simpler approach without options
builder.Services.AddSingleton<CustomOpenApiTransformer>();

// Register version service (make sure it's registered before Swagger config)
builder.Services.AddSingleton<VersionService>();

// Configure Swagger through the SwaggerVersionSetup class which uses the IConfigureOptions pattern
// This properly integrates with ASP.NET Core's configuration system and runs once at startup
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerVersionSetup>();
builder.Services.AddSwaggerGen();

// Register services for question-answer functionality
builder.Services.AddSingleton<IQuestionService, SimpleQuestionService>();

// Configure HTTPS redirection
// This explicitly sets the HTTPS port to prevent the warning:
// "Failed to determine the https port for redirect"
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 7043; // Or whatever your HTTPS port should be
});

var app = builder.Build();

// Add version information to logs
app.Lifetime.ApplicationStarted.Register(() =>
{
    var versionService = app.Services.GetRequiredService<VersionService>();
    app.Logger.LogInformation("Application started. Version: {Version}", versionService.GetDisplayVersion());
});

// Configure the HTTP request pipeline.
// Only use HTTPS redirection in non-development environments
// This prevents certificate issues during local development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Always make OpenAPI available regardless of environment
app.MapOpenApi(); // Makes JSON spec available at /openapi

// Configure Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{ApiContractVersion}/swagger.json", $"Clarus Mens API {app.Services.GetRequiredService<VersionService>().GetDisplayVersion()}");
    options.RoutePrefix = "swagger";
});

// No UI configuration for .NET 9 SimpleAPI - UI is automatically 
// available at /openapi/ui when using MapOpenApi()

// Add the question-answer endpoint
app.MapGet("/api/question", async (string query, IQuestionService questionService) =>
{
    // Input validation
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new { error = "Question cannot be empty" });
    }
    
    if (query.Length > 500)
    {
        return Results.BadRequest(new { error = "Question is too long. Maximum length is 500 characters." });
    }

    try
    {
        // Process the question and get an answer
        var answer = await questionService.GetAnswerAsync(query);
        
        // Return the answer
        return Results.Ok(new QuestionAnswerResponse
        {
            Question = query,
            Answer = answer,
            ProcessedAt = DateTime.UtcNow
        });
    }
    catch (Exception)
    {
        // No exception details needed
        return Results.Problem(
            title: "Error processing question",
            detail: "An unexpected error occurred while processing your question.",
            statusCode: 500
        );
    }
})
.WithName("GetAnswer")
.WithOpenApi(operation => {
    operation.Summary = "Get an answer to a question";
    operation.Description = "Provides a short answer to a user's question";
    operation.Parameters[0].Description = "The question to be answered";

    operation.Parameters[0].Example = new Microsoft.OpenApi.Any.OpenApiString("What is your name?");
    return operation;
});

/**
    Returns detailed version information of the API.
*/
app.MapGet("/api/version", (VersionService versionService) =>
{
    var semVersion = versionService.GetSemVersion();
    var dotNetVersion = versionService.GetDotNetVersion();
    
    return Results.Ok(new 
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
    });
})
.WithName("GetVersion")
.WithOpenApi(operation => {
    operation.Summary = "Get API version information";
    operation.Description = "Returns detailed version information of the API following SemVer 2.0.0 specification";
    return operation;
});

app.Run();

// Response model
record QuestionAnswerResponse
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}

// Service interfaces and implementations
public interface IQuestionService
{
    Task<string> GetAnswerAsync(string question);
}

public class SimpleQuestionService : IQuestionService
{
    // For the MVP, this could be a simple implementation
    // Later, you can replace this with actual AI integration
    public Task<string> GetAnswerAsync(string question)
    {
        // Simple mapping of questions to answers
        // In a real implementation, this would call an AI service
        var answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "hello", "Hello there! How can I help you?" },
            { "what is your name", "I am Clarus Mens, an AI assistant." },
            { "what time is it", "I don't have real-time capabilities, but you can check your device's clock." },
            { "how does this work", "You ask a question, and I provide an answer using my AI capabilities." }
        };

        // Check if we have a direct match
        foreach (var key in answers.Keys)
        {
            if (question.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(answers[key]);
            }
        }

        // Default response
        return Task.FromResult("I don't have an answer for that question yet. As we grow, I'll learn to answer more questions.");
    }
}

// SwaggerVersionSetup configures Swagger documentation at application startup
// This is the recommended way to configure Swagger in ASP.NET Core
public class SwaggerVersionSetup : IConfigureOptions<SwaggerGenOptions>
{
    private readonly VersionService _versionService;
    private readonly string _apiVersion;

    public SwaggerVersionSetup(VersionService versionService, IConfiguration config)
    {
        _versionService = versionService;
        _apiVersion = config["ApiVersion"] ?? "v0"; // Get from config or use default
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc(_apiVersion, new OpenApiInfo
        {
            Title = "Clarus Mens API",
            Version = _versionService.GetDisplayVersion(),
            Description = "API for Clarus Mens question answering service"
        });
    }
}