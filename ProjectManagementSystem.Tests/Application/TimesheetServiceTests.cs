using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class TimesheetServiceTests
{
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly TimesheetService _sut;

    public TimesheetServiceTests()
    {
        _sut = new TimesheetService(
            _timesheetRepo.Object,
            _projectRepo.Object,
            _allocationRepo.Object,
            _employeeRepo.Object);
    }

    [Fact]
    public async Task GetTimesheetForManagerAsync_ReturnsNullWhenEmployeeNotOnTeam()
    {
        _timesheetRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new TimesheetDto { TimesheetId = 1, EmployeeId = 5 });
        _employeeRepo.Setup(r => r.IsOnManagerTeamAsync(10, 5)).ReturnsAsync(false);

        var result = await _sut.GetTimesheetForManagerAsync(10, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTimesheetAsync_ReturnsCreatedTimesheet()
    {
        var dto = new CreateTimesheetDto { EmployeeId = 1, WeekStartDate = DateTime.Today };
        var created = new TimesheetDto { TimesheetId = 8, EmployeeId = 1 };
        _timesheetRepo.Setup(r => r.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _sut.CreateTimesheetAsync(dto);

        Assert.Equal(8, result.TimesheetId);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ReturnsRepositoryResult()
    {
        _timesheetRepo.Setup(r => r.SubmitAsync(3)).ReturnsAsync(true);

        var result = await _sut.SubmitTimesheetAsync(3);

        Assert.True(result);
    }

    [Fact]
    public async Task GetTeamTimesheetsAsync_ReturnsWeekStartFromHelper()
    {
        var weekStart = new DateTime(2026, 5, 5);
        var expectedMonday = WeekDateHelper.GetMondayOfWeek(weekStart);

        _employeeRepo.Setup(r => r.GetTeamEmployeeIdsAsync(1)).ReturnsAsync(Array.Empty<int>());
        _projectRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Core.DTOs.Project.ProjectDto>());
        _timesheetRepo.Setup(r => r.GetByWeekStartAsync(expectedMonday)).ReturnsAsync(Array.Empty<TimesheetDto>());

        var result = await _sut.GetTeamTimesheetsAsync(1, weekStart);

        Assert.Equal(expectedMonday, result.WeekStart);
    }
}
