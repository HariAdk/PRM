using ProjectManagementSystem.Core.DTOs.Allocation;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface IAllocationRepository
{
    Task<IEnumerable<AllocationDto>> GetAllActiveAsync();
    Task<IEnumerable<AllocationDto>> GetAllAsync();
    Task<IEnumerable<AllocationDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AllocationDto>> GetByProjectIdAsync(int projectId);
    Task<AllocationDto?> GetByIdAsync(int id);
    Task<IEnumerable<int>> GetEmployeeIdsAllocatedBetweenAsync(DateOnly from, DateOnly to);
    Task<AllocationDto> CreateAsync(CreateAllocationDto dto);
    Task<AllocationDto> UpdateEndDateAsync(int id, DateOnly endDate);
    Task<int> DeactivateExpiredAsync(DateOnly asOfDate);
}
