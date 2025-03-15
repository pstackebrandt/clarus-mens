using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ClarusMensAPI.Extensions;

/// <summary>
/// Extension methods for working with IResult types in ASP.NET Core Minimal APIs.
/// These extensions are specifically designed to work around serialization issues in .NET 9
/// with the PipeWriter.UnflushedBytes problem.
/// </summary>
public static class ResultsExtensions
{
    /// <summary>
    /// Default JSON serialization options that apply CamelCase property naming.
    /// </summary>
    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Creates an IResult that serializes the specified value to JSON and returns it with
    /// a 200 OK status code and Content-Type: application/json. This method works around 
    /// the PipeWriter.UnflushedBytes serialization issue in .NET 9 by manually serializing 
    /// to a string first.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="options">Optional JsonSerializerOptions (uses default CamelCase options if not specified)</param>
    /// <returns>An IResult that produces a 200 OK response with JSON content</returns>
    public static IResult JsonSafeOk<T>(this T value, JsonSerializerOptions? options = null)
    {
        var jsonString = JsonSerializer.Serialize(value, options ?? _defaultOptions);
        return Results.Text(jsonString, "application/json", statusCode: 200);
    }

    /// <summary>
    /// Creates an IResult that serializes the specified value to JSON and returns it with
    /// the specified status code and Content-Type: application/json. This method works around 
    /// the PipeWriter.UnflushedBytes serialization issue in .NET 9.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="statusCode">The HTTP status code to use for the response</param>
    /// <param name="options">Optional JsonSerializerOptions (uses default CamelCase options if not specified)</param>
    /// <returns>An IResult that produces a response with the specified status code and JSON content</returns>
    public static IResult JsonSafeWithStatus<T>(this T value, int statusCode, JsonSerializerOptions? options = null)
    {
        var jsonString = JsonSerializer.Serialize(value, options ?? _defaultOptions);
        return Results.Text(jsonString, "application/json", statusCode: statusCode);
    }
}