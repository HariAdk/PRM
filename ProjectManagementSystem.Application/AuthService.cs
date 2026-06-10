using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Auth;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Core.Validation;

using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Application;

public class AuthService(IUserRepository userRepo, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var creds = await userRepo.GetCredentialsAsync(request.Username)
            ?? throw new UnauthorizedAppException(ErrorMessages.InvalidCredentials);

        if (!creds.IsActive)
            throw new UnauthorizedAppException(ErrorMessages.AccountDeactivated);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, creds.PasswordHash))
            throw new UnauthorizedAppException(ErrorMessages.InvalidCredentials);

        var token = jwtTokenService.GenerateToken(creds.UserId, request.Username, creds.Role);

        return new LoginResponseDto
        {
            UserId              = creds.UserId,
            FullName            = creds.FullName,
            Role                = creds.Role,
            ForcePasswordChange = creds.ForcePasswordChange,
            Token               = token
        };
    }

    public async Task<UserDto> SignUpAsync(SignUpRequestDto request)
    {
        var role = EnumParseHelper.ParseUserRole(request.Role);
        if (role == UserRole.Admin)
            throw new BusinessRuleException(ErrorMessages.AdminSignUpForbidden);

        PasswordValidator.Validate(request.Password);

        if (await userRepo.ExistsAsync(request.Username, request.Email))
            throw new BusinessRuleException(ErrorMessages.DuplicateUsernameOrEmail);

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var createDto = new CreateUserDto
        {
            FullName = request.FullName,
            Email = request.Email,
            Username = request.Username,
            Role = role.ToString(),
            TemporaryPassword = request.Password
        };

        return await userRepo.CreateAsync(createDto, hash, forcePasswordChange: false);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new BusinessRuleException(ErrorMessages.PasswordsDoNotMatch);

        PasswordValidator.Validate(dto.NewPassword);

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await userRepo.UpdatePasswordAsync(userId, hash, forcePasswordChange: false);
    }
}
