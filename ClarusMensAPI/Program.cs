using Microsoft.AspNetCore.HttpsPolicy;
using ClarusMensAPI.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

// API contract version constant
const string ApiContractVersion = "v0";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Using the simpler approach without options
builder.Services.AddSingleton<CustomOpenApiTransformer>();

// Register version service (make sure it's registered before Swagger config)
builder.Services.AddSingleton<VersionService>();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    // Get the version service to use the actual version
    var serviceProvider = builder.Services.BuildServiceProvider();
    var versionService = serviceProvider.GetRequiredService<VersionService>();
    
    c.SwaggerDoc(ApiContractVersion, new OpenApiInfo 
    { 
        Title = "Clarus Mens API", 
        Version = versionService.GetDisplayVersion(),
        Description = "API for Clarus Mens question answering service (Pre-release version)"
    });
});

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
    options.SwaggerEndpoint($"/swagger/{ApiContractVersion}/swagger.json", $"Clarus Mens API {app.Services.GetRequiredService<VersionService>().GetDisplayVersion()} (Beta)");
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
    return operation;
});

/**
    Returns detailed version information of the API.
*/
app.MapGet("/api/version", (VersionService versionService) =>
{
    var version = versionService.GetVersion();
    return Results.Ok(new 
    { 
        version = versionService.GetDisplayVersion(),
        major = version.Major,
        minor = version.Minor,
        build = version.Build,
        revision = version.Revision
    });
})
.WithName("GetVersion")
.WithOpenApi(operation => {
    operation.Summary = "Get API version information";
    operation.Description = "Returns detailed version information of the API including major, minor, build, and revision numbers";
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
