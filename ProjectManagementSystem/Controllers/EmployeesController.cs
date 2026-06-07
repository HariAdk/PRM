using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.1 ù Manage Employees (Admin only)</summary>
[ApiController]
[Route("api/employees")]
[Authorize(Roles = RoleNames.Admin)]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    /// <summary>Screen 3.1.2 ù View All Employees</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await employeeService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(employees));
    }

    /// <summary>Get single employee by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var employee = await employeeService.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeDto>.Ok(employee));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1.1 ù Add Employee</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var employee = await employeeService.CreateAsync(dto);
            return Created(string.Empty, ApiResponse<EmployeeDto>.Ok(employee, "Employee added with status BENCH. ?"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1 ù Update Employee</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await employeeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<EmployeeDto>.Ok(employee, "Employee updated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1.3 ù Deactivate Employee</summary>
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await employeeService.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null!, "Employee deactivated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1.4 ù View Employee Skills</summary>
    [HttpGet("{id:int}/skills")]
    public async Task<IActionResult> GetSkills(int id)
    {
        var skills = await employeeService.GetSkillsAsync(id);
        return Ok(ApiResponse<IEnumerable<EmployeeSkillDto>>.Ok(skills));
    }

    /// <summary>Screen 3.1.4 ù Add Skill</summary>
    [HttpPost("{id:int}/skills")]
    public async Task<IActionResult> AddSkill(int id, [FromBody] AddSkillDto dto)
    {
        try
        {
            var skill = await employeeService.AddSkillAsync(id, dto);
            return Created(string.Empty, ApiResponse<EmployeeSkillDto>.Ok(skill, "Skill added. ?"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1.4 ù Update Proficiency Level</summary>
    [HttpPut("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> UpdateSkill(int id, int skillId, [FromBody] UpdateSkillDto dto)
    {
        try
        {
            var skill = await employeeService.UpdateSkillAsync(id, skillId, dto);
            return Ok(ApiResponse<EmployeeSkillDto>.Ok(skill, "Proficiency updated. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Screen 3.1.4 ù Remove Skill</summary>
    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId)
    {
        try
        {
            await employeeService.RemoveSkillAsync(id, skillId);
            return Ok(ApiResponse<object>.Ok(null!, "Skill removed. ?"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
