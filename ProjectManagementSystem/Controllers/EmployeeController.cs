using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Extensions;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 5 — Employee endpoints</summary>
[ApiController]
[Route("api/employee")]
[Authorize(Roles = RoleNames.Employee)]
public class EmployeeController(IEmployeePortalService employeePortalService) : ControllerBase
{
    [HttpGet("reminder")]
    public async Task<IActionResult> GetReminder()
    {
        var reminder = await employeePortalService.GetReminderAsync(User.GetCurrentUserId());
        return Ok(ApiResponse<EmployeeReminderDto>.Ok(reminder));
    }

    [HttpGet("allocations")]
    public async Task<IActionResult> GetMyAllocations()
    {
        var profile = await employeePortalService.GetProfileAsync(User.GetCurrentUserId());
        return Ok(ApiResponse<EmployeeProfileDto>.Ok(profile));
    }

    [HttpGet("timesheets/context")]
    public async Task<IActionResult> GetSubmitContext([FromQuery] string? weekStart)
    {
        var week = WeekDateHelper.TryParseWeekStart(weekStart);
        var context = await employeePortalService.GetSubmitContextAsync(User.GetCurrentUserId(), week);
        return Ok(ApiResponse<EmployeeSubmitContextDto>.Ok(context));
    }

    [HttpPost("timesheets")]
    public async Task<IActionResult> SubmitTimesheet([FromBody] SubmitEmployeeTimesheetDto dto)
    {
        var result = await employeePortalService.SubmitTimesheetAsync(User.GetCurrentUserId(), dto);
        return Ok(ApiResponse<TimesheetDto>.Ok(result, SuccessMessages.TimesheetSubmitted));
    }

    [HttpGet("timesheets")]
    public async Task<IActionResult> GetMyTimesheets()
    {
        var timesheets = await employeePortalService.GetMyTimesheetsAsync(User.GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<TimesheetDto>>.Ok(timesheets));
    }

    [HttpGet("timesheets/{id:int}")]
    public async Task<IActionResult> GetMyTimesheet(int id)
    {
        var timesheet = await employeePortalService.GetMyTimesheetAsync(User.GetCurrentUserId(), id);
        if (timesheet is null)
            return NotFound(ApiResponse<object>.Fail(ErrorMessages.TimesheetNotFound));

        return Ok(ApiResponse<TimesheetDto>.Ok(timesheet));
    }
}
