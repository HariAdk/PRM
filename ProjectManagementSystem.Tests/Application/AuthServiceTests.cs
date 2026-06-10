using Moq;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Auth;
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
        _userRepo.Setup(r => r.GetCredentialsAsync("unknown")).ReturnsAsync((ValueTuple<string, bool, bool, int, string, string>?)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "unknown", Password = "x" }));

        Assert.Equal(ErrorMessages.InvalidCredentials, ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenAccountDeactivated()
    {
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            ("hash", false, false, 1, "User", RoleNames.Employee));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "Pass1234" }));

        Assert.Equal(ErrorMessages.AccountDeactivated, ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsWhenPasswordWrong()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1");
        _userRepo.Setup(r => r.GetCredentialsAsync("user")).ReturnsAsync(
            (hash, false, true, 1, "User", RoleNames.Employee));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(new LoginRequestDto { Username = "user", Password = "WrongPass1" }));

        Assert.Equal(ErrorMessages.InvalidCredentials, ex.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_ThrowsWhenPasswordsDoNotMatch()
    {
        var dto = new ChangePasswordDto { NewPassword = "NewPass123", ConfirmPassword = "Different1" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ChangePasswordAsync(1, dto));

        Assert.Equal(ErrorMessages.PasswordsDoNotMatch, ex.Message);
    }
}
