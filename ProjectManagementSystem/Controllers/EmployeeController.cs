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
        try
        {
            var reminder = await employeePortalService.GetReminderAsync(User.GetCurrentUserId());
            return Ok(ApiResponse<EmployeeReminderDto>.Ok(reminder));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("allocations")]
    public async Task<IActionResult> GetMyAllocations()
    {
        try
        {
            var profile = await employeePortalService.GetProfileAsync(User.GetCurrentUserId());
            return Ok(ApiResponse<EmployeeProfileDto>.Ok(profile));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("timesheets/context")]
    public async Task<IActionResult> GetSubmitContext([FromQuery] string? weekStart)
    {
        try
        {
            var week = WeekDateHelper.TryParseWeekStart(weekStart);
            var context = await employeePortalService.GetSubmitContextAsync(User.GetCurrentUserId(), week);
            return Ok(ApiResponse<EmployeeSubmitContextDto>.Ok(context));
        }
        catch (FormatException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("timesheets")]
    public async Task<IActionResult> SubmitTimesheet([FromBody] SubmitEmployeeTimesheetDto dto)
    {
        try
        {
            var result = await employeePortalService.SubmitTimesheetAsync(User.GetCurrentUserId(), dto);
            return Ok(ApiResponse<TimesheetDto>.Ok(result, "Timesheet submitted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("timesheets")]
    public async Task<IActionResult> GetMyTimesheets()
    {
        try
        {
            var timesheets = await employeePortalService.GetMyTimesheetsAsync(User.GetCurrentUserId());
            return Ok(ApiResponse<IEnumerable<TimesheetDto>>.Ok(timesheets));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("timesheets/{id:int}")]
    public async Task<IActionResult> GetMyTimesheet(int id)
    {
        try
        {
            var timesheet = await employeePortalService.GetMyTimesheetAsync(User.GetCurrentUserId(), id);
            if (timesheet is null)
                return NotFound(ApiResponse<object>.Fail(ErrorMessages.TimesheetNotFound));

            return Ok(ApiResponse<TimesheetDto>.Ok(timesheet));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
