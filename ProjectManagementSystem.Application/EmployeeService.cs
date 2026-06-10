using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class EmployeeService(
    IEmployeeRepository employeeRepo,
    ISkillRepository skillRepo,
    IUserRepository userRepo) : IEmployeeService
{
    public async Task<IEnumerable<EmployeeDto>> GetAllAsync() =>
        await employeeRepo.GetAllAsync();

    public async Task<EmployeeDto> GetByIdAsync(int id) =>
        await employeeRepo.GetByIdAsync(id)
        ?? throw new KeyNotFoundException(ErrorMessages.EmployeeNotFoundById(id));

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (!await employeeRepo.UserExistsForEmployeeAsync(dto.UserId))
            throw new InvalidOperationException(ErrorMessages.InvalidEmployeeUserId);

        if (await employeeRepo.UserHasEmployeeProfileAsync(dto.UserId))
            throw new InvalidOperationException(ErrorMessages.EmployeeProfileAlreadyExists);

        return await employeeRepo.CreateAsync(dto);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto) =>
        await employeeRepo.UpdateAsync(id, dto);

    public async Task DeactivateAsync(int id) =>
        await employeeRepo.DeactivateAsync(id);

    public async Task<EmployeeDto> AssignManagerAsync(AssignManagerDto dto)
    {
        var employeeUser = await userRepo.GetByIdAsync(dto.EmployeeUserId)
            ?? throw new KeyNotFoundException(ErrorMessages.EmployeeProfileRequired);

        if (!employeeUser.Role.Equals(RoleNames.Employee, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(ErrorMessages.InvalidManagerAssignment);

        var managerUser = await userRepo.GetByIdAsync(dto.ManagerUserId)
            ?? throw new KeyNotFoundException(ErrorMessages.ManagerUserNotFound);

        if (!managerUser.Role.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(ErrorMessages.InvalidManagerAssignment);

        if (!await employeeRepo.UserHasEmployeeProfileAsync(dto.EmployeeUserId))
            throw new KeyNotFoundException(ErrorMessages.EmployeeProfileRequired);

        return await employeeRepo.AssignManagerAsync(dto.EmployeeUserId, dto.ManagerUserId);
    }

    public async Task<IEnumerable<EmployeeSkillDto>> GetSkillsAsync(int employeeId) =>
        await skillRepo.GetSkillsByEmployeeAsync(employeeId);

    public async Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto) =>
        await skillRepo.AddSkillAsync(employeeId, dto);

    public async Task<EmployeeSkillDto> UpdateSkillAsync(int employeeId, int skillId, UpdateSkillDto dto) =>
        await skillRepo.UpdateSkillAsync(employeeId, skillId, dto);

    public async Task RemoveSkillAsync(int employeeId, int skillId) =>
        await skillRepo.RemoveSkillAsync(employeeId, skillId);
}
