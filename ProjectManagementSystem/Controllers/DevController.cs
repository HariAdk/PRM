using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Development-only helpers. Not available in production.</summary>
[ApiController]
[Route("api/dev")]
public class DevController(ISchedulerService schedulerService, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("run-scheduler")]
    public async Task<IActionResult> RunScheduler()
    {
        if (!environment.IsDevelopment())
            return NotFound();

        await schedulerService.RunScheduledTasksAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Scheduler run completed. Check email-outbox folder if using mock email."));
    }
}
