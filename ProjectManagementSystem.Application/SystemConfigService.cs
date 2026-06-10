using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class SystemConfigService(ISystemConfigRepository configRepo) : ISystemConfigService
{
    public async Task<SystemConfigDto> GetAsync() =>
        await configRepo.GetAsync()
        ?? throw new InvalidOperationException(ErrorMessages.SystemConfigNotFound);

    public async Task UpdateAsync(SystemConfigDto dto) =>
        await configRepo.UpdateAsync(dto);
}
