using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens;
using ProjectManagementSystem.Client.Screens.Admin;
using ProjectManagementSystem.Client.Screens.Employee;
using ProjectManagementSystem.Client.Screens.Manager;
using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Client.Navigation;

public sealed class ScreenFactory(ApiClient api, SessionContext session) : IScreenFactory
{
    public IScreen CreateRoleMenu(UserRole role) => role switch
    {
        UserRole.Admin => new AdminMenuScreen(api, session, this),
        UserRole.Manager => new ManagerMenuScreen(api, session, this),
        UserRole.Employee => new EmployeeMenuScreen(api, session, this),
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported user role.")
    };

    public IScreen CreateAdminScreen(AdminMenuAction action) => action switch
    {
        AdminMenuAction.ManageEmployees => new ManageEmployeesScreen(api, this),
        AdminMenuAction.ManageProjects => new ManageProjectsScreen(api),
        AdminMenuAction.ViewAllocations => new ViewAllocationsScreen(api),
        AdminMenuAction.ManageUsers => new ManageUsersScreen(api),
        AdminMenuAction.SystemConfiguration => new SystemConfigScreen(api),
        AdminMenuAction.Logout => throw new InvalidOperationException("Logout is handled by the menu screen."),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    public IScreen CreateAdminEmployeeScreen(AdminEmployeeMenuAction action) => action switch
    {
        AdminEmployeeMenuAction.ManageSkills => new ManageSkillsScreen(api),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    public IScreen CreateManagerScreen(ManagerMenuAction action) => action switch
    {
        ManagerMenuAction.ResourceDashboard => new ResourceDashboardScreen(api),
        ManagerMenuAction.AllocateResource => new AllocateResourceScreen(api),
        ManagerMenuAction.MyProjects => new MyProjectsScreen(api),
        ManagerMenuAction.Timesheets => new TimesheetManagerScreen(api),
        ManagerMenuAction.AiAssistant => new AiAssistantScreen(api),
        ManagerMenuAction.Logout => throw new InvalidOperationException("Logout is handled by the menu screen."),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    public IScreen CreateEmployeeScreen(EmployeeMenuAction action) => action switch
    {
        EmployeeMenuAction.SubmitTimesheet => new SubmitTimesheetScreen(api),
        EmployeeMenuAction.ViewMyTimesheets => new ViewTimesheetsScreen(api),
        EmployeeMenuAction.ViewMyAllocations => new ViewMyAllocationsScreen(api),
        EmployeeMenuAction.Logout => throw new InvalidOperationException("Logout is handled by the menu screen."),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}
