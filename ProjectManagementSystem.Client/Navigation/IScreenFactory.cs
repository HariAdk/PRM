using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Client.Navigation;

public interface IScreenFactory
{
    IScreen CreateRoleMenu(UserRole role);
    IScreen CreateAdminScreen(AdminMenuAction action);
    IScreen CreateAdminEmployeeScreen(AdminEmployeeMenuAction action);
    IScreen CreateManagerScreen(ManagerMenuAction action);
    IScreen CreateEmployeeScreen(EmployeeMenuAction action);
}
