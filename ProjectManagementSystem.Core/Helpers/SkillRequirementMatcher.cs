using System.Text.RegularExpressions;
using ProjectManagementSystem.Core.DTOs.AI;

namespace ProjectManagementSystem.Core.Helpers;

public static class SkillRequirementMatcher
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "for", "with", "from", "need", "want", "who", "has",
        "have", "using", "use", "developer", "engineer", "resource", "person", "someone",
        "months", "month", "weeks", "week", "days", "day", "hrs", "hr", "hours", "hour",
        "domain", "availability", "available", "free", "capacity", "fully", "full", "percent"
    };

    private static readonly Dictionary<string, string[]> SkillAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["javascript"] = ["javascript", "js"],
        ["typescript"] = ["typescript", "ts"],
        ["dotnet"] = ["dotnet", ".net", "c#", "csharp"],
        ["java"] = ["java"],
        ["python"] = ["python"],
        ["react"] = ["react"],
        ["angular"] = ["angular"],
        ["vue"] = ["vue", "vuejs"],
        ["node"] = ["node", "nodejs", "node.js"],
        ["kubernetes"] = ["kubernetes", "k8s"],
        ["devops"] = ["devops"],
        ["sql"] = ["sql", "mssql", "postgres", "postgresql", "mysql"]
    };

    public static int? TryParseMinAvailabilityPercent(string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
            return null;

        if (Regex.IsMatch(requirement, @"\b(fully|full)\s+availab", RegexOptions.IgnoreCase))
            return 100;

        var percentMatch = Regex.Match(
            requirement,
            @"(\d{1,3})\s*%\s*(free|availability|available|capacity|utili[sz]ation)?",
            RegexOptions.IgnoreCase);

        if (percentMatch.Success &&
            int.TryParse(percentMatch.Groups[1].Value, out var pct) &&
            pct is >= 1 and <= 100)
        {
            return pct;
        }

        return null;
    }

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

    public static IReadOnlyList<string> ExtractSkillKeywords(string requirement)
    {
        var minAvail = TryParseMinAvailabilityPercent(requirement);

        return ExtractKeywords(requirement)
            .Where(k => !Regex.IsMatch(k, @"^\d+$"))
            .Where(k => minAvail is null || !k.Equals(minAvail.Value.ToString(), StringComparison.Ordinal))
            .ToList();
    }

    public static bool HasSkillConstraints(string requirement) =>
        ExtractSkillKeywords(requirement).Count > 0;

    public static bool MeetsAvailability(string requirement, decimal availabilityPercent)
    {
        var minAvail = TryParseMinAvailabilityPercent(requirement);
        return !minAvail.HasValue || availabilityPercent >= minAvail.Value;
    }

    public static bool MeetsSkillKeywords(
        string requirement,
        string skills,
        string department,
        string recentActivity)
    {
        if (!HasSkillConstraints(requirement))
            return true;

        return ScoreProfile(requirement, skills, department, recentActivity) > 0;
    }

    public static bool MeetsRequirement(string requirement, SkillMatchCandidateDto candidate) =>
        MeetsAvailability(requirement, candidate.AvailabilityPercent) &&
        MeetsSkillKeywords(requirement, candidate.Skills, candidate.Department, candidate.RecentActivity);

    public static int ScoreProfile(string requirement, string skills, string department, string recentActivity)
    {
        var keywords = ExtractSkillKeywords(requirement);
        if (keywords.Count == 0)
            return 0;

        var skillParts = skills.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var haystack = $"{skills} {department} {recentActivity}";
        var score = 0;

        foreach (var keyword in keywords)
        {
            var terms = ExpandKeyword(keyword);

            foreach (var term in terms)
            {
                if (haystack.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                    break;
                }

                if (skillParts.Any(skill =>
                        skill.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        term.Contains(skill, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 3;
                    break;
                }
            }
        }

        return score;
    }

    private static IEnumerable<string> ExpandKeyword(string keyword)
    {
        yield return keyword;

        if (SkillAliases.TryGetValue(keyword, out var aliases))
        {
            foreach (var alias in aliases)
                yield return alias;
        }
    }
}
