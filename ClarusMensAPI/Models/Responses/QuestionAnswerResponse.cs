namespace ClarusMensAPI.Models.Responses;

public record QuestionAnswerResponse
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
} 