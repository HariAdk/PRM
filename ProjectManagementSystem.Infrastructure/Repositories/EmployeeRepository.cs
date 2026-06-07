using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext db, IMapper mapper) : IEmployeeRepository
{
    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await db.Employees.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        return e is null ? null : mapper.Map<EmployeeDto>(e);
    }

    public async Task<EmployeeDto?> GetByUserIdAsync(int userId)
    {
        var e = await db.Employees.FirstOrDefaultAsync(x => x.UserId == userId);
        return e is null ? null : mapper.Map<EmployeeDto>(e);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var list = await db.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    /// <summary>
    /// Active employees whose user account has role Employee — individual contributors
    /// eligible for project allocation (Resource Dashboard, AI skill match).
    /// </summary>
    public async Task<IEnumerable<EmployeeDto>> GetAllocatableResourcesAsync()
    {
        var list = await db.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive && e.User.Role == UserRole.Employee)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    public async Task<bool> IsAllocatableResourceAsync(int employeeId) =>
        await db.Employees
            .Include(e => e.User)
            .AnyAsync(e => e.Id == employeeId && e.IsActive && e.User.Role == UserRole.Employee);

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var employee = mapper.Map<Employee>(dto);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var created = await db.Employees.Include(e => e.User).FirstAsync(e => e.Id == employee.Id);
        return mapper.Map<EmployeeDto>(created);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await db.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Employee {id} not found."); ;
        employee.FullName = dto.FullName;
        employee.Email = dto.Email;
        employee.Department = dto.Department;
        employee.Designation = dto.Designation;
        await db.SaveChangesAsync();
        return mapper.Map<EmployeeDto>(employee);
    }

    public async Task DeactivateAsync(int id)
    {
        var employee = await db.Employees.FindAsync(id)
                       ?? throw new KeyNotFoundException($"Employee {id} not found.");

        var activeAllocations = await db.Allocations
            .Where(a => a.EmployeeId == id && a.IsActive)
            .ToListAsync();

        foreach (var allocation in activeAllocations)
        {
            allocation.IsActive = false;
            allocation.ToDate = DateOnly.FromDateTime(DateTime.Today);
        }

        employee.IsActive = false;
        employee.Status = EmployeeStatus.Bench;

        var user = await db.Users.FindAsync(employee.UserId);
        if (user is not null) user.IsActive = false;

        await db.SaveChangesAsync();
    }

    public async Task<bool> UserHasEmployeeProfileAsync(int userId) =>
        await db.Employees.AnyAsync(e => e.UserId == userId);

    public async Task<bool> UserExistsForEmployeeAsync(int userId) =>
        await db.Users.AnyAsync(u => u.Id == userId && (u.Role == UserRole.Employee || u.Role == UserRole.Manager));

    public async Task SetStatusAsync(int employeeId, EmployeeStatus status)
    {
        var employee = await db.Employees.FindAsync(employeeId)
                       ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");
        employee.Status = status;
        await db.SaveChangesAsync();
    }
}
