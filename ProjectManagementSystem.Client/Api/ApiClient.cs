using System.Net.Http.Json;
using System.Text.Json;
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

/// <summary>
/// Single HTTP wrapper for all API calls.
/// Returns (data, errorMessage) tuples so screens never deal with exceptions.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl)
    {
        // Accept self-signed dev certificate without error
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

    // ?? Auth ?????????????????????????????????????????????????????????????????
    public Task<(LoginResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto dto) =>
        PostAsync<LoginRequestDto, LoginResponseDto>("api/auth/login", dto);

    public Task<(UserDto? Data, string? Error)> SignUpAsync(SignUpRequestDto dto) =>
        PostAsync<SignUpRequestDto, UserDto>("api/auth/signup", dto);

    public Task<(object? Data, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto) =>
        PutAsync<ChangePasswordDto, object>($"api/auth/change-password/{userId}", dto);

    // ?? Users ?????????????????????????????????????????????????????????????????
    public Task<(IEnumerable<UserDto>? Data, string? Error)> GetUsersAsync() =>
        GetAsync<IEnumerable<UserDto>>("api/users");

    public Task<(UserDto? Data, string? Error)> CreateUserAsync(CreateUserDto dto) =>
        PostAsync<CreateUserDto, UserDto>("api/users", dto);

    public Task<(object? Data, string? Error)> ResetPasswordAsync(int userId, ResetPasswordDto dto) =>
        PutAsync<ResetPasswordDto, object>($"api/users/{userId}/reset-password", dto);

    public Task<(object? Data, string? Error)> DeactivateUserAsync(int userId) =>
        PutAsync<object, object>($"api/users/{userId}/deactivate", null!);

    public Task<(object? Data, string? Error)> ReactivateUserAsync(int userId) =>
        PutAsync<object, object>($"api/users/{userId}/reactivate", null!);

    // ?? Employees ?????????????????????????????????????????????????????????????
    public Task<(IEnumerable<EmployeeDto>? Data, string? Error)> GetEmployeesAsync() =>
        GetAsync<IEnumerable<EmployeeDto>>("api/employees");

    public Task<(EmployeeDto? Data, string? Error)> GetEmployeeAsync(int id) =>
        GetAsync<EmployeeDto>($"api/employees/{id}");

    public Task<(EmployeeDto? Data, string? Error)> CreateEmployeeAsync(CreateEmployeeDto dto) =>
        PostAsync<CreateEmployeeDto, EmployeeDto>("api/employees", dto);

    public Task<(EmployeeDto? Data, string? Error)> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto) =>
        PutAsync<UpdateEmployeeDto, EmployeeDto>($"api/employees/{id}", dto);

    public Task<(object? Data, string? Error)> DeactivateEmployeeAsync(int id) =>
        PutAsync<object, object>($"api/employees/{id}/deactivate", null!);

    public Task<(EmployeeDto? Data, string? Error)> AssignManagerAsync(AssignManagerDto dto) =>
        PutAsync<AssignManagerDto, EmployeeDto>("api/employees/assign-manager", dto);

    public Task<(IEnumerable<EmployeeSkillDto>? Data, string? Error)> GetSkillsAsync(int id) =>
        GetAsync<IEnumerable<EmployeeSkillDto>>($"api/employees/{id}/skills");

    public Task<(EmployeeSkillDto? Data, string? Error)> AddSkillAsync(int id, AddSkillDto dto) =>
        PostAsync<AddSkillDto, EmployeeSkillDto>($"api/employees/{id}/skills", dto);

    public Task<(EmployeeSkillDto? Data, string? Error)> UpdateSkillAsync(int id, int skillId, UpdateSkillDto dto) =>
        PutAsync<UpdateSkillDto, EmployeeSkillDto>($"api/employees/{id}/skills/{skillId}", dto);

    public Task<(object? Data, string? Error)> RemoveSkillAsync(int id, int skillId) =>
        DeleteAsync($"api/employees/{id}/skills/{skillId}");

    // ?? Projects ??????????????????????????????????????????????????????????????
    public Task<(IEnumerable<ProjectDto>? Data, string? Error)> GetProjectsAsync() =>
        GetAsync<IEnumerable<ProjectDto>>("api/projects");

    public Task<(ProjectDto? Data, string? Error)> GetProjectAsync(int id) =>
        GetAsync<ProjectDto>($"api/projects/{id}");

    public Task<(ProjectDto? Data, string? Error)> CreateProjectAsync(CreateProjectDto dto) =>
        PostAsync<CreateProjectDto, ProjectDto>("api/projects", dto);

    public Task<(ProjectDto? Data, string? Error)> UpdateProjectAsync(int id, UpdateProjectDto dto) =>
        PutAsync<UpdateProjectDto, ProjectDto>($"api/projects/{id}", dto);

    public Task<(IEnumerable<MilestoneDto>? Data, string? Error)> GetMilestonesAsync(int projectId) =>
        GetAsync<IEnumerable<MilestoneDto>>($"api/projects/{projectId}/milestones");

    public Task<(MilestoneDto? Data, string? Error)> AddMilestoneAsync(int projectId, CreateMilestoneDto dto) =>
        PostAsync<CreateMilestoneDto, MilestoneDto>($"api/projects/{projectId}/milestones", dto);

    public Task<(MilestoneDto? Data, string? Error)> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto) =>
        PutAsync<UpdateMilestoneStatusDto, MilestoneDto>($"api/projects/{projectId}/milestones/{milestoneId}", dto);

    // ?? Allocations ???????????????????????????????????????????????????????????
    public Task<(IEnumerable<AllocationDto>? Data, string? Error)> GetAllocationsAsync() =>
        GetAsync<IEnumerable<AllocationDto>>("api/allocations");

    // ?? Config ????????????????????????????????????????????????????????????????
    public Task<(SystemConfigDto? Data, string? Error)> GetConfigAsync() =>
        GetAsync<SystemConfigDto>("api/config");

    public Task<(object? Data, string? Error)> UpdateConfigAsync(SystemConfigDto dto) =>
        PutAsync<SystemConfigDto, object>("api/config", dto);

    // ?? Manager ????????????????????????????????????????????????????????????????
    public Task<(ResourceDashboardDto? Data, string? Error)> GetManagerDashboardAsync() =>
        GetAsync<ResourceDashboardDto>("api/manager/dashboard");

    public Task<(EmployeeDetailDto? Data, string? Error)> GetManagerEmployeeDetailAsync(int id) =>
        GetAsync<EmployeeDetailDto>($"api/manager/employees/{id}");

    public Task<(IEnumerable<ProjectDto>? Data, string? Error)> GetManagerProjectsAsync() =>
        GetAsync<IEnumerable<ProjectDto>>("api/manager/projects");

    public Task<(ProjectDto? Data, string? Error)> GetManagerProjectAsync(int id) =>
        GetAsync<ProjectDto>($"api/manager/projects/{id}");

    public Task<(ProjectDetailDto? Data, string? Error)> GetManagerProjectDetailAsync(int id) =>
        GetAsync<ProjectDetailDto>($"api/manager/projects/{id}/detail");

    public Task<(IEnumerable<MilestoneDto>? Data, string? Error)> GetManagerProjectMilestonesAsync(int projectId) =>
        GetAsync<IEnumerable<MilestoneDto>>($"api/manager/projects/{projectId}/milestones");

    public Task<(IEnumerable<AllocationDto>? Data, string? Error)> GetManagerProjectAllocationsAsync(int projectId) =>
        GetAsync<IEnumerable<AllocationDto>>($"api/manager/projects/{projectId}/allocations");

    public Task<(AllocationDto? Data, string? Error)> CreateManagerAllocationAsync(CreateAllocationDto dto) =>
        PostAsync<CreateAllocationDto, AllocationDto>("api/manager/allocations", dto);

    public Task<(object? Data, string? Error)> EndManagerAllocationAsync(int id, EndAllocationDto dto) =>
        PutAsync<EndAllocationDto, object>($"api/manager/allocations/{id}/end", dto);

    public Task<(ManagerTeamTimesheetDto? Data, string? Error)> GetManagerTeamTimesheetsAsync(string? weekStart = null)
    {
        var url = string.IsNullOrEmpty(weekStart)
            ? "api/manager/timesheets"
            : $"api/manager/timesheets?weekStart={weekStart}";
        return GetAsync<ManagerTeamTimesheetDto>(url);
    }

    public Task<(TimesheetDto? Data, string? Error)> GetManagerTimesheetDetailAsync(int id) =>
        GetAsync<TimesheetDto>($"api/manager/timesheets/{id}");

    public Task<(AISkillMatchResultDto? Data, string? Error)> ManagerSkillMatchAsync(AISkillMatchRequestDto dto) =>
        PostAsync<AISkillMatchRequestDto, AISkillMatchResultDto>("api/manager/ai/skill-match", dto);

    public Task<(AIRiskSummaryResultDto? Data, string? Error)> ManagerRiskSummaryAsync(AIRiskSummaryRequestDto dto) =>
        PostAsync<AIRiskSummaryRequestDto, AIRiskSummaryResultDto>("api/manager/ai/risk-summary", dto);

    // ?? Employee ???????????????????????????????????????????????????????????????
    public Task<(EmployeeReminderDto? Data, string? Error)> GetEmployeeReminderAsync() =>
        GetAsync<EmployeeReminderDto>("api/employee/reminder");

    public Task<(EmployeeProfileDto? Data, string? Error)> GetEmployeeAllocationsAsync() =>
        GetAsync<EmployeeProfileDto>("api/employee/allocations");

    public Task<(EmployeeSubmitContextDto? Data, string? Error)> GetEmployeeSubmitContextAsync(string? weekStart = null)
    {
        var url = string.IsNullOrEmpty(weekStart)
            ? "api/employee/timesheets/context"
            : $"api/employee/timesheets/context?weekStart={weekStart}";
        return GetAsync<EmployeeSubmitContextDto>(url);
    }

    public Task<(TimesheetDto? Data, string? Error)> SubmitEmployeeTimesheetAsync(SubmitEmployeeTimesheetDto dto) =>
        PostAsync<SubmitEmployeeTimesheetDto, TimesheetDto>("api/employee/timesheets", dto);

    public Task<(IEnumerable<TimesheetDto>? Data, string? Error)> GetEmployeeTimesheetsAsync() =>
        GetAsync<IEnumerable<TimesheetDto>>("api/employee/timesheets");

    public Task<(TimesheetDto? Data, string? Error)> GetEmployeeTimesheetAsync(int id) =>
        GetAsync<TimesheetDto>($"api/employee/timesheets/{id}");

    // ?? HTTP primitives ???????????????????????????????????????????????????????
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
