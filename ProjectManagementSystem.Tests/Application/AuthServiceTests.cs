using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Auth;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepo.Object, _jwt.Object);
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenUserNotFound()
    {
        _userRepo.Setup(r => r.GetCredentialsAsync("unknown")).ReturnsAsync((ValueTuple<string, DateTime, bool, int, string, string>?)null);

        await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "unknown", Password = "x" }));
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenAccountDeactivated()
    {
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            ("hash", DateTime.UtcNow.AddMonths(3), false, 1, "User", RoleNames.Employee));

        await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "Pass1234" }));
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenPasswordWrong()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1");
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            (hash, DateTime.UtcNow.AddMonths(3), true, 1, "User", RoleNames.Employee));

        await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "WrongPass1" }));
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokenWhenCredentialsValid()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("ValidPass1");
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            (hash, DateTime.UtcNow.AddMonths(3), true, 42, "Jane Doe", RoleNames.Employee));
        _jwt.Setup(j => j.GenerateToken(42, "user", RoleNames.Employee)).Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "ValidPass1" });

        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsForcePasswordChangeWhenPasswordExpired()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("ValidPass1");
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            (hash, DateTime.UtcNow.AddMinutes(-1), true, 42, "Jane Doe", RoleNames.Employee));
        _jwt.Setup(j => j.GenerateToken(42, "user", RoleNames.Employee)).Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "ValidPass1" });

        Assert.True(result.ForcePasswordChange);
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsWhenPasswordsDoNotMatch()
    {
        var dto = new ChangePasswordDto { NewPassword = "NewPass123", ConfirmPassword = "Different1" };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.ChangePasswordAsync(1, dto));
    }

    [Fact]
    public async Task SignUpAsync_ThrowsWhenRoleIsAdmin()
    {
        var dto = new SignUpRequestDto
        {
            FullName = "Admin User",
            Email = "admin@test.com",
            Username = "adminuser",
            Password = "ValidPass1",
            Role = RoleNames.Admin
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.SignUpAsync(dto));
    }
}
