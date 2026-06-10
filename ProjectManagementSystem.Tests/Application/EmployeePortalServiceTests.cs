using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class EmployeePortalServiceTests
{
    private const int UserId = 1;
    private const int EmployeeId = 10;
    private const int ProjectId = 101;

    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<ISystemConfigRepository> _configRepo = new();
    private readonly EmployeePortalService _sut;
    private readonly DateTime _weekMonday;

    public EmployeePortalServiceTests()
    {
        _weekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        _sut = new EmployeePortalService(
            _employeeRepo.Object,
            _allocationRepo.Object,
            _timesheetRepo.Object,
            _configRepo.Object);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenWeekAlreadySubmitted()
    {
        SetupEmployeeConfigAndAllocation();
        _timesheetRepo
            .Setup(r => r.HasSubmittedForWeekAsync(EmployeeId, _weekMonday))
            .ReturnsAsync(true);

        var dto = ValidSubmitDto(hours: 8);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitTimesheetAsync(UserId, dto));

        Assert.Equal(ErrorMessages.TimesheetAlreadySubmitted, ex.Message);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenFutureWeek()
    {
        SetupEmployeeConfigAndAllocation();
        var futureMonday = WeekDateHelper.GetMondayOfWeek(DateTime.Today).AddDays(7);

        var dto = new SubmitEmployeeTimesheetDto
        {
            WeekStartDate = futureMonday,
            Projects = [new SubmitTimesheetProjectEntryDto
            {
                ProjectId = ProjectId,
                Hours = 8,
                ActivityTags = ActivityTags.All[0]
            }]
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitTimesheetAsync(UserId, dto));
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenNotAllocatedToProject()
    {
        SetupEmployeeConfigAndAllocation();

        var dto = ValidSubmitDto(hours: 8, projectId: 999);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitTimesheetAsync(UserId, dto));

        Assert.Contains("not allocated", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenHoursExceedProjectAllocationCap()
    {
        SetupEmployeeConfigAndAllocation(utilisationPercent: 50, maxWeeklyHours: 40);

        var dto = ValidSubmitDto(hours: 25);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitTimesheetAsync(UserId, dto));

        Assert.Contains("exceed the allowed maximum", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenTotalHoursExceedWeeklyMax()
    {
        SetupEmployeeConfigAndAllocation(utilisationPercent: 60, maxWeeklyHours: 40);
        SetupSecondProjectAllocation(projectId: 102, utilisationPercent: 60);

        var dto = new SubmitEmployeeTimesheetDto
        {
            WeekStartDate = _weekMonday,
            Projects =
            [
                new SubmitTimesheetProjectEntryDto { ProjectId = ProjectId, Hours = 22, ActivityTags = ActivityTags.All[0] },
                new SubmitTimesheetProjectEntryDto { ProjectId = 102, Hours = 20, ActivityTags = ActivityTags.All[1] }
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitTimesheetAsync(UserId, dto));

        Assert.Contains("exceed the maximum weekly limit", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenActivityTagsMissingForHours()
    {
        SetupEmployeeConfigAndAllocation();

        var dto = ValidSubmitDto(hours: 8, activityTags: "");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitTimesheetAsync(UserId, dto));

        Assert.Contains("activity tag", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private void SetupEmployeeConfigAndAllocation(int utilisationPercent = 50, int maxWeeklyHours = 40)
    {
        _employeeRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(new EmployeeDto
        {
            Id = EmployeeId,
            UserId = UserId,
            IsActive = true,
            FullName = "Test Employee"
        });

        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync(new SystemConfigDto
        {
            MaxWeeklyHours = maxWeeklyHours,
            LlmProvider = "Gemini",
            SchedulerIntervalHours = 4
        });

        _timesheetRepo.Setup(r => r.HasSubmittedForWeekAsync(EmployeeId, _weekMonday)).ReturnsAsync(false);
        _timesheetRepo.Setup(r => r.GetByEmployeeAndWeekAsync(EmployeeId, _weekMonday)).ReturnsAsync((TimesheetDto?)null);

        var weekStart = DateOnly.FromDateTime(_weekMonday);
        var weekEnd = DateOnly.FromDateTime(_weekMonday.AddDays(6));

        _allocationRepo.Setup(r => r.GetByEmployeeIdAsync(EmployeeId)).ReturnsAsync(new[]
        {
            new AllocationDto
            {
                EmployeeId = EmployeeId,
                ProjectId = ProjectId,
                ProjectName = "Alpha Portal",
                UtilisationPercent = utilisationPercent,
                IsActive = true,
                FromDate = weekStart.AddDays(-14),
                ToDate = weekEnd.AddDays(14)
            }
        });
    }

    private void SetupSecondProjectAllocation(int projectId, int utilisationPercent)
    {
        var weekStart = DateOnly.FromDateTime(_weekMonday);
        var weekEnd = DateOnly.FromDateTime(_weekMonday.AddDays(6));

        _allocationRepo.Setup(r => r.GetByEmployeeIdAsync(EmployeeId)).ReturnsAsync(new[]
        {
            new AllocationDto
            {
                EmployeeId = EmployeeId,
                ProjectId = ProjectId,
                ProjectName = "Alpha Portal",
                UtilisationPercent = 60,
                IsActive = true,
                FromDate = weekStart.AddDays(-14),
                ToDate = weekEnd.AddDays(14)
            },
            new AllocationDto
            {
                EmployeeId = EmployeeId,
                ProjectId = projectId,
                ProjectName = "Beta CRM",
                UtilisationPercent = utilisationPercent,
                IsActive = true,
                FromDate = weekStart.AddDays(-14),
                ToDate = weekEnd.AddDays(14)
            }
        });
    }

    private SubmitEmployeeTimesheetDto ValidSubmitDto(
        decimal hours,
        int projectId = ProjectId,
        string? activityTags = null) => new()
    {
        WeekStartDate = _weekMonday,
        Projects =
        [
            new SubmitTimesheetProjectEntryDto
            {
                ProjectId = projectId,
                Hours = hours,
                ActivityTags = activityTags ?? ActivityTags.All[0]
            }
        ]
    };
}
