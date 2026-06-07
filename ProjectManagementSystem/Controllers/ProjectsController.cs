using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.2 ù Manage Projects (Admin only)</summary>
[ApiController]
[Route("api/projects")]
[Authorize(Roles = RoleNames.Admin)]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    /// <summary>Screen 3.2.2 ù View All Projects</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await projectService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProjectDto>>.Ok(projects));
    }

    /// <summary>Get single project</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id);
            return Ok(ApiResponse<ProjectDto>.Ok(project));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.2.1 ù Create Project</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        try
        {
            var project = await projectService.CreateAsync(dto);
            return Created(string.Empty, ApiResponse<ProjectDto>.Ok(project, "Project created. ?"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.2 ù Update Project Details</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var project = await projectService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProjectDto>.Ok(project, "Project updated. ?"));
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

    /// <summary>Screen 3.2.3 ù View Milestones</summary>
    [HttpGet("{id:int}/milestones")]
    public async Task<IActionResult> GetMilestones(int id)
    {
        var milestones = await projectService.GetMilestonesAsync(id);
        return Ok(ApiResponse<IEnumerable<MilestoneDto>>.Ok(milestones));
    }

    /// <summary>Screen 3.2.3 ù Add Milestone</summary>
    [HttpPost("{id:int}/milestones")]
    public async Task<IActionResult> AddMilestone(int id, [FromBody] CreateMilestoneDto dto)
    {
        try
        {
            var milestone = await projectService.AddMilestoneAsync(id, dto);
            return Created(string.Empty, ApiResponse<MilestoneDto>.Ok(milestone, "Milestone added. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.2.3 ù Update Milestone Status</summary>
    [HttpPut("{id:int}/milestones/{milestoneId:int}")]
    public async Task<IActionResult> UpdateMilestoneStatus(int id, int milestoneId, [FromBody] UpdateMilestoneStatusDto dto)
    {
        try
        {
            var milestone = await projectService.UpdateMilestoneStatusAsync(id, milestoneId, dto);
            return Ok(ApiResponse<MilestoneDto>.Ok(milestone, "Milestone status updated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
