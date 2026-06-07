using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.1 � Manage Employees</summary>
public class ManageEmployeesScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("MANAGE EMPLOYEES");

            ConsoleUI.Menu(1, "Add Employee");
            ConsoleUI.Menu(2, "View All Employees");
            ConsoleUI.Menu(3, "Update Employee");
            ConsoleUI.Menu(4, "Deactivate Employee");
            ConsoleUI.Menu(5, "Manage Employee Skills");
            ConsoleUI.Menu(6, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await AddEmployeeAsync();          break;
                case "2": await ViewAllEmployeesAsync();     break;
                case "3": await UpdateEmployeeAsync();       break;
                case "4": await DeactivateEmployeeAsync();   break;
                case "5": await new ManageSkillsScreen(api).ShowAsync(); break;
                case "6": return;
                default:
                    ConsoleUI.Error("Invalid option.");
                    ConsoleUI.PressAnyKey();
                    break;
            }
        }
    }

    private async Task AddEmployeeAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("ADD EMPLOYEE");

        Console.Write("User ID      : (from Manage Users ? View All Users) ");
        var userId = Console.ReadLine()?.Trim() ?? string.Empty;
        var fullName    = ConsoleUI.Prompt("Full Name    ");
        var email       = ConsoleUI.Prompt("Email        ");
        var department  = ConsoleUI.Prompt("Department   ");
        var designation = ConsoleUI.Prompt("Designation  ");

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        var opt = ConsoleUI.PromptOption();
        if (opt.ToUpper() == "B") return;

        if (!int.TryParse(userId, out var uid))
        {
            ConsoleUI.Error("User ID must be a number.");
            ConsoleUI.PressAnyKey();
            return;
        }

        var (data, error) = await api.CreateEmployeeAsync(new CreateEmployeeDto
        {
            UserId      = uid,
            FullName    = fullName,
            Email       = email,
            Department  = department,
            Designation = designation
        });

        if (error is not null) ConsoleUI.Error(error);
        else
        {
            var msg = data!.UserRole.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase)
                ? $"Manager profile linked. ID: {data.Id}"
                : $"Employee added with status BENCH. ID: {data.Id}";
            ConsoleUI.Success(msg);
        }

        ConsoleUI.PressAnyKey();
    }

    private async Task ViewAllEmployeesAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("ALL EMPLOYEES");

        var (employees, error) = await api.GetEmployeesAsync();
        if (error is not null) { ConsoleUI.Error(error); ConsoleUI.PressAnyKey(); return; }

        var list = employees?.ToList() ?? [];

        RenderEmployeeTable(list);

        ConsoleUI.Divider();
        var resources = list.Where(e => e.IsAllocatableResource).ToList();
        var allocated = resources.Count(e => e.Status.Equals(nameof(Core.Enums.EmployeeStatus.Allocated), StringComparison.OrdinalIgnoreCase));
        var bench = resources.Count - allocated;
        var managers = list.Count(e => !e.IsAllocatableResource);
        Console.WriteLine($"Total: {list.Count}   |   Allocated: {allocated}   |   Bench: {bench}" +
                          (managers > 0 ? $"   |   Managers (N/A): {managers}" : string.Empty));
        ConsoleUI.BlankLine();
        ConsoleUI.ActionBar("[F] Filter by Status / Department", "[B] Back");

        var opt = ConsoleUI.PromptOption();
        if (opt.ToUpper() == "F") await FilterEmployeesAsync(list);
    }

    private async Task FilterEmployeesAsync(List<EmployeeDto> list)
    {
        Console.Clear();
        ConsoleUI.DrawBox("FILTER EMPLOYEES");
        ConsoleUI.Menu(1, "Filter by Status (Bench / Allocated)");
        ConsoleUI.Menu(2, "Filter by Department");
        ConsoleUI.Menu(3, "Back");
        var opt = ConsoleUI.PromptOption();

        IEnumerable<EmployeeDto> filtered = list;
        if (opt == "1")
        {
            var status = ConsoleUI.Prompt("Status (Bench / Allocated)");
            filtered = list.Where(e =>
                e.IsAllocatableResource &&
                e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        else if (opt == "2")
        {
            var dept = ConsoleUI.Prompt("Department");
            filtered = list.Where(e => e.Department.Contains(dept, StringComparison.OrdinalIgnoreCase));
        }
        else return;

        Console.Clear();
        ConsoleUI.DrawBox("FILTERED EMPLOYEES");
        RenderEmployeeTable(filtered);

        ConsoleUI.PressAnyKey();
    }

    private static void RenderEmployeeTable(IEnumerable<EmployeeDto> employees)
    {
        ConsoleUI.RenderTable(
            ["ID", "Name", "Role", "Department", "Status"],
            employees.Select(e => new[]
            {
                e.Id.ToString(),
                e.FullName,
                ConsoleUI.StatusUpper(e.UserRole),
                e.Department,
                EmployeeAvailabilityLabels.ProfileStatus(e.UserRole, e.Status)
            }),
            rightAlignColumnIndexes: [0]);
    }

    private async Task UpdateEmployeeAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("UPDATE EMPLOYEE");

        var idStr = ConsoleUI.Prompt("Enter Employee ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (emp, err) = await api.GetEmployeeAsync(id);
        if (err is not null) { ConsoleUI.Error(err); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.BlankLine();
        Console.WriteLine($"Current: {emp!.FullName} | {emp.Department} | {emp.Designation}");
        ConsoleUI.Divider();

        var fullName    = ConsoleUI.Prompt("Full Name    (Enter to keep current)");
        var email       = ConsoleUI.Prompt("Email        (Enter to keep current)");
        var department  = ConsoleUI.Prompt("Department   (Enter to keep current)");
        var designation = ConsoleUI.Prompt("Designation  (Enter to keep current)");

        var dto = new UpdateEmployeeDto
        {
            FullName    = string.IsNullOrEmpty(fullName)    ? emp.FullName    : fullName,
            Email       = string.IsNullOrEmpty(email)       ? emp.Email       : email,
            Department  = string.IsNullOrEmpty(department)  ? emp.Department  : department,
            Designation = string.IsNullOrEmpty(designation) ? emp.Designation : designation
        };

        ConsoleUI.ActionBar("[S] Save", "[B] Back");
        if (ConsoleUI.PromptOption().ToUpper() == "B") return;

        var (_, error) = await api.UpdateEmployeeAsync(id, dto);
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Employee updated.");
        ConsoleUI.PressAnyKey();
    }

    private async Task DeactivateEmployeeAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("DEACTIVATE EMPLOYEE");

        var idStr = ConsoleUI.Prompt("Enter Employee ID");
        if (!int.TryParse(idStr, out var id)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (emp, err) = await api.GetEmployeeAsync(id);
        if (err is not null) { ConsoleUI.Error(err); ConsoleUI.PressAnyKey(); return; }

        ConsoleUI.BlankLine();
        ConsoleUI.SubHeader(emp!.FullName);
        Console.WriteLine($"Department : {emp.Department}");
        Console.WriteLine($"Status     : {ConsoleUI.StatusUpper(emp.Status)}");
        ConsoleUI.BlankLine();
        ConsoleUI.Warning("This will: set is_active = false, end all active allocations today, and block their login account.");
        ConsoleUI.BlankLine();
        Console.WriteLine($"Are you sure you want to deactivate {emp.FullName}?");
        ConsoleUI.ActionBar("[Y] Yes, Deactivate", "[B] Cancel");

        var opt = ConsoleUI.PromptOption();
        if (opt.ToUpper() != "Y") return;

        var (_, error) = await api.DeactivateEmployeeAsync(id);
        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success("Employee deactivated.");
        ConsoleUI.PressAnyKey();
    }
}
