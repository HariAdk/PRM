using ProjectManagementSystem.Core.DTOs.Employee;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task DeactivateAsync(int id);
    Task<IEnumerable<EmployeeSkillDto>> GetSkillsAsync(int employeeId);
    Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto);
    Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto);
    Task RemoveSkillAsync(int employeeId, int skillId);
}
