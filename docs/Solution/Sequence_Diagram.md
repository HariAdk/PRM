# PRM Tool — Sequence Diagrams

> Rendered with [Mermaid](https://mermaid.js.org/). View in GitHub, VS Code (Markdown Preview Mermaid Support), or [mermaid.live](https://mermaid.live).

---

## Table of Contents

1. [Login & Force Password Change](#1-login--force-password-change)
2. [Admin — Create User with Auto Employee Profile](#2-admin--create-user-with-auto-employee-profile)
3. [Admin — Assign Manager](#3-admin--assign-manager)
4. [Admin — Manage Employee Skills](#4-admin--manage-employee-skills)
5. [Admin — Create Project & Add Milestone](#5-admin--create-project--add-milestone)
6. [Manager — AI-Assisted Resource Allocation](#6-manager--ai-assisted-resource-allocation)
7. [Manager — Direct Allocation (Team Check)](#7-manager--direct-allocation-team-check)
8. [Manager — End an Allocation](#8-manager--end-an-allocation)
9. [Manager — View Project Health + AI Risk Summary](#9-manager--view-project-health--ai-risk-summary)
10. [Employee — Submit Timesheet](#10-employee--submit-timesheet)
11. [Background Scheduler — Auto Tasks](#11-background-scheduler--auto-tasks)
12. [Admin — System Configuration Update](#12-admin--system-configuration-update)
13. [Exception Handling Middleware](#13-exception-handling-middleware)
14. [Repository — Entity to DTO Mapping (AutoMapper)](#14-repository--entity-to-dto-mapping-automapper)

---

## 1. Login & Force Password Change

```mermaid
sequenceDiagram
    actor User
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant MW    as ExceptionHandlingMiddleware
    participant AuthSvc as AuthService (Application)
    participant DB      as SQL Server
    participant JWT     as JwtTokenService

    User->>Client: StartScreen → [1] Login
    Client->>User: Prompt username / password
    User->>Client: Enter credentials

    Client->>API: POST /api/auth/login {username, password}
    API->>AuthSvc: LoginAsync(request)
    AuthSvc->>DB: SELECT user WHERE username = ?
    DB-->>AuthSvc: User record

    alt Invalid credentials
        AuthSvc-->>API: throw UnauthorizedAppException
        API-->>MW: exception propagates
        MW-->>Client: 401 Unauthorized { userMessage }
        Client->>User: "Invalid username or password."
    else Valid credentials
        AuthSvc->>JWT: GenerateToken(userId, role)
        JWT-->>AuthSvc: JWT string
        AuthSvc-->>API: LoginResponseDto { token, forcePasswordChange }
        API-->>Client: 200 OK { token, role, forcePasswordChange }
        Client->>Client: Store token in SessionContext

        alt forcePasswordChange = true
            Client->>User: Show ChangePasswordScreen
            User->>Client: Enter new password + confirm
            Client->>API: PUT /api/auth/change-password/{userId}
            API->>AuthSvc: ChangePasswordAsync(userId, dto)
            AuthSvc->>DB: UPDATE users SET password_hash, force_password_change = false
            DB-->>AuthSvc: OK
            AuthSvc-->>API: OK
            API-->>Client: 200 OK
            Client->>User: "Password updated. Welcome!"
        end

        Client->>Client: ScreenRouter.RouteAsync() → ScreenFactory.CreateRoleMenu()
        Client->>User: Show role-specific main menu
    end
```

---

## 2. Admin — Create User with Auto Employee Profile

```mermaid
sequenceDiagram
    actor Admin
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant UserSvc  as UserService (Application)
    participant EmpRepo  as EmployeeRepository
    participant DB       as SQL Server

    Admin->>Client: AdminMenu → [4] Manage Users → [1] Create User Account
    Client->>Admin: Show CreateUserScreen form
    Admin->>Client: Enter FullName, Email, Username, TempPassword, Role=Employee

    Client->>API: POST /api/users {fullName, email, username, tempPassword, role}
    API->>UserSvc: CreateAsync(dto)
    UserSvc->>DB: Validate uniqueness, BCrypt hash
    UserSvc->>DB: INSERT users (force_password_change=true, role=EMPLOYEE)
    DB-->>UserSvc: New User { id=5 }

    UserSvc->>EmpRepo: UserHasEmployeeProfileAsync(5)?
    EmpRepo-->>UserSvc: false
    UserSvc->>EmpRepo: CreateProfileForUserAsync(5, fullName, email)
    EmpRepo->>DB: INSERT employees (userId=5, status=BENCH, dept=Unassigned)
    DB-->>EmpRepo: New Employee { id=101 }
    EmpRepo-->>UserSvc: EmployeeDto

    UserSvc-->>API: UserDto { id=5 }
    API-->>Client: 201 Created
    Client->>Admin: "Account created. Employee profile auto-created (BENCH)."
```

---

## 3. Admin — Assign Manager

```mermaid
sequenceDiagram
    actor Admin
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant EmpSvc   as EmployeeService (Application)
    participant EmpRepo  as EmployeeRepository
    participant DB       as SQL Server

    Admin->>Client: Manage Employees → [5] Assign Manager
    Client->>Admin: Prompt Employee User ID + Manager User ID
    Admin->>Client: employeeUserId=10, managerUserId=3

    Client->>API: PUT /api/employees/assign-manager {employeeUserId, managerUserId}
    API->>EmpSvc: AssignManagerAsync(dto)
    EmpSvc->>DB: Validate employee user role = EMPLOYEE
    EmpSvc->>DB: Validate manager user role = MANAGER
    EmpSvc->>DB: Validate employee profile exists

    EmpSvc->>EmpRepo: AssignManagerAsync(10, 3)
    EmpRepo->>DB: UPDATE employees SET manager_id = 3 WHERE user_id = 10
    DB-->>EmpRepo: OK
    EmpRepo-->>EmpSvc: EmployeeDto
    EmpSvc-->>API: EmployeeDto
    API-->>Client: 200 OK
    Client->>Admin: "Manager assigned successfully."
```

---

## 4. Admin — Manage Employee Skills

```mermaid
sequenceDiagram
    actor Admin
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant EmpSvc  as EmployeeService (Application)
    participant DB      as SQL Server

    Admin->>Client: Manage Employees → [4] Manage Employee Skills
    Client->>Admin: Prompt: Enter Employee ID
    Admin->>Client: 101

    Client->>API: GET /api/employees/101/skills
    API->>DB: SELECT employee_skills JOIN skills WHERE employee_id=101
    DB-->>API: Skill list
    API-->>Client: 200 OK [Java-Intermediate, SpringBoot-Advanced]
    Client->>Admin: Display current skills + sub-menu

    Admin->>Client: [1] Add Skill
    Client->>Admin: Prompt: Skill Name, Category, Proficiency
    Admin->>Client: "WebSocket", Backend, Intermediate

    Client->>API: POST /api/employees/101/skills {name, category, proficiencyLevel}
    API->>EmpSvc: AddSkillAsync(101, dto)
    EmpSvc->>DB: SELECT or INSERT skills WHERE name="WebSocket"
    EmpSvc->>DB: INSERT employee_skills (employeeId=101, skillId=?, level=INTERMEDIATE)
    DB-->>EmpSvc: OK
    EmpSvc-->>API: EmployeeSkillDto
    API-->>Client: 201 Created
    Client->>Admin: "Skill added."
```

---

## 5. Admin — Create Project & Add Milestone

```mermaid
sequenceDiagram
    actor Admin
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant ProjSvc  as ProjectService (Application)
    participant DB       as SQL Server

    Admin->>Client: Manage Projects → [1] Create Project
    Client->>Admin: Show CreateProjectScreen
    Admin->>Client: Name, Description, Dates, Status, ManagerId, TotalStoryPoints=40

    Client->>API: POST /api/projects {name, desc, dates, status, managerId, totalStoryPoints}
    API->>ProjSvc: CreateAsync(dto)
    ProjSvc->>DB: Validate managerId exists + has MANAGER role
    ProjSvc->>DB: INSERT projects (health_status=ON_TRACK, total_story_points=40)
    DB-->>ProjSvc: Project { id=201 }
    ProjSvc-->>API: ProjectDto
    API-->>Client: 201 Created { projectId=201 }
    Client->>Admin: "Project created."

    Admin->>Client: Manage Projects → [4] Manage Milestones → Enter ProjectId=201
    Client->>API: GET /api/projects/201/milestones
    API-->>Client: 200 OK []
    Client->>Admin: Show milestone list + sub-menu

    Admin->>Client: [1] Add Milestone → Title, DueDate, StoryPoints=8
    Client->>API: POST /api/projects/201/milestones {title, dueDate, storyPoints, status=NOT_STARTED}
    API->>ProjSvc: AddMilestoneAsync(201, dto)
    ProjSvc->>DB: INSERT milestones (story_points=8)
    DB-->>ProjSvc: Milestone { id=1 }
    ProjSvc-->>API: MilestoneDto
    API-->>Client: 201 Created
    Client->>Admin: "Milestone added."
```

---

## 6. Manager — AI-Assisted Resource Allocation

```mermaid
sequenceDiagram
    actor Manager
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant MgrSvc   as ManagerService (Application)
    participant AiSvc    as AiService (Application)
    participant EmpRepo  as EmployeeRepository
    participant AI       as IAiProvider (Gemini/Groq)
    participant AllocSvc as AllocationService
    participant DB       as SQL Server

    Manager->>Client: Allocate Resource → [1] Find resource using AI
    Client->>API: POST /api/manager/ai/skill-match {projectId, requirement}

    API->>MgrSvc: GetAISkillMatchAsync(request, managerUserId)
    MgrSvc->>AiSvc: GetSkillMatchAsync(request, managerUserId)
    AiSvc->>DB: Verify project.ManagerId = managerUserId
    AiSvc->>EmpRepo: GetTeamAllocatableResourcesAsync(managerUserId)
    EmpRepo-->>AiSvc: Team employees only

    AiSvc->>AiSvc: Pre-filter: availability, skill keywords, weekly hours
    AiSvc->>AI: CompleteAsync(prompt) [if LLM configured]

    alt LLM success + validation passes
        AI-->>AiSvc: JSON response
        AiSvc-->>MgrSvc: AISkillMatchResultDto (UsedFallback=false)
    else LLM fails or unconfigured
        AiSvc->>AiSvc: AiFallbackMatcher.BuildSkillMatch()
        AiSvc-->>MgrSvc: AISkillMatchResultDto (UsedFallback=true)
    end

    MgrSvc-->>API: AISkillMatchResultDto
    API-->>Client: 200 OK
    Client->>Manager: Show ranked results (AI or KEYWORD-MATCHED)

    Manager->>Client: Select employee + set utilisation/dates
    Client->>API: POST /api/manager/allocations
    API->>AllocSvc: CreateAsync(dto, managerUserId)
    AllocSvc->>EmpRepo: IsOnManagerTeamAsync(managerUserId, employeeId)
    AllocSvc->>DB: INSERT allocation, UPDATE employee status
    AllocSvc-->>API: AllocationDto
    API-->>Client: 201 Created
```

---

## 7. Manager — Direct Allocation (Team Check)

```mermaid
sequenceDiagram
    actor Manager
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant AllocSvc as AllocationService (Application)
    participant EmpRepo  as EmployeeRepository
    participant DB       as SQL Server

    Manager->>Client: Allocate Resource → [2] Allocate directly
    Client->>API: POST /api/manager/allocations {employeeId, projectId, utilisation, from, to}
    API->>AllocSvc: CreateAsync(dto, managerUserId)

    AllocSvc->>EmpRepo: IsOnManagerTeamAsync(managerUserId, employeeId)

    alt Employee not on team
        AllocSvc-->>API: throw ForbiddenAppException
        API-->>Client: 403 Forbidden "Employee not on team"
    else Over-allocation
        AllocSvc-->>API: throw BusinessRuleException
        API-->>Client: 400 Bad Request
    else Valid
        AllocSvc->>DB: INSERT allocations
        AllocSvc->>DB: UPDATE employee status = ALLOCATED
        AllocSvc-->>API: AllocationDto
        API-->>Client: 201 Created
    end
```

---

## 8. Manager — End an Allocation

```mermaid
sequenceDiagram
    actor Manager
    participant Client    as ProjectManagementSystem.Client
    participant API       as Web API
    participant AllocSvc  as AllocationService (Application)
    participant DB        as SQL Server

    Manager->>Client: Allocate Resource → [3] End an existing allocation
    Client->>Manager: Prompt: Select Project
    Manager->>Client: Alpha Portal (201)

    Client->>API: GET /api/manager/projects/201/allocations
    API->>DB: SELECT active allocations WHERE project_id=201 AND manager owns project
    DB-->>API: [Ravi Kumar 50%, Neha Joshi 50%]
    API-->>Client: Allocation list
    Client->>Manager: Show active allocation list

    Manager->>Client: Select allocation #1 (Ravi Kumar) → Confirm
    Client->>API: PUT /api/manager/allocations/1/end
    API->>AllocSvc: EndAsync(allocationId=1, managerUserId)
    AllocSvc->>DB: UPDATE allocations SET to_date = TODAY, is_active = false
    AllocSvc->>DB: SELECT COUNT(*) active allocations WHERE employee_id = Ravi.id

    alt No other active allocations
        AllocSvc->>DB: UPDATE employees SET status = BENCH WHERE id = Ravi.id
    end

    AllocSvc-->>API: OK
    API-->>Client: 200 OK
    Client->>Manager: "Allocation ended. Ravi Kumar freed from Alpha Portal."
```

---

## 9. Manager — View Project Health + AI Risk Summary

```mermaid
sequenceDiagram
    actor Manager
    participant Client as ProjectManagementSystem.Client
    participant API    as Web API
    participant MgrSvc as ManagerService (Application)
    participant AiSvc  as AiService (Application)
    participant DB     as SQL Server

    Manager->>Client: Manager Menu → [3] My Projects
    Client->>API: GET /api/manager/projects
    API->>MgrSvc: GetMyProjectsAsync(managerId)
    MgrSvc->>DB: Load projects, milestones, timesheets
    MgrSvc->>MgrSvc: ComputeDisplayHealth() per project
    MgrSvc-->>API: List of ProjectDto with live health + story points
    API-->>Client: 200 OK
    Client->>Manager: Show projects with health icons

    Manager->>Client: Select project detail
    Client->>API: GET /api/manager/projects/201/detail
    API->>MgrSvc: GetProjectDetailAsync(managerId, 201)
    MgrSvc-->>API: ProjectDetailDto with risk flags + milestones
    API-->>Client: 200 OK

    Manager->>Client: [A] Get AI Risk Summary
    Client->>API: POST /api/manager/ai/risk-summary {projectId=201}
    API->>MgrSvc: GetAIRiskSummaryAsync(request)
    MgrSvc->>AiSvc: GetRiskSummaryAsync(request)
    AiSvc->>DB: Load milestones, allocations, timesheets
    AiSvc->>AiSvc: Build risk flags + call LLM or AiFallbackMatcher
    AiSvc-->>MgrSvc: AIRiskSummaryResultDto
    MgrSvc-->>API: AIRiskSummaryResultDto
    API-->>Client: 200 OK
    Client->>Manager: Display summary paragraph
```

---

## 10. Employee — Submit Timesheet

```mermaid
sequenceDiagram
    actor Employee
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant EmpSvc  as EmployeePortalService (Application)
    participant DB      as SQL Server

    Employee->>Client: Employee Menu → [1] Submit Timesheet
    Client->>API: GET /api/employee/timesheets/context?weekStart=2026-05-12
    API->>EmpSvc: GetSubmitContextAsync(userId, weekStart)
    EmpSvc->>DB: SELECT active allocations + max weekly hours
    EmpSvc-->>API: EmployeeSubmitContextDto
    API-->>Client: Active projects for week

    Employee->>Client: Enter hours + activity tags per project
    Client->>API: POST /api/employee/timesheets {weekStart, entries}
    API->>EmpSvc: SubmitTimesheetAsync(userId, dto)

    alt Validation fails
        EmpSvc-->>API: throw BusinessRuleException / ValidationException
        API-->>Client: 400 Bad Request { userMessage }
    else All valid
        EmpSvc->>DB: INSERT timesheets + timesheet_entries
        EmpSvc-->>API: TimesheetDto
        API-->>Client: 201 Created
    end
```

---

## 11. Background Scheduler — Auto Tasks

```mermaid
sequenceDiagram
    participant Host     as SchedulerHostedService
    participant Scheduler as SchedulerService (Application)
    participant ConfigRepo as ISystemConfigRepository
    participant AllocRepo  as IAllocationRepository
    participant EmpRepo    as IEmployeeRepository
    participant ProjRepo   as IProjectRepository
    participant TsRepo     as ITimesheetRepository
    participant DB         as SQL Server

    Note over Host: App starts — AddHostedService auto-starts ExecuteAsync()

    loop Every N hours (from system_config)
        Host->>Scheduler: RunScheduledTasksAsync() via scoped ISchedulerService

        Note over Scheduler,DB: Task A — Recompute Employee Status
        Scheduler->>AllocRepo: GetAllActiveAsync()
        Scheduler->>EmpRepo: SetStatusAsync(ALLOCATED / BENCH)

        Note over Scheduler,DB: Task B — Project Health Flagging
        Scheduler->>ProjRepo: GetActiveAsync()
        Scheduler->>ProjRepo: UpdateHealthStatusAsync per project

        Note over Scheduler,DB: Task C — Missed Timesheet Detection
        Scheduler->>AllocRepo: GetEmployeeIdsAllocatedBetweenAsync(last week)
        loop For each employee
            Scheduler->>TsRepo: ExistsForEmployeeWeekAsync()
            alt No submission
                Scheduler->>TsRepo: CreateMissedAsync()
            end
        end

        Host->>Host: await Task.Delay(intervalHours)
    end
```

---

## 12. Admin — System Configuration Update

```mermaid
sequenceDiagram
    actor Admin
    participant Client     as ProjectManagementSystem.Client
    participant API        as Web API
    participant ConfigSvc  as SystemConfigService (Application)
    participant DB         as SQL Server

    Admin->>Client: Admin Menu → [5] System Configuration
    Client->>API: GET /api/config
    API->>DB: SELECT * FROM system_config WHERE id=1
    DB-->>API: SystemConfig record
    API-->>Client: 200 OK { llmProvider, apiKey(masked), schedulerInterval, maxWeeklyHours }
    Client->>Admin: Display current settings

    Admin->>Client: [1] Update LLM API Key
    Admin->>Client: Enter new key value
    Client->>API: PUT /api/config { llmApiKey: "sk-new-key..." }
    API->>ConfigSvc: UpdateAsync(dto)
    ConfigSvc->>DB: UPDATE system_config SET llm_api_key WHERE id=1
    ConfigSvc-->>API: OK
    API-->>Client: 200 OK
    Client->>Admin: "API key updated."

    Admin->>Client: [3] Update Scheduler Interval → 2 hours
    Client->>API: PUT /api/config { schedulerIntervalHours: 2 }
    API->>ConfigSvc: UpdateAsync(dto)
    ConfigSvc->>DB: UPDATE system_config SET scheduler_interval_hours = 2
    API-->>Client: 200 OK
    Client->>Admin: "Scheduler interval updated to 2 hours."
```

---

## 13. Exception Handling Middleware

```mermaid
sequenceDiagram
    participant Client as ProjectManagementSystem.Client
    participant API    as Web API Controller
    participant MW     as ExceptionHandlingMiddleware
    participant Mapper as ExceptionResponseMapper
    participant Svc    as Application Service

    Client->>API: HTTP Request (with JWT)
    API->>MW: Invoke pipeline
    MW->>API: await next()
    API->>Svc: Business operation

    alt Business rule violation
        Svc-->>API: throw BusinessRuleException("Over-allocation...")
        API-->>MW: exception bubbles up
        MW->>Mapper: Map(exception)
        Mapper-->>MW: ExceptionMappingResult (400, userMessage, Warning)
        MW-->>Client: 400 { success: false, message: "Over-allocation..." }
    else Not found
        Svc-->>API: throw NotFoundException("Employee not found")
        MW->>Mapper: Map(exception)
        Mapper-->>MW: ExceptionMappingResult (404, ...)
        MW-->>Client: 404 { success: false, message: "Employee not found" }
    else Forbidden (team check)
        Svc-->>API: throw ForbiddenAppException("Employee not on team")
        MW-->>Client: 403 { success: false, message: "Employee not on team" }
    else Unhandled
        MW-->>Client: 500 { success: false, message: "An unexpected error occurred." }
    end
```

---

## 14. Repository — Entity to DTO Mapping (AutoMapper)

```mermaid
sequenceDiagram
    participant Svc  as Application Service
    participant Repo as EmployeeRepository
    participant EF   as AppDbContext
    participant Map  as IMapper
    participant Prof as MappingProfile

    Svc->>Repo: GetByIdAsync(101)
    Repo->>EF: Employees.FindAsync(101)
    EF-->>Repo: Employee entity

    Repo->>Map: Map<EmployeeDto>(entity)
    Map->>Prof: Apply mapping rules
    Note over Prof: Status enum → string<br/>ManagerName from navigation<br/>Ignore navigation collections
    Prof-->>Map: EmployeeDto
    Map-->>Repo: EmployeeDto
    Repo-->>Svc: EmployeeDto

    Note over Svc,Prof: Create/Update uses Create*Dto / Update*Dto → Entity<br/>then Entity back to DTO after SaveChanges
```

**DI registration:** `builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);`

**Composition root:** Repositories registered in `Program.cs`; application services via `AddApplicationServices()`.
