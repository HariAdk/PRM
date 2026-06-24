using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Notification;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class TimesheetReminderRepository(AppDbContext db) : ITimesheetReminderRepository
{
    public async Task<TimesheetReminderStateDto?> GetAsync(int employeeId, DateOnly weekStart)
    {
        var state = await db.TimesheetReminderStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ResourceId == employeeId && s.WeekStartDate == weekStart);

        return state is null ? null : Map(state);
    }

    public async Task<TimesheetReminderStateDto> GetOrCreateAsync(int employeeId, DateOnly weekStart)
    {
        var state = await db.TimesheetReminderStates
            .FirstOrDefaultAsync(s => s.ResourceId == employeeId && s.WeekStartDate == weekStart);

        if (state is null)
        {
            state = new TimesheetReminderState
            {
                ResourceId = employeeId,
                WeekStartDate = weekStart,
                ReminderCount = 0
            };
            db.TimesheetReminderStates.Add(state);
            await db.SaveChangesAsync();
        }

        return Map(state);
    }

    public async Task UpdateAsync(TimesheetReminderStateDto dto)
    {
        var state = await db.TimesheetReminderStates
            .FirstAsync(s => s.ResourceId == dto.EmployeeId && s.WeekStartDate == dto.WeekStartDate);

        state.ReminderCount = dto.ReminderCount;
        state.LastReminderDate = dto.LastReminderDate;
        state.IsFrozen = dto.IsFrozen;
        state.FreezeNotifiedAt = dto.FreezeNotifiedAt;
        state.RestoredAt = dto.RestoredAt;

        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<FrozenTimesheetDto>> GetFrozenForManagerTeamAsync(int managerUserId)
    {
        return await db.TimesheetReminderStates
            .AsNoTracking()
            .Include(s => s.Resource)
            .ThenInclude(r => r.User)
            .Where(s => s.IsFrozen &&
                        s.RestoredAt == null &&
                        s.Resource.ReportingManagerId == managerUserId)
            .OrderBy(s => s.Resource.User.FullName)
            .ThenByDescending(s => s.WeekStartDate)
            .Select(s => new FrozenTimesheetDto
            {
                EmployeeId = s.ResourceId,
                EmployeeName = s.Resource.User.FullName,
                WeekStartDate = s.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                FrozenAt = s.FreezeNotifiedAt
            })
            .ToListAsync();
    }

    public async Task RestoreAccessAsync(int employeeId, DateOnly weekStart, int managerUserId)
    {
        var state = await db.TimesheetReminderStates
            .FirstOrDefaultAsync(s => s.ResourceId == employeeId && s.WeekStartDate == weekStart);

        if (state is null)
            return;

        state.IsFrozen = false;
        state.RestoredAt = DateTime.UtcNow;
        state.RestoredByManagerId = managerUserId;
        await db.SaveChangesAsync();
    }

    private static TimesheetReminderStateDto Map(TimesheetReminderState state) => new()
    {
        EmployeeId = state.ResourceId,
        WeekStartDate = state.WeekStartDate,
        ReminderCount = state.ReminderCount,
        LastReminderDate = state.LastReminderDate,
        IsFrozen = state.IsFrozen,
        FreezeNotifiedAt = state.FreezeNotifiedAt,
        RestoredAt = state.RestoredAt
    };
}
