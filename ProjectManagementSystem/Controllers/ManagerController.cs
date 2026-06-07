using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Helpers;
using ProjectManagementSystem.Core.Interfaces.Services;
using ProjectManagementSystem.Extensions;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 4 — Manager endpoints</summary>
[ApiController]
[Route("api/manager")]
[Authorize(Roles = RoleNames.Manager)]
public class ManagerController(
    IManagerService managerService,
    IAllocationService allocationService,
    IProjectService projectService,
    ITimesheetService timesheetService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await managerService.GetResourceDashboardAsync(User.GetCurrentUserId());
        return Ok(ApiResponse<ResourceDashboardDto>.Ok(dashboard));
    }

    [HttpGet("employees/{id:int}")]
    public async Task<IActionResult> GetEmployeeDetail(int id)
    {
        var detail = await managerService.GetEmployeeDetailAsync(User.GetCurrentUserId(), id);
        if (detail is null)
            return NotFound(ApiResponse<object>.Fail("Employee not found."));

        return Ok(ApiResponse<EmployeeDetailDto>.Ok(detail));
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var projects = await managerService.GetMyProjectsAsync(User.GetCurrentUserId());
        return Ok(ApiResponse<IEnumerable<ProjectDto>>.Ok(projects));
    }

    [HttpGet("projects/{id:int}/detail")]
    public async Task<IActionResult> GetProjectDetail(int id)
    {
        var detail = await managerService.GetProjectDetailAsync(User.GetCurrentUserId(), id);
        if (detail is null)
            return NotFound(ApiResponse<object>.Fail("Project not found."));

        return Ok(ApiResponse<ProjectDetailDto>.Ok(detail));
    }

    [HttpGet("projects/{id:int}")]
    public async Task<IActionResult> GetProject(int id)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            return Ok(ApiResponse<ProjectDto>.Ok(project));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("projects/{id:int}/milestones")]
    public async Task<IActionResult> GetProjectMilestones(int id)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            var milestones = await projectService.GetMilestonesAsync(id);
            return Ok(ApiResponse<IEnumerable<MilestoneDto>>.Ok(milestones));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("projects/{id:int}/allocations")]
    public async Task<IActionResult> GetProjectAllocations(int id)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            var allocations = await allocationService.GetByProjectIdAsync(id);
            return Ok(ApiResponse<IEnumerable<AllocationDto>>.Ok(allocations));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("allocations")]
    public async Task<IActionResult> CreateAllocation([FromBody] CreateAllocationDto dto)
    {
        try
        {
            var project = await projectService.GetByIdAsync(dto.ProjectId);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            var result = await allocationService.CreateAsync(dto, User.GetCurrentUserId());
            return Ok(ApiResponse<AllocationDto>.Ok(result, "Allocation created successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
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

    [HttpPut("allocations/{id:int}/end")]
    public async Task<IActionResult> EndAllocation(int id, [FromBody] EndAllocationDto dto)
    {
        try
        {
            var allocation = await allocationService.GetByIdAsync(id);
            if (allocation is null)
                return NotFound(ApiResponse<object>.Fail("Allocation not found."));

            var project = await projectService.GetByIdAsync(allocation.ProjectId);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            await allocationService.EndAsync(id, dto.EndDate);
            return Ok(ApiResponse<object>.Ok(null!, "Allocation ended successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
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
    public async Task<IActionResult> GetTeamTimesheets([FromQuery] string? weekStart)
    {
        try
        {
            var week = WeekDateHelper.TryParseWeekStart(weekStart)
                       ?? WeekDateHelper.GetMondayOfWeek(DateTime.Today);

            var result = await timesheetService.GetTeamTimesheetsAsync(User.GetCurrentUserId(), week);
            return Ok(ApiResponse<ManagerTeamTimesheetDto>.Ok(result));
        }
        catch (FormatException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("timesheets/{id:int}")]
    public async Task<IActionResult> GetTimesheetDetail(int id)
    {
        try
        {
            var timesheet = await timesheetService.GetTimesheetForManagerAsync(User.GetCurrentUserId(), id);
            if (timesheet is null)
                return NotFound(ApiResponse<object>.Fail(ErrorMessages.TimesheetNotFound));

            return Ok(ApiResponse<TimesheetDto>.Ok(timesheet));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("ai/skill-match")]
    public async Task<IActionResult> AISkillMatch([FromBody] AISkillMatchRequestDto request)
    {
        try
        {
            var project = await projectService.GetByIdAsync(request.ProjectId);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            var result = await managerService.GetAISkillMatchAsync(request, User.GetCurrentUserId());
            return Ok(ApiResponse<AISkillMatchResultDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("ai/risk-summary")]
    public async Task<IActionResult> AIRiskSummary([FromBody] AIRiskSummaryRequestDto request)
    {
        try
        {
            var project = await projectService.GetByIdAsync(request.ProjectId);

            if (project.ManagerId != User.GetCurrentUserId())
                return Forbid();

            var result = await managerService.GetAIRiskSummaryAsync(request);
            return Ok(ApiResponse<AIRiskSummaryResultDto>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
