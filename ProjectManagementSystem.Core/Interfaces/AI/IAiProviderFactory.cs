using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Core.Interfaces.AI;

public interface IAiProviderFactory
{
    bool IsConfigured(SystemConfigDto? config);

    IAiProvider Create(SystemConfigDto config);
}
