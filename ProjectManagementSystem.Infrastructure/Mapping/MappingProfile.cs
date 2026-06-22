using AutoMapper;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Validation;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Name))
            .ForMember(d => d.ForcePasswordChange, o => o.MapFrom(s => PasswordExpiryHelper.RequiresChange(s.PasswordExpiresAt)));

        CreateMap<Resource, EmployeeDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.Designation, o => o.MapFrom(s => s.User.Designation))
            .ForMember(d => d.Department, o => o.MapFrom(_ => string.Empty))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.User.IsActive))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.UserRole, o => o.MapFrom(s => s.User.Role.Name))
            .ForMember(d => d.ManagerId, o => o.MapFrom(s => s.ReportingManagerId))
            .ForMember(d => d.ManagerName, o => o.MapFrom(s =>
                s.ReportingManager != null ? s.ReportingManager.FullName : string.Empty));

        CreateMap<UpdateEmployeeDto, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.RoleId, o => o.Ignore())
            .ForMember(d => d.Username, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.PasswordExpiresAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.Resource, o => o.Ignore())
            .ForMember(d => d.ManagedProjects, o => o.Ignore())
            .ForMember(d => d.DirectReports, o => o.Ignore());

        CreateMap<AddSkillDto, Skill>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.SkillName))
            .ForMember(d => d.Category, o => o.MapFrom(s => Enum.Parse<SkillCategory>(s.Category, true)))
            .ForMember(d => d.ResourceSkills, o => o.Ignore());

        CreateMap<AddSkillDto, ResourceSkill>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ResourceId, o => o.Ignore())
            .ForMember(d => d.SkillId, o => o.Ignore())
            .ForMember(d => d.ProficiencyLevel, o => o.MapFrom(s => Enum.Parse<ProficiencyLevel>(s.ProficiencyLevel, true)))
            .ForMember(d => d.Resource, o => o.Ignore())
            .ForMember(d => d.Skill, o => o.Ignore());

        CreateMap<UpdateSkillDto, ResourceSkill>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ResourceId, o => o.Ignore())
            .ForMember(d => d.SkillId, o => o.Ignore())
            .ForMember(d => d.ProficiencyLevel, o => o.MapFrom(s => Enum.Parse<ProficiencyLevel>(s.ProficiencyLevel, true)))
            .ForMember(d => d.Resource, o => o.Ignore())
            .ForMember(d => d.Skill, o => o.Ignore());

        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.ManagerName, o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HealthStatus, o => o.MapFrom(s => s.HealthStatus.ToString()))
            .ForMember(d => d.CompletedStoryPoints, o => o.MapFrom(s =>
                s.Milestones.Where(m => m.Status == MilestoneStatus.Done).Sum(m => m.StoryPoints)));

        CreateMap<CreateProjectDto, Project>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => Enum.Parse<ProjectStatus>(s.Status, true)))
            .ForMember(d => d.HealthStatus, o => o.MapFrom(_ => ProjectHealth.OnTrack))
            .ForMember(d => d.Manager, o => o.Ignore())
            .ForMember(d => d.Milestones, o => o.Ignore())
            .ForMember(d => d.Allocations, o => o.Ignore())
            .ForMember(d => d.TimesheetEntries, o => o.Ignore());

        CreateMap<UpdateProjectDto, Project>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.HealthStatus, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => Enum.Parse<ProjectStatus>(s.Status, true)))
            .ForMember(d => d.Manager, o => o.Ignore())
            .ForMember(d => d.Milestones, o => o.Ignore())
            .ForMember(d => d.Allocations, o => o.Ignore())
            .ForMember(d => d.TimesheetEntries, o => o.Ignore());

        CreateMap<Milestone, MilestoneDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateMilestoneDto, Milestone>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => MilestoneStatus.NotStarted))
            .ForMember(d => d.Project, o => o.Ignore());

        CreateMap<UpdateMilestoneStatusDto, Milestone>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.Title, o => o.Ignore())
            .ForMember(d => d.DueDate, o => o.Ignore())
            .ForMember(d => d.StoryPoints, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => Enum.Parse<MilestoneStatus>(s.Status, true)))
            .ForMember(d => d.Project, o => o.Ignore());

        CreateMap<Allocation, AllocationDto>()
            .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.ResourceId))
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Resource.User.FullName))
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project.Name));

        CreateMap<CreateAllocationDto, Allocation>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ResourceId, o => o.MapFrom(s => s.EmployeeId))
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Resource, o => o.Ignore())
            .ForMember(d => d.Project, o => o.Ignore());

        CreateMap<ResourceSkill, EmployeeSkillDto>()
            .ForMember(d => d.SkillName, o => o.MapFrom(s => s.Skill.Name))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Skill.Category.ToString()))
            .ForMember(d => d.ProficiencyLevel, o => o.MapFrom(s => s.ProficiencyLevel.ToString()));

        CreateMap<SystemConfig, SystemConfigDto>();
        CreateMap<SystemConfigDto, SystemConfig>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<CreateTimesheetDto, Timesheet>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ResourceId, o => o.MapFrom(s => s.EmployeeId))
            .ForMember(d => d.WeekStartDate, o => o.MapFrom(s => DateOnly.FromDateTime(s.WeekStartDate)))
            .ForMember(d => d.Status, o => o.MapFrom(_ => TimesheetStatus.Draft))
            .ForMember(d => d.TotalHours, o => o.MapFrom(s => s.Entries.Sum(e => e.Hours)))
            .ForMember(d => d.SubmittedAt, o => o.Ignore())
            .ForMember(d => d.Resource, o => o.Ignore())
            .ForMember(d => d.Entries, o => o.Ignore());

        CreateMap<CreateTimesheetEntryDto, TimesheetEntry>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TimesheetId, o => o.Ignore())
            .ForMember(d => d.Timesheet, o => o.Ignore())
            .ForMember(d => d.Project, o => o.Ignore());

        CreateMap<Timesheet, TimesheetDto>()
            .ForMember(d => d.TimesheetId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.ResourceId))
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Resource != null ? s.Resource.User.FullName : string.Empty))
            .ForMember(d => d.WeekStartDate, o => o.MapFrom(s => s.WeekStartDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(d => d.WeekEndDate, o => o.MapFrom(s => s.WeekStartDate.AddDays(6).ToDateTime(TimeOnly.MinValue)))
            .ForMember(d => d.Entries, o => o.MapFrom(s => s.Entries.Select(e => new TimesheetEntryDto
            {
                EntryId = e.Id,
                ProjectId = e.ProjectId,
                ProjectName = e.Project != null ? e.Project.Name : string.Empty,
                Date = s.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                Hours = e.Hours,
                ActivityTags = e.ActivityTags
            })));
    }
}
