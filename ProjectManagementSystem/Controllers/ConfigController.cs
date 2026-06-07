using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.5 ù System Configuration (Admin only)</summary>
[ApiController]
[Route("api/config")]
[Authorize(Roles = RoleNames.Admin)]
public class ConfigController(ISystemConfigService configService) : ControllerBase
{
    /// <summary>Screen 3.5 ù Get current settings</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await configService.GetAsync();
        // Mask API key ù show only asterisks
        var masked = new SystemConfigDto
        {
            LlmProvider = config.LlmProvider,
            LlmApiKey = string.IsNullOrEmpty(config.LlmApiKey) ? "(not set)" : new string('*', 28),
            SchedulerIntervalHours = config.SchedulerIntervalHours,
            MaxWeeklyHours = config.MaxWeeklyHours
        };
        return Ok(ApiResponse<SystemConfigDto>.Ok(masked));
    }

    /// <summary>Screen 3.5 ù Update settings</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SystemConfigDto dto)
    {
        try
        {
            await configService.UpdateAsync(dto);
            return Ok(ApiResponse<object>.Ok(null!, "Configuration updated. ?"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
