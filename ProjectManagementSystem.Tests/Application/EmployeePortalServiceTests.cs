using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Exceptions;
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
    private readonly Mock<ITimesheetReminderRepository> _reminderRepo = new();
    private readonly EmployeePortalService _sut;
    private readonly DateTime _weekMonday;

    public EmployeePortalServiceTests()
    {
        _weekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        _sut = new EmployeePortalService(
            _employeeRepo.Object,
            _allocationRepo.Object,
            _timesheetRepo.Object,
            _configRepo.Object,
            _reminderRepo.Object);
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenWeekAlreadySubmitted()
    {
        SetupEmployeeConfigAndAllocation();
        _timesheetRepo.Setup(r => r.HasSubmittedForWeekAsync(EmployeeId, _weekMonday)).ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.SubmitTimesheetAsync(UserId, ValidSubmitDto(hours: 8)));
    }

    [Fact]
    public async Task SubmitTimesheetAsync_ThrowsWhenFutureWeek()
    {
        SetupEmployeeConfigAndAllocation();
        var futureMonday = WeekDateHelper.GetMondayOfWeek(DateTime.Today).AddDays(7);
        var dto = new SubmitEmployeeTimesheetDto
        {
            WeekStartDate = futureMonday,
            Projects =
            [
                new SubmitTimesheetProjectEntryDto
                {
                    ProjectId = ProjectId,
                    Hours = 8,
                    ActivityTags = ActivityTags.All[0]
                }
            ]
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.SubmitTimesheetAsync(UserId, dto));
    }

    [Fact]
    public async Task GetReminderAsync_ReturnsNoReminderWhenNoAllocationDuringWeek()
    {
        SetupActiveEmployee();
        _allocationRepo.Setup(r => r.GetByEmployeeIdAsync(EmployeeId)).ReturnsAsync(Array.Empty<AllocationDto>());

        var result = await _sut.GetReminderAsync(UserId);

        Assert.False(result.ShowReminder);
    }

    [Fact]
    public async Task GetProfileAsync_ReturnsTotalUtilisationFromActiveAllocations()
    {
        SetupActiveEmployee();
        var today = DateOnly.FromDateTime(DateTime.Today);
        _allocationRepo.Setup(r => r.GetByEmployeeIdAsync(EmployeeId)).ReturnsAsync(new[]
        {
            new AllocationDto
            {
                EmployeeId = EmployeeId,
                ProjectId = ProjectId,
                ProjectName = "Alpha",
                UtilisationPercent = 60,
                IsActive = true,
                FromDate = today.AddDays(-7),
                ToDate = today.AddDays(7)
            },
            new AllocationDto
            {
                EmployeeId = EmployeeId,
                ProjectId = 102,
                ProjectName = "Beta",
                UtilisationPercent = 40,
                IsActive = true,
                FromDate = today.AddDays(-7),
                ToDate = today.AddDays(7)
            }
        });

        var result = await _sut.GetProfileAsync(UserId);

        Assert.Equal(100, result.TotalUtilisation);
    }

    [Fact]
    public async Task GetMyTimesheetAsync_ReturnsNullWhenTimesheetBelongsToAnotherEmployee()
    {
        SetupActiveEmployee();
        _timesheetRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new TimesheetDto { TimesheetId = 5, EmployeeId = 999 });

        var result = await _sut.GetMyTimesheetAsync(UserId, 5);

        Assert.Null(result);
    }

    private void SetupActiveEmployee()
    {
        _employeeRepo.Setup(r => r.GetByUserIdAsync(UserId)).ReturnsAsync(new EmployeeDto
        {
            Id = EmployeeId,
            UserId = UserId,
            IsActive = true,
            FullName = "Test Employee"
        });
    }

    private void SetupEmployeeConfigAndAllocation(int utilisationPercent = 50, int maxWeeklyHours = 40)
    {
        SetupActiveEmployee();

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

    private SubmitEmployeeTimesheetDto ValidSubmitDto(decimal hours) => new()
    {
        WeekStartDate = _weekMonday,
        Projects =
        [
            new SubmitTimesheetProjectEntryDto
            {
                ProjectId = ProjectId,
                Hours = hours,
                ActivityTags = ActivityTags.All[0]
            }
        ]
    };
}
