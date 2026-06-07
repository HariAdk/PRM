using ProjectManagementSystem.Core.DTOs.Manager;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IAiService
{
    Task<AISkillMatchResultDto> GetSkillMatchAsync(AISkillMatchRequestDto request, int managerUserId);

    Task<AIRiskSummaryResultDto> GetRiskSummaryAsync(AIRiskSummaryRequestDto request);
}
