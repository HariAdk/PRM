using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Timesheet;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class TimesheetRepository(AppDbContext db, IMapper mapper) : ITimesheetRepository
{
    private IQueryable<Timesheet> WithIncludes() =>
        db.Timesheets
            .Include(t => t.Resource).ThenInclude(r => r.User)
            .Include(t => t.Entries).ThenInclude(e => e.Project);

    public async Task<IEnumerable<TimesheetDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var list = await WithIncludes()
            .Where(t => t.ResourceId == employeeId)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();
        return mapper.Map<IEnumerable<TimesheetDto>>(list);
    }

    public async Task<IEnumerable<TimesheetDto>> GetByWeekStartAsync(DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var list = await WithIncludes().Where(t => t.WeekStartDate == weekStartDate).ToListAsync();
        return mapper.Map<IEnumerable<TimesheetDto>>(list);
    }

    public async Task<IEnumerable<string>> GetRecentActivityTagsAsync(int employeeId, int count = 5) =>
        await db.TimesheetEntries
            .Where(e => e.Timesheet.ResourceId == employeeId && e.ActivityTags != string.Empty)
            .OrderByDescending(e => e.Timesheet.WeekStartDate)
            .Select(e => e.ActivityTags)
            .Distinct()
            .Take(count)
            .ToListAsync();

    public async Task<TimesheetDto?> GetByIdAsync(int timesheetId)
    {
        var timesheet = await WithIncludes().FirstOrDefaultAsync(t => t.Id == timesheetId);
        return timesheet is null ? null : mapper.Map<TimesheetDto>(timesheet);
    }

    public async Task<TimesheetDto?> GetByEmployeeAndWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        var timesheet = await WithIncludes()
            .FirstOrDefaultAsync(t => t.ResourceId == employeeId && t.WeekStartDate == weekStartDate);
        return timesheet is null ? null : mapper.Map<TimesheetDto>(timesheet);
    }

    public async Task<bool> HasSubmittedForWeekAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        return await db.Timesheets.AnyAsync(t =>
            t.ResourceId == employeeId &&
            t.WeekStartDate == weekStartDate &&
            t.Status == TimesheetStatus.Submitted);
    }

    public async Task<TimesheetDto> CreateAsync(CreateTimesheetDto dto)
    {
        var timesheet = mapper.Map<Timesheet>(dto);
        db.Timesheets.Add(timesheet);
        await db.SaveChangesAsync();

        foreach (var entryDto in dto.Entries)
        {
            var entry = mapper.Map<TimesheetEntry>(entryDto);
            entry.TimesheetId = timesheet.Id;
            db.TimesheetEntries.Add(entry);
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
            t.ResourceId == employeeId && t.WeekStartDate == weekStartDate);
    }

    public async Task CreateMissedAsync(int employeeId, DateTime weekStart)
    {
        var weekStartDate = DateOnly.FromDateTime(weekStart);
        if (await db.Timesheets.AnyAsync(t => t.ResourceId == employeeId && t.WeekStartDate == weekStartDate))
            return;

        db.Timesheets.Add(new Timesheet
        {
            ResourceId = employeeId,
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
            ?? throw new NotFoundException(ErrorMessages.TimesheetNotFoundById(timesheetId));

        if (timesheet.Status == TimesheetStatus.Submitted)
            throw new BusinessRuleException(ErrorMessages.TimesheetAlreadySubmitted);

        db.TimesheetEntries.RemoveRange(timesheet.Entries);
        foreach (var entryDto in dto.Entries)
        {
            var entry = mapper.Map<TimesheetEntry>(entryDto);
            entry.TimesheetId = timesheet.Id;
            db.TimesheetEntries.Add(entry);
        }

        timesheet.TotalHours = dto.Entries.Sum(e => e.Hours);
        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (await GetByIdAsync(timesheetId))!;
    }
}
