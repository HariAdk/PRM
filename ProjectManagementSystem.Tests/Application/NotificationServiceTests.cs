using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Notification;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Tests.Application;

public class NotificationServiceTests
{
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<ITimesheetReminderRepository> _reminderRepo = new();
    private readonly Mock<IAiService> _aiService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _allocationRepo.Object,
            _timesheetRepo.Object,
            _employeeRepo.Object,
            _userRepo.Object,
            _projectRepo.Object,
            _reminderRepo.Object,
            _aiService.Object,
            _emailService.Object,
            Mock.Of<ILogger<NotificationService>>());
    }

    [Fact]
    public async Task ProcessAtRiskProjectNotificationsAsync_SendsEmailOncePerProject()
    {
        var project = new AtRiskProjectEmailDto
        {
            ProjectId = 1,
            ProjectName = "Alpha",
            ManagerName = "Mgr",
            ManagerEmail = "mgr@test.com",
            HealthStatus = "AtRisk",
            MilestoneSummary = "M1 overdue"
        };

        _projectRepo.Setup(r => r.GetAtRiskProjectsPendingNotificationAsync())
            .ReturnsAsync(new[] { project });
        _aiService.Setup(s => s.GetRiskSummaryAsync(It.IsAny<Core.DTOs.Manager.AIRiskSummaryRequestDto>()))
            .ReturnsAsync(new Core.DTOs.Manager.AIRiskSummaryResultDto { Summary = "Risk text" });
        _aiService.Setup(s => s.GetSkillMatchAsync(It.IsAny<Core.DTOs.Manager.AISkillMatchRequestDto>()))
            .ReturnsAsync(new Core.DTOs.Manager.AISkillMatchResultDto { Matches = [] });

        var markedNotified = false;
        _projectRepo.Setup(r => r.MarkAtRiskNotifiedAsync(1))
            .Callback(() => markedNotified = true)
            .Returns(Task.CompletedTask);

        await _sut.ProcessAtRiskProjectNotificationsAsync();

        Assert.True(markedNotified);
    }
}
