using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.Interfaces.Services;

namespace ProjectManagementSystem.Controllers;

[ApiController]
[Route("api/allocations")]
[Authorize(Roles = RoleNames.Admin)]
public class AllocationsController(IAllocationService allocationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allocations = await allocationService.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<AllocationDto>>.Ok(allocations));
    }
}
