using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class SystemConfigServiceTests
{
    private readonly Mock<ISystemConfigRepository> _configRepo = new();
    private readonly SystemConfigService _sut;

    public SystemConfigServiceTests()
    {
        _sut = new SystemConfigService(_configRepo.Object);
    }

    [Fact]
    public async Task GetAsync_ThrowsWhenConfigurationMissing()
    {
        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync((SystemConfigDto?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.GetAsync());
    }

    [Fact]
    public async Task GetAsync_ReturnsConfigurationFromRepository()
    {
        var config = new SystemConfigDto { MaxWeeklyHours = 40, SchedulerIntervalHours = 4 };
        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync(config);

        var result = await _sut.GetAsync();

        Assert.Equal(40, result.MaxWeeklyHours);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var existing = new SystemConfigDto { MaxWeeklyHours = 40, SchedulerIntervalHours = 4 };
        var dto = new SystemConfigDto { MaxWeeklyHours = 45, SchedulerIntervalHours = 6 };
        var updated = false;
        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync(existing);
        _configRepo.Setup(r => r.UpdateAsync(It.IsAny<SystemConfigDto>()))
            .Callback(() => updated = true)
            .Returns(Task.CompletedTask);

        await _sut.UpdateAsync(dto);

        Assert.True(updated);
    }
}
