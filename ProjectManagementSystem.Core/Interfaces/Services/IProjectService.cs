using ProjectManagementSystem.Core.DTOs.Project;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<ProjectDto> GetByIdAsync(int id);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto);
    Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int projectId);
    Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto);
    Task<MilestoneDto> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto);
}
