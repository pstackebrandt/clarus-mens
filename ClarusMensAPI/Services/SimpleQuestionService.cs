using ClarusMensAPI.Services.Interfaces;

namespace ClarusMensAPI.Services;

// TEMPLATE: Service interfaces and implementations - Replace with your domain services
// but maintain this separation of concerns pattern
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

        // Check if we have a direct match using LINQ
        return Task.FromResult(
            answers.Keys
                .Where(key => question.Contains(key, StringComparison.OrdinalIgnoreCase))
                .Select(key => answers[key])
                .FirstOrDefault()
            ?? "I don't have an answer for that question yet. As we grow, I'll learn to answer more questions.");
    }
}