using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.AI;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.AI;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class AiServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<ISkillRepository> _skillRepo = new();
    private readonly Mock<ITimesheetRepository> _timesheetRepo = new();
    private readonly Mock<ISystemConfigRepository> _configRepo = new();
    private readonly Mock<IAiProviderFactory> _providerFactory = new();
    private readonly AiService _sut;

    public AiServiceTests()
    {
        _sut = new AiService(
            _employeeRepo.Object,
            _allocationRepo.Object,
            _projectRepo.Object,
            _skillRepo.Object,
            _timesheetRepo.Object,
            _configRepo.Object,
            _providerFactory.Object,
            Mock.Of<ILogger<AiService>>());

        _allocationRepo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(Array.Empty<Core.DTOs.Allocation.AllocationDto>());
        _configRepo.Setup(r => r.GetAsync()).ReturnsAsync((Core.DTOs.Config.SystemConfigDto?)null);
    }

    [Fact]
    public async Task GetSkillMatchAsync_ReturnsEmptyMatchesWhenNoCandidates()
    {
        _employeeRepo.Setup(r => r.GetAllocatableResourcesAsync()).ReturnsAsync(Array.Empty<EmployeeDto>());

        var result = await _sut.GetSkillMatchAsync(new AISkillMatchRequestDto { Requirement = "dotnet developer" });

        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task GetTeamBuildAsync_UsesFallbackWhenNoBenchEmployees()
    {
        _employeeRepo.Setup(r => r.GetAllocatableResourcesAsync()).ReturnsAsync(new[]
        {
            new EmployeeDto { Id = 1, FullName = "Allocated Dev", Status = nameof(EmployeeStatus.Allocated) }
        });

        var result = await _sut.GetTeamBuildAsync(new AITeamBuildRequestDto { Requirement = "QA, Dotnet" });

        Assert.True(result.UsedFallback);
    }

    [Fact]
    public async Task GetRiskSummaryAsync_ReturnsNotFoundMessageWhenProjectMissing()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProjectDto?)null);

        var result = await _sut.GetRiskSummaryAsync(new AIRiskSummaryRequestDto { ProjectId = 999 });

        Assert.Equal("Project not found.", result.Summary);
    }
}
