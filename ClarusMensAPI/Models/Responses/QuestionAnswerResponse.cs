namespace ClarusMensAPI.Models.Responses;

// TEMPLATE: Response Models - Replace with your own domain models but maintain the pattern of
// clear separation between API response types and internal domain models
public record QuestionAnswerResponse
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
} 