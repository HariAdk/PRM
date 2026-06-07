using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Services;

public class ProjectService(IProjectRepository projectRepo) : IProjectService
{
    public async Task<IEnumerable<ProjectDto>> GetAllAsync() =>
        await projectRepo.GetAllAsync();

    public async Task<ProjectDto> GetByIdAsync(int id) =>
        await projectRepo.GetByIdAsync(id)
        ?? throw new KeyNotFoundException($"Project {id} not found.");

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        if (!await projectRepo.ManagerExistsAsync(dto.ManagerId))
            throw new InvalidOperationException("Manager ID does not exist or is not an active Manager.");

        if (dto.EndDate <= dto.StartDate)
            throw new InvalidOperationException("End date must be after start date.");

        return await projectRepo.CreateAsync(dto);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        if (!await projectRepo.ManagerExistsAsync(dto.ManagerId))
            throw new InvalidOperationException("Manager ID does not exist or is not an active Manager.");

        return await projectRepo.UpdateAsync(id, dto);
    }

    public async Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int projectId) =>
        await projectRepo.GetMilestonesAsync(projectId);

    public async Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto)
    {
        var project = await projectRepo.GetByIdAsync(projectId)
                      ?? throw new KeyNotFoundException($"Project {projectId} not found.");
        return await projectRepo.AddMilestoneAsync(projectId, dto);
    }

    public async Task<MilestoneDto> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto) =>
        await projectRepo.UpdateMilestoneStatusAsync(projectId, milestoneId, dto);
}
