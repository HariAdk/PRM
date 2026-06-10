namespace ProjectManagementSystem.Core.Constants;

public static class ApiRoutes
{
    public const string AuthLogin = "api/auth/login";
    public const string AuthSignup = "api/auth/signup";
    public static string AuthChangePassword(int userId) => $"api/auth/change-password/{userId}";

    public const string Users = "api/users";
    public static string UserResetPassword(int id) => $"api/users/{id}/reset-password";
    public static string UserDeactivate(int id) => $"api/users/{id}/deactivate";
    public static string UserReactivate(int id) => $"api/users/{id}/reactivate";

    public const string Employees = "api/employees";
    public static string EmployeeById(int id) => $"api/employees/{id}";
    public const string EmployeesAssignManager = "api/employees/assign-manager";
    public static string EmployeeDeactivate(int id) => $"api/employees/{id}/deactivate";
    public static string EmployeeSkills(int id) => $"api/employees/{id}/skills";
    public static string EmployeeSkill(int id, int skillId) => $"api/employees/{id}/skills/{skillId}";

    public const string Projects = "api/projects";
    public static string ProjectById(int id) => $"api/projects/{id}";
    public static string ProjectMilestones(int projectId) => $"api/projects/{projectId}/milestones";
    public static string ProjectMilestone(int projectId, int milestoneId) =>
        $"api/projects/{projectId}/milestones/{milestoneId}";

    public const string Allocations = "api/allocations";
    public const string Config = "api/config";

    public const string ManagerDashboard = "api/manager/dashboard";
    public static string ManagerEmployee(int id) => $"api/manager/employees/{id}";
    public const string ManagerProjects = "api/manager/projects";
    public static string ManagerProject(int id) => $"api/manager/projects/{id}";
    public static string ManagerProjectDetail(int id) => $"api/manager/projects/{id}/detail";
    public static string ManagerProjectMilestones(int projectId) => $"api/manager/projects/{projectId}/milestones";
    public static string ManagerProjectAllocations(int projectId) => $"api/manager/projects/{projectId}/allocations";
    public const string ManagerAllocations = "api/manager/allocations";
    public static string ManagerAllocationEnd(int id) => $"api/manager/allocations/{id}/end";
    public const string ManagerTimesheets = "api/manager/timesheets";
    public static string ManagerTimesheetsWithWeek(string weekStart) => $"api/manager/timesheets?weekStart={weekStart}";
    public static string ManagerTimesheet(int id) => $"api/manager/timesheets/{id}";
    public const string ManagerAiSkillMatch = "api/manager/ai/skill-match";
    public const string ManagerAiRiskSummary = "api/manager/ai/risk-summary";

    public const string EmployeeReminder = "api/employee/reminder";
    public const string EmployeeAllocations = "api/employee/allocations";
    public const string EmployeeTimesheetsContext = "api/employee/timesheets/context";
    public static string EmployeeTimesheetsContextWithWeek(string weekStart) =>
        $"api/employee/timesheets/context?weekStart={weekStart}";
    public const string EmployeeTimesheets = "api/employee/timesheets";
    public static string EmployeeTimesheet(int id) => $"api/employee/timesheets/{id}";
}
