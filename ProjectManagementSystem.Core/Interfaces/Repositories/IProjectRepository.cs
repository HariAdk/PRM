using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<IEnumerable<ProjectDto>> GetActiveAsync();
    Task UpdateHealthStatusAsync(int projectId, ProjectHealth health);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto);
    Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int projectId);
    Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto);
    Task<MilestoneDto> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto);
    Task<bool> ManagerExistsAsync(int managerId);
}
