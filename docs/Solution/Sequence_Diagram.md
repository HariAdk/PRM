# PRM Tool � Sequence Diagrams

> Rendered with [Mermaid](https://mermaid.js.org/). View in GitHub, VS Code (Markdown Preview Mermaid Support), or [mermaid.live](https://mermaid.live).

---

## Table of Contents

1. [Login & Force Password Change](#1-login--force-password-change)
2. [Sign Up (Self-Registration)](#2-sign-up-self-registration)
3. [Admin � Create User + Add Employee](#3-admin--create-user--add-employee)
4. [Admin � Manage Employee Skills](#4-admin--manage-employee-skills)
5. [Admin � Create Project & Add Milestone](#5-admin--create-project--add-milestone)
6. [Manager � AI-Assisted Resource Allocation](#6-manager--ai-assisted-resource-allocation)
7. [Manager � Direct Allocation](#7-manager--direct-allocation)
8. [Manager � End an Allocation](#8-manager--end-an-allocation)
9. [Manager � View Project Health + AI Risk Summary](#9-manager--view-project-health--ai-risk-summary)
10. [Employee � Submit Timesheet](#10-employee--submit-timesheet)
11. [Background Scheduler � Auto Tasks](#11-background-scheduler--auto-tasks)
12. [Admin � System Configuration Update](#12-admin--system-configuration-update)
13. [Repository � Entity to DTO Mapping (AutoMapper)](#13-repository--entity-to-dto-mapping-automapper)

---

## 1. Login & Force Password Change

```mermaid
sequenceDiagram
    actor User
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant AuthSvc as AuthService
    participant DB      as SQL Server
    participant JWT     as JwtTokenService

    User->>Client: StartScreen ? [1] Login
    Client->>User: Prompt username / password
    User->>Client: Enter credentials

    Client->>API: POST /api/auth/login {username, password}
    API->>AuthSvc: LoginAsync(request)
    AuthSvc->>DB: SELECT user WHERE username = ?
    DB-->>AuthSvc: User record

    alt Invalid credentials
        AuthSvc-->>API: Throw UnauthorizedException
        API-->>Client: 401 Unauthorized
        Client->>User: "Invalid username or password." ?
    else Valid credentials
        AuthSvc->>JWT: GenerateToken(userId, role)
        JWT-->>AuthSvc: JWT string
        AuthSvc-->>API: LoginResponseDto { token, forcePasswordChange }
        API-->>Client: 200 OK { token, role, forcePasswordChange }
        Client->>Client: Store token in SessionContext

        alt forcePasswordChange = true
            Client->>User: Show ChangePasswordScreen
            User->>Client: Enter new password + confirm
            Client->>API: PUT /api/auth/change-password {newPassword}
            API->>AuthSvc: ChangePasswordAsync(userId, dto)
            AuthSvc->>DB: UPDATE users SET password_hash, force_password_change = false
            DB-->>AuthSvc: OK
            AuthSvc-->>API: 204 No Content
            API-->>Client: 204 No Content
            Client->>User: "Password updated. Welcome! ?"
        end

        Client->>Client: ScreenRouter.RouteAsync() ? role menu
        Client->>User: Show role-specific main menu
    end
```

---

## 2. Sign Up (Self-Registration)

```mermaid
sequenceDiagram
    actor User
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant AuthSvc as AuthService
    participant Hasher  as PasswordHasher
    participant DB      as SQL Server

    User->>Client: StartScreen ? [2] Sign Up
    Client->>User: Show SignUpScreen form
    User->>Client: Enter FullName, Email, Username, Password, Role(Manager|Employee)

    Client->>API: POST /api/auth/signup {fullName, email, username, password, role}

    API->>AuthSvc: SignUpAsync(request)
    AuthSvc->>DB: SELECT COUNT(*) WHERE username = ? OR email = ?
    DB-->>AuthSvc: count

    alt Username or email already taken
        AuthSvc-->>API: Throw ConflictException
        API-->>Client: 409 Conflict "Username already taken"
        Client->>User: Show error message
    else Password validation fails
        AuthSvc-->>API: Throw ValidationException
        API-->>Client: 400 Bad Request
        Client->>User: Show validation error
    else All valid
        AuthSvc->>Hasher: Hash(password)
        Hasher-->>AuthSvc: passwordHash
        AuthSvc->>DB: INSERT INTO users (fullName, email, username, hash, role, force_password_change=false)
        DB-->>AuthSvc: New user record
        AuthSvc-->>API: UserDto
        API-->>Client: 201 Created
        Client->>User: "Account created. Please log in. ?"
        Client->>Client: Navigate back to StartScreen
    end
```

---

## 3. Admin � Create User + Add Employee

```mermaid
sequenceDiagram
    actor Admin
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant UserSvc  as UserService
    participant EmpSvc   as EmployeeService
    participant DB       as SQL Server

    Note over Admin,DB: Step 1 � Create User Account
    Admin->>Client: AdminMenu ? [4] Manage Users ? [1] Create User Account
    Client->>Admin: Show CreateUserScreen form
    Admin->>Client: Enter FullName, Email, Username, TempPassword, Role

    Client->>API: POST /api/users {fullName, email, username, tempPassword, role}
    API->>UserSvc: CreateAsync(dto)
    UserSvc->>DB: Validate uniqueness, BCrypt hash, INSERT users (force_password_change=true)
    DB-->>UserSvc: New User { id }
    UserSvc-->>API: UserDto { id=5 }
    API-->>Client: 201 Created { userId=5 }
    Client->>Admin: "Account created. User must change password on first login. ?"

    Note over Admin,DB: Step 2 � Add Employee Profile
    Admin->>Client: AdminMenu ? [1] Manage Employees ? [1] Add Employee
    Client->>Admin: Show AddEmployeeScreen form
    Admin->>Client: Enter UserId=5, FullName, Email, Department, Designation

    Client->>API: POST /api/employees {userId=5, fullName, email, department, designation}
    API->>EmpSvc: CreateAsync(dto)
    EmpSvc->>DB: Validate userId exists, role is EMPLOYEE|MANAGER, no existing profile
    DB-->>EmpSvc: OK
    EmpSvc->>DB: INSERT employees (userId=5, status=BENCH, is_active=true)
    DB-->>EmpSvc: New Employee { id=101 }
    EmpSvc-->>API: EmployeeDto
    API-->>Client: 201 Created
    Client->>Admin: "Employee added with status BENCH. ?"
```

---

## 4. Admin � Manage Employee Skills

```mermaid
sequenceDiagram
    actor Admin
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant EmpSvc  as EmployeeService
    participant DB      as SQL Server

    Admin->>Client: Manage Employees ? [5] Manage Employee Skills
    Client->>Admin: Prompt: Enter Employee ID
    Admin->>Client: 101

    Client->>API: GET /api/employees/101/skills
    API->>DB: SELECT employee_skills JOIN skills WHERE employee_id=101
    DB-->>API: Skill list
    API-->>Client: 200 OK [Java-Intermediate, SpringBoot-Advanced, MySQL-Intermediate]
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
    Client->>Admin: "Skill added. ?"
```

---

## 5. Admin � Create Project & Add Milestone

```mermaid
sequenceDiagram
    actor Admin
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant ProjSvc  as ProjectService
    participant DB       as SQL Server

    Admin->>Client: Manage Projects ? [1] Create Project
    Client->>Admin: Show CreateProjectScreen
    Admin->>Client: ProjectName, Description, StartDate, EndDate, Status, ManagerId

    Client->>API: POST /api/projects {name, desc, startDate, endDate, status, managerId}
    API->>ProjSvc: CreateAsync(dto)
    ProjSvc->>DB: Validate managerId exists + has MANAGER role
    ProjSvc->>DB: INSERT projects (health_status=ON_TRACK)
    DB-->>ProjSvc: Project { id=201 }
    ProjSvc-->>API: ProjectDto
    API-->>Client: 201 Created { projectId=201 }
    Client->>Admin: "Project created. ?"

    Admin->>Client: Manage Projects ? [4] Manage Milestones ? Enter ProjectId=201
    Client->>API: GET /api/projects/201/milestones
    API-->>Client: 200 OK []
    Client->>Admin: Show milestone list + sub-menu

    Admin->>Client: [1] Add Milestone ? Title, DueDate
    Client->>API: POST /api/projects/201/milestones {title, dueDate, status=NOT_STARTED}
    API->>ProjSvc: AddMilestoneAsync(201, dto)
    ProjSvc->>DB: INSERT milestones
    DB-->>ProjSvc: Milestone { id=1 }
    ProjSvc-->>API: MilestoneDto
    API-->>Client: 201 Created
    Client->>Admin: "Milestone added. ?"
```

---

## 6. Manager � AI-Assisted Resource Allocation

```mermaid
sequenceDiagram
    actor Manager
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant MgrSvc   as ManagerService
    participant AllocSvc as AllocationService
    participant EmpRepo  as EmployeeRepository
    participant DB       as SQL Server

    Manager->>Client: Allocate Resource - [1] Find resource using AI
    Client->>API: POST /api/manager/ai/skill-match {projectId, requirement}

    API->>MgrSvc: GetAISkillMatchAsync(request)
    MgrSvc->>EmpRepo: GetAllAsync()
    EmpRepo->>DB: SELECT employees
    DB-->>EmpRepo: entities
    EmpRepo->>EmpRepo: mapper.Map EmployeeDto
    EmpRepo-->>MgrSvc: Employee DTOs
    MgrSvc->>MgrSvc: Filter + placeholder AI ranking
    MgrSvc-->>API: AISkillMatchResultDto
    API-->>Client: 200 OK

    Manager->>Client: Select employee + set utilisation/dates
    Client->>API: POST /api/manager/allocations
    API->>AllocSvc: CreateAsync(dto)
    AllocSvc->>DB: INSERT allocation, UPDATE employee status
    AllocSvc-->>API: AllocationDto
    API-->>Client: 201 Created
```

---

## 7. Manager � Direct Allocation

```mermaid
sequenceDiagram
    actor Manager
    participant Client   as ProjectManagementSystem.Client
    participant API      as Web API
    participant AllocSvc as AllocationService
    participant DB       as SQL Server

    Manager->>Client: Allocate Resource - [2] Allocate directly
    Client->>API: POST /api/manager/allocations {employeeId, projectId, utilisation, from, to}
    API->>AllocSvc: CreateAsync(dto)

    alt Over-allocation
        AllocSvc-->>API: 400 Bad Request
        API-->>Client: Error message
    else Valid
        AllocSvc->>DB: INSERT allocations
        AllocSvc->>DB: UPDATE employee status = ALLOCATED
        AllocSvc-->>API: AllocationDto
        API-->>Client: 201 Created
    end
```

---

## 8. Manager � End an Allocation

```mermaid
sequenceDiagram
    actor Manager
    participant Client    as ProjectManagementSystem.Client
    participant API       as Web API
    participant AllocSvc  as AllocationService
    participant DB        as SQL Server

    Manager->>Client: Allocate Resource ? [3] End an existing allocation
    Client->>Manager: Prompt: Select Project
    Manager->>Client: Alpha Portal (201)

    Client->>API: GET /api/manager/projects/201/allocations
    API->>DB: SELECT active allocations WHERE project_id=201 AND manager owns project
    DB-->>API: [Ravi Kumar 50%, Neha Joshi 50%]
    API-->>Client: Allocation list
    Client->>Manager: Show active allocation list

    Manager->>Client: Select allocation #1 (Ravi Kumar)
    Client->>Manager: Confirm: "End Ravi Kumar's allocation on Alpha Portal? Set end date to today?"
    Manager->>Client: [Y] Yes

    Client->>API: PUT /api/manager/allocations/1/end
    API->>AllocSvc: EndAsync(allocationId=1)
    AllocSvc->>DB: Validate manager owns the project
    AllocSvc->>DB: UPDATE allocations SET to_date = TODAY, is_active = false WHERE id=1
    AllocSvc->>DB: SELECT COUNT(*) active allocations WHERE employee_id = Ravi.id
    DB-->>AllocSvc: count = 0

    alt No other active allocations
        AllocSvc->>DB: UPDATE employees SET status = BENCH WHERE id = Ravi.id
    end

    DB-->>AllocSvc: OK
    AllocSvc-->>API: 204 No Content
    API-->>Client: 204 No Content
    Client->>Manager: "Allocation ended. Ravi Kumar freed from Alpha Portal as of today. ?"
```

---

## 9. Manager � View Project Health + AI Risk Summary

```mermaid
sequenceDiagram
    actor Manager
    participant Client as ProjectManagementSystem.Client
    participant API    as Web API
    participant MgrSvc as ManagerService
    participant DB     as SQL Server

    Manager->>Client: Manager Menu - [3] My Projects
    Client->>API: GET /api/manager/projects
    API->>MgrSvc: GetMyProjectsAsync(managerId)
    MgrSvc->>DB: Load projects, milestones, timesheets
    MgrSvc->>MgrSvc: ComputeDisplayHealth() per project
    MgrSvc-->>API: List of ProjectDto with live health
    API-->>Client: 200 OK
    Client->>Manager: Show projects with health icons

    Manager->>Client: Select project detail
    Client->>API: GET /api/manager/projects/201/detail
    API->>MgrSvc: GetProjectDetailAsync(managerId, 201)
    MgrSvc-->>API: ProjectDetailDto with risk flags
    API-->>Client: 200 OK

    Manager->>Client: [A] Get AI Risk Summary
    Client->>API: POST /api/manager/ai/risk-summary {projectId=201}
    API->>MgrSvc: GetAIRiskSummaryAsync(request)
    MgrSvc->>MgrSvc: Placeholder AI summary from milestone/timesheet data
    MgrSvc-->>API: AIRiskSummaryResultDto
    API-->>Client: 200 OK
```

---

## 10. Employee � Submit Timesheet

```mermaid
sequenceDiagram
    actor Employee
    participant Client  as ProjectManagementSystem.Client
    participant API     as Web API
    participant EmpSvc  as EmployeePortalService
    participant DB      as SQL Server

    Employee->>Client: Employee Menu - [1] Submit Timesheet
    Client->>API: GET /api/employee/timesheets/context?weekStart=2026-05-12
    API->>EmpSvc: GetSubmitContextAsync(userId, weekStart)
    EmpSvc->>DB: SELECT active allocations + max weekly hours
    EmpSvc-->>API: EmployeeSubmitContextDto
    API-->>Client: Active projects for week

    Employee->>Client: Enter hours + activity tags per project
    Client->>API: POST /api/employee/timesheets {weekStart, entries}
    API->>EmpSvc: SubmitTimesheetAsync(userId, dto)

    alt Validation fails
        EmpSvc-->>API: 400 Bad Request
        API-->>Client: Error message
    else All valid
        EmpSvc->>DB: INSERT timesheets + timesheet_entries
        EmpSvc-->>API: TimesheetDto
        API-->>Client: 201 Created
    end
```

---

## 11. Background Scheduler � Auto Tasks

```mermaid
sequenceDiagram
    participant Host     as SchedulerHostedService
    participant Scheduler as SchedulerService
    participant ConfigRepo as ISystemConfigRepository
    participant AllocRepo  as IAllocationRepository
    participant EmpRepo    as IEmployeeRepository
    participant ProjRepo   as IProjectRepository
    participant TsRepo     as ITimesheetRepository
    participant DB         as SQL Server

    Note over Host: App starts - ExecuteAsync() begins loop

    loop Every N hours (from system_config)
        Host->>Scheduler: RunScheduledTasksAsync()
        Scheduler->>ConfigRepo: GetAsync()
        ConfigRepo->>DB: SELECT system_config
        ConfigRepo-->>Scheduler: SystemConfigDto via IMapper

        Note over Scheduler,DB: Task A - Recompute Employee Status
        Scheduler->>AllocRepo: GetAllActiveAsync()
        Scheduler->>EmpRepo: SetStatusAsync(ALLOCATED / BENCH)

        Note over Scheduler,DB: Task B - Project Health Flagging
        Scheduler->>ProjRepo: GetActiveAsync()
        Scheduler->>ProjRepo: UpdateHealthStatusAsync per project

        Note over Scheduler,DB: Task C - Missed Timesheet Detection
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

## 12. Admin � System Configuration Update

```mermaid
sequenceDiagram
    actor Admin
    participant Client     as ProjectManagementSystem.Client
    participant API        as Web API
    participant ConfigSvc  as SystemConfigService
    participant DB         as SQL Server

    Admin->>Client: Admin Menu ? [5] System Configuration
    Client->>API: GET /api/config
    API->>DB: SELECT * FROM system_config WHERE id=1
    DB-->>API: SystemConfig record
    API-->>Client: 200 OK { llmProvider, apiKey(masked), schedulerInterval, maxWeeklyHours }
    Client->>Admin: Display current settings

    Admin->>Client: [1] Update LLM API Key
    Client->>Admin: Prompt: New API Key
    Admin->>Client: Enter new key value

    Client->>API: PUT /api/config { llmApiKey: "sk-new-key..." }
    API->>ConfigSvc: UpdateAsync(dto)
    ConfigSvc->>DB: UPDATE system_config SET llm_api_key = encrypt(newKey) WHERE id=1
    DB-->>ConfigSvc: OK
    ConfigSvc-->>API: 204 No Content
    API-->>Client: 204 No Content
    Client->>Admin: "API key updated. ?"

    Admin->>Client: [3] Update Scheduler Interval
    Client->>Admin: Prompt: New interval in hours
    Admin->>Client: 2

    Client->>API: PUT /api/config { schedulerIntervalHours: 2 }
    API->>ConfigSvc: UpdateAsync(dto)
    ConfigSvc->>DB: UPDATE system_config SET scheduler_interval_hours = 2 WHERE id=1
    DB-->>ConfigSvc: OK
    API-->>Client: 204 No Content
    Client->>Admin: "Scheduler interval updated to 2 hours."
```

---

## 13. Repository � Entity to DTO Mapping (AutoMapper)

```mermaid
sequenceDiagram
    participant Svc  as Service
    participant Repo as EmployeeRepository
    participant EF   as AppDbContext
    participant Map  as IMapper
    participant Prof as MappingProfile

    Svc->>Repo: GetByIdAsync(101)
    Repo->>EF: Employees.FindAsync(101)
    EF-->>Repo: Employee entity

    Repo->>Map: Map EmployeeDto(entity)
    Map->>Prof: Apply mapping rules
    Note over Prof: Status enum to string<br/>Ignore navigation collections
    Prof-->>Map: EmployeeDto
    Map-->>Repo: EmployeeDto
    Repo-->>Svc: EmployeeDto

    Note over Svc,Prof: CreateAsync uses CreateEmployeeDto to Entity<br/>then Entity back to EmployeeDto after SaveChanges
```

**DI registration:** `builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);`
