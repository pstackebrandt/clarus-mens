namespace ClarusMensAPI.Services.Interfaces;

public interface IQuestionService
{
    Task<string> GetAnswerAsync(string question);
} 