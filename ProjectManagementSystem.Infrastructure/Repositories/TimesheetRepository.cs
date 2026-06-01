using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class TimesheetRepository(AppDbContext db) : ITimesheetRepository
{
    public async Task<IEnumerable<TimesheetDto>> GetByEmployeeIdAsync(int employeeId)
    {
        return await db.Timesheets
            .Where(t => t.EmployeeId == employeeId)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<IEnumerable<TimesheetDto>> GetByWeekStartAsync(DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        return await db.Timesheets
            .Where(t => t.WeekStartDate == weekStartDate)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetRecentActivityTagsAsync(int employeeId, int count = 5)
    {
        return await db.TimesheetEntries
            .Where(e => e.Timesheet.EmployeeId == employeeId && e.ActivityTags != string.Empty)
            .OrderByDescending(e => e.Timesheet.WeekStartDate)
            .Select(e => e.ActivityTags)
            .Distinct()
            .Take(count)
            .ToListAsync();
    }

    public async Task<TimesheetDto?> GetByIdAsync(int timesheetId)
    {
        var timesheet = await db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .FirstOrDefaultAsync(t => t.Id == timesheetId);

        return timesheet == null ? null : MapToDto(timesheet);
    }

    public async Task<TimesheetDto?> GetByEmployeeAndWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var timesheet = await db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.WeekStartDate == weekStartDate);

        return timesheet is null ? null : MapToDto(timesheet);
    }

    public async Task<bool> HasSubmittedForWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        return await db.Timesheets.AnyAsync(t =>
            t.EmployeeId == employeeId &&
            t.WeekStartDate == weekStartDate &&
            t.Status == TimesheetStatus.Submitted);
    }

    public async Task<TimesheetDto> CreateAsync(CreateTimesheetDto dto)
    {
        var timesheet = new Timesheet
        {
            EmployeeId = dto.EmployeeId,
            WeekStartDate = DateOnly.FromDateTime(dto.WeekStartDate),
            Status = TimesheetStatus.Draft,
            TotalHours = dto.Entries.Sum(e => e.Hours)
        };

        db.Timesheets.Add(timesheet);
        await db.SaveChangesAsync();

        foreach (var entryDto in dto.Entries)
        {
            var entry = new TimesheetEntry
            {
                TimesheetId = timesheet.Id,
                ProjectId = entryDto.ProjectId,
                Hours = entryDto.Hours,
                ActivityTags = entryDto.ActivityTags
            };
            db.TimesheetEntries.Add(entry);
        }

        await db.SaveChangesAsync();

        return (await GetByIdAsync(timesheet.Id))!;
    }

    public async Task<bool> SubmitAsync(int timesheetId)
    {
        var timesheet = await db.Timesheets.FindAsync(timesheetId);
        if (timesheet == null) return false;

        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExistsForEmployeeWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        return await db.Timesheets.AnyAsync(t =>
            t.EmployeeId == employeeId && t.WeekStartDate == weekStartDate);
    }

    public async Task CreateMissedAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var exists = await db.Timesheets.AnyAsync(t =>
            t.EmployeeId == employeeId && t.WeekStartDate == weekStartDate);
        if (exists) return;

        db.Timesheets.Add(new Timesheet
        {
            EmployeeId = employeeId,
            WeekStartDate = weekStartDate,
            TotalHours = 0,
            Status = TimesheetStatus.Missed
        });
        await db.SaveChangesAsync();
    }

    private static TimesheetDto MapToDto(Timesheet timesheet)
    {
        var weekEnd = timesheet.WeekStartDate.AddDays(6);
        return new TimesheetDto
        {
            TimesheetId = timesheet.Id,
            EmployeeId = timesheet.EmployeeId,
            EmployeeName = timesheet.Employee?.FullName ?? string.Empty,
            WeekStartDate = timesheet.WeekStartDate.ToDateTime(TimeOnly.MinValue),
            WeekEndDate = weekEnd.ToDateTime(TimeOnly.MinValue),
            Status = timesheet.Status,
            TotalHours = timesheet.TotalHours,
            Entries = timesheet.Entries.Select(e => new TimesheetEntryDto
            {
                EntryId = e.Id,
                ProjectId = e.ProjectId,
                ProjectName = e.Project?.Name ?? string.Empty,
                Date = timesheet.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                Hours = e.Hours,
                ActivityTags = e.ActivityTags
            }).ToList()
        };
    }
}
