namespace ProjectManagementSystem.Core.Constants;

public static class ExternalApiDefaults
{
    public const string GeminiBaseUrl = "https://generativelanguage.googleapis.com/";
    public const string GroqBaseUrl = "https://api.groq.com/";
    public const string OllamaBaseUrl = "http://164.52.211.238/";
    public const string GroqChatCompletionsPath = "openai/v1/chat/completions";
    public const string OllamaGeneratePath = "api/generate";
    public const string OllamaApiKeyHeader = "apikey";
    public const int OllamaConnectTimeoutSeconds = 60;
    public const int HttpTimeoutSeconds = 90;
    public const int OllamaHttpTimeoutSeconds = 180;

    public const string GeminiModel = "models/gemini-2.0-flash";
    public const string GroqModel = "llama-3.1-8b-instant";
    public const string OllamaModel = "gemma3:12b-it-q8_0";

    public const double LlmTemperature = 0.3;
    public const int LlmMaxOutputTokens = 2048;
}
