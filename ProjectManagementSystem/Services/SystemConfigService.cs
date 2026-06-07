using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Services;

public class SystemConfigService(ISystemConfigRepository configRepo) : ISystemConfigService
{
    public async Task<SystemConfigDto> GetAsync() =>
        await configRepo.GetAsync()
        ?? throw new InvalidOperationException("System configuration not found.");

    public async Task UpdateAsync(SystemConfigDto dto) =>
        await configRepo.UpdateAsync(dto);
}
