using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Notification;

namespace ProjectManagementSystem.Application.Email;

public static class EmailTemplates
{
    public static string TimesheetFreezeEmployee(string employeeName, DateOnly weekStart) =>
        $"""
        Dear {employeeName},

        Your timesheet for the week starting {weekStart:dd-MMM-yyyy} was not submitted after two reminders.
        Timesheet submission access has been frozen. You can still log in and view your data, but you cannot create, update, or submit timesheet entries until your reporting manager restores access.

        Please contact your manager to resolve this.
        """;

    public static string TimesheetFreezeManager(string employeeName, DateOnly weekStart) =>
        $"""
        Dear Manager,

        {employeeName} did not submit a timesheet for the week starting {weekStart:dd-MMM-yyyy} after two reminders.
        Their timesheet submission access has been frozen. You can restore access from the manager portal after reviewing the situation.
        """;

    public static string ProjectAtRisk(
        AtRiskProjectEmailDto project,
        string healthLabel,
        string aiSummary,
        IReadOnlyList<AIMatchedEmployeeDto> skillSuggestions) =>
        $"""
        Dear {project.ManagerName},

        Project "{project.ProjectName}" has been flagged as AT RISK.

        HEALTH STATUS: {healthLabel}

        PROJECT DETAILS
        Manager: {project.ManagerName}
        Key milestones: {project.MilestoneSummary}

        AI RISK SUMMARY
        {aiSummary}

        SUGGESTED HELP (available employees)
        {FormatSkillSuggestions(skillSuggestions)}

        This is a one-time automated alert for this at-risk period. Please review the project dashboard for full details.
        """;

    public static string HealthLabel(string healthStatus) => healthStatus switch
    {
        nameof(Core.Enums.ProjectHealth.AtRisk) => "RED - At Risk",
        nameof(Core.Enums.ProjectHealth.Attention) => "AMBER - Needs Attention",
        _ => "GREEN - On Track"
    };

    private static string FormatSkillSuggestions(IReadOnlyList<AIMatchedEmployeeDto> matches)
    {
        if (matches.Count == 0)
            return "No matching bench or partially available employees found at this time.";

        return string.Join(Environment.NewLine,
            matches.Take(5).Select(m =>
                $"- {m.Name}: {m.SkillsMatch} — {m.AvailabilityPercentage}% available. {m.Reason}"));
    }
}
