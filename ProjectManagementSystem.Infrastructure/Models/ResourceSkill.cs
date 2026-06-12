using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class ResourceSkill
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }

    public Resource Resource { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
