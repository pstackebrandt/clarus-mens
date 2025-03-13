using System.Text.Json;

namespace ClarusMensAPI.Extensions;

/// <summary>
/// Extension methods for safely serializing JSON responses in .NET 9
/// to work around the PipeWriter.UnflushedBytes issue.
/// </summary>
public static class ResultsExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    /// <summary>
    /// Safely serializes an object to JSON and returns it with a 200 OK status
    /// </summary>
    public static IResult JsonSafeOk<T>(this T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return Results.Text(json, "application/json", statusCode: 200);
    }
    
    /// <summary>
    /// Safely serializes an object to JSON and returns it with the specified status code
    /// </summary>
    public static IResult JsonSafeWithStatus<T>(this T value, int statusCode)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return Results.Text(json, "application/json", statusCode: statusCode);
    }
} 