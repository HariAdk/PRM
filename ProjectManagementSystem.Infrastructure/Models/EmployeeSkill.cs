using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class EmployeeSkill
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
