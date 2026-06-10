using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class AllocationServiceTests
{
    private readonly Mock<IAllocationRepository> _allocationRepo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly AllocationService _sut;

    public AllocationServiceTests()
    {
        _sut = new AllocationService(_allocationRepo.Object, _employeeRepo.Object, _projectRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenEmployeeNotOnManagerTeam()
    {
        SetupValidEmployeeAndProject();

        _employeeRepo
            .Setup(r => r.IsOnManagerTeamAsync(10, 1))
            .ReturnsAsync(false);

        var dto = CreateValidAllocationDto();

        var ex = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _sut.CreateAsync(dto, managerUserId: 10));

        Assert.Equal(ErrorMessages.EmployeeNotOnTeam, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenTotalUtilisationExceeds100()
    {
        SetupValidEmployeeAndProject();

        _employeeRepo
            .Setup(r => r.IsOnManagerTeamAsync(10, 1))
            .ReturnsAsync(true);

        _allocationRepo
            .Setup(r => r.GetByEmployeeIdAsync(1))
            .ReturnsAsync(new[]
            {
                new AllocationDto
                {
                    Id = 99,
                    EmployeeId = 1,
                    ProjectId = 2,
                    UtilisationPercent = 80,
                    FromDate = new DateOnly(2026, 6, 1),
                    ToDate = new DateOnly(2026, 9, 30),
                    IsActive = true
                }
            });

        var dto = CreateValidAllocationDto(utilisation: 30);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto, managerUserId: 10));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenToDateBeforeFromDate()
    {
        SetupValidEmployeeAndProject();

        var dto = new CreateAllocationDto
        {
            EmployeeId = 1,
            ProjectId = 201,
            UtilisationPercent = 50,
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 6, 1)
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenProjectNotOpenForAllocation()
    {
        SetupValidEmployeeAndProject();

        _projectRepo
            .Setup(r => r.GetByIdAsync(201))
            .ReturnsAsync(new ProjectDto { Id = 201, Status = "Completed" });

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(CreateValidAllocationDto()));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenEmployeeInactive()
    {
        _employeeRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new EmployeeDto { Id = 1, IsActive = false });

        _employeeRepo
            .Setup(r => r.IsAllocatableResourceAsync(1))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(CreateValidAllocationDto()));
    }

    [Fact]
    public async Task EndAsync_ThrowsWhenEndDateBeforeStartDate()
    {
        _allocationRepo
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(new AllocationDto
            {
                Id = 5,
                FromDate = new DateOnly(2026, 6, 1),
                ToDate = new DateOnly(2026, 9, 30),
                IsActive = true
            });

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.EndAsync(5, new DateOnly(2026, 5, 1)));
    }

    private void SetupValidEmployeeAndProject()
    {
        _employeeRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new EmployeeDto { Id = 1, IsActive = true });

        _employeeRepo
            .Setup(r => r.IsAllocatableResourceAsync(1))
            .ReturnsAsync(true);

        _projectRepo
            .Setup(r => r.GetByIdAsync(201))
            .ReturnsAsync(new ProjectDto { Id = 201, Status = "Active" });

        _allocationRepo
            .Setup(r => r.GetByEmployeeIdAsync(1))
            .ReturnsAsync(Array.Empty<AllocationDto>());
    }

    private static CreateAllocationDto CreateValidAllocationDto(int utilisation = 50) => new()
    {
        EmployeeId = 1,
        ProjectId = 201,
        UtilisationPercent = utilisation,
        FromDate = new DateOnly(2026, 6, 1),
        ToDate = new DateOnly(2026, 9, 30)
    };
}
