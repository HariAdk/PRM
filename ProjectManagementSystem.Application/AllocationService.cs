using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Exceptions;
using ProjectManagementSystem.Core.Interfaces.Repositories;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Application;

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

    public async Task<AllocationDto> CreateAsync(CreateAllocationDto dto, int? managerUserId = null)
    {
        await ValidateEmployeeAsync(dto.EmployeeId);

        if (managerUserId.HasValue &&
            !await employeeRepo.IsOnManagerTeamAsync(managerUserId.Value, dto.EmployeeId))
        {
            throw new ForbiddenAppException(ErrorMessages.EmployeeNotOnTeam);
        }

        await ValidateProjectOpenForAllocationAsync(dto.ProjectId);
        ValidateDates(dto.FromDate, dto.ToDate);
        ValidateUtilisation(dto.UtilisationPercent);
        await EnsureNoOverAllocationAsync(dto.EmployeeId, dto.FromDate, dto.ToDate, dto.UtilisationPercent);

        return await allocationRepo.CreateAsync(dto);
    }

    public async Task<AllocationDto> EndAsync(int id, DateOnly endDate)
    {
        var existing = await allocationRepo.GetByIdAsync(id)
                       ?? throw new NotFoundException(ErrorMessages.AllocationNotFoundById(id));

        if (!existing.IsActive)
            throw new BusinessRuleException(ErrorMessages.AllocationAlreadyEnded);

        if (endDate < existing.FromDate)
            throw new BusinessRuleException(ErrorMessages.AllocationEndBeforeStart);

        return await allocationRepo.UpdateEndDateAsync(id, endDate);
    }

    private async Task ValidateEmployeeAsync(int employeeId)
    {
        var employee = await employeeRepo.GetByIdAsync(employeeId)
                       ?? throw new NotFoundException(ErrorMessages.EmployeeNotFoundById(employeeId));

        if (!employee.IsActive)
            throw new BusinessRuleException(ErrorMessages.InactiveEmployeeCannotAllocate);

        if (!await employeeRepo.IsAllocatableResourceAsync(employeeId))
            throw new BusinessRuleException(ErrorMessages.OnlyEmployeesCanBeAllocated);
    }

    private async Task ValidateProjectOpenForAllocationAsync(int projectId)
    {
        var project = await projectRepo.GetByIdAsync(projectId)
                      ?? throw new NotFoundException(ErrorMessages.ProjectNotFoundById(projectId));

        var isOpen = string.Equals(project.Status, nameof(ProjectStatus.Active), StringComparison.OrdinalIgnoreCase)
                     || string.Equals(project.Status, nameof(ProjectStatus.Planned), StringComparison.OrdinalIgnoreCase);

        if (!isOpen)
            throw new BusinessRuleException(ErrorMessages.ProjectNotOpenForAllocation);
    }

    private static void ValidateDates(DateOnly from, DateOnly to)
    {
        if (to < from)
            throw new BusinessRuleException(ErrorMessages.AllocationDateRangeInvalid);
    }

    private static void ValidateUtilisation(int percent)
    {
        if (percent is < AllocationLimits.MinUtilisationPercent or > AllocationLimits.MaxUtilisationPercent)
            throw new BusinessRuleException(ErrorMessages.UtilisationOutOfRange());
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
            throw new BusinessRuleException(ErrorMessages.OverAllocationDetected(current, newPercent));
        }
    }
}
