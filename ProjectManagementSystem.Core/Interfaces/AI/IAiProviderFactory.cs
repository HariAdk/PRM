using ProjectManagementSystem.Core.DTOs.Config;

namespace ProjectManagementSystem.Core.Interfaces.AI;

/// <summary>Creates the configured LLM adapter when an API key is present.</summary>
public interface IAiProviderFactory
{
    bool IsConfigured(SystemConfigDto? config);

    IAiProvider Create(SystemConfigDto config);
}
