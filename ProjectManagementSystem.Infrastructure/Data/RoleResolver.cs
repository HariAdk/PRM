using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Constants;

namespace ProjectManagementSystem.Infrastructure.Data;

internal static class RoleResolver
{
    public static async Task<int> GetRoleIdAsync(AppDbContext db, string roleName)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                   ?? throw new InvalidOperationException($"Role '{roleName}' is not seeded.");
        return role.Id;
    }

    public static Task<int> GetEmployeeRoleIdAsync(AppDbContext db) =>
        GetRoleIdAsync(db, RoleNames.Employee);
}
