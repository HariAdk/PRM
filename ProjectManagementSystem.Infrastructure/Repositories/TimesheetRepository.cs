using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class TimesheetRepository(AppDbContext db, IMapper mapper) : ITimesheetRepository
{
    public async Task<IEnumerable<TimesheetDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var list = await db.Timesheets
            .Where(t => t.EmployeeId == employeeId)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();

        return mapper.Map<IEnumerable<TimesheetDto>>(list);
    }

    public async Task<IEnumerable<TimesheetDto>> GetByWeekStartAsync(DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var list = await db.Timesheets
            .Where(t => t.WeekStartDate == weekStartDate)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .ToListAsync();

        return mapper.Map<IEnumerable<TimesheetDto>>(list);
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

        return timesheet is null ? null : mapper.Map<TimesheetDto>(timesheet);
    }

    public async Task<TimesheetDto?> GetByEmployeeAndWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var timesheet = await db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.WeekStartDate == weekStartDate);

        return timesheet is null ? null : mapper.Map<TimesheetDto>(timesheet);
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
            db.TimesheetEntries.Add(new TimesheetEntry
            {
                TimesheetId = timesheet.Id,
                ProjectId = entryDto.ProjectId,
                Hours = entryDto.Hours,
                ActivityTags = entryDto.ActivityTags
            });
        }

        await db.SaveChangesAsync();

        return (await GetByIdAsync(timesheet.Id))!;
    }

    public async Task<bool> SubmitAsync(int timesheetId)
    {
        var timesheet = await db.Timesheets.FindAsync(timesheetId);
        if (timesheet is null) return false;

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

    public async Task<TimesheetDto> ReplaceEntriesAndSubmitAsync(int timesheetId, CreateTimesheetDto dto)
    {
        var timesheet = await db.Timesheets
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == timesheetId)
            ?? throw new KeyNotFoundException(ErrorMessages.TimesheetNotFoundById(timesheetId));

        if (timesheet.Status == TimesheetStatus.Submitted)
            throw new InvalidOperationException(ErrorMessages.TimesheetAlreadySubmitted);

        db.TimesheetEntries.RemoveRange(timesheet.Entries);

        foreach (var entryDto in dto.Entries)
        {
            db.TimesheetEntries.Add(new TimesheetEntry
            {
                TimesheetId = timesheet.Id,
                ProjectId = entryDto.ProjectId,
                Hours = entryDto.Hours,
                ActivityTags = entryDto.ActivityTags
            });
        }

        timesheet.TotalHours = dto.Entries.Sum(e => e.Hours);
        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return (await GetByIdAsync(timesheetId))!;
    }
}
