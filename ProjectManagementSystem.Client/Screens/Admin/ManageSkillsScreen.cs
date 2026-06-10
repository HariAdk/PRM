using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Helpers;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Employee;

namespace ProjectManagementSystem.Client.Screens.Admin;

/// <summary>Screen 3.1.4 — Manage Employee Skills</summary>
public class ManageSkillsScreen(ApiClient api)
{
    public async Task ShowAsync()
    {
        Console.Clear();
        ConsoleUI.DrawBox("MANAGE SKILLS");

        var idStr = ConsoleUI.Prompt("Enter Employee ID");
        if (!int.TryParse(idStr, out var empId)) { ConsoleUI.Error("Invalid ID."); ConsoleUI.PressAnyKey(); return; }

        var (emp, empErr) = await api.GetEmployeeAsync(empId);
        if (empErr is not null) { ConsoleUI.Error(empErr); ConsoleUI.PressAnyKey(); return; }

        while (true)
        {
            Console.Clear();
            ConsoleUI.DrawBox("MANAGE SKILLS");
            ConsoleUI.SubHeader(emp!.FullName);
            Console.WriteLine("Current Skills:");

            var (skills, skillErr) = await api.GetSkillsAsync(empId);
            if (skillErr is not null) { ConsoleUI.Error(skillErr); ConsoleUI.PressAnyKey(); return; }

            var list = skills?.ToList() ?? [];
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine($"  {i + 1}.  {list[i].SkillName,-20} {list[i].ProficiencyLevel}");

            ConsoleUI.Divider();
            ConsoleUI.Menu(1, "Add Skill");
            ConsoleUI.Menu(2, "Update Proficiency Level");
            ConsoleUI.Menu(3, "Remove Skill");
            ConsoleUI.Menu(4, "Back");

            var opt = ConsoleUI.PromptOption();
            switch (opt)
            {
                case "1": await AddSkillAsync(empId);        break;
                case "2": await UpdateSkillAsync(empId, list); break;
                case "3": await RemoveSkillAsync(empId, list); break;
                case "4": return;
                default:  ConsoleUI.Error("Invalid option."); ConsoleUI.PressAnyKey(); break;
            }
        }
    }

    private async Task AddSkillAsync(int empId)
    {
        ConsoleUI.BlankLine();
        var skillName = ConsoleUI.Prompt("Skill Name        ");

        Console.WriteLine("Category          : (1) Backend  (2) Frontend  (3) DevOps  (4) QA  (5) Other");
        var catChoice = ConsoleUI.Prompt("Enter choice      ");
        if (!int.TryParse(catChoice, out var ci) || ci < 1 || ci > EnumMenuOptions.SkillCategories.Length)
        { ConsoleUI.Error("Invalid category."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
        var profChoice = ConsoleUI.Prompt("Enter choice      ");
        if (!int.TryParse(profChoice, out var pi) || pi < 1 || pi > EnumMenuOptions.ProficiencyLevels.Length)
        { ConsoleUI.Error("Invalid proficiency."); ConsoleUI.PressAnyKey(); return; }

        var (_, error) = await api.AddSkillAsync(empId, new AddSkillDto
        {
            SkillName       = skillName,
            Category        = EnumMenuOptions.SkillCategories[ci - 1],
            ProficiencyLevel = EnumMenuOptions.ProficiencyLevels[pi - 1]
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success(SuccessMessages.SkillAdded);
        ConsoleUI.PressAnyKey();
    }

    private async Task UpdateSkillAsync(int empId, List<EmployeeSkillDto> list)
    {
        if (list.Count == 0) { ConsoleUI.Warning("No skills to update."); ConsoleUI.PressAnyKey(); return; }

        var idxStr = ConsoleUI.Prompt("Enter skill number");
        if (!int.TryParse(idxStr, out var idx) || idx < 1 || idx > list.Count)
        { ConsoleUI.Error("Invalid selection."); ConsoleUI.PressAnyKey(); return; }

        Console.WriteLine("Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
        var profChoice = ConsoleUI.Prompt("Enter choice      ");
        if (!int.TryParse(profChoice, out var pi) || pi < 1 || pi > EnumMenuOptions.ProficiencyLevels.Length)
        { ConsoleUI.Error("Invalid proficiency."); ConsoleUI.PressAnyKey(); return; }

        var (_, error) = await api.UpdateSkillAsync(empId, list[idx - 1].Id, new UpdateSkillDto
        {
            ProficiencyLevel = EnumMenuOptions.ProficiencyLevels[pi - 1]
        });

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success(SuccessMessages.ProficiencyUpdated);
        ConsoleUI.PressAnyKey();
    }

    private async Task RemoveSkillAsync(int empId, List<EmployeeSkillDto> list)
    {
        if (list.Count == 0) { ConsoleUI.Warning("No skills to remove."); ConsoleUI.PressAnyKey(); return; }

        var idxStr = ConsoleUI.Prompt("Enter skill number");
        if (!int.TryParse(idxStr, out var idx) || idx < 1 || idx > list.Count)
        { ConsoleUI.Error("Invalid selection."); ConsoleUI.PressAnyKey(); return; }

        var (_, error) = await api.RemoveSkillAsync(empId, list[idx - 1].Id);

        if (error is not null) ConsoleUI.Error(error);
        else ConsoleUI.Success(SuccessMessages.SkillRemoved);
        ConsoleUI.PressAnyKey();
    }
}
