using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class ManagerService(
    IEmployeeRepository employeeRepo,
    IAllocationRepository allocationRepo,
    IProjectRepository projectRepo,
    ISkillRepository skillRepo,
    ITimesheetRepository timesheetRepo,
    ISystemConfigRepository configRepo,
    IAiService aiService) : IManagerService
{
    public async Task<ResourceDashboardDto> GetResourceDashboardAsync(int managerUserId)
    {
        var allEmployees = await employeeRepo.GetTeamAllocatableResourcesAsync(managerUserId);
        var allAllocations = (await allocationRepo.GetAllActiveAsync()).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var benchEmployees = new List<BenchEmployeeDto>();
        var activeEmployees = new List<ActiveEmployeeDto>();

        foreach (var emp in allEmployees)
        {
            var empAllocations = allAllocations
                .Where(a => a.EmployeeId == emp.Id &&
                            a.FromDate <= today &&
                            a.ToDate >= today)
                .ToList();

            var totalAllocation = empAllocations.Sum(a => a.UtilisationPercent);

            if (totalAllocation == 0)
            {
                var skills = await skillRepo.GetSkillsByEmployeeAsync(emp.Id);
                benchEmployees.Add(new BenchEmployeeDto
                {
                    EmployeeId = emp.Id,
                    Name = emp.FullName,
                    Department = emp.Department,
                    Skills = string.Join(", ", skills.Select(s => s.SkillName))
                });
            }
            else
            {
                var availability = AllocationLimits.MaxTotalUtilisationPercent - totalAllocation;
                var status = totalAllocation switch
                {
                    > AllocationLimits.MaxTotalUtilisationPercent => EmployeeAvailabilityLabels.OverAllocated,
                    AllocationLimits.MaxTotalUtilisationPercent => EmployeeAvailabilityLabels.FullyAllocated,
                    _ => EmployeeAvailabilityLabels.PartialAvailability(availability)
                };

                activeEmployees.Add(new ActiveEmployeeDto
                {
                    EmployeeId = emp.Id,
                    Name = emp.FullName,
                    AllocationPercentage = totalAllocation,
                    AvailabilityPercentage = availability,
                    AvailabilityStatus = status
                });
            }
        }

        return new ResourceDashboardDto
        {
            BenchEmployees = benchEmployees.OrderBy(e => e.Name).ToList(),
            ActiveEmployees = activeEmployees.OrderBy(e => e.Name).ToList(),
            BenchCount = benchEmployees.Count,
            OverUtilisedCount = activeEmployees.Count(e =>
                e.AllocationPercentage > AllocationLimits.MaxTotalUtilisationPercent),
            PartialCount = activeEmployees.Count(e =>
                e.AllocationPercentage is > 0 and < AllocationLimits.MaxTotalUtilisationPercent)
        };
    }

    public async Task<EmployeeDetailDto?> GetEmployeeDetailAsync(int managerUserId, int employeeId)
    {
        var employee = await employeeRepo.GetByIdAsync(employeeId);
        if (employee is null || !employee.IsActive) return null;

        if (!await employeeRepo.IsOnManagerTeamAsync(managerUserId, employeeId))
            return null;

        if (!await employeeRepo.IsAllocatableResourceAsync(employeeId))
            return null;

        var allocations = await allocationRepo.GetByEmployeeIdAsync(employeeId);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var activeAllocations = allocations
            .Where(a => a.IsActive && a.ToDate >= today)
            .Select(a => new AllocationDetailDto
            {
                ProjectName = a.ProjectName,
                Percentage = a.UtilisationPercent,
                FromDate = a.FromDate.ToDateTime(TimeOnly.MinValue),
                ToDate = a.ToDate.ToDateTime(TimeOnly.MinValue)
            })
            .ToList();

        var currentAllocation = allocations
            .Where(a => a.IsActive && a.FromDate <= today && a.ToDate >= today)
            .Sum(a => a.UtilisationPercent);

        var status = currentAllocation switch
        {
            0 => EmployeeAvailabilityLabels.Bench,
            AllocationLimits.MaxTotalUtilisationPercent => EmployeeAvailabilityLabels.AllocatedFull,
            _ => EmployeeAvailabilityLabels.AllocatedPartial(currentAllocation)
        };

        var skills = await skillRepo.GetSkillsByEmployeeAsync(employeeId);
        var activityTags = await timesheetRepo.GetRecentActivityTagsAsync(employeeId);

        return new EmployeeDetailDto
        {
            EmployeeId = employee.Id,
            Name = employee.FullName,
            Department = employee.Department,
            CurrentStatus = status,
            CurrentAllocation = currentAllocation,
            Skills = string.Join(", ", skills.Select(s => $"{s.SkillName} ({s.ProficiencyLevel})")),
            ActiveAllocations = activeAllocations,
            RecentActivityTags = activityTags.ToList()
        };
    }

    public Task<AISkillMatchResultDto> GetAISkillMatchAsync(AISkillMatchRequestDto request, int managerUserId) =>
        aiService.GetSkillMatchAsync(request, managerUserId);

    public Task<AIRiskSummaryResultDto> GetAIRiskSummaryAsync(AIRiskSummaryRequestDto request) =>
        aiService.GetRiskSummaryAsync(request);

    public async Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int managerId)
    {
        var maxWeeklyHours = await GetMaxWeeklyHoursAsync();
        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var weekTimesheets = (await timesheetRepo.GetByWeekStartAsync(lastWeekMonday)).ToList();

        var projects = (await projectRepo.GetAllAsync()).Where(p => p.ManagerId == managerId).ToList();
        var result = new List<ProjectDto>();

        foreach (var project in projects)
        {
            var milestones = (await projectRepo.GetMilestonesAsync(project.Id)).ToList();
            var allocations = (await allocationRepo.GetByProjectIdAsync(project.Id)).ToList();
            var displayHealth = ProjectHealthCalculator.ComputeDisplayHealth(
                project, milestones, allocations, weekTimesheets, maxWeeklyHours);

            result.Add(new ProjectDto
            {
                Id = project.Id,
                ManagerId = project.ManagerId,
                ManagerName = project.ManagerName,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status,
                HealthStatus = displayHealth
            });
        }

        return result;
    }

    public async Task<ProjectDetailDto?> GetProjectDetailAsync(int managerId, int projectId)
    {
        var project = await projectRepo.GetByIdAsync(projectId);
        if (project is null || project.ManagerId != managerId)
            return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var maxWeeklyHours = await GetMaxWeeklyHoursAsync();
        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var weekTimesheets = (await timesheetRepo.GetByWeekStartAsync(lastWeekMonday)).ToList();

        var milestones = (await projectRepo.GetMilestonesAsync(projectId)).ToList();
        var allocations = (await allocationRepo.GetByProjectIdAsync(projectId)).ToList();

        var milestoneDetails = milestones.Select(m =>
        {
            var isOverdue = MilestoneStatusHelper.IsOverdue(m.Status, m.DueDate, today);
            return new ProjectMilestoneDetailDto
            {
                Id = m.Id,
                Title = m.Title,
                DueDate = m.DueDate,
                Status = m.Status,
                IsOverdue = isOverdue,
                DaysOverdue = isOverdue ? today.DayNumber - m.DueDate.DayNumber : 0
            };
        }).ToList();

        var riskFlags = BuildRiskFlags(milestoneDetails, allocations, projectId, maxWeeklyHours, weekTimesheets);
        var displayHealth = ProjectHealthCalculator.ComputeDisplayHealth(
            project, milestones, allocations, weekTimesheets, maxWeeklyHours);

        return new ProjectDetailDto
        {
            Project = project,
            DisplayHealth = displayHealth,
            RiskFlags = riskFlags,
            Milestones = milestoneDetails,
            Allocations = allocations
        };
    }

    private static List<RiskFlagDto> BuildRiskFlags(
        IReadOnlyList<ProjectMilestoneDetailDto> milestoneDetails,
        IReadOnlyList<AllocationDto> allocations,
        int projectId,
        int maxWeeklyHours,
        IReadOnlyList<TimesheetDto> weekTimesheets)
    {
        var riskFlags = new List<RiskFlagDto>();

        foreach (var milestone in milestoneDetails.Where(x => x.IsOverdue))
        {
            riskFlags.Add(new RiskFlagDto
            {
                Message = $"{milestone.Title} milestone is {milestone.DaysOverdue} day(s) overdue",
                IsPositive = false
            });
        }

        foreach (var alloc in allocations.Where(a => a.IsActive))
        {
            var expected = Math.Round((decimal)alloc.UtilisationPercent * maxWeeklyHours / AllocationLimits.MaxUtilisationPercent, 0);
            if (expected <= 0) continue;

            var logged = weekTimesheets
                .Where(t => t.EmployeeId == alloc.EmployeeId)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == projectId)
                .Sum(e => e.Hours);

            if (logged < expected)
            {
                var hrs = logged == 0 ? "0" : logged.ToString("0");
                riskFlags.Add(new RiskFlagDto
                {
                    Message = $"{alloc.EmployeeName} logged only {hrs} hrs last week (expected {expected} hrs)",
                    IsPositive = false
                });
            }
        }

        if (allocations.Any(a => a.IsActive) && !riskFlags.Any(f => !f.IsPositive))
        {
            riskFlags.Add(new RiskFlagDto
            {
                Message = "Resources are correctly allocated",
                IsPositive = true
            });
        }

        return riskFlags;
    }

    private async Task<int> GetMaxWeeklyHoursAsync()
    {
        var config = await configRepo.GetAsync();
        return config?.MaxWeeklyHours ?? SystemDefaults.MaxWeeklyHours;
    }
}
