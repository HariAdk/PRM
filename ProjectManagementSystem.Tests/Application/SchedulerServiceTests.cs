using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Tests.Application;

public class SchedulerServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly SchedulerService _sut;

    public SchedulerServiceTests()
    {
        _notificationService
            .Setup(n => n.ProcessTimesheetEscalationsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _notificationService
            .Setup(n => n.ProcessAtRiskProjectNotificationsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new SchedulerService(
            _employeeRepo.Object,
            _allocationRepo.Object,
            _projectRepo.Object,
            _timesheetRepo.Object,
            _notificationService.Object,
            Mock.Of<ILogger<SchedulerService>>());
    }

    [Fact]
    public async Task RunScheduledTasksAsync_SetsBenchStatusWhenEmployeeHasNoAllocation()
    {
        EmployeeStatus? capturedStatus = null;
        _employeeRepo.Setup(r => r.GetAllocatableResourcesAsync()).ReturnsAsync(new[]
        {
            new EmployeeDto { Id = 1, Status = nameof(EmployeeStatus.Allocated) }
        });
        _allocationRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(Array.Empty<AllocationDto>());
        _projectRepo.Setup(r => r.GetActiveAsync()).ReturnsAsync(Array.Empty<ProjectDto>());
        _allocationRepo.Setup(r => r.GetEmployeeIdsAllocatedBetweenAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(Array.Empty<int>());
        _employeeRepo.Setup(r => r.SetStatusAsync(1, It.IsAny<EmployeeStatus>()))
            .Callback<int, EmployeeStatus>((_, status) => capturedStatus = status)
            .Returns(Task.CompletedTask);

        await _sut.RunScheduledTasksAsync();

        Assert.Equal(EmployeeStatus.Bench, capturedStatus);
    }

    [Fact]
    public async Task RunScheduledTasksAsync_CreatesMissedTimesheetWhenSubmissionMissing()
    {
        var lastWeekMonday = WeekDateHelper.GetLastCompletedWeekMonday(DateTime.Today);
        var employeeId = 5;

        _employeeRepo.Setup(r => r.GetAllocatableResourcesAsync()).ReturnsAsync(Array.Empty<EmployeeDto>());
        _allocationRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(Array.Empty<AllocationDto>());
        _projectRepo.Setup(r => r.GetActiveAsync()).ReturnsAsync(Array.Empty<ProjectDto>());
        _allocationRepo.Setup(r => r.GetEmployeeIdsAllocatedBetweenAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new[] { employeeId });
        _timesheetRepo.Setup(r => r.ExistsForEmployeeWeekAsync(employeeId, lastWeekMonday)).ReturnsAsync(false);

        var missedCreated = false;
        _timesheetRepo.Setup(r => r.CreateMissedAsync(employeeId, lastWeekMonday))
            .Callback(() => missedCreated = true)
            .Returns(Task.CompletedTask);

        await _sut.RunScheduledTasksAsync();

        Assert.True(missedCreated);
    }
}
