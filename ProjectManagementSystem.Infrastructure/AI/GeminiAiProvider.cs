using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

/// <summary>Google Gemini REST adapter.</summary>
public sealed class GeminiAiProvider(
    HttpClient httpClient,
    string apiKey,
    ILogger<GeminiAiProvider> logger) : IAiProvider
{
    public string ProviderName => LlmProviders.Gemini;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var url = $"v1/{ExternalApiDefaults.GeminiModel}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        var combined = $"{systemPrompt}\n\n{userPrompt}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = combined } } }
            },
            generationConfig = new
            {
                temperature = ExternalApiDefaults.LlmTemperature,
                maxOutputTokens = ExternalApiDefaults.LlmMaxOutputTokens
            }
        };

        using var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Gemini API error {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException(ErrorMessages.GeminiApiError((int)response.StatusCode));
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException(ErrorMessages.GeminiEmptyResponse);

        return text.Trim();
    }
}
