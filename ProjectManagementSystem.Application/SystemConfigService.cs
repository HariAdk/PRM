using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public class SystemConfigService(ISystemConfigRepository configRepo) : ISystemConfigService
{
    public async Task<SystemConfigDto> GetAsync() =>
        await configRepo.GetAsync()
        ?? throw new BusinessRuleException(ErrorMessages.SystemConfigNotFound);

    public async Task UpdateAsync(SystemConfigDto dto)
    {
        var existing = await GetAsync();
        var merged = dto with
        {
            LlmApiKey = ConfigDisplayDefaults.ShouldPreserveSecret(dto.LlmApiKey)
                ? existing.LlmApiKey
                : dto.LlmApiKey,
            SmtpPassword = ConfigDisplayDefaults.ShouldPreserveSecret(dto.SmtpPassword)
                ? existing.SmtpPassword
                : dto.SmtpPassword
        };
        await configRepo.UpdateAsync(merged);
    }
}
