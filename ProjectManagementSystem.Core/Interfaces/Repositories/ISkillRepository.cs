using ProjectManagementSystem.Core.DTOs.Employee;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface ISkillRepository
{
    Task<IEnumerable<EmployeeSkillDto>> GetSkillsByEmployeeAsync(int employeeId);
    Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto);
    Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto);
    Task RemoveSkillAsync(int employeeId, int skillId);
}
