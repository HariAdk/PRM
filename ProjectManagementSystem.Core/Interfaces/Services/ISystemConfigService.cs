using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface ISystemConfigService
{
    Task<SystemConfigDto> GetAsync();
    Task UpdateAsync(SystemConfigDto dto);
}
