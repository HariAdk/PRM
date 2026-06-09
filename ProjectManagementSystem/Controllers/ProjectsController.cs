using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.2 ? Manage Projects (Admin only)</summary>
[ApiController]
[Route("api/projects")]
[Authorize(Roles = RoleNames.Admin)]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await projectService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProjectDto>>.Ok(projects));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        return Ok(ApiResponse<ProjectDto>.Ok(project));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var project = await projectService.CreateAsync(dto);
        return Created(string.Empty, ApiResponse<ProjectDto>.Ok(project, SuccessMessages.ProjectCreated));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var project = await projectService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProjectDto>.Ok(project, SuccessMessages.ProjectUpdated));
    }

    [HttpGet("{id:int}/milestones")]
    public async Task<IActionResult> GetMilestones(int id)
    {
        var milestones = await projectService.GetMilestonesAsync(id);
        return Ok(ApiResponse<IEnumerable<MilestoneDto>>.Ok(milestones));
    }

    [HttpPost("{id:int}/milestones")]
    public async Task<IActionResult> AddMilestone(int id, [FromBody] CreateMilestoneDto dto)
    {
        var milestone = await projectService.AddMilestoneAsync(id, dto);
        return Created(string.Empty, ApiResponse<MilestoneDto>.Ok(milestone, SuccessMessages.MilestoneAdded));
    }

    [HttpPut("{id:int}/milestones/{milestoneId:int}")]
    public async Task<IActionResult> UpdateMilestoneStatus(int id, int milestoneId, [FromBody] UpdateMilestoneStatusDto dto)
    {
        var milestone = await projectService.UpdateMilestoneStatusAsync(id, milestoneId, dto);
        return Ok(ApiResponse<MilestoneDto>.Ok(milestone, SuccessMessages.MilestoneStatusUpdated));
    }
}
