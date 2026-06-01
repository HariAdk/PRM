using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface ISystemConfigRepository
{
    Task<SystemConfigDto?> GetAsync();
    Task UpdateAsync(SystemConfigDto dto);
}
