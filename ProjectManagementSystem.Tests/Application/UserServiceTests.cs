using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;

namespace ProjectManagementSystem.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepo.Object, _employeeRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenUsernameOrEmailExists()
    {
        _userRepo.Setup(r => r.ExistsAsync("dup", "dup@test.com")).ReturnsAsync(true);

        var dto = new CreateUserDto
        {
            FullName = "Test User",
            Email = "dup@test.com",
            Username = "dup",
            Role = RoleNames.Manager,
            TemporaryPassword = "ValidPass1"
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedUser()
    {
        var dto = new CreateUserDto
        {
            FullName = "New Manager",
            Email = "mgr@test.com",
            Username = "mgr",
            Role = RoleNames.Manager,
            TemporaryPassword = "ValidPass1"
        };

        _userRepo.Setup(r => r.ExistsAsync(dto.Username, dto.Email)).ReturnsAsync(false);
        _userRepo.Setup(r => r.CreateAsync(dto, It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new UserDto { Id = 5, Username = "mgr", Role = RoleNames.Manager });

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUsersFromRepository()
    {
        var users = new[] { new UserDto { Id = 1 }, new UserDto { Id = 2 } };
        _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ReactivateAsync_SetsUserActiveInRepository()
    {
        bool? capturedActive = null;
        _userRepo.Setup(r => r.SetActiveAsync(7, It.IsAny<bool>()))
            .Callback<int, bool>((_, active) => capturedActive = active)
            .Returns(Task.CompletedTask);

        await _sut.ReactivateAsync(7);

        Assert.True(capturedActive);
    }
}
