using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_projectRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenProjectNotFound()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(50)).ReturnsAsync((ProjectDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(50));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenEndDateBeforeStartDate()
    {
        _projectRepo.Setup(r => r.ManagerExistsAsync(1)).ReturnsAsync(true);

        var dto = new CreateProjectDto
        {
            ManagerId = 1,
            Name = "Alpha",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 6, 1)
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenManagerInvalid()
    {
        _projectRepo.Setup(r => r.ManagerExistsAsync(99)).ReturnsAsync(false);

        var dto = new CreateProjectDto
        {
            ManagerId = 99,
            Name = "Alpha",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 12, 1)
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task AddMilestoneAsync_ThrowsWhenProjectNotFound()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((ProjectDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddMilestoneAsync(10, new CreateMilestoneDto { Title = "M1", DueDate = new DateOnly(2026, 8, 1) }));
    }

    [Fact]
    public async Task GetMilestonesAsync_ReturnsMilestonesFromRepository()
    {
        var milestones = new[] { new MilestoneDto { Id = 1, Title = "Kickoff" } };
        _projectRepo.Setup(r => r.GetMilestonesAsync(5)).ReturnsAsync(milestones);

        var result = await _sut.GetMilestonesAsync(5);

        Assert.Equal("Kickoff", result.First().Title);
    }
}
