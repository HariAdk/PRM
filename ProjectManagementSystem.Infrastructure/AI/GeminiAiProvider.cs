using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

public sealed class GeminiAiProvider(
    HttpClient httpClient,
    string apiKey,
    ILogger<GeminiAiProvider> logger) : IAiProvider
{
    public string ProviderName => LlmProviders.Gemini;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default,
        bool jsonResponse = false)
    {
        var url = $"v1beta/{ExternalApiDefaults.GeminiModel}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var generationConfig = new Dictionary<string, object>
        {
            ["temperature"] = ExternalApiDefaults.LlmTemperature,
            ["maxOutputTokens"] = ExternalApiDefaults.LlmMaxOutputTokens
        };
        if (jsonResponse)
            generationConfig["responseMimeType"] = "application/json";

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userPrompt } } }
            },
            generationConfig
        };

        using var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Gemini API error {Status}: {Body}", response.StatusCode, body);
            var detail = ExtractGeminiError(body);
            throw new BusinessRuleException(
                string.IsNullOrWhiteSpace(detail)
                    ? ErrorMessages.GeminiApiError((int)response.StatusCode)
                    : $"{ErrorMessages.GeminiApiError((int)response.StatusCode)} {detail}");
        }

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.GetArrayLength() == 0)
            throw new BusinessRuleException(ErrorMessages.GeminiEmptyResponse);

        var text = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new BusinessRuleException(ErrorMessages.GeminiEmptyResponse);

        return text.Trim();
    }

    private static string ExtractGeminiError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
                return message.GetString() ?? string.Empty;
        }
        catch (JsonException)
        {
        }

        return body.Length <= 200 ? body : body[..200];
    }
}
