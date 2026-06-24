namespace ProjectManagementSystem.Core.Interfaces.AI;

public interface IAiProvider
{
    string ProviderName { get; }

    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default,
        bool jsonResponse = false);
}
