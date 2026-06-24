using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController(
    ISchedulerService schedulerService,
    IEmailService emailService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("run-scheduler")]
    public async Task<IActionResult> RunScheduler()
    {
        if (!environment.IsDevelopment())
            return NotFound();

        await schedulerService.RunScheduledTasksAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Scheduler run completed. Check email-outbox folder if using mock email."));
    }

    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string to)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        if (string.IsNullOrWhiteSpace(to))
            return BadRequest(ApiResponse<object>.Fail("Query parameter 'to' is required."));

        try
        {
            await emailService.SendAsync(
                to.Trim(),
                "PRM Tool — SMTP test",
                "This is a test email from Project Management System. If you received this, SMTP is configured correctly.");

            return Ok(ApiResponse<object>.Ok(null!, $"Test email sent to {to.Trim()}."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"SMTP send failed: {ex.Message}"));
        }
    }
}
