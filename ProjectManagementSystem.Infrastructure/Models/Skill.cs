using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = [];
}
