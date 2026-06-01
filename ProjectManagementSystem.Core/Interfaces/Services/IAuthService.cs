using ProjectManagementSystem.Core.DTOs.Auth;
using ProjectManagementSystem.Core.DTOs.User;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto> SignUpAsync(SignUpRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
