using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IManagerService
{
    Task<ResourceDashboardDto> GetResourceDashboardAsync(int managerUserId);
    Task<EmployeeDetailDto?> GetEmployeeDetailAsync(int managerUserId, int employeeId);
    Task<AISkillMatchResultDto> GetAISkillMatchAsync(AISkillMatchRequestDto request, int managerUserId);
    Task<AIRiskSummaryResultDto> GetAIRiskSummaryAsync(AIRiskSummaryRequestDto request);
    Task<ProjectDetailDto?> GetProjectDetailAsync(int managerId, int projectId);
    Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int managerId);
}
