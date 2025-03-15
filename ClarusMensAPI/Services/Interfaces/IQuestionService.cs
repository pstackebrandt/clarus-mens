namespace ClarusMensAPI.Services.Interfaces;

// TEMPLATE: Service interfaces and implementations - Replace with your domain services
// but maintain this separation of concerns pattern
public interface IQuestionService
{
    Task<string> GetAnswerAsync(string question);
} 