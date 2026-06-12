namespace ProjectManagementSystem.Core.Constants;

public static class ConfigDisplayDefaults
{
    public const int MaskedApiKeyLength = 28;
    public const string ApiKeyNotSetLabel = "(not set)";

    public static string MaskSecret(string? value) =>
        string.IsNullOrEmpty(value)
            ? ApiKeyNotSetLabel
            : new string('*', MaskedApiKeyLength);

    public static bool ShouldPreserveSecret(string? incoming) =>
        string.IsNullOrEmpty(incoming) ||
        incoming == ApiKeyNotSetLabel ||
        (incoming.Length == MaskedApiKeyLength && incoming.All(c => c == '*'));
}
