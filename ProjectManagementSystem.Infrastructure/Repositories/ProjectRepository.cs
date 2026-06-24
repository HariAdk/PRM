using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Notification;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class ProjectRepository(AppDbContext db, IMapper mapper) : IProjectRepository
{
    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var p = await db.Projects.Include(x => x.Manager).Include(x => x.Milestones).FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : mapper.Map<ProjectDto>(p);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync()
    {
        var list = await db.Projects.Include(p => p.Manager).Include(p => p.Milestones).OrderBy(p => p.Name).ToListAsync();
        return mapper.Map<IEnumerable<ProjectDto>>(list);
    }

    public async Task<IEnumerable<ProjectDto>> GetActiveAsync()
    {
        var list = await db.Projects
            .Include(p => p.Manager)
            .Where(p => p.Status == ProjectStatus.Active)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return mapper.Map<IEnumerable<ProjectDto>>(list);
    }

    public async Task UpdateHealthStatusAsync(int projectId, ProjectHealth health)
    {
        var project = await db.Projects.FindAsync(projectId)
                      ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(projectId));
        project.HealthStatus = health;
        if (health != ProjectHealth.AtRisk)
            project.AtRiskNotifiedAt = null;
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AtRiskProjectEmailDto>> GetAtRiskProjectsPendingNotificationAsync()
    {
        var projects = await db.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .Where(p => p.Status == ProjectStatus.Active &&
                        p.HealthStatus == ProjectHealth.AtRisk &&
                        p.AtRiskNotifiedAt == null)
            .ToListAsync();

        return projects.Select(p => new AtRiskProjectEmailDto
        {
            ProjectId = p.Id,
            ProjectName = p.Name,
            ManagerName = p.Manager.FullName,
            ManagerEmail = p.Manager.Email,
            HealthStatus = nameof(ProjectHealth.AtRisk),
            MilestoneSummary = string.Join("; ",
                p.Milestones
                    .OrderBy(m => m.DueDate)
                    .Take(5)
                    .Select(m => $"{m.Title} ({m.Status}, due {m.DueDate:dd-MMM-yyyy})"))
        }).ToList();
    }

    public async Task MarkAtRiskNotifiedAsync(int projectId)
    {
        var project = await db.Projects.FindAsync(projectId)
                      ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(projectId));
        project.AtRiskNotifiedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        var project = mapper.Map<Project>(dto);
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        await db.Entry(project).Reference(p => p.Manager).LoadAsync();
        return mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await db.Projects.Include(p => p.Manager).FirstOrDefaultAsync(p => p.Id == id)
                      ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(id));
        mapper.Map(dto, project);
        await db.SaveChangesAsync();
        await db.Entry(project).Reference(p => p.Manager).LoadAsync();
        return mapper.Map<ProjectDto>(project);
    }

    public async Task<IEnumerable<MilestoneDto>> GetMilestonesAsync(int projectId)
    {
        var milestones = await db.Milestones
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.DueDate)
            .ToListAsync();
        return mapper.Map<IEnumerable<MilestoneDto>>(milestones);
    }

    public async Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto)
    {
        var milestone = mapper.Map<Milestone>(dto);
        milestone.ProjectId = projectId;
        db.Milestones.Add(milestone);
        await db.SaveChangesAsync();
        return mapper.Map<MilestoneDto>(milestone);
    }

    public async Task<MilestoneDto> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto)
    {
        var milestone = await db.Milestones
            .FirstOrDefaultAsync(m => m.Id == milestoneId && m.ProjectId == projectId)
            ?? throw new NotFoundException(ErrorMessages.MilestoneNotFound(milestoneId));
        mapper.Map(dto, milestone);
        await db.SaveChangesAsync();
        return mapper.Map<MilestoneDto>(milestone);
    }

    public async Task<bool> ManagerExistsAsync(int managerId) =>
        await db.Users
            .Include(u => u.Role)
            .AnyAsync(u => u.Id == managerId && u.Role.Name == RoleNames.Manager && u.IsActive);
}
