using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Repositories;

public class AllocationRepository(AppDbContext db, IMapper mapper) : IAllocationRepository
{
    public async Task<IEnumerable<AllocationDto>> GetAllActiveAsync()
    {
        var list = await db.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .Where(a => a.IsActive)
            .OrderBy(a => a.Employee.FullName)
            .ToListAsync();

        return mapper.Map<IEnumerable<AllocationDto>>(list);
    }

    public async Task<IEnumerable<AllocationDto>> GetAllAsync()
    {
        var list = await db.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .OrderBy(a => a.Employee.FullName)
            .ToListAsync();

        return mapper.Map<IEnumerable<AllocationDto>>(list);
    }

    public async Task<IEnumerable<AllocationDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var list = await db.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.FromDate)
            .ToListAsync();

        return mapper.Map<IEnumerable<AllocationDto>>(list);
    }

    public async Task<IEnumerable<AllocationDto>> GetByProjectIdAsync(int projectId)
    {
        var list = await db.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .Where(a => a.ProjectId == projectId && a.IsActive)
            .OrderBy(a => a.Employee.FullName)
            .ToListAsync();

        return mapper.Map<IEnumerable<AllocationDto>>(list);
    }

    public async Task<AllocationDto?> GetByIdAsync(int id)
    {
        var allocation = await db.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == id);

        return allocation is null ? null : mapper.Map<AllocationDto>(allocation);
    }

    public async Task<AllocationDto> CreateAsync(CreateAllocationDto dto)
    {
        var allocation = mapper.Map<Allocation>(dto);
        db.Allocations.Add(allocation);
        await db.SaveChangesAsync();

        return (await GetByIdAsync(allocation.Id))!;
    }

    public async Task<AllocationDto> UpdateEndDateAsync(int id, DateOnly endDate)
    {
        var allocation = await db.Allocations.FindAsync(id)
                         ?? throw new NotFoundException(ErrorMessages.AllocationNotFoundById(id));

        allocation.ToDate = endDate;
        if (endDate <= DateOnly.FromDateTime(DateTime.Today))
            allocation.IsActive = false;

        await db.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<IEnumerable<int>> GetEmployeeIdsAllocatedBetweenAsync(DateOnly from, DateOnly to)
    {
        return await db.Allocations
            .Where(a => a.IsActive && a.FromDate <= to && a.ToDate >= from)
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync();
    }
}
