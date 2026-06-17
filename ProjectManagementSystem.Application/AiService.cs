using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.AI;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Application.AI;

namespace ProjectManagementSystem.Application;

public class AiService(
    IEmployeeRepository employeeRepo,
    IAllocationRepository allocationRepo,
    IProjectRepository projectRepo,
    ISkillRepository skillRepo,
    ITimesheetRepository timesheetRepo,
    ISystemConfigRepository configRepo,
    IAiProviderFactory providerFactory,
    ILogger<AiService> logger) : IAiService
{
    public async Task<AISkillMatchResultDto> GetSkillMatchAsync(AISkillMatchRequestDto request)
    {
        var maxWeeklyHours = await GetMaxWeeklyHoursAsync();
        var candidates = await BuildSkillMatchCandidatesAsync(request.Requirement, maxWeeklyHours);

        if (candidates.Count == 0)
            return new AISkillMatchResultDto { Matches = [] };

        var config = await configRepo.GetAsync();
        if (!providerFactory.IsConfigured(config))
        {
            logger.LogWarning(
                "LLM not configured for skill match (provider={Provider}, hasApiKey={HasKey})",
                config?.LlmProvider ?? "(none)",
                !string.IsNullOrWhiteSpace(config?.LlmApiKey));
            return AiFallbackMatcher.BuildSkillMatch(
                candidates,
                request.Requirement,
                "LLM is not configured. In Admin → System Configuration set provider to Ollama (and add the apikey in LLM API Key), or add an API key for Gemini/Groq.");
        }

        try
        {
            var provider = providerFactory.Create(config!);
            var userPrompt = AiPromptBuilder.BuildSkillMatchUserPrompt(
                request.Requirement, candidates);

            var response = await provider.CompleteAsync(
                AiPromptBuilder.SkillMatchSystemPrompt,
                userPrompt,
                jsonResponse: true);

            var parsed = AiResponseParser.ParseSkillMatchResponse(response, candidates);
            var validated = AiResponseParser.ValidateSkillMatches(parsed, request.Requirement, candidates);
            if (validated.Count > 0)
            {
                var sorted = AiResponseParser.SortSkillMatches(validated, request.Requirement, candidates);
                logger.LogInformation(
                    "Skill match completed via {Provider} with {MatchCount} result(s)",
                    provider.ProviderName, sorted.Count);
                return new AISkillMatchResultDto { Matches = sorted, UsedFallback = false };
            }

            if (parsed.Count > 0)
            {
                logger.LogWarning(
                    "LLM skill-match returned {Count} result(s) that failed requirement validation; using fallback. Response snippet: {Snippet}",
                    parsed.Count, Truncate(response, 300));
                return AiFallbackMatcher.BuildSkillMatch(
                    candidates,
                    request.Requirement,
                    "AI returned matches that could not be validated. Showing rule-based matches instead.");
            }

            logger.LogWarning(
                "LLM skill-match response could not be parsed; using fallback. Response snippet: {Snippet}",
                Truncate(response, 300));
            return AiFallbackMatcher.BuildSkillMatch(
                candidates,
                request.Requirement,
                "AI response could not be parsed as JSON. Showing rule-based matches instead.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM skill-match failed; using rule-based fallback.");
            return AiFallbackMatcher.BuildSkillMatch(
                candidates,
                request.Requirement,
                $"AI request failed ({ex.Message}). Showing rule-based matches instead.");
        }
    }

    public async Task<AITeamBuildResultDto> GetTeamBuildAsync(AITeamBuildRequestDto request)
    {
        var candidates = await BuildBenchTeamCandidatesAsync();

        if (candidates.Count == 0)
        {
            return new AITeamBuildResultDto
            {
                Roles = TeamRequirementParser.ExtractRoles(request.Requirement)
                    .Select(role => new TeamBuildRoleSuggestionDto
                    {
                        Role = role,
                        Status = "Not Found",
                        Reason = "No bench employees are available in the organization."
                    })
                    .ToList(),
                UsedFallback = true,
                FallbackReason = "No bench candidates available."
            };
        }

        var config = await configRepo.GetAsync();
        if (!providerFactory.IsConfigured(config))
        {
            logger.LogWarning(
                "LLM not configured for team build (provider={Provider}, hasApiKey={HasKey})",
                config?.LlmProvider ?? "(none)",
                !string.IsNullOrWhiteSpace(config?.LlmApiKey));
            return TeamBuildFallbackMatcher.BuildTeamBuild(
                candidates,
                request.Requirement,
                "LLM is not configured. In Admin → System Configuration set provider to Ollama (and add the apikey in LLM API Key), or add an API key for Gemini/Groq.");
        }

        try
        {
            var provider = providerFactory.Create(config!);
            var userPrompt = TeamBuildAiPromptBuilder.BuildTeamBuildUserPrompt(
                request.Requirement, candidates);

            var response = await provider.CompleteAsync(
                TeamBuildAiPromptBuilder.TeamBuildSystemPrompt,
                userPrompt,
                jsonResponse: true);

            var parsed = TeamBuildAiResponseParser.ParseTeamBuildResponse(response, candidates);
            if (parsed.Count > 0)
            {
                logger.LogInformation(
                    "Team build completed via {Provider} with {RoleCount} role(s)",
                    provider.ProviderName, parsed.Count);
                return new AITeamBuildResultDto { Roles = parsed, UsedFallback = false };
            }

            logger.LogWarning(
                "LLM team-build response could not be parsed; using fallback. Response snippet: {Snippet}",
                Truncate(response, 300));
            return TeamBuildFallbackMatcher.BuildTeamBuild(
                candidates,
                request.Requirement,
                "AI response could not be parsed as JSON. Showing rule-based matches instead.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LLM team-build failed; using rule-based fallback.");
            return TeamBuildFallbackMatcher.BuildTeamBuild(
                candidates,
                request.Requirement,
                $"AI request failed ({ex.Message}). Showing rule-based matches instead.");
        }
    }

    public async Task<AIRiskSummaryResultDto> GetRiskSummaryAsync(AIRiskSummaryRequestDto request)
    {
        var project = await projectRepo.GetByIdAsync(request.ProjectId);
        if (project is null)
            return new AIRiskSummaryResultDto { Summary = "Project not found." };

        var context = await BuildRiskSummaryContextAsync(request.ProjectId, project);

        var config = await configRepo.GetAsync();
        if (providerFactory.IsConfigured(config))
        {
            try
            {
                var provider = providerFactory.Create(config!);
                var userPrompt = AiPromptBuilder.BuildRiskSummaryUserPrompt(context);

                var response = await provider.CompleteAsync(
                    AiPromptBuilder.RiskSummarySystemPrompt,
                    userPrompt);

                var summary = AiResponseParser.ParseRiskSummaryResponse(response);
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    logger.LogInformation(
                        "Risk summary completed via {Provider} for project {ProjectId}",
                        provider.ProviderName, request.ProjectId);
                    return new AIRiskSummaryResultDto { Summary = summary };
                }

                logger.LogWarning("LLM risk-summary returned empty text; using fallback.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "LLM risk-summary failed; using rule-based fallback.");
            }
        }

        return AiFallbackMatcher.BuildRiskSummary(context);
    }

    private async Task<List<TeamBuildCandidateDto>> BuildBenchTeamCandidatesAsync()
    {
        var benchEmployees = (await employeeRepo.GetAllocatableResourcesAsync())
            .Where(e => e.Status.Equals(nameof(EmployeeStatus.Bench), StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName)
            .ToList();

        var allAllocations = (await allocationRepo.GetAllActiveAsync()).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var candidates = new List<TeamBuildCandidateDto>();
        foreach (var emp in benchEmployees)
        {
            var allocated = allAllocations
                .Where(a => a.EmployeeId == emp.Id &&
                            a.FromDate <= today &&
                            a.ToDate >= today)
                .Sum(a => a.UtilisationPercent);
            var availability = AllocationLimits.MaxTotalUtilisationPercent - allocated;
            if (availability <= 0)
                continue;

            var skills = (await skillRepo.GetSkillsByEmployeeAsync(emp.Id)).ToList();
            var skillsWithProficiency = skills.Count == 0
                ? string.Empty
                : string.Join(", ", skills.Select(s => $"{s.SkillName} ({s.ProficiencyLevel})"));
            var activityTags = await timesheetRepo.GetRecentActivityTagsAsync(emp.Id, 3);

            candidates.Add(new TeamBuildCandidateDto
            {
                EmployeeId = emp.Id,
                Name = emp.FullName,
                Department = emp.Department,
                Designation = emp.Designation,
                AvailabilityPercent = availability,
                SkillsWithProficiency = skillsWithProficiency,
                RecentActivity = string.Join(", ", activityTags)
            });
        }

        return candidates;
    }

    private async Task<List<SkillMatchCandidateDto>> BuildSkillMatchCandidatesAsync(
        string requirement,
        int maxWeeklyHours)
    {
        var requiredHours = AiRequirementParser.TryParseWeeklyHours(requirement);
        var availabilityConstraint = SkillRequirementMatcher.ParseAvailabilityConstraint(requirement);
        var skillFilterRequired = SkillRequirementMatcher.HasSkillConstraints(requirement);
        var proficiencyRequired = SkillRequirementMatcher.TryParseProficiencyLevel(requirement).HasValue;
        var allEmployees = await employeeRepo.GetAllocatableResourcesAsync();
        var allAllocations = (await allocationRepo.GetAllActiveAsync()).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var candidates = new List<SkillMatchCandidateDto>();

        foreach (var emp in allEmployees)
        {
            var allocated = allAllocations
                .Where(a => a.EmployeeId == emp.Id &&
                            a.FromDate <= today &&
                            a.ToDate >= today)
                .Sum(a => a.UtilisationPercent);

            var availability = AllocationLimits.MaxTotalUtilisationPercent - allocated;
            if (availability <= 0) continue;

            if (!SkillRequirementMatcher.MeetsAvailabilityConstraint(availabilityConstraint, availability))
                continue;

            var freeHours = maxWeeklyHours * availability / AllocationLimits.MaxTotalUtilisationPercent;
            if (requiredHours.HasValue && freeHours < requiredHours.Value)
                continue;

            var skills = (await skillRepo.GetSkillsByEmployeeAsync(emp.Id)).ToList();
            var activityTags = await timesheetRepo.GetRecentActivityTagsAsync(emp.Id, 3);
            var skillsCsv = string.Join(", ", skills.Select(s => s.SkillName).Take(5));
            var skillsWithProficiency = skills.Count == 0
                ? string.Empty
                : string.Join(", ", skills.Select(s => $"{s.SkillName} ({s.ProficiencyLevel})"));
            var activityCsv = string.Join(", ", activityTags.Take(3));

            if (skillFilterRequired &&
                !SkillRequirementMatcher.MeetsSkillKeywords(
                    requirement, skillsWithProficiency, emp.Department, activityCsv))
                continue;

            if (proficiencyRequired &&
                !SkillRequirementMatcher.MeetsProficiency(requirement, skillsWithProficiency))
                continue;

            candidates.Add(new SkillMatchCandidateDto
            {
                EmployeeId = emp.Id,
                Name = emp.FullName,
                Designation = emp.Designation,
                Department = emp.Department,
                Skills = skillsCsv,
                SkillsWithProficiency = skillsWithProficiency,
                AvailabilityPercent = availability,
                FreeHoursPerWeek = freeHours,
                RecentActivity = activityCsv
            });
        }

        return candidates;
    }

    private async Task<RiskSummaryContext> BuildRiskSummaryContextAsync(
        int projectId,
        Core.DTOs.Project.ProjectDto project)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var maxWeeklyHours = await GetMaxWeeklyHoursAsync();
        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var weekTimesheets = (await timesheetRepo.GetByWeekStartAsync(lastWeekMonday)).ToList();

        var milestones = (await projectRepo.GetMilestonesAsync(projectId)).ToList();
        var allocations = (await allocationRepo.GetByProjectIdAsync(projectId)).ToList();

        var milestoneDetails = milestones.Select(m =>
        {
            var isOverdue = MilestoneStatusHelper.IsOverdue(m.Status, m.DueDate, today);
            return new RiskSummaryMilestone
            {
                Title = m.Title,
                DueDate = m.DueDate,
                Status = m.Status,
                IsOverdue = isOverdue,
                DaysOverdue = isOverdue ? today.DayNumber - m.DueDate.DayNumber : 0
            };
        }).ToList();

        var allocationDetails = allocations
            .Where(a => a.IsActive)
            .Select(a =>
            {
                var expected = Math.Round(
                    (decimal)a.UtilisationPercent * maxWeeklyHours / AllocationLimits.MaxUtilisationPercent, 0);
                var logged = weekTimesheets
                    .Where(t => t.EmployeeId == a.EmployeeId)
                    .SelectMany(t => t.Entries)
                    .Where(e => e.ProjectId == projectId)
                    .Sum(e => e.Hours);

                return new RiskSummaryAllocation
                {
                    EmployeeName = a.EmployeeName,
                    UtilisationPercent = a.UtilisationPercent,
                    LoggedHoursLastWeek = logged,
                    ExpectedHoursLastWeek = expected
                };
            })
            .ToList();

        var riskFlags = BuildRiskFlags(milestoneDetails, allocationDetails);

        var displayHealth = ProjectHealthCalculator.ComputeDisplayHealth(
            project, milestones, allocations, weekTimesheets, maxWeeklyHours);

        return new RiskSummaryContext
        {
            ProjectName = project.Name,
            HealthStatus = displayHealth,
            EndDate = project.EndDate,
            Milestones = milestoneDetails,
            Allocations = allocationDetails,
            RiskFlags = riskFlags
        };
    }

    private static List<RiskFlagDto> BuildRiskFlags(
        IReadOnlyList<RiskSummaryMilestone> milestones,
        IReadOnlyList<RiskSummaryAllocation> allocations)
    {
        var flags = new List<RiskFlagDto>();

        foreach (var milestone in milestones.Where(m => m.IsOverdue))
        {
            flags.Add(new RiskFlagDto
            {
                Message = $"{milestone.Title} milestone is {milestone.DaysOverdue} day(s) overdue",
                IsPositive = false
            });
        }

        foreach (var alloc in allocations)
        {
            if (alloc.ExpectedHoursLastWeek <= 0) continue;

            if (alloc.LoggedHoursLastWeek < alloc.ExpectedHoursLastWeek)
            {
                var hrs = alloc.LoggedHoursLastWeek == 0 ? "0" : alloc.LoggedHoursLastWeek.ToString("0");
                flags.Add(new RiskFlagDto
                {
                    Message = $"{alloc.EmployeeName} logged only {hrs} hrs last week (expected {alloc.ExpectedHoursLastWeek:0} hrs)",
                    IsPositive = false
                });
            }
        }

        if (allocations.Count > 0 && !flags.Any(f => !f.IsPositive))
        {
            flags.Add(new RiskFlagDto
            {
                Message = "Resources are correctly allocated",
                IsPositive = true
            });
        }

        return flags;
    }

    private async Task<int> GetMaxWeeklyHoursAsync()
    {
        var config = await configRepo.GetAsync();
        return config?.MaxWeeklyHours ?? SystemDefaults.MaxWeeklyHours;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
