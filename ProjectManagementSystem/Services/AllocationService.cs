using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Services;

public class AllocationService(
    IAllocationRepository allocationRepo,
    IEmployeeRepository employeeRepo,
    IProjectRepository projectRepo) : IAllocationService
{
    public async Task<IEnumerable<AllocationDto>> GetAllActiveAsync() =>
        await allocationRepo.GetAllActiveAsync();

    public async Task<IEnumerable<AllocationDto>> GetByProjectIdAsync(int projectId) =>
        await allocationRepo.GetByProjectIdAsync(projectId);

    public async Task<AllocationDto?> GetByIdAsync(int id) =>
        await allocationRepo.GetByIdAsync(id);

    public async Task<AllocationDto> CreateAsync(CreateAllocationDto dto)
    {
        await ValidateEmployeeAsync(dto.EmployeeId);
        await ValidateProjectOpenForAllocationAsync(dto.ProjectId);
        ValidateDates(dto.FromDate, dto.ToDate);
        ValidateUtilisation(dto.UtilisationPercent);
        await EnsureNoOverAllocationAsync(dto.EmployeeId, dto.FromDate, dto.ToDate, dto.UtilisationPercent);

        return await allocationRepo.CreateAsync(dto);
    }

    public async Task<AllocationDto> EndAsync(int id, DateOnly endDate)
    {
        var existing = await allocationRepo.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Allocation {id} not found.");

        if (!existing.IsActive)
            throw new InvalidOperationException(ErrorMessages.AllocationAlreadyEnded);

        if (endDate < existing.FromDate)
            throw new InvalidOperationException("End date cannot be before allocation start date.");

        return await allocationRepo.UpdateEndDateAsync(id, endDate);
    }

    private async Task ValidateEmployeeAsync(int employeeId)
    {
        var employee = await employeeRepo.GetByIdAsync(employeeId)
                       ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

        if (!employee.IsActive)
            throw new InvalidOperationException("Cannot allocate an inactive employee.");

        if (!await employeeRepo.IsAllocatableResourceAsync(employeeId))
            throw new InvalidOperationException(ErrorMessages.OnlyEmployeesCanBeAllocated);
    }

    private async Task ValidateProjectOpenForAllocationAsync(int projectId)
    {
        var project = await projectRepo.GetByIdAsync(projectId)
                      ?? throw new KeyNotFoundException($"Project {projectId} not found.");

        var isOpen = string.Equals(project.Status, nameof(ProjectStatus.Active), StringComparison.OrdinalIgnoreCase)
                     || string.Equals(project.Status, nameof(ProjectStatus.Planned), StringComparison.OrdinalIgnoreCase);

        if (!isOpen)
            throw new InvalidOperationException("Project is not open for allocation.");
    }

    private static void ValidateDates(DateOnly from, DateOnly to)
    {
        if (to < from)
            throw new InvalidOperationException("End date must be on or after start date.");
    }

    private static void ValidateUtilisation(int percent)
    {
        if (percent is < AllocationLimits.MinUtilisationPercent or > AllocationLimits.MaxUtilisationPercent)
        {
            throw new InvalidOperationException(
                $"Utilisation must be between {AllocationLimits.MinUtilisationPercent} " +
                $"and {AllocationLimits.MaxUtilisationPercent} percent.");
        }
    }

    private async Task EnsureNoOverAllocationAsync(
        int employeeId, DateOnly from, DateOnly to, int newPercent, int? excludeAllocationId = null)
    {
        var allocations = await allocationRepo.GetByEmployeeIdAsync(employeeId);
        var overlapping = allocations
            .Where(a => a.IsActive &&
                        a.Id != excludeAllocationId &&
                        a.FromDate <= to &&
                        a.ToDate >= from)
            .ToList();

        var total = overlapping.Sum(a => a.UtilisationPercent) + newPercent;
        if (total > AllocationLimits.MaxTotalUtilisationPercent)
        {
            var current = overlapping.Sum(a => a.UtilisationPercent);
            throw new InvalidOperationException(
                $"Over-allocation detected. Employee already has {current}% allocated in this period. " +
                $"Adding {newPercent}% would exceed {AllocationLimits.MaxTotalUtilisationPercent}%.");
        }
    }
}
