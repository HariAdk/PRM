using ProjectManagementSystem.Core.DTOs.User;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(CreateUserDto dto, string passwordHash, bool forcePasswordChange);
    Task UpdatePasswordAsync(int userId, string passwordHash, bool forcePasswordChange);
    Task SetActiveAsync(int userId, bool isActive);
    Task<bool> ExistsAsync(string username, string email);
    Task<(string PasswordHash, bool ForcePasswordChange, bool IsActive, int UserId, string FullName, string Role)?> GetCredentialsAsync(string username);
}
