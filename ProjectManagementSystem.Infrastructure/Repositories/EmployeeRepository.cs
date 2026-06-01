using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await db.Employees.FindAsync(id);
        return e is null ? null : MapToDto(e);
    }

    public async Task<EmployeeDto?> GetByUserIdAsync(int userId)
    {
        var e = await db.Employees.FirstOrDefaultAsync(x => x.UserId == userId);
        return e is null ? null : MapToDto(e);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var list = await db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
        return list.Select(MapToDto);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            UserId = dto.UserId,
            FullName = dto.FullName,
            Email = dto.Email,
            Department = dto.Department,
            Designation = dto.Designation,
            Status = EmployeeStatus.Bench,
            IsActive = true
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return MapToDto(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await db.Employees.FindAsync(id)
                       ?? throw new KeyNotFoundException($"Employee {id} not found.");
        employee.FullName = dto.FullName;
        employee.Email = dto.Email;
        employee.Department = dto.Department;
        employee.Designation = dto.Designation;
        await db.SaveChangesAsync();
        return MapToDto(employee);
    }

    public async Task DeactivateAsync(int id)
    {
        var employee = await db.Employees.FindAsync(id)
                       ?? throw new KeyNotFoundException($"Employee {id} not found.");

        // End all active allocations
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

        // Block linked user account
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

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        FullName = e.FullName,
        Email = e.Email,
        Department = e.Department,
        Designation = e.Designation,
        Status = e.Status.ToString(),
        IsActive = e.IsActive
    };
}
