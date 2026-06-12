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
            LlmApiKey = ConfigDisplayDefaults.MaskSecret(config.LlmApiKey),
            SchedulerIntervalHours = config.SchedulerIntervalHours,
            MaxWeeklyHours = config.MaxWeeklyHours,
            EmailEnabled = config.EmailEnabled,
            SmtpHost = config.SmtpHost,
            SmtpPort = config.SmtpPort,
            SmtpUsername = config.SmtpUsername,
            SmtpPassword = ConfigDisplayDefaults.MaskSecret(config.SmtpPassword),
            EmailFromAddress = config.EmailFromAddress
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
