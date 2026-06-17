using System.Text.RegularExpressions;

namespace ProjectManagementSystem.Core.Helpers;

public static class DesignationRanker
{
    /// <summary>Higher rank = more senior (SSE above SE above JSE).</summary>
    public static int GetRank(string? designation)
    {
        if (string.IsNullOrWhiteSpace(designation))
            return 0;

        var normalized = Regex.Replace(designation.Trim().ToLowerInvariant(), @"[^a-z0-9+#.]+", " ").Trim();
        if (normalized.Length == 0)
            return 0;

        if (normalized.Contains("principal") ||
            normalized.Contains("architect") ||
            normalized.Contains("staff engineer") ||
            Regex.IsMatch(normalized, @"\btech(?:nical)?\s+lead\b"))
            return 6;

        if (Regex.IsMatch(normalized, @"\bsse\b") ||
            normalized.Contains("senior software") ||
            normalized.Contains("sr software"))
            return 5;

        if (Regex.IsMatch(normalized, @"\bjse\b") ||
            normalized.Contains("junior software") ||
            normalized.Contains("jr software"))
            return 3;

        if (Regex.IsMatch(normalized, @"\bse\b") ||
            normalized.Contains("software engineer"))
            return 4;

        if (normalized.Contains("senior") || normalized.Contains("lead"))
            return 5;

        if (normalized.Contains("junior") || normalized.Contains("trainee"))
            return 3;

        return 1;
    }
}
