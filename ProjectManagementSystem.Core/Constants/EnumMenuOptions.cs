using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Constants;

/// <summary>Ordered enum labels for console menus — keeps UI aligned with domain enums.</summary>
public static class EnumMenuOptions
{
    public static readonly string[] UserRoles = [RoleNames.Admin, RoleNames.Manager, RoleNames.Employee];
    public static readonly string[] ProjectStatuses = Enum.GetNames<ProjectStatus>();
    public static readonly string[] MilestoneStatuses = Enum.GetNames<MilestoneStatus>();
    public static readonly string[] SkillCategories = Enum.GetNames<SkillCategory>();
    public static readonly string[] ProficiencyLevels = Enum.GetNames<ProficiencyLevel>();
}
