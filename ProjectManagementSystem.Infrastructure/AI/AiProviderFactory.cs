using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.AI;

namespace ProjectManagementSystem.Infrastructure.AI;

public sealed class AiProviderFactory(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory) : IAiProviderFactory
{
    public bool IsConfigured(SystemConfigDto? config) =>
        config is not null &&
        !string.IsNullOrWhiteSpace(config.LlmProvider) &&
        !string.IsNullOrWhiteSpace(config.LlmApiKey);

    public IAiProvider Create(SystemConfigDto config)
    {
        if (!IsConfigured(config))
            throw new BusinessRuleException(ErrorMessages.LlmNotConfigured);

        var provider = config.LlmProvider.Trim();

        if (provider.Equals(LlmProviders.Groq, StringComparison.OrdinalIgnoreCase))
        {
            return new GroqAiProvider(
                httpClientFactory.CreateClient(HttpClientNames.Groq),
                config.LlmApiKey.Trim(),
                loggerFactory.CreateLogger<GroqAiProvider>());
        }

        if (provider.Equals(LlmProviders.Ollama, StringComparison.OrdinalIgnoreCase))
        {
            return new OllamaAiProvider(
                httpClientFactory.CreateClient(HttpClientNames.Ollama),
                config.LlmApiKey,
                loggerFactory.CreateLogger<OllamaAiProvider>());
        }

        if (provider.Equals(LlmProviders.Gemini, StringComparison.OrdinalIgnoreCase))
        {
            return new GeminiAiProvider(
                httpClientFactory.CreateClient(HttpClientNames.Gemini),
                config.LlmApiKey.Trim(),
                loggerFactory.CreateLogger<GeminiAiProvider>());
        }

        throw new BusinessRuleException(ErrorMessages.LlmProviderNotSupported);
    }
}
