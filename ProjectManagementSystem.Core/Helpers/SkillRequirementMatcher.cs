using System.Text.RegularExpressions;
using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Helpers;

public record AvailabilityConstraint(decimal? MinFreePercent, decimal? MaxFreePercent);

public static class SkillRequirementMatcher
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "for", "with", "from", "need", "want", "who", "has",
        "have", "using", "use", "developer", "engineer", "resource", "person", "someone",
        "months", "month", "weeks", "week", "days", "day", "hrs", "hr", "hours", "hour",
        "domain", "availability", "available", "free", "capacity", "fully", "full", "percent",
        "proficiency", "beginner", "beginer", "intermediate", "advanced", "only", "max", "min",
        "maximum", "minimum", "least", "most", "utilization", "utilisation"
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
        ["sql"] = ["sql", "mssql", "postgres", "postgresql", "mysql"],
        ["qa"] = ["qa", "quality", "assurance", "testing", "tester", "sdet"],
        ["sdet"] = ["sdet", "qa", "testing", "automation"]
    };

    public static int? TryParseMinAvailabilityPercent(string requirement) =>
        ParseAvailabilityConstraint(requirement).MinFreePercent is decimal min
            ? (int)min
            : null;

    public static AvailabilityConstraint ParseAvailabilityConstraint(string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
            return new AvailabilityConstraint(null, null);

        if (Regex.IsMatch(requirement, @"\b(fully|full)\s+availab", RegexOptions.IgnoreCase))
            return new AvailabilityConstraint(100, null);

        decimal? maxFree = null;
        decimal? minFree = null;

        var maxAvailability = Regex.Match(
            requirement,
            @"\b(?:max|maximum|at most|up to)\s+(\d{1,3})\s*%\s*(?:free\s+)?availability\b",
            RegexOptions.IgnoreCase);
        if (maxAvailability.Success &&
            int.TryParse(maxAvailability.Groups[1].Value, out var maxPct) &&
            maxPct is >= 1 and <= 100)
            maxFree = maxPct;

        var maxUtilization = Regex.Match(
            requirement,
            @"\b(?:max|maximum|at most|up to)\s+(\d{1,3})\s*%\s*utili[sz]ation\b",
            RegexOptions.IgnoreCase);
        if (maxUtilization.Success &&
            int.TryParse(maxUtilization.Groups[1].Value, out var maxUtil) &&
            maxUtil is >= 1 and <= 100)
            minFree = 100 - maxUtil;

        var minAvailability = Regex.Match(
            requirement,
            @"\b(?:min|minimum|at least)\s+(\d{1,3})\s*%\s*(?:free\s+)?availability\b",
            RegexOptions.IgnoreCase);
        if (minAvailability.Success &&
            int.TryParse(minAvailability.Groups[1].Value, out var minPct) &&
            minPct is >= 1 and <= 100)
            minFree = minPct;

        if (!minFree.HasValue && !maxFree.HasValue)
        {
            var plainPercent = Regex.Match(
                requirement,
                @"(\d{1,3})\s*%\s*(?:free\s+)?(?:availability|available|capacity)\b",
                RegexOptions.IgnoreCase);
            if (plainPercent.Success &&
                int.TryParse(plainPercent.Groups[1].Value, out var pct) &&
                pct is >= 1 and <= 100)
                minFree = pct;
        }

        return new AvailabilityConstraint(minFree, maxFree);
    }

    public static ProficiencyLevel? TryParseProficiencyLevel(string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
            return null;

        if (Regex.IsMatch(requirement, @"\b(?:only\s+)?advanced\b", RegexOptions.IgnoreCase))
            return ProficiencyLevel.Advanced;
        if (Regex.IsMatch(requirement, @"\b(?:only\s+)?intermediate\b", RegexOptions.IgnoreCase))
            return ProficiencyLevel.Intermediate;
        if (Regex.IsMatch(requirement, @"\b(?:only\s+)?beginn?er\b", RegexOptions.IgnoreCase))
            return ProficiencyLevel.Beginner;

        return null;
    }

    public static bool RequiresExactProficiency(string requirement) =>
        Regex.IsMatch(requirement, @"\b(?:only|exactly)\s+(?:beginn?er|intermediate|advanced)\b", RegexOptions.IgnoreCase);

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
        var availability = ParseAvailabilityConstraint(requirement);
        var availabilityNumbers = new HashSet<string>(StringComparer.Ordinal);
        if (availability.MinFreePercent.HasValue)
            availabilityNumbers.Add(((int)availability.MinFreePercent.Value).ToString());
        if (availability.MaxFreePercent.HasValue)
            availabilityNumbers.Add(((int)availability.MaxFreePercent.Value).ToString());

        return ExtractKeywords(requirement)
            .Where(k => !Regex.IsMatch(k, @"^\d+$") || !availabilityNumbers.Contains(k))
            .ToList();
    }

    public static bool HasSkillConstraints(string requirement) =>
        ExtractSkillKeywords(requirement).Count > 0;

    public static bool MeetsAvailability(string requirement, decimal availabilityPercent) =>
        MeetsAvailabilityConstraint(ParseAvailabilityConstraint(requirement), availabilityPercent);

    public static bool MeetsAvailabilityConstraint(AvailabilityConstraint constraint, decimal availabilityPercent)
    {
        if (constraint.MinFreePercent.HasValue && availabilityPercent < constraint.MinFreePercent.Value)
            return false;
        if (constraint.MaxFreePercent.HasValue && availabilityPercent > constraint.MaxFreePercent.Value)
            return false;
        return true;
    }

    public static bool MeetsSkillKeywords(
        string requirement,
        string skills,
        string department,
        string recentActivity) =>
        !HasSkillConstraints(requirement) ||
        ScoreProfile(requirement, skills, department, recentActivity) > 0;

    public static bool MeetsProficiency(string requirement, string skillsWithProficiency)
    {
        var requiredLevel = TryParseProficiencyLevel(requirement);
        if (!requiredLevel.HasValue || string.IsNullOrWhiteSpace(skillsWithProficiency))
            return !requiredLevel.HasValue;

        var skillKeywords = ExtractSkillKeywords(requirement);
        if (skillKeywords.Count == 0)
            return true;

        var exactOnly = RequiresExactProficiency(requirement);
        foreach (var part in skillsWithProficiency.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!SkillPartMatchesKeywords(part, skillKeywords))
                continue;

            var candidateLevel = ParseProficiencyFromSkillPart(part);
            if (!candidateLevel.HasValue)
                return false;

            if (exactOnly)
                return candidateLevel.Value == requiredLevel.Value;

            return ProficiencyRank(candidateLevel.Value) >= ProficiencyRank(requiredLevel.Value);
        }

        return false;
    }

    public static bool MeetsRequirement(string requirement, SkillMatchCandidateDto candidate)
    {
        var skillsForMatch = string.IsNullOrWhiteSpace(candidate.SkillsWithProficiency)
            ? candidate.Skills
            : candidate.SkillsWithProficiency;

        return MeetsAvailability(requirement, candidate.AvailabilityPercent) &&
               MeetsSkillKeywords(requirement, skillsForMatch, candidate.Department, candidate.RecentActivity) &&
               MeetsProficiency(requirement, skillsForMatch);
    }

    public static int GetBestMatchingProficiencyRank(string requirement, string skillsWithProficiency)
    {
        if (string.IsNullOrWhiteSpace(skillsWithProficiency))
            return 0;

        var skillKeywords = ExtractSkillKeywords(requirement);
        if (skillKeywords.Count == 0)
            return 0;

        var best = 0;
        foreach (var part in skillsWithProficiency.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!SkillPartMatchesKeywords(part, skillKeywords))
                continue;

            var candidateLevel = ParseProficiencyFromSkillPart(part);
            if (candidateLevel.HasValue)
                best = Math.Max(best, ProficiencyRank(candidateLevel.Value));
        }

        return best;
    }

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
                        SkillPartName(skill).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        term.Contains(SkillPartName(skill), StringComparison.OrdinalIgnoreCase)))
                {
                    score += 3;
                    break;
                }
            }
        }

        return score;
    }

    private static bool SkillPartMatchesKeywords(string skillPart, IReadOnlyList<string> keywords)
    {
        var skillName = SkillPartName(skillPart);
        foreach (var keyword in keywords)
        {
            foreach (var term in ExpandKeyword(keyword))
            {
                if (skillName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    term.Contains(skillName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static string SkillPartName(string skillPart)
    {
        var idx = skillPart.IndexOf('(');
        return idx > 0 ? skillPart[..idx].Trim() : skillPart.Trim();
    }

    private static ProficiencyLevel? ParseProficiencyFromSkillPart(string skillPart)
    {
        var match = Regex.Match(skillPart, @"\(([^)]+)\)\s*$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        var level = match.Groups[1].Value.Trim();
        if (Enum.TryParse<ProficiencyLevel>(level, ignoreCase: true, out var parsed))
            return parsed;

        if (level.Contains("begin", StringComparison.OrdinalIgnoreCase))
            return ProficiencyLevel.Beginner;

        return null;
    }

    private static int ProficiencyRank(ProficiencyLevel level) => level switch
    {
        ProficiencyLevel.Beginner => 1,
        ProficiencyLevel.Intermediate => 2,
        ProficiencyLevel.Advanced => 3,
        _ => 0
    };

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
