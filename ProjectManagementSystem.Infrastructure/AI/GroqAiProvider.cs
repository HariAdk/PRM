using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

/// <summary>Groq OpenAI-compatible chat adapter.</summary>
public sealed class GroqAiProvider(
    HttpClient httpClient,
    string apiKey,
    ILogger<GroqAiProvider> logger) : IAiProvider
{
    private const string Model = "llama-3.1-8b-instant";

    public string ProviderName => LlmProviders.Groq;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = Model,
            temperature = 0.3,
            max_tokens = 2048,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Groq API error {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Groq API returned {(int)response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Groq API returned an empty response.");

        return text.Trim();
    }
}
