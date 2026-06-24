using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<ISkillRepository> _skillRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _sut = new EmployeeService(_employeeRepo.Object, _skillRepo.Object, _userRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenEmployeeNotFound()
    {
        _employeeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((EmployeeDto?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenUserIdInvalid()
    {
        _employeeRepo.Setup(r => r.UserExistsForEmployeeAsync(1)).ReturnsAsync(false);

        var dto = new CreateEmployeeDto { UserId = 1, Department = "IT", Designation = "Dev" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenProfileAlreadyExists()
    {
        _employeeRepo.Setup(r => r.UserExistsForEmployeeAsync(1)).ReturnsAsync(true);
        _employeeRepo.Setup(r => r.UserHasEmployeeProfileAsync(1)).ReturnsAsync(true);

        var dto = new CreateEmployeeDto { UserId = 1, Department = "IT", Designation = "Dev" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task AssignManagerAsync_ThrowsWhenEmployeeUserIsNotEmployeeRole()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new UserDto { Id = 1, Role = RoleNames.Manager });

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.AssignManagerAsync(new AssignManagerDto { EmployeeUserId = 1, ManagerUserId = 2 }));
    }

    [Fact]
    public async Task GetSkillsAsync_ReturnsSkillsFromRepository()
    {
        var skills = new[] { new EmployeeSkillDto { SkillName = "C#" } };
        _skillRepo.Setup(r => r.GetSkillsByEmployeeAsync(3)).ReturnsAsync(skills);

        var result = await _sut.GetSkillsAsync(3);

        Assert.Single(result);
    }
}
