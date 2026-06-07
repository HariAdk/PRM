namespace ProjectManagementSystem.Core.Interfaces.AI;

/// <summary>
/// Adapter for an external LLM (Strategy pattern — Gemini, Groq, etc.).
/// </summary>
public interface IAiProvider
{
    string ProviderName { get; }

    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
