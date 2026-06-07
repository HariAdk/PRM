using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Services;

public class EmployeeService(IEmployeeRepository employeeRepo, ISkillRepository skillRepo) : IEmployeeService
{
    public async Task<IEnumerable<EmployeeDto>> GetAllAsync() =>
        await employeeRepo.GetAllAsync();

    public async Task<EmployeeDto> GetByIdAsync(int id) =>
        await employeeRepo.GetByIdAsync(id)
        ?? throw new KeyNotFoundException($"Employee {id} not found.");

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (!await employeeRepo.UserExistsForEmployeeAsync(dto.UserId))
            throw new InvalidOperationException("User ID does not exist or is not an Employee/Manager role.");

        if (await employeeRepo.UserHasEmployeeProfileAsync(dto.UserId))
            throw new InvalidOperationException("This user already has an employee profile.");

        return await employeeRepo.CreateAsync(dto);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto) =>
        await employeeRepo.UpdateAsync(id, dto);

    public async Task DeactivateAsync(int id) =>
        await employeeRepo.DeactivateAsync(id);

    public async Task<IEnumerable<EmployeeSkillDto>> GetSkillsAsync(int employeeId) =>
        await skillRepo.GetSkillsByEmployeeAsync(employeeId);

    public async Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto) =>
        await skillRepo.AddSkillAsync(employeeId, dto);

    public async Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto) =>
        await skillRepo.UpdateSkillAsync(employeeId, skillId, dto);

    public async Task RemoveSkillAsync(int employeeId, int skillId) =>
        await skillRepo.RemoveSkillAsync(employeeId, skillId);
}
