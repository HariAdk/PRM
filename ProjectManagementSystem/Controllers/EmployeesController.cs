using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.1 ? Manage Employees (Admin only)</summary>
[ApiController]
[Route("api/employees")]
[Authorize(Roles = RoleNames.Admin)]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await employeeService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(employees));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await employeeService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var employee = await employeeService.CreateAsync(dto);
        return Created(string.Empty, ApiResponse<EmployeeDto>.Ok(employee, SuccessMessages.EmployeeAdded));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await employeeService.UpdateAsync(id, dto);
        return Ok(ApiResponse<EmployeeDto>.Ok(employee, SuccessMessages.EmployeeUpdated));
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await employeeService.DeactivateAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.EmployeeDeactivated));
    }

    [HttpPut("assign-manager")]
    public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
    {
        var employee = await employeeService.AssignManagerAsync(dto);
        return Ok(ApiResponse<EmployeeDto>.Ok(employee, SuccessMessages.ManagerAssigned));
    }

    [HttpGet("{id:int}/skills")]
    public async Task<IActionResult> GetSkills(int id)
    {
        var skills = await employeeService.GetSkillsAsync(id);
        return Ok(ApiResponse<IEnumerable<EmployeeSkillDto>>.Ok(skills));
    }

    [HttpPost("{id:int}/skills")]
    public async Task<IActionResult> AddSkill(int id, [FromBody] AddSkillDto dto)
    {
        var skill = await employeeService.AddSkillAsync(id, dto);
        return Created(string.Empty, ApiResponse<EmployeeSkillDto>.Ok(skill, SuccessMessages.SkillAdded));
    }

    [HttpPut("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> UpdateSkill(int id, int skillId, [FromBody] UpdateSkillDto dto)
    {
        var skill = await employeeService.UpdateSkillAsync(id, skillId, dto);
        return Ok(ApiResponse<EmployeeSkillDto>.Ok(skill, SuccessMessages.ProficiencyUpdated));
    }

    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId)
    {
        await employeeService.RemoveSkillAsync(id, skillId);
        return Ok(ApiResponse<object>.Ok(null!, SuccessMessages.SkillRemoved));
    }
}
