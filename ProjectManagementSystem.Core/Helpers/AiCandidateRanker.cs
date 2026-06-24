using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Manager;

namespace ProjectManagementSystem.Core.Helpers;

public static class AiCandidateRanker
{
    public static IOrderedEnumerable<T> OrderByMatchQuality<T>(
        IEnumerable<T> source,
        string requirement,
        Func<T, string> skillsWithProficiency,
        Func<T, string> designation,
        Func<T, decimal> availabilityPercent,
        Func<T, string> name)
    {
        return source
            .OrderByDescending(x =>
                SkillRequirementMatcher.GetBestMatchingProficiencyRank(
                    requirement, skillsWithProficiency(x)))
            .ThenByDescending(x => DesignationRanker.GetRank(designation(x)))
            .ThenByDescending(x => availabilityPercent(x))
            .ThenBy(x => name(x), StringComparer.OrdinalIgnoreCase);
    }

    public static List<AIMatchedEmployeeDto> SortSkillMatches(
        IReadOnlyList<AIMatchedEmployeeDto> matches,
        string requirement,
        IReadOnlyDictionary<int, SkillMatchCandidateDto> candidateMap)
    {
        return OrderByMatchQuality(
                matches,
                requirement,
                m => ResolveSkills(m, candidateMap),
                m => ResolveDesignation(m, candidateMap),
                m => m.AvailabilityPercentage,
                m => m.Name)
            .Select(m => EnrichMatch(m, candidateMap))
            .ToList();
    }

    public static List<AIMatchedEmployeeDto> CompleteSkillMatches(
        IReadOnlyList<AIMatchedEmployeeDto> aiMatches,
        string requirement,
        IReadOnlyList<SkillMatchCandidateDto> candidates,
        int maxMatches = 5)
    {
        var aiByEmployee = aiMatches.ToDictionary(m => m.EmployeeId);

        var qualifying = candidates
            .Where(c => SkillRequirementMatcher.MeetsRequirement(requirement, c))
            .Where(c => SkillRequirementMatcher.ScoreProfile(
                requirement, c.Skills, c.Department, c.RecentActivity) > 0);

        return OrderByMatchQuality(
                qualifying,
                requirement,
                c => string.IsNullOrWhiteSpace(c.SkillsWithProficiency) ? c.Skills : c.SkillsWithProficiency,
                c => c.Designation,
                c => c.AvailabilityPercent,
                c => c.Name)
            .Take(maxMatches)
            .Select(c => ToMatchDto(c, aiByEmployee))
            .ToList();
    }

    private static AIMatchedEmployeeDto ToMatchDto(
        SkillMatchCandidateDto c,
        IReadOnlyDictionary<int, AIMatchedEmployeeDto> aiByEmployee)
    {
        if (aiByEmployee.TryGetValue(c.EmployeeId, out var aiMatch))
        {
            return aiMatch with
            {
                Designation = c.Designation,
                SkillsMatch = string.IsNullOrWhiteSpace(aiMatch.SkillsMatch)
                    ? (string.IsNullOrWhiteSpace(c.SkillsWithProficiency) ? c.Skills : c.SkillsWithProficiency)
                    : aiMatch.SkillsMatch,
                AvailabilityPercentage = c.AvailabilityPercent,
                RecentActivity = c.RecentActivity
            };
        }

        var skills = string.IsNullOrWhiteSpace(c.SkillsWithProficiency) ? c.Skills : c.SkillsWithProficiency;
        return new AIMatchedEmployeeDto
        {
            EmployeeId = c.EmployeeId,
            Name = c.Name,
            Designation = c.Designation,
            SkillsMatch = skills,
            AvailabilityPercentage = c.AvailabilityPercent,
            RecentActivity = c.RecentActivity,
            Reason =
                $"{c.Name} matches based on skills/activity ({skills}) " +
                $"with {c.AvailabilityPercent}% free capacity."
        };
    }

    private static string ResolveSkills(
        AIMatchedEmployeeDto match,
        IReadOnlyDictionary<int, SkillMatchCandidateDto> candidateMap)
    {
        if (candidateMap.TryGetValue(match.EmployeeId, out var candidate))
        {
            return string.IsNullOrWhiteSpace(candidate.SkillsWithProficiency)
                ? candidate.Skills
                : candidate.SkillsWithProficiency;
        }

        return match.SkillsMatch;
    }

    private static string ResolveDesignation(
        AIMatchedEmployeeDto match,
        IReadOnlyDictionary<int, SkillMatchCandidateDto> candidateMap) =>
        candidateMap.TryGetValue(match.EmployeeId, out var candidate)
            ? candidate.Designation
            : match.Designation;

    private static AIMatchedEmployeeDto EnrichMatch(
        AIMatchedEmployeeDto match,
        IReadOnlyDictionary<int, SkillMatchCandidateDto> candidateMap)
    {
        if (!candidateMap.TryGetValue(match.EmployeeId, out var candidate))
            return match;

        return match with
        {
            Designation = candidate.Designation,
            SkillsMatch = string.IsNullOrWhiteSpace(match.SkillsMatch)
                ? (string.IsNullOrWhiteSpace(candidate.SkillsWithProficiency)
                    ? candidate.Skills
                    : candidate.SkillsWithProficiency)
                : match.SkillsMatch
        };
    }
}
