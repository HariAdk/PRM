using ProjectManagementSystem.Core.DTOs.Notification;

namespace ProjectManagementSystem.Core.Interfaces.Repositories;

public interface ITimesheetReminderRepository
{
    Task<TimesheetReminderStateDto?> GetAsync(int employeeId, DateOnly weekStart);
    Task<TimesheetReminderStateDto> GetOrCreateAsync(int employeeId, DateOnly weekStart);
    Task UpdateAsync(TimesheetReminderStateDto state);
    Task RestoreAccessAsync(int employeeId, DateOnly weekStart, int managerUserId);
    Task<IReadOnlyList<FrozenTimesheetDto>> GetFrozenForManagerTeamAsync(int managerUserId);
}
