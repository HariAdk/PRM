using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Application.AI;

internal static class TeamBuildAiPromptBuilder
{
    public const string TeamBuildSystemPrompt =
        """
        You are a resource planning assistant for an IT services company.
        The manager wants to assemble a complete project team from BENCH employees across the entire organization.
        For EACH role mentioned in the requirement, recommend exactly one best-matching bench employee OR mark as not found.
        Hard rules:
        - Use ONLY employeeIds from the candidate list (all are on BENCH).
        - Do not assign the same employeeId to more than one role.
        - skillsMatch must list relevant skills with proficiency from the candidate data (e.g. "Java (Advanced)").
        - When multiple bench candidates fit a role, prefer higher skill proficiency, then senior designation (SSE > SE > JSE), then availability.
        - If no bench candidate matches a role, set status to "not_found", employeeId to null, and explain why in reason.
        Respond with valid JSON only — no markdown fences, no extra text.
        JSON shape:
        {"roles":[{"role":"JAVA Developer","status":"matched","employeeId":1,"skillsMatch":"Java (Advanced)","reason":"plain English"},
        {"role":"QA Engineer","status":"not_found","employeeId":null,"skillsMatch":"","reason":"no bench employee has QA skills"}]}
        """;

    public static string BuildTeamBuildUserPrompt(
        string requirement,
        IReadOnlyList<TeamBuildCandidateDto> candidates)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Team requirement: {requirement}");
        sb.AppendLine();
        sb.AppendLine("Bench candidates (organization-wide, status=BENCH):");
        foreach (var c in candidates)
        {
            sb.AppendLine(
                $"- employeeId={c.EmployeeId}, name={c.Name}, designation={c.Designation}, " +
                $"department={c.Department}, skills=[{c.SkillsWithProficiency}], " +
                $"freeCapacity={c.AvailabilityPercent}%, recentActivity=[{c.RecentActivity}]");
        }

        return sb.ToString();
    }
}

internal static class TeamRequirementParser
{
    public static List<string> ExtractRoles(string requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement))
            return [];

        return requirement
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeRolePhrase)
            .Where(r => r.Length > 0)
            .ToList();
    }

    private static readonly Regex LeadingCountPattern = new(@"^\d+\s*", RegexOptions.IgnoreCase);
    private static readonly Regex LeadingPhrasePattern = new(
        @"^(?:(?:i\s+)?(?:want|need)|looking\s+for|require(?:d)?)\s+(?:a\s+|an\s+|the\s+)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string NormalizeRolePhrase(string phrase)
    {
        phrase = LeadingCountPattern.Replace(phrase.Trim(), "").Trim();
        phrase = LeadingPhrasePattern.Replace(phrase, "").Trim();
        return phrase;
    }
}

internal static class TeamBuildAiResponseParser
{
    public static List<TeamBuildRoleSuggestionDto> ParseTeamBuildResponse(
        string llmResponse,
        IReadOnlyList<TeamBuildCandidateDto> candidates)
    {
        var json = AiJsonExtractor.ExtractJson(llmResponse);
        if (json is null) return [];

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!AiJsonExtractor.TryGetProperty(doc.RootElement, "roles", out var rolesElement) ||
                rolesElement.ValueKind != JsonValueKind.Array)
                return [];

            var candidateMap = candidates.ToDictionary(c => c.EmployeeId);
            var usedIds = new HashSet<int>();
            var results = new List<TeamBuildRoleSuggestionDto>();

            foreach (var item in rolesElement.EnumerateArray())
            {
                var role = AiJsonExtractor.TryGetProperty(item, "role", out var roleElement)
                    ? roleElement.GetString()?.Trim() ?? string.Empty
                    : string.Empty;
                if (string.IsNullOrWhiteSpace(role))
                    continue;

                var statusRaw = AiJsonExtractor.TryGetProperty(item, "status", out var statusElement)
                    ? statusElement.GetString()?.Trim() ?? string.Empty
                    : string.Empty;
                var isMatched = statusRaw.Equals("matched", StringComparison.OrdinalIgnoreCase);

                var skillsMatch = AiJsonExtractor.TryGetProperty(item, "skillsMatch", out var skillsElement)
                    ? skillsElement.GetString()?.Trim() ?? string.Empty
                    : string.Empty;
                if (AiJsonExtractor.TryGetProperty(item, "skills_match", out var skillsSnake))
                    skillsMatch = skillsSnake.GetString()?.Trim() ?? skillsMatch;

                var reason = AiJsonExtractor.TryGetProperty(item, "reason", out var reasonElement)
                    ? reasonElement.GetString()?.Trim() ?? string.Empty
                    : string.Empty;

                int? employeeId = null;
                string employeeName = string.Empty;

                if (isMatched && AiJsonExtractor.TryGetEmployeeId(item, out var parsedId) &&
                    candidateMap.TryGetValue(parsedId, out var candidate) &&
                    !usedIds.Contains(parsedId))
                {
                    employeeId = parsedId;
                    employeeName = candidate.Name;
                    usedIds.Add(parsedId);
                    if (string.IsNullOrWhiteSpace(skillsMatch))
                        skillsMatch = candidate.SkillsWithProficiency;
                    if (string.IsNullOrWhiteSpace(reason))
                        reason = $"{candidate.Name} is on bench with matching skills for {role}.";
                }
                else if (isMatched)
                {
                    isMatched = false;
                    if (string.IsNullOrWhiteSpace(reason))
                        reason = $"Could not validate a unique bench match for {role}.";
                }
                else if (string.IsNullOrWhiteSpace(reason))
                {
                    reason = $"No bench employee found with skills matching {role}.";
                }

                var designation = string.Empty;
                var availability = 0m;
                if (isMatched && employeeId.HasValue && candidateMap.TryGetValue(employeeId.Value, out var matchedCandidate))
                {
                    designation = matchedCandidate.Designation;
                    availability = matchedCandidate.AvailabilityPercent;
                }

                results.Add(new TeamBuildRoleSuggestionDto
                {
                    Role = role,
                    Status = isMatched ? "Matched" : "Not Found",
                    EmployeeId = employeeId,
                    EmployeeName = employeeName,
                    Designation = designation,
                    AvailabilityPercentage = availability,
                    SkillsMatch = skillsMatch,
                    Reason = reason
                });
            }

            return results;
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

internal static class TeamBuildFallbackMatcher
{
    public static AITeamBuildResultDto BuildTeamBuild(
        IReadOnlyList<TeamBuildCandidateDto> candidates,
        string requirement,
        string? fallbackReason = null)
    {
        var rolePhrases = TeamRequirementParser.ExtractRoles(requirement);
        if (rolePhrases.Count == 0)
            rolePhrases = [requirement.Trim()];

        var usedIds = new HashSet<int>();
        var results = new List<TeamBuildRoleSuggestionDto>();

        foreach (var role in rolePhrases)
        {
            var best = AiCandidateRanker.OrderByMatchQuality(
                    candidates.Where(c => !usedIds.Contains(c.EmployeeId)),
                    role,
                    c => c.SkillsWithProficiency,
                    c => c.Designation,
                    c => c.AvailabilityPercent,
                    c => c.Name)
                .FirstOrDefault(c => SkillRequirementMatcher.ScoreProfile(
                    role, c.SkillsWithProficiency, c.Department, c.RecentActivity) > 0);

            if (best is null)
            {
                results.Add(new TeamBuildRoleSuggestionDto
                {
                    Role = role,
                    Status = "Not Found",
                    Reason = $"No bench employee in the organization has skills matching '{role}'."
                });
                continue;
            }

            usedIds.Add(best.EmployeeId);
            results.Add(new TeamBuildRoleSuggestionDto
            {
                Role = role,
                Status = "Matched",
                EmployeeId = best.EmployeeId,
                EmployeeName = best.Name,
                Designation = best.Designation,
                AvailabilityPercentage = best.AvailabilityPercent,
                SkillsMatch = best.SkillsWithProficiency,
                Reason =
                    $"{best.Name} is on bench with matching skills: {best.SkillsWithProficiency}."
            });
        }

        return new AITeamBuildResultDto
        {
            Roles = results,
            UsedFallback = true,
            FallbackReason = fallbackReason
        };
    }
}

internal static class AiJsonExtractor
{
    public static string? ExtractJson(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                return text[start..(end + 1)];
        }

        if (text.StartsWith('{'))
            return text;

        var first = text.IndexOf('{');
        var last = text.LastIndexOf('}');
        return first >= 0 && last > first ? text[first..(last + 1)] : null;
    }

    public static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
            return true;

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static bool TryGetEmployeeId(JsonElement item, out int employeeId)
    {
        employeeId = 0;
        if (!TryGetProperty(item, "employeeId", out var idElement) &&
            !TryGetProperty(item, "employee_id", out idElement))
            return false;

        switch (idElement.ValueKind)
        {
            case JsonValueKind.Number when idElement.TryGetInt32(out employeeId):
                return true;
            case JsonValueKind.String:
                return int.TryParse(idElement.GetString(), out employeeId);
            case JsonValueKind.Null:
                return false;
            default:
                return false;
        }
    }
}
