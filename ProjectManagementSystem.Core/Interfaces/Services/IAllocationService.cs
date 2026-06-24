using ProjectManagementSystem.Core.DTOs.Allocation;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IAllocationService
{
    Task<IEnumerable<AllocationDto>> GetAllActiveAsync();
    Task<IEnumerable<AllocationDto>> GetByProjectIdAsync(int projectId);
    Task<AllocationDto?> GetByIdAsync(int id);
    Task<AllocationDto> CreateAsync(CreateAllocationDto dto, int? managerUserId = null);
    Task<AllocationDto> EndAsync(int id, DateOnly endDate);
}
