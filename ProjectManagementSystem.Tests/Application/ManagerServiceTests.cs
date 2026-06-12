using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Tests.Application;

public class ManagerServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<ISkillRepository> _skillRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<ISystemConfigRepository> _configRepo = new();
    private readonly Mock<ITimesheetReminderRepository> _reminderRepo = new();
    private readonly Mock<IAiService> _aiService = new();
    private readonly ManagerService _sut;

    public ManagerServiceTests()
    {
        _sut = new ManagerService(
            _employeeRepo.Object,
            _allocationRepo.Object,
            _projectRepo.Object,
            _skillRepo.Object,
            _timesheetRepo.Object,
            _configRepo.Object,
            _reminderRepo.Object,
            _aiService.Object);
    }

    [Fact]
    public async Task GetEmployeeDetailAsync_ReturnsNullWhenNotOnManagerTeam()
    {
        _employeeRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new EmployeeDto { Id = 5, IsActive = true });
        _employeeRepo.Setup(r => r.IsOnManagerTeamAsync(10, 5)).ReturnsAsync(false);

        var result = await _sut.GetEmployeeDetailAsync(10, 5);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetResourceDashboardAsync_CountsBenchEmployees()
    {
        _employeeRepo.Setup(r => r.GetTeamAllocatableResourcesAsync(1)).ReturnsAsync(new[]
        {
            new EmployeeDto { Id = 1, FullName = "Bench User", Department = "IT" }
        });
        _allocationRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(Array.Empty<AllocationDto>());
        _skillRepo.Setup(r => r.GetSkillsByEmployeeAsync(1)).ReturnsAsync(Array.Empty<EmployeeSkillDto>());

        var result = await _sut.GetResourceDashboardAsync(1);

        Assert.Equal(1, result.BenchCount);
    }

    [Fact]
    public async Task GetMyProjectsAsync_ReturnsOnlyManagerProjects()
    {
        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync(new Core.DTOs.Config.SystemConfigDto { MaxWeeklyHours = 40 });
        _timesheetRepo.Setup(r => r.GetByWeekStartAsync(It.IsAny<DateTime>())).ReturnsAsync(Array.Empty<Core.DTOs.Timesheet.TimesheetDto>());
        _projectRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new ProjectDto { Id = 1, ManagerId = 7, Name = "Mine" },
            new ProjectDto { Id = 2, ManagerId = 8, Name = "Other" }
        });
        _projectRepo.Setup(r => r.GetMilestonesAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<MilestoneDto>());
        _allocationRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<AllocationDto>());

        var result = await _sut.GetMyProjectsAsync(7);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetProjectDetailAsync_ReturnsNullWhenManagerDoesNotOwnProject()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new ProjectDto { Id = 3, ManagerId = 99 });

        var result = await _sut.GetProjectDetailAsync(7, 3);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAISkillMatchAsync_DelegatesToAiService()
    {
        var expected = new AISkillMatchResultDto { Matches = [] };
        _aiService.Setup(s => s.GetSkillMatchAsync(It.IsAny<AISkillMatchRequestDto>())).ReturnsAsync(expected);

        var result = await _sut.GetAISkillMatchAsync(new AISkillMatchRequestDto { Requirement = "java" });

        Assert.Same(expected, result);
    }
}
