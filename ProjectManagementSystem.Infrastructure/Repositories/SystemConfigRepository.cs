using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class SystemConfigRepository(AppDbContext db, IMapper mapper) : ISystemConfigRepository
{
    public async Task<SystemConfigDto?> GetAsync()
    {
        var config = await db.SystemConfigs.FirstOrDefaultAsync();
        return config is null ? null : mapper.Map<SystemConfigDto>(config);
    }

    public async Task UpdateAsync(SystemConfigDto dto)
    {
        var config = await db.SystemConfigs.FirstOrDefaultAsync()
                     ?? throw new BusinessRuleException(ErrorMessages.SystemConfigMissing);
        mapper.Map(dto, config);
        await db.SaveChangesAsync();
    }
}
