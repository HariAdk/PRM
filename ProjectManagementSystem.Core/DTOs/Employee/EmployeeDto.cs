namespace ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.Constants;

public class EmployeeDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Bench/allocated applies to individual contributors only — not delivery managers.</summary>
    public bool IsAllocatableResource =>
        UserRole.Equals(RoleNames.Employee, StringComparison.OrdinalIgnoreCase);
}