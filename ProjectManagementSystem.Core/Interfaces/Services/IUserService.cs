using ProjectManagementSystem.Core.DTOs.User;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task ResetPasswordAsync(int userId, ResetPasswordDto dto);
    Task DeactivateAsync(int userId);
    Task ReactivateAsync(int userId);
}
