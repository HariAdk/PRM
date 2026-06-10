using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddScoped<ISystemConfigService, SystemConfigService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IManagerService, ManagerService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        services.AddScoped<IEmployeePortalService, EmployeePortalService>();
        services.AddScoped<ISchedulerService, SchedulerService>();

        return services;
    }
}
