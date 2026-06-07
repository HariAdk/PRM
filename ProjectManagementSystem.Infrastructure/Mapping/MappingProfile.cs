using AutoMapper;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.UserRole, o => o.MapFrom(s => s.User.Role.ToString()))
            .ForMember(d => d.ManagerName, o => o.MapFrom(s =>
                s.ReportingManager != null ? s.ReportingManager.FullName : string.Empty));

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => Core.Enums.EmployeeStatus.Bench))
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.ManagerId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.ReportingManager, o => o.Ignore())
            .ForMember(d => d.Skills, o => o.Ignore())
            .ForMember(d => d.Allocations, o => o.Ignore())
            .ForMember(d => d.Timesheets, o => o.Ignore());

        // Project
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.ManagerName, o => o.MapFrom(s => s.Manager != null ? s.Manager.FullName : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HealthStatus, o => o.MapFrom(s => s.HealthStatus.ToString()))
            .ForMember(d => d.CompletedStoryPoints, o => o.MapFrom(s =>
                s.Milestones.Where(m => m.Status == Core.Enums.MilestoneStatus.Done).Sum(m => m.StoryPoints)));

        CreateMap<Milestone, MilestoneDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateMilestoneDto, Milestone>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => Core.Enums.MilestoneStatus.NotStarted))
            .ForMember(d => d.Project, o => o.Ignore());

        // Allocation
        CreateMap<Allocation, AllocationDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project.Name));

        CreateMap<CreateAllocationDto, Allocation>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Employee, o => o.Ignore())
            .ForMember(d => d.Project, o => o.Ignore());

        // Skill
        CreateMap<EmployeeSkill, EmployeeSkillDto>()
            .ForMember(d => d.SkillName, o => o.MapFrom(s => s.Skill.Name))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Skill.Category.ToString()))
            .ForMember(d => d.ProficiencyLevel, o => o.MapFrom(s => s.ProficiencyLevel.ToString()));

        // System config
        CreateMap<SystemConfig, SystemConfigDto>();

        // Timesheet
        CreateMap<Timesheet, TimesheetDto>()
            .ForMember(d => d.TimesheetId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : string.Empty))
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
