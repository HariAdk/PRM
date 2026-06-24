namespace ProjectManagementSystem.Core.DTOs.Allocation;

public class CreateAllocationDto
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public int UtilisationPercent { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
}
