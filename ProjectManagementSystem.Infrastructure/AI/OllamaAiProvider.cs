using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

public sealed class OllamaAiProvider(
    HttpClient httpClient,
    string apiKey,
    ILogger<OllamaAiProvider> logger) : IAiProvider
{
    public string ProviderName => LlmProviders.Ollama;

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default,
        bool jsonResponse = false)
    {
        var combined = $"{systemPrompt}\n\n{userPrompt}";

        var payload = new Dictionary<string, object>
        {
            ["model"] = ExternalApiDefaults.OllamaModel,
            ["prompt"] = combined,
            ["stream"] = false,
            ["options"] = new
            {
                temperature = ExternalApiDefaults.LlmTemperature,
                num_predict = ExternalApiDefaults.LlmMaxOutputTokens
            }
        };
        if (jsonResponse)
            payload["format"] = "json";

        using var request = new HttpRequestMessage(HttpMethod.Post, ExternalApiDefaults.OllamaGeneratePath);
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.TryAddWithoutValidation(ExternalApiDefaults.OllamaApiKeyHeader, apiKey.Trim());

        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Ollama API error {Status}: {Body}", response.StatusCode, body);
            throw new BusinessRuleException(ErrorMessages.OllamaApiError((int)response.StatusCode));
        }

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement.GetProperty("response").GetString();

        if (string.IsNullOrWhiteSpace(text))
            throw new BusinessRuleException(ErrorMessages.OllamaEmptyResponse);

        return text.Trim();
    }
}
