using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

/// <summary>Screen 3.3 ù View All Allocations (Admin read-only)</summary>
[ApiController]
[Route("api/allocations")]
[Authorize(Roles = RoleNames.Admin)]
public class AllocationsController(IAllocationService allocationService) : ControllerBase
{
    /// <summary>Screen 3.3 ù All active allocations</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allocations = await allocationService.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<AllocationDto>>.Ok(allocations));
    }
}
