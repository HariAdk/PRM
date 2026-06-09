using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Auth;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Extensions;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, SuccessMessages.LoginSuccessful));
    }

    [HttpPost("signup")]
    public IActionResult SignUp([FromBody] SignUpRequestDto request) =>
        StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(ErrorMessages.SignUpDisabled));

    [HttpPut("change-password/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto dto)
    {
        User.EnsureUserIdMatches(userId);
        await authService.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.PasswordUpdatedWelcome));
    }
}
