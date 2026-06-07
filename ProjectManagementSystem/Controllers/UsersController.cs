using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.4 ù Manage Users (Admin only)</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.Admin)]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Screen 3.4.2 ù View All Users</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(users));
    }

    /// <summary>Screen 3.4.1 ù Create User Account</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await userService.CreateAsync(dto);
            return Created(string.Empty,
                ApiResponse<UserDto>.Ok(user, "Account created. User must change password on first login. ?"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.4.3 ù Reset User Password</summary>
    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            await userService.ResetPasswordAsync(id, dto);
            return Ok(ApiResponse<object>.Ok(null!, "Password reset. User will be prompted to change it on next login. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.4.4 ù Deactivate User</summary>
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await userService.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "User deactivated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.4.2 ù Reactivate User [R]</summary>
    [HttpPut("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        try
        {
            await userService.ReactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Account reactivated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
