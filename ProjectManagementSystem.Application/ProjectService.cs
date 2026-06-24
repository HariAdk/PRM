using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class ProjectService(IProjectRepository projectRepo) : IProjectService
{
    public async Task<IEnumerable<ProjectDto>> GetAllAsync() =>
        await projectRepo.GetAllAsync();

    public async Task<ProjectDto> GetByIdAsync(int id) =>
        await projectRepo.GetByIdAsync(id)
        ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(id));

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        if (!await projectRepo.ManagerExistsAsync(dto.ManagerId))
            throw new BusinessRuleException(ErrorMessages.InvalidManagerId);

        if (dto.EndDate <= dto.StartDate)
            throw new BusinessRuleException(ErrorMessages.ProjectEndBeforeStart);

        return await projectRepo.CreateAsync(dto);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        if (!await projectRepo.ManagerExistsAsync(dto.ManagerId))
            throw new BusinessRuleException(ErrorMessages.InvalidManagerId);

        return await projectRepo.UpdateAsync(id, dto);
    }

    public async Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int projectId) =>
        await projectRepo.GetMilestonesAsync(projectId);

    public async Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto)
    {
        _ = await projectRepo.GetByIdAsync(projectId)
            ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(projectId));

        return await projectRepo.AddMilestoneAsync(projectId, dto);
    }

    public async Task<MilestoneDto> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto) =>
        await projectRepo.UpdateMilestoneStatusAsync(projectId, milestoneId, dto);
}
