namespace ProjectManagementSystem.Core.DTOs.Employee;

public record AssignManagerDto
{
    public int EmployeeUserId { get; init; }
    public int ManagerUserId { get; init; }
}
