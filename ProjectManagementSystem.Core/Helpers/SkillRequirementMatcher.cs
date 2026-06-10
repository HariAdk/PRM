using System.Text.RegularExpressions;

namespace ProjectManagementSystem.Core.Helpers;

/// <summary>
/// Scores how well an employee profile matches a free-text manager requirement.
/// Used by the rule-based skill-match fallback when the LLM is unavailable.
/// </summary>
public static class SkillRequirementMatcher
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "for", "with", "from", "need", "want", "who", "has",
        "have", "using", "use", "developer", "engineer", "resource", "person", "someone",
        "months", "month", "weeks", "week", "days", "day", "hrs", "hr", "hours", "hour"
    };

    public static IReadOnlyList<string> ExtractKeywords(string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
            return [];

        return Regex.Matches(requirement, @"[A-Za-z0-9+#.]+")
            .Select(m => m.Value.Trim())
            .Where(token => token.Length >= 2 && !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static int ScoreProfile(string requirement, string skills, string department, string recentActivity)
    {
        var keywords = ExtractKeywords(requirement);
        if (keywords.Count == 0)
            return 0;

        var skillParts = skills.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var haystack = $"{skills} {department} {recentActivity}";
        var score = 0;

        foreach (var keyword in keywords)
        {
            if (haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 2;
                continue;
            }

            if (skillParts.Any(skill =>
                    skill.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    keyword.Contains(skill, StringComparison.OrdinalIgnoreCase)))
            {
                score += 3;
            }
        }

        return score;
    }
}
