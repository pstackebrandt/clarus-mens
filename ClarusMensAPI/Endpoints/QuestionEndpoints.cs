using ClarusMensAPI.Extensions;
using ClarusMensAPI.Models.Responses;
using ClarusMensAPI.Services.Interfaces;
using Microsoft.OpenApi.Any;

namespace ClarusMensAPI.Endpoints;

/// <summary>
/// Endpoints for question answering functionality
/// </summary>
public static class QuestionEndpoints
{
    /// <summary>
    /// Registers endpoints for question answering functionality
    /// </summary>
    public static WebApplication MapQuestionEndpoints(this WebApplication app)
    {
        // TEMPLATE: API Endpoint Pattern - Replace with your own endpoints but follow this structure
        // Add the question-answer endpoint
        app.MapGet("/api/question", async (string query, IQuestionService questionService) =>
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(query))
            {
                return new { error = "Question cannot be empty" }.JsonSafeWithStatus(400);
            }
            
            if (query.Length > 500)
            {
                return new { error = "Question is too long. Maximum length is 500 characters." }.JsonSafeWithStatus(400);
            }

            try
            {
                // Process the question and get an answer
                var answer = await questionService.GetAnswerAsync(query);
                
                // Return the answer using safe serialization
                return new QuestionAnswerResponse
                {
                    Question = query,
                    Answer = answer,
                    ProcessedAt = DateTime.UtcNow
                }.JsonSafeOk();
            }
            catch (Exception)
            {
                // No exception details needed
                return new
                {
                    title = "Error processing question",
                    detail = "An unexpected error occurred while processing your question."
                }.JsonSafeWithStatus(500);
            }
        })
        .WithName("GetAnswer")
        .WithOpenApi(operation => {
            operation.Summary = "Get an answer to a question";
            operation.Description = "Provides a short answer to a user's question";
            operation.Parameters[0].Description = "The question to be answered";

            operation.Parameters[0].Example = new OpenApiString("What is your name?");
            return operation;
        });

        return app;
    }
} 