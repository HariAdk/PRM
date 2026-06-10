namespace ProjectManagementSystem.Core.Constants;

public static class ErrorMessages
{
    // Auth & users
    public const string InvalidCredentials = "Invalid username or password.";
    public const string AccountDeactivated = "This account has been deactivated.";
    public const string AdminSignUpForbidden = "Admin accounts cannot be created via Sign Up.";
    public const string SignUpDisabled = "Self-registration is disabled. Contact your Admin to create an account.";
    public const string DuplicateUsernameOrEmail = "Username or email is already in use.";
    public const string PasswordsDoNotMatch = "Passwords do not match.";
    public const string InvalidUserIdentity = "Invalid or missing user identity.";
    public const string CannotChangeOtherUserPassword = "You can only change your own password.";
    public const string PasswordMissingUppercase = "Password must contain at least one uppercase letter.";
    public const string PasswordMissingDigit = "Password must contain at least one number.";

    // Employee & manager
    public const string EmployeeNotOnTeam = "Employee is not on your team.";
    public const string InvalidManagerAssignment = "Employee must have Employee role and manager must have Manager role.";
    public const string EmployeeProfileRequired = "Employee profile not found for the given user ID.";
    public const string ManagerUserNotFound = "Manager user not found.";
    public const string NoEmployeeProfile = "No active employee profile linked to your account.";
    public const string InvalidEmployeeUserId = "User ID does not exist or is not an Employee/Manager role.";
    public const string EmployeeProfileAlreadyExists = "This user already has an employee profile.";
    public const string InactiveEmployeeCannotAllocate = "Cannot allocate an inactive employee.";
    public const string EmployeeAlreadyHasSkill = "Employee already has this skill.";

    // Allocation
    public const string AllocationAlreadyEnded = "This allocation has already ended.";
    public const string AllocationEndBeforeStart = "End date cannot be before allocation start date.";
    public const string AllocationDateRangeInvalid = "End date must be on or after start date.";
    public const string ProjectNotOpenForAllocation = "Project is not open for allocation.";
    public const string OnlyEmployeesCanBeAllocated =
        "Only individual contributors (Employee role) can be allocated to projects.";

    // Project
    public const string InvalidManagerId = "Manager ID does not exist or is not an active Manager.";
    public const string ProjectEndBeforeStart = "End date must be after start date.";
    public const string EmployeeNotFound = "Employee not found.";
    public const string ProjectNotFound = "Project not found.";
    public const string AllocationNotFound = "Allocation not found.";

    // Timesheet
    public const string TimesheetAlreadySubmitted = "A timesheet for this week has already been submitted.";
    public const string TimesheetNotFound = "Timesheet not found.";
    public const string InvalidWeekStartDate = "Invalid week start date format. Use yyyy-MM-dd.";
    public const string FutureWeekTimesheet = "Cannot submit a timesheet for a future week.";
    public const string NoAllocationsForWeek = "You have no active project allocations for this week.";
    public const string TimesheetHoursRequired = "At least one project must have hours greater than zero.";
    public const string TimesheetEntryRequired = "At least one project entry is required.";
    public const string DuplicateTimesheetProject = "Duplicate project entries are not allowed in one timesheet.";
    public const string NegativeHours = "Hours cannot be negative.";
    public const string ActivityTagRequired = "At least one activity tag is required for entries with hours.";

    // System & AI
    public const string SystemConfigNotFound = "System configuration not found.";
    public const string SystemConfigMissing = "SystemConfig not found.";
    public const string LlmNotConfigured = "LLM is not configured. Add an API key in System Configuration.";
    public const string UnexpectedError = "An unexpected error occurred. Please try again later.";

    // Entity not-found helpers (include id in message for API clients)
    public static string EmployeeNotFoundById(int id) => $"Employee {id} not found.";
    public static string ProjectNotFoundById(int id) => $"Project {id} not found.";
    public static string AllocationNotFoundById(int id) => $"Allocation {id} not found.";
    public static string UserNotFoundById(int id) => $"User {id} not found.";
    public static string TimesheetNotFoundById(int id) => $"Timesheet {id} not found.";
    public static string MilestoneNotFound(int milestoneId) => $"Milestone {milestoneId} not found.";
    public static string EmployeeProfileNotFoundForUser(int userId) =>
        $"Employee profile not found for user {userId}.";
    public static string SkillNotFoundForEmployee() => "Skill not found for this employee.";
    public static string NotAllocatedToProject(int projectId) =>
        $"You are not allocated to project ID {projectId} during this week.";
    public static string ProjectHoursExceedMax(int projectId, decimal maxHours) =>
        $"Hours for project ID {projectId} exceed the allowed maximum ({maxHours} hrs based on your allocation).";
    public static string TotalHoursExceedWeeklyMax(decimal totalHours, int maxWeeklyHours) =>
        $"Total hours ({totalHours}) exceed the maximum weekly limit of {maxWeeklyHours} hrs.";
    public static string UtilisationOutOfRange() =>
        $"Utilisation must be between {AllocationLimits.MinUtilisationPercent} " +
        $"and {AllocationLimits.MaxUtilisationPercent} percent.";
    public static string OverAllocationDetected(int currentPercent, int newPercent) =>
        $"Over-allocation detected. Employee already has {currentPercent}% allocated in this period. " +
        $"Adding {newPercent}% would exceed {AllocationLimits.MaxTotalUtilisationPercent}%.";
    public static string PasswordTooShort() =>
        $"Password must be at least {PasswordPolicy.MinLength} characters.";
    public static string InvalidActivityTag(string tag) =>
        $"Invalid activity tag '{tag}'. Choose from the predefined list.";
    public static string GeminiApiError(int statusCode) => $"Gemini API returned {statusCode}.";
    public static string GroqApiError(int statusCode) => $"Groq API returned {statusCode}.";
    public const string GeminiEmptyResponse = "Gemini API returned an empty response.";
    public const string GroqEmptyResponse = "Groq API returned an empty response.";
}
