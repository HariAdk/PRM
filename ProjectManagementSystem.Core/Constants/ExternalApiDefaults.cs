namespace ProjectManagementSystem.Core.Constants;

public static class ExternalApiDefaults
{
    public const string GeminiBaseUrl = "https://generativelanguage.googleapis.com/";
    public const string GroqBaseUrl = "https://api.groq.com/";
    public const string GroqChatCompletionsPath = "openai/v1/chat/completions";
    public const int HttpTimeoutSeconds = 90;

    public const string GeminiModel = "models/gemini-2.5-flash";
    public const string GroqModel = "llama-3.1-8b-instant";

    public const double LlmTemperature = 0.3;
    public const int LlmMaxOutputTokens = 2048;
}
