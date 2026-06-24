using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext db, IMapper mapper) : IEmployeeRepository
{
    private IQueryable<Resource> ActiveResourcesQuery() =>
        db.Resources
            .Include(r => r.User).ThenInclude(u => u.Role)
            .Include(r => r.ReportingManager)
            .Where(r => r.User.IsActive);

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var resource = await ActiveResourcesQuery().FirstOrDefaultAsync(r => r.Id == id);
        return resource is null ? null : mapper.Map<EmployeeDto>(resource);
    }

    public async Task<EmployeeDto?> GetByUserIdAsync(int userId)
    {
        var resource = await ActiveResourcesQuery().FirstOrDefaultAsync(r => r.UserId == userId);
        return resource is null ? null : mapper.Map<EmployeeDto>(resource);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var list = await ActiveResourcesQuery().OrderBy(r => r.User.FullName).ToListAsync();
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllocatableResourcesAsync()
    {
        var employeeRoleId = await RoleResolver.GetEmployeeRoleIdAsync(db);
        var list = await ActiveResourcesQuery()
            .Where(r => r.User.RoleId == employeeRoleId)
            .OrderBy(r => r.User.FullName)
            .ToListAsync();
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    public async Task<bool> IsAllocatableResourceAsync(int employeeId) =>
        await db.Resources
            .Include(r => r.User).ThenInclude(u => u.Role)
            .AnyAsync(r => r.Id == employeeId &&
                           r.User.IsActive &&
                           r.User.Role.Name == RoleNames.Employee);

    public async Task<IEnumerable<EmployeeDto>> GetTeamAllocatableResourcesAsync(int managerUserId)
    {
        var employeeRoleId = await RoleResolver.GetEmployeeRoleIdAsync(db);
        var list = await ActiveResourcesQuery()
            .Where(r => r.User.RoleId == employeeRoleId && r.ReportingManagerId == managerUserId)
            .OrderBy(r => r.User.FullName)
            .ToListAsync();
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    public async Task<bool> IsOnManagerTeamAsync(int managerUserId, int employeeId) =>
        await db.Resources
            .Include(r => r.User)
            .AnyAsync(r => r.Id == employeeId &&
                           r.User.IsActive &&
                           r.ReportingManagerId == managerUserId);

    public async Task<IEnumerable<int>> GetTeamEmployeeIdsAsync(int managerUserId) =>
        await db.Resources
            .Where(r => r.User.IsActive && r.ReportingManagerId == managerUserId)
            .Select(r => r.Id)
            .ToListAsync();

    public async Task<EmployeeDto> AssignManagerAsync(int employeeUserId, int managerUserId)
    {
        var resource = await db.Resources
            .Include(r => r.User).ThenInclude(u => u.Role)
            .Include(r => r.ReportingManager)
            .FirstOrDefaultAsync(r => r.UserId == employeeUserId)
            ?? throw new NotFoundException(ErrorMessages.EmployeeProfileNotFoundForUser(employeeUserId));

        resource.ReportingManagerId = managerUserId;
        resource.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.Entry(resource).Reference(r => r.ReportingManager).LoadAsync();
        return mapper.Map<EmployeeDto>(resource);
    }

    public async Task<EmployeeDto> CreateProfileForUserAsync(int userId, string fullName, string email)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new NotFoundException(ErrorMessages.UserNotFoundById(userId));

        user.FullName = fullName;
        user.Email = email;
        if (string.IsNullOrWhiteSpace(user.Designation))
            user.Designation = SystemDefaults.UnassignedDesignation;
        user.UpdatedAt = DateTime.UtcNow;

        var now = DateTime.UtcNow;
        var resource = new Resource
        {
            UserId = userId,
            Status = EmployeeStatus.Bench,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Resources.Add(resource);
        await db.SaveChangesAsync();

        var created = await ActiveResourcesQuery().FirstAsync(r => r.Id == resource.Id);
        return mapper.Map<EmployeeDto>(created);
    }

    public Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto) =>
        throw new NotSupportedException("Create employee profile via user creation.");

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var resource = await db.Resources
            .Include(r => r.User).ThenInclude(u => u.Role)
            .Include(r => r.ReportingManager)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(ErrorMessages.EmployeeNotFoundById(id));

        mapper.Map(dto, resource.User);
        resource.User.UpdatedAt = DateTime.UtcNow;
        resource.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return mapper.Map<EmployeeDto>(resource);
    }

    public async Task DeactivateAsync(int id)
    {
        var resource = await db.Resources
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(ErrorMessages.EmployeeNotFoundById(id));

        foreach (var allocation in await db.Allocations
                     .Where(a => a.ResourceId == id && a.IsActive).ToListAsync())
        {
            allocation.IsActive = false;
            allocation.ToDate = DateOnly.FromDateTime(DateTime.Today);
        }

        resource.Status = EmployeeStatus.Bench;
        resource.User.IsActive = false;
        resource.User.UpdatedAt = DateTime.UtcNow;
        resource.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<bool> UserHasEmployeeProfileAsync(int userId) =>
        await db.Resources.AnyAsync(r => r.UserId == userId);

    public async Task<bool> UserExistsForEmployeeAsync(int userId) =>
        await db.Users
            .Include(u => u.Role)
            .AnyAsync(u => u.Id == userId &&
                           (u.Role.Name == RoleNames.Employee || u.Role.Name == RoleNames.Manager));

    public async Task SetStatusAsync(int employeeId, EmployeeStatus status)
    {
        var resource = await db.Resources.FindAsync(employeeId)
                       ?? throw new NotFoundException(ErrorMessages.EmployeeNotFoundById(employeeId));
        resource.Status = status;
        resource.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
