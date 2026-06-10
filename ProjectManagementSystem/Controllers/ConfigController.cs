using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/config")]
[Authorize(Roles = RoleNames.Admin)]
public class ConfigController(ISystemConfigService configService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await configService.GetAsync();
        var masked = new SystemConfigDto
        {
            LlmProvider = config.LlmProvider,
            LlmApiKey = string.IsNullOrEmpty(config.LlmApiKey)
                ? ConfigDisplayDefaults.ApiKeyNotSetLabel
                : new string('*', ConfigDisplayDefaults.MaskedApiKeyLength),
            SchedulerIntervalHours = config.SchedulerIntervalHours,
            MaxWeeklyHours = config.MaxWeeklyHours
        };
        return Ok(ApiResponse<SystemConfigDto>.Ok(masked));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SystemConfigDto dto)
    {
        await configService.UpdateAsync(dto);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.ConfigurationUpdated));
    }
}
