using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace ClarusMensAPI.Services;

// TEMPLATE: This extension class provides critical workarounds for .NET 9 JSON serialization issues.
// When using this project as a template, keep this helper class intact as it solves a common
// problem with ASP.NET Core minimal APIs in .NET 9. You can add more helper extensions here
// as needed for your specific API requirements.

/// <summary>
/// Extension methods for working with IResult types in ASP.NET Core Minimal APIs.
/// These extensions are specifically designed to work around serialization issues in .NET 9.
/// </summary>
public static class ResultsExtensions
{
    /// <summary>
    /// Creates an IResult that serializes the specified value to JSON and returns it with
    /// a 200 OK status code and Content-Type: application/json. This method works around 
    /// the PipeWriter.UnflushedBytes serialization issue in .NET 9 by manually serializing 
    /// to a string first.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="options">Optional JsonSerializerOptions</param>
    /// <returns>An IResult that produces a 200 OK response with JSON content</returns>
    public static IResult JsonSafeOk<T>(this T value, JsonSerializerOptions? options = null)
    {
        var jsonString = JsonSerializer.Serialize(value, options);
        return TypedResults.Text(jsonString, "application/json");
    }
    
    /// <summary>
    /// Creates an IResult that serializes the specified value to JSON and returns it with
    /// the specified status code and Content-Type: application/json. This method works around 
    /// the PipeWriter.UnflushedBytes serialization issue in .NET 9.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="statusCode">The HTTP status code to use for the response</param>
    /// <param name="options">Optional JsonSerializerOptions</param>
    /// <returns>An IResult that produces a response with the specified status code and JSON content</returns>
    public static IResult JsonSafeWithStatus<T>(this T value, int statusCode, JsonSerializerOptions? options = null)
    {
        var jsonString = JsonSerializer.Serialize(value, options);
        return TypedResults.Text(jsonString, "application/json", statusCode: statusCode);
    }
} 