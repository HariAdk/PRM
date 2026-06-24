using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> GetByUserIdAsync(int userId);
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<IEnumerable<EmployeeDto>> GetAllocatableResourcesAsync();
    Task<bool> IsAllocatableResourceAsync(int employeeId);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task DeactivateAsync(int id);
    Task<bool> UserHasEmployeeProfileAsync(int userId);
    Task<bool> UserExistsForEmployeeAsync(int userId);
    Task SetStatusAsync(int employeeId, EmployeeStatus status);
    Task<IEnumerable<EmployeeDto>> GetTeamAllocatableResourcesAsync(int managerUserId);
    Task<bool> IsOnManagerTeamAsync(int managerUserId, int employeeId);
    Task<IEnumerable<int>> GetTeamEmployeeIdsAsync(int managerUserId);
    Task<EmployeeDto> AssignManagerAsync(int employeeUserId, int managerUserId);
    Task<EmployeeDto> CreateProfileForUserAsync(int userId, string fullName, string email);
}
