using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.4 ? Manage Users (Admin only)</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.Admin)]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(users));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await userService.CreateAsync(dto);
        return Created(string.Empty,
            ApiResponse<UserDto>.Ok(user, SuccessMessages.AccountCreated));
    }

    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        await userService.ResetPasswordAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.PasswordReset));
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await userService.DeactivateAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.UserDeactivated));
    }

    [HttpPut("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        await userService.ReactivateAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.AccountReactivated));
    }
}
