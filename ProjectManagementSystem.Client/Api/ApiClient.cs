using System.Net.Http.Json;
using System.Text.Json;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.DTOs.Auth;
using ProjectManagementSystem.Core.DTOs.User;
using ProjectManagementSystem.Core.DTOs.Employee;
using ProjectManagementSystem.Core.DTOs.Project;
using ProjectManagementSystem.Core.DTOs.Allocation;
using ProjectManagementSystem.Core.DTOs.Config;
using ProjectManagementSystem.Core.DTOs.Common;
using ProjectManagementSystem.Core.DTOs.Manager;
using ProjectManagementSystem.Core.DTOs.Timesheet;

namespace ProjectManagementSystem.Client.Api;

public class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken() =>
        _http.DefaultRequestHeaders.Authorization = null;

    public Task<(LoginResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto dto) =>
        PostAsync<LoginRequestDto, LoginResponseDto>(ApiRoutes.AuthLogin, dto);

    public Task<(object? Data, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto) =>
        PutAsync<ChangePasswordDto, object>(ApiRoutes.AuthChangePassword(userId), dto);

    public Task<(IEnumerable<UserDto>? Data, string? Error)> GetUsersAsync() =>
        GetAsync<IEnumerable<UserDto>>(ApiRoutes.Users);

    public Task<(UserDto? Data, string? Error)> CreateUserAsync(CreateUserDto dto) =>
        PostAsync<CreateUserDto, UserDto>(ApiRoutes.Users, dto);

    public Task<(object? Data, string? Error)> ResetPasswordAsync(int userId, ResetPasswordDto dto) =>
        PutAsync<ResetPasswordDto, object>(ApiRoutes.UserResetPassword(userId), dto);

    public Task<(object? Data, string? Error)> DeactivateUserAsync(int userId) =>
        PutAsync<object, object>(ApiRoutes.UserDeactivate(userId), null!);

    public Task<(object? Data, string? Error)> ReactivateUserAsync(int userId) =>
        PutAsync<object, object>(ApiRoutes.UserReactivate(userId), null!);

    public Task<(IEnumerable<EmployeeDto>? Data, string? Error)> GetEmployeesAsync() =>
        GetAsync<IEnumerable<EmployeeDto>>(ApiRoutes.Employees);

    public Task<(EmployeeDto? Data, string? Error)> GetEmployeeAsync(int id) =>
        GetAsync<EmployeeDto>(ApiRoutes.EmployeeById(id));

    public Task<(EmployeeDto? Data, string? Error)> CreateEmployeeAsync(CreateEmployeeDto dto) =>
        PostAsync<CreateEmployeeDto, EmployeeDto>(ApiRoutes.Employees, dto);

    public Task<(EmployeeDto? Data, string? Error)> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto) =>
        PutAsync<UpdateEmployeeDto, EmployeeDto>(ApiRoutes.EmployeeById(id), dto);

    public Task<(object? Data, string? Error)> DeactivateEmployeeAsync(int id) =>
        PutAsync<object, object>(ApiRoutes.EmployeeDeactivate(id), null!);

    public Task<(EmployeeDto? Data, string? Error)> AssignManagerAsync(AssignManagerDto dto) =>
        PutAsync<AssignManagerDto, EmployeeDto>(ApiRoutes.EmployeesAssignManager, dto);

    public Task<(IEnumerable<EmployeeSkillDto>? Data, string? Error)> GetSkillsAsync(int id) =>
        GetAsync<IEnumerable<EmployeeSkillDto>>(ApiRoutes.EmployeeSkills(id));

    public Task<(EmployeeSkillDto? Data, string? Error)> AddSkillAsync(int id, AddSkillDto dto) =>
        PostAsync<AddSkillDto, EmployeeSkillDto>(ApiRoutes.EmployeeSkills(id), dto);

    public Task<(EmployeeSkillDto? Data, string? Error)> UpdateSkillAsync(int id, int skillId, UpdateSkillDto dto) =>
        PutAsync<UpdateSkillDto, EmployeeSkillDto>(ApiRoutes.EmployeeSkill(id, skillId), dto);

    public Task<(object? Data, string? Error)> RemoveSkillAsync(int id, int skillId) =>
        DeleteAsync(ApiRoutes.EmployeeSkill(id, skillId));

    public Task<(IEnumerable<ProjectDto>? Data, string? Error)> GetProjectsAsync() =>
        GetAsync<IEnumerable<ProjectDto>>(ApiRoutes.Projects);

    public Task<(ProjectDto? Data, string? Error)> GetProjectAsync(int id) =>
        GetAsync<ProjectDto>(ApiRoutes.ProjectById(id));

    public Task<(ProjectDto? Data, string? Error)> CreateProjectAsync(CreateProjectDto dto) =>
        PostAsync<CreateProjectDto, ProjectDto>(ApiRoutes.Projects, dto);

    public Task<(ProjectDto? Data, string? Error)> UpdateProjectAsync(int id, UpdateProjectDto dto) =>
        PutAsync<UpdateProjectDto, ProjectDto>(ApiRoutes.ProjectById(id), dto);

    public Task<(IEnumerable<MilestoneDto>? Data, string? Error)> GetMilestonesAsync(int projectId) =>
        GetAsync<IEnumerable<MilestoneDto>>(ApiRoutes.ProjectMilestones(projectId));

    public Task<(MilestoneDto? Data, string? Error)> AddMilestoneAsync(int projectId, CreateMilestoneDto dto) =>
        PostAsync<CreateMilestoneDto, MilestoneDto>(ApiRoutes.ProjectMilestones(projectId), dto);

    public Task<(MilestoneDto? Data, string? Error)> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto) =>
        PutAsync<UpdateMilestoneStatusDto, MilestoneDto>(ApiRoutes.ProjectMilestone(projectId, milestoneId), dto);

    public Task<(IEnumerable<AllocationDto>? Data, string? Error)> GetAllocationsAsync() =>
        GetAsync<IEnumerable<AllocationDto>>(ApiRoutes.Allocations);

    public Task<(SystemConfigDto? Data, string? Error)> GetConfigAsync() =>
        GetAsync<SystemConfigDto>(ApiRoutes.Config);

    public Task<(object? Data, string? Error)> UpdateConfigAsync(SystemConfigDto dto) =>
        PutAsync<SystemConfigDto, object>(ApiRoutes.Config, dto);

    public Task<(ResourceDashboardDto? Data, string? Error)> GetManagerDashboardAsync() =>
        GetAsync<ResourceDashboardDto>(ApiRoutes.ManagerDashboard);

    public Task<(EmployeeDetailDto? Data, string? Error)> GetManagerEmployeeDetailAsync(int id) =>
        GetAsync<EmployeeDetailDto>(ApiRoutes.ManagerEmployee(id));

    public Task<(IEnumerable<ProjectDto>? Data, string? Error)> GetManagerProjectsAsync() =>
        GetAsync<IEnumerable<ProjectDto>>(ApiRoutes.ManagerProjects);

    public Task<(ProjectDto? Data, string? Error)> GetManagerProjectAsync(int id) =>
        GetAsync<ProjectDto>(ApiRoutes.ManagerProject(id));

    public Task<(ProjectDetailDto? Data, string? Error)> GetManagerProjectDetailAsync(int id) =>
        GetAsync<ProjectDetailDto>(ApiRoutes.ManagerProjectDetail(id));

    public Task<(IEnumerable<MilestoneDto>? Data, string? Error)> GetManagerProjectMilestonesAsync(int projectId) =>
        GetAsync<IEnumerable<MilestoneDto>>(ApiRoutes.ManagerProjectMilestones(projectId));

    public Task<(IEnumerable<AllocationDto>? Data, string? Error)> GetManagerProjectAllocationsAsync(int projectId) =>
        GetAsync<IEnumerable<AllocationDto>>(ApiRoutes.ManagerProjectAllocations(projectId));

    public Task<(AllocationDto? Data, string? Error)> CreateManagerAllocationAsync(CreateAllocationDto dto) =>
        PostAsync<CreateAllocationDto, AllocationDto>(ApiRoutes.ManagerAllocations, dto);

    public Task<(object? Data, string? Error)> EndManagerAllocationAsync(int id, EndAllocationDto dto) =>
        PutAsync<EndAllocationDto, object>(ApiRoutes.ManagerAllocationEnd(id), dto);

    public Task<(ManagerTeamTimesheetDto? Data, string? Error)> GetManagerTeamTimesheetsAsync(string? weekStart = null)
    {
        var url = string.IsNullOrEmpty(weekStart)
            ? ApiRoutes.ManagerTimesheets
            : ApiRoutes.ManagerTimesheetsWithWeek(weekStart);
        return GetAsync<ManagerTeamTimesheetDto>(url);
    }

    public Task<(TimesheetDto? Data, string? Error)> GetManagerTimesheetDetailAsync(int id) =>
        GetAsync<TimesheetDto>(ApiRoutes.ManagerTimesheet(id));

    public Task<(AISkillMatchResultDto? Data, string? Error)> ManagerSkillMatchAsync(AISkillMatchRequestDto dto) =>
        PostAsync<AISkillMatchRequestDto, AISkillMatchResultDto>(ApiRoutes.ManagerAiSkillMatch, dto);

    public Task<(AIRiskSummaryResultDto? Data, string? Error)> ManagerRiskSummaryAsync(AIRiskSummaryRequestDto dto) =>
        PostAsync<AIRiskSummaryRequestDto, AIRiskSummaryResultDto>(ApiRoutes.ManagerAiRiskSummary, dto);

    public Task<(EmployeeReminderDto? Data, string? Error)> GetEmployeeReminderAsync() =>
        GetAsync<EmployeeReminderDto>(ApiRoutes.EmployeeReminder);

    public Task<(EmployeeProfileDto? Data, string? Error)> GetEmployeeAllocationsAsync() =>
        GetAsync<EmployeeProfileDto>(ApiRoutes.EmployeeAllocations);

    public Task<(EmployeeSubmitContextDto? Data, string? Error)> GetEmployeeSubmitContextAsync(string? weekStart = null)
    {
        var url = string.IsNullOrEmpty(weekStart)
            ? ApiRoutes.EmployeeTimesheetsContext
            : ApiRoutes.EmployeeTimesheetsContextWithWeek(weekStart);
        return GetAsync<EmployeeSubmitContextDto>(url);
    }

    public Task<(TimesheetDto? Data, string? Error)> SubmitEmployeeTimesheetAsync(SubmitEmployeeTimesheetDto dto) =>
        PostAsync<SubmitEmployeeTimesheetDto, TimesheetDto>(ApiRoutes.EmployeeTimesheets, dto);

    public Task<(IEnumerable<TimesheetDto>? Data, string? Error)> GetEmployeeTimesheetsAsync() =>
        GetAsync<IEnumerable<TimesheetDto>>(ApiRoutes.EmployeeTimesheets);

    public Task<(TimesheetDto? Data, string? Error)> GetEmployeeTimesheetAsync(int id) =>
        GetAsync<TimesheetDto>(ApiRoutes.EmployeeTimesheet(id));

    private async Task<(T? Data, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            var resp = await _http.GetAsync(url);
            return await ParseAsync<T>(resp);
        }
        catch (Exception ex) { return (default, ex.Message); }
    }

    private async Task<(TOut? Data, string? Error)> PostAsync<TIn, TOut>(string url, TIn body)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(url, body, JsonOpts);
            return await ParseAsync<TOut>(resp);
        }
        catch (Exception ex) { return (default, ex.Message); }
    }

    private async Task<(TOut? Data, string? Error)> PutAsync<TIn, TOut>(string url, TIn body)
    {
        try
        {
            var resp = await _http.PutAsJsonAsync(url, body, JsonOpts);
            return await ParseAsync<TOut>(resp);
        }
        catch (Exception ex) { return (default, ex.Message); }
    }

    private async Task<(object? Data, string? Error)> DeleteAsync(string url)
    {
        try
        {
            var resp = await _http.DeleteAsync(url);
            return await ParseAsync<object>(resp);
        }
        catch (Exception ex) { return (default, ex.Message); }
    }

    private static async Task<(T? Data, string? Error)> ParseAsync<T>(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        if (resp.IsSuccessStatusCode)
        {
            var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOpts);
            return (wrapper!.Data, null);
        }
        try
        {
            var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, JsonOpts);
            return (default, err?.Message ?? resp.ReasonPhrase);
        }
        catch { return (default, resp.ReasonPhrase); }
    }
}
