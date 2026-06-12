using System.Text;
using System.Text.Json;
using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.Helpers;

namespace ProjectManagementSystem.Application.AI;

internal static class AiPromptBuilder
{
    public const string SkillMatchSystemPrompt =
        """
        You are a resource planning assistant for an IT services company.
        Rank employees from the entire organization for a project requirement using ONLY the candidate data provided.
        Hard rules:
        - Only include employees whose listed skills (with proficiency levels) or recent activity match the requirement.
        - Respect proficiency when stated (e.g. beginner, intermediate, advanced).
        - Respect availability constraints: "min X% availability" means freeCapacity >= X; "max X% availability" means freeCapacity <= X.
        - If no candidate satisfies both, return {"matches":[]}.
        Respond with valid JSON only — no markdown fences, no extra text.
        JSON shape:
        {"matches":[{"employeeId":1,"reason":"plain English explanation"}]}
        Include at most 5 matches, best first. Only use employeeIds from the candidate list.
        """;

    public const string RiskSummarySystemPrompt =
        """
        You are a project delivery assistant for an IT services company.
        Write a brief, readable paragraph (3-6 sentences) summarising risks and concerns.
        Use everyday language. Base your answer ONLY on the project facts provided.
        Do not invent people, dates, or numbers. No bullet lists — prose only.
        """;

    public static string BuildSkillMatchUserPrompt(
        string requirement,
        IReadOnlyList<SkillMatchCandidateDto> candidates)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Requirement: {requirement}");
        sb.AppendLine();
        sb.AppendLine("Organization candidates (already filtered for availability):");
        foreach (var c in candidates)
        {
            sb.AppendLine(
                $"- employeeId={c.EmployeeId}, name={c.Name}, department={c.Department}, " +
                $"skills=[{c.SkillsWithProficiency}], freeCapacity={c.AvailabilityPercent}%, " +
                $"freeHoursPerWeek={c.FreeHoursPerWeek:0.#}, recentActivity=[{c.RecentActivity}]");
        }

        return sb.ToString();
    }

    public static string BuildRiskSummaryUserPrompt(RiskSummaryContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Project: {context.ProjectName}");
        sb.AppendLine($"Health status: {context.HealthStatus}");
        sb.AppendLine($"End date: {context.EndDate:dd-MMM-yyyy}");
        sb.AppendLine();
        sb.AppendLine("Milestones:");
        foreach (var m in context.Milestones)
        {
            sb.AppendLine(
                $"- {m.Title}: due {m.DueDate:dd-MMM-yyyy}, status {m.Status}" +
                (m.IsOverdue ? $" (OVERDUE by {m.DaysOverdue} day(s))" : string.Empty));
        }

        sb.AppendLine();
        sb.AppendLine("Allocated resources:");
        foreach (var a in context.Allocations)
        {
            sb.AppendLine(
                $"- {a.EmployeeName}: {a.UtilisationPercent}% allocation, " +
                $"logged {a.LoggedHoursLastWeek:0.#} hrs last week (expected {a.ExpectedHoursLastWeek:0.#} hrs)");
        }

        if (context.RiskFlags.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Known risk flags:");
            foreach (var flag in context.RiskFlags)
                sb.AppendLine($"- {(flag.IsPositive ? "OK" : "RISK")}: {flag.Message}");
        }

        return sb.ToString();
    }
}

internal sealed record RiskSummaryContext
{
    public string ProjectName { get; init; } = string.Empty;
    public string HealthStatus { get; init; } = string.Empty;
    public DateOnly EndDate { get; init; }
    public IReadOnlyList<RiskSummaryMilestone> Milestones { get; init; } = [];
    public IReadOnlyList<RiskSummaryAllocation> Allocations { get; init; } = [];
    public IReadOnlyList<RiskFlagDto> RiskFlags { get; init; } = [];
}

internal sealed record RiskSummaryMilestone
{
    public string Title { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsOverdue { get; init; }
    public int DaysOverdue { get; init; }
}

internal sealed record RiskSummaryAllocation
{
    public string EmployeeName { get; init; } = string.Empty;
    public int UtilisationPercent { get; init; }
    public decimal LoggedHoursLastWeek { get; init; }
    public decimal ExpectedHoursLastWeek { get; init; }
}

internal static class AiResponseParser
{
    public static List<AIMatchedEmployeeDto> ParseSkillMatchResponse(
        string llmResponse,
        IReadOnlyList<SkillMatchCandidateDto> candidates)
    {
        var json = ExtractJson(llmResponse);
        if (json is null) return [];

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!TryGetProperty(doc.RootElement, "matches", out var matchesElement) ||
                matchesElement.ValueKind != JsonValueKind.Array)
                return [];

            var candidateMap = candidates.ToDictionary(c => c.EmployeeId);
            var results = new List<AIMatchedEmployeeDto>();

            foreach (var item in matchesElement.EnumerateArray())
            {
                if (!TryGetEmployeeId(item, out var employeeId))
                    continue;

                if (!candidateMap.TryGetValue(employeeId, out var candidate))
                    continue;

                var reason = TryGetProperty(item, "reason", out var reasonElement)
                    ? reasonElement.GetString()?.Trim() ?? string.Empty
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(reason))
                    reason = $"{candidate.Name} is a strong match based on skills and availability.";

                results.Add(new AIMatchedEmployeeDto
                {
                    EmployeeId = candidate.EmployeeId,
                    Name = candidate.Name,
                    SkillsMatch = string.IsNullOrWhiteSpace(candidate.SkillsWithProficiency)
                        ? candidate.Skills
                        : candidate.SkillsWithProficiency,
                    AvailabilityPercentage = candidate.AvailabilityPercent,
                    RecentActivity = candidate.RecentActivity,
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

    private static bool TryGetEmployeeId(JsonElement item, out int employeeId)
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
            default:
                return false;
        }
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
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

    public static List<AIMatchedEmployeeDto> ValidateSkillMatches(
        IReadOnlyList<AIMatchedEmployeeDto> matches,
        string requirement,
        IReadOnlyList<SkillMatchCandidateDto> candidates)
    {
        var candidateMap = candidates.ToDictionary(c => c.EmployeeId);

        return matches
            .Where(m => candidateMap.TryGetValue(m.EmployeeId, out var candidate) &&
                        SkillRequirementMatcher.MeetsRequirement(requirement, candidate))
            .ToList();
    }

    public static string ParseRiskSummaryResponse(string llmResponse) =>
        llmResponse.Trim().Trim('"');

    private static string? ExtractJson(string text)
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
}

internal static class AiFallbackMatcher
{
    public static AISkillMatchResultDto BuildSkillMatch(
        IReadOnlyList<SkillMatchCandidateDto> candidates,
        string requirement,
        string? fallbackReason = null)
    {
        var matches = candidates
            .Where(c => SkillRequirementMatcher.MeetsRequirement(requirement, c))
            .Select(c => new
            {
                Candidate = c,
                Score = SkillRequirementMatcher.ScoreProfile(
                    requirement, c.Skills, c.Department, c.RecentActivity)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Candidate.AvailabilityPercent)
            .Take(5)
            .Select(x => new AIMatchedEmployeeDto
            {
                EmployeeId = x.Candidate.EmployeeId,
                Name = x.Candidate.Name,
                SkillsMatch = string.IsNullOrWhiteSpace(x.Candidate.SkillsWithProficiency)
                    ? x.Candidate.Skills
                    : x.Candidate.SkillsWithProficiency,
                AvailabilityPercentage = x.Candidate.AvailabilityPercent,
                RecentActivity = x.Candidate.RecentActivity,
                Reason =
                    $"{x.Candidate.Name} matches based on skills/activity " +
                    $"({(string.IsNullOrWhiteSpace(x.Candidate.SkillsWithProficiency) ? x.Candidate.Skills : x.Candidate.SkillsWithProficiency)}) " +
                    $"with {x.Candidate.AvailabilityPercent}% free capacity."
            })
            .ToList();

        return new AISkillMatchResultDto
        {
            Matches = matches,
            UsedFallback = true,
            FallbackReason = fallbackReason
        };
    }

    public static AIRiskSummaryResultDto BuildRiskSummary(RiskSummaryContext context)
    {
        var overdue = context.Milestones.Count(m => m.IsOverdue);
        var summary =
            $"Project \"{context.ProjectName}\" is currently marked as {context.HealthStatus}. " +
            $"{context.Milestones.Count} milestone(s) tracked, {overdue} overdue. " +
            "This is a rule-based summary — add an LLM API key in System Configuration for AI-generated analysis. " +
            (overdue > 0
                ? "Attention: overdue milestones may put delivery at risk."
                : "No overdue milestones detected at this time.");

        return new AIRiskSummaryResultDto { Summary = summary };
    }
}

internal static class AiRequirementParser
{
    public static int? TryParseWeeklyHours(string requirement)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            requirement,
            @"(\d+)\s*(hrs?|hours?)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success && int.TryParse(match.Groups[1].Value, out var hours) ? hours : null;
    }
}
