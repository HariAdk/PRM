using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Validation;

namespace ProjectManagementSystem.Application;

public class UserService(IUserRepository userRepo, IEmployeeRepository employeeRepo) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllAsync() =>
        await userRepo.GetAllAsync();

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        PasswordValidator.Validate(dto.TemporaryPassword);

        if (await userRepo.ExistsAsync(dto.Username, dto.Email))
            throw new BusinessRuleException(ErrorMessages.DuplicateUsernameOrEmail);

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.TemporaryPassword);
        var user = await userRepo.CreateAsync(dto, hash, PasswordExpiryHelper.ImmediateChangeRequired);

        if (user.Role.Equals(RoleNames.Employee, StringComparison.OrdinalIgnoreCase) &&
            !await employeeRepo.UserHasEmployeeProfileAsync(user.Id))
        {
            await employeeRepo.CreateProfileForUserAsync(user.Id, dto.FullName, dto.Email);
        }

        return user;
    }

    public async Task ResetPasswordAsync(int userId, ResetPasswordDto dto)
    {
        PasswordValidator.Validate(dto.NewTemporaryPassword);
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.NewTemporaryPassword);
        await userRepo.UpdatePasswordAsync(userId, hash, PasswordExpiryHelper.ImmediateChangeRequired);
    }

    public async Task DeactivateAsync(int userId) =>
        await userRepo.SetActiveAsync(userId, isActive: false);

    public async Task ReactivateAsync(int userId) =>
        await userRepo.SetActiveAsync(userId, isActive: true);
}
