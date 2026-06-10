using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

/// <summary>Selects Gemini or Groq adapter based on <see cref="SystemConfigDto.LlmProvider"/>.</summary>
public sealed class AiProviderFactory(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory) : IAiProviderFactory
{
    public bool IsConfigured(SystemConfigDto? config) =>
        config is not null &&
        !string.IsNullOrWhiteSpace(config.LlmApiKey) &&
        !string.IsNullOrWhiteSpace(config.LlmProvider);

    public IAiProvider Create(SystemConfigDto config)
    {
        if (!IsConfigured(config))
            throw new BusinessRuleException(ErrorMessages.LlmNotConfigured);

        var provider = config.LlmProvider.Trim();
        var apiKey = config.LlmApiKey.Trim();

        return provider.Equals(LlmProviders.Groq, StringComparison.OrdinalIgnoreCase)
            ? new GroqAiProvider(
                httpClientFactory.CreateClient(HttpClientNames.Groq),
                apiKey,
                loggerFactory.CreateLogger<GroqAiProvider>())
            : new GeminiAiProvider(
                httpClientFactory.CreateClient(HttpClientNames.Gemini),
                apiKey,
                loggerFactory.CreateLogger<GeminiAiProvider>());
    }
}
