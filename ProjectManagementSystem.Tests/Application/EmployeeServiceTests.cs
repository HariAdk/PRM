using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.User;
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
    public async Task AssignManagerAsync_ThrowsWhenEmployeeUserIsNotEmployeeRole()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new UserDto
        {
            Id = 1,
            Role = RoleNames.Manager
        });

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.AssignManagerAsync(new AssignManagerDto { EmployeeUserId = 1, ManagerUserId = 2 }));
    }

    [Fact]
    public async Task AssignManagerAsync_ThrowsWhenManagerUserIsNotManagerRole()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new UserDto { Id = 1, Role = RoleNames.Employee });
        _userRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new UserDto { Id = 2, Role = RoleNames.Employee });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _sut.AssignManagerAsync(new AssignManagerDto { EmployeeUserId = 1, ManagerUserId = 2 }));

        Assert.Equal(ErrorMessages.InvalidManagerAssignment, ex.Message);
    }

    [Fact]
    public async Task AssignManagerAsync_ThrowsWhenEmployeeProfileMissing()
    {
        _userRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new UserDto { Id = 1, Role = RoleNames.Employee });
        _userRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new UserDto { Id = 2, Role = RoleNames.Manager });
        _employeeRepo.Setup(r => r.UserHasEmployeeProfileAsync(1)).ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AssignManagerAsync(new AssignManagerDto { EmployeeUserId = 1, ManagerUserId = 2 }));

        Assert.Equal(ErrorMessages.EmployeeProfileRequired, ex.Message);
    }
}
