# PRM Tool — Flow Diagrams

## Table of Contents

1. [Application Startup & Login Flow](#1-application-startup--login-flow)
2. [Role-Based Navigation Flow](#2-role-based-navigation-flow)
3. [Admin — Full Feature Flow](#3-admin--full-feature-flow)
4. [Manager — Full Feature Flow](#4-manager--full-feature-flow)
5. [Employee — Full Feature Flow](#5-employee--full-feature-flow)
6. [Allocation Validation Flow](#6-allocation-validation-flow)
7. [Timesheet Submission Validation Flow](#7-timesheet-submission-validation-flow)
8. [AI Skill Match Flow](#8-ai-skill-match-flow)
9. [AI Risk Summary Flow](#9-ai-risk-summary-flow)
10. [Background Scheduler Flow](#10-background-scheduler-flow)
11. [Employee Deactivation Flow](#11-employee-deactivation-flow)
12. [Password Policy & Reset Flow](#12-password-policy--reset-flow)
13. [Exception Handling Flow](#13-exception-handling-flow)
14. [Entity to DTO Mapping Flow (AutoMapper)](#14-entity-to-dto-mapping-flow-automapper)

---

## 1. Application Startup & Login Flow

```mermaid
flowchart TD
    A([App Launches]) --> B[Show StartScreen]
    B --> C{User Choice}

    C -->|1. Login| D[StartScreen — prompt username + password]
    C -->|2. Exit| Z([Terminate App])

    D --> F[POST /api/auth/login]
    F --> G{Credentials valid?}

    G -->|No| H[Show error message\n'Invalid username or password']
    H --> D

    G -->|Yes| I[Store JWT in SessionContext\nStore role, userId, fullName]
    I --> J{force_password_change?}

    J -->|Yes| K[Show ChangePasswordScreen\nCannot skip]
    K --> L[PUT /api/auth/change-password]
    L --> M{Password meets policy?\n8+ chars, 1 uppercase, 1 number}

    M -->|No| N[Show validation error]
    N --> K

    M -->|Yes| O[Update DB\nSet force_password_change = false]
    O --> P[Show success message]
    P --> Q[ScreenRouter — route by role via ScreenFactory]

    J -->|No| Q

    Q -->|ADMIN| R[AdminMenuScreen]
    Q -->|MANAGER| S[ManagerMenuScreen]
    Q -->|EMPLOYEE| T[EmployeeMenuScreen]
```

---

## 2. Role-Based Navigation Flow

```mermaid
flowchart LR
    Login([Logged In]) --> Router{ScreenRouter\nread UserRole from SessionContext}

    Router -->|ADMIN| AM[AdminMenuScreen]
    Router -->|MANAGER| MM[ManagerMenuScreen]
    Router -->|EMPLOYEE| EM[EmployeeMenuScreen]

    AM --> AM1[Manage Employees]
    AM --> AM2[Manage Projects]
    AM --> AM3[View All Allocations]
    AM --> AM4[Manage Users]
    AM --> AM5[System Configuration]
    AM --> AM6([Logout → StartScreen])

    MM --> MM1[Resource Dashboard]
    MM --> MM2[Allocate Resource]
    MM --> MM3[My Projects]
    MM --> MM4[Timesheets - Team View]
    MM --> MM5[AI Assistant]
    MM --> MM6([Logout → StartScreen])

    EM --> EM1[Submit Timesheet]
    EM --> EM2[View My Timesheets]
    EM --> EM3[View My Allocations]
    EM --> EM4([Logout → StartScreen])
```

Menu screens use `IScreenFactory` to create feature screens — no direct `new` of concrete screen types.

---

## 3. Admin — Full Feature Flow

```mermaid
flowchart TD
    AM([Admin Menu]) --> A1 & A2 & A3 & A4 & A5

    %% — Manage Employees —
    A1[Manage Employees] --> A1b[View All Employees\nFilter by Status / Dept]
    A1 --> A1c[Update Employee]
    A1 --> A1d[Deactivate Employee]
    A1 --> A1e[Manage Employee Skills]
    A1 --> A1f[Assign Manager]

    A1f --> VA1f{Employee role = EMPLOYEE\nManager role = MANAGER?}
    VA1f -->|No| ErrA1f[Show validation error]
    VA1f -->|Yes| SaveA1f[UPDATE employees\nSET manager_id = manager user id]

    A1d --> VA1d{Employee has\nactive allocations?}
    VA1d -->|Yes| WarnA1d[Show warning:\nlist affected projects]
    WarnA1d --> ConfA1d{Confirm?}
    ConfA1d -->|No| A1
    ConfA1d -->|Yes| DeactA1d[End all allocations\nSet is_active = false\nBlock user login]
    VA1d -->|No| DeactA1d

    A1e --> A1e1[Add Skill\nName + Category + Level]
    A1e --> A1e2[Update Proficiency]
    A1e --> A1e3[Remove Skill]

    %% — Manage Projects —
    A2[Manage Projects] --> A2a[Create Project\n+ Total Story Points]
    A2 --> A2b[View All Projects]
    A2 --> A2c[Update Project Details]
    A2 --> A2d[Manage Milestones\n+ Story Points per milestone]

    A2a --> VA2a{Manager ID valid?}
    VA2a -->|No| ErrA2a[Show error]
    VA2a -->|Yes| SaveA2a[INSERT project\nhealth = ON_TRACK]

    A2d --> A2d1[Add Milestone with StoryPoints]
    A2d --> A2d2[Update Milestone Status\nNOT_STARTED → IN_PROGRESS → DONE]

    %% — Allocations —
    A3[View All Allocations] --> A3F[Filter by Employee / Project]

    %% — Manage Users —
    A4[Manage Users] --> A4a[Create User Account\nAdmin/Manager/Employee]
    A4 --> A4b[View All Users]
    A4 --> A4c[Reset User Password]
    A4 --> A4d[Deactivate User]

    A4a --> AutoEmp{Role = Employee?}
    AutoEmp -->|Yes| AutoEmpCreate[AUTO-CREATE employee profile\nstatus = BENCH, dept/designation = Unassigned]
    AutoEmp -->|No| DoneCreate[User created only]
    AutoEmpCreate --> DoneCreate

    A4b --> A4bR{Reactivate user?}
    A4bR -->|Yes| ReactUser[Set is_active = true\nNote: allocations NOT restored]

    A4c --> SavePwd[BCrypt hash new temp password\nSet force_password_change = true]

    %% — System Config —
    A5[System Configuration] --> A5a[Update LLM API Key]
    A5 --> A5b[Change LLM Provider\nGemini / Groq]
    A5 --> A5c[Update Scheduler Interval]
    A5 --> A5d[Update Max Weekly Hours]
```

---

## 4. Manager — Full Feature Flow

```mermaid
flowchart TD
    MM([Manager Menu]) --> M1 & M2 & M3 & M4 & M5

    %% — Resource Dashboard (team-scoped) —
    M1[Resource Dashboard] --> M1a[Show ON BENCH team members\nwith skills]
    M1 --> M1b[Show ACTIVE team members\nwith utilisation %]
    M1a & M1b --> M1d{Drill into employee?}
    M1d -->|D + Employee ID| M1e[Show profile, allocations,\nrecent activity tags\nonly if employee.ManagerId = current manager]

    %% — Allocate Resource (team-scoped) —
    M2[Allocate Resource] --> M2a[AI-Assisted Search]
    M2 --> M2b[Direct Allocation]
    M2 --> M2c[End an Allocation]

    M2a --> M2a1[Select owned Project]
    M2a1 --> M2a2[Type natural language requirement]
    M2a2 --> M2a3[POST /api/manager/ai/skill-match]
    M2a3 --> M2a4[Show ranked results\nAI or KEYWORD-MATCHED if fallback]
    M2a4 --> M2a5{Select employee\nor search again?}
    M2a5 -->|0 - search again| M2a2
    M2a5 -->|Select #N| M2a6[Set Utilisation%, From, To]
    M2a6 --> AllocValidation[(Allocation Validation\nFlow — see §6)]
    AllocValidation --> M2Saved[Allocation saved]

    M2b --> M2b1[Select Project + Team Employee ID]
    M2b1 --> M2b2[Set Utilisation%, From, To]
    M2b2 --> TeamCheck{Employee on\nmanager's team?}
    TeamCheck -->|No| ErrTeam[403 Forbidden\nEmployee not on team]
    TeamCheck -->|Yes| AllocValidation

    M2c --> M2c1[Select owned Project]
    M2c1 --> M2c2[Show active allocations on project]
    M2c2 --> M2c3[Select allocation to end]
    M2c3 --> M2c4{Confirm end?}
    M2c4 -->|No| M2c2
    M2c4 -->|Yes| M2c5[Set to_date = today\nSet is_active = false]
    M2c5 --> M2c6{Any other active\nallocations for employee?}
    M2c6 -->|No| M2c7[Set employee status = BENCH]
    M2c6 -->|Yes| M2c8[Keep status = ALLOCATED]

    %% — My Projects —
    M3[My Projects] --> M3a["List projects with AT_RISK / ATTENTION / ON_TRACK - computed live via ManagerService"]
    M3a --> M3b[Select project → Project Detail\nGET /api/manager/projects/id/detail]
    M3b --> M3c[Show milestones + story points\n+ allocations + risk flags]
    M3c --> M3d{Get AI Risk Summary?}
    M3d -->|A| M3e[POST /api/manager/ai/risk-summary]
    M3e --> M3f[Display plain-English paragraph\nor template fallback]

    %% — Timesheets (team-scoped) —
    M4[Timesheets - Team View] --> M4a[Filter by week\ndefault = current week\nGET /api/manager/timesheets]
    M4a --> M4b[Show team timesheet status\nSUBMITTED / MISSED\nonly direct reports]
    M4b --> M4c{View detail?}
    M4c -->|V + Employee| M4d[Show hours per project\n+ activity tags read-only]

    %% — AI Assistant —
    M5[AI Assistant] --> M5a[Skill Match\nNatural language search]
    M5 --> M5b[Risk Summary\nSelect project]
    M5a --> M5aGo{Go to Allocate?}
    M5aGo -->|A| M2
```

---

## 5. Employee — Full Feature Flow

```mermaid
flowchart TD
    EM([Employee Menu]) --> CheckReminder{Missing timesheet\nfor last week?}
    CheckReminder -->|Yes| Reminder[Show reminder banner\nGET /api/employee/reminder]
    CheckReminder -->|No| NoReminder[No reminder shown]
    Reminder & NoReminder --> MenuOpts

    MenuOpts --> E1 & E2 & E3

    %% — Submit Timesheet —
    E1[Submit Timesheet] --> E1a[Enter week start date\nor press Enter for last Monday]
    E1a --> E1b[GET /api/employee/timesheets/context\nLoad active allocations for that week]
    E1b --> E1c{Any allocations found?}
    E1c -->|No| E1cNo[Show: No active projects for this week]
    E1c -->|Yes| E1d[For each project:\nEnter hours + select activity tags]
    E1d --> E1e[Show SUMMARY:\nhours per project + total vs max]
    E1e --> E1f{Submit or Back?}
    E1f -->|B - Back| E1d
    E1f -->|S - Submit| TsValidation[(Timesheet Validation\nFlow — see §7)]
    TsValidation --> E1g[POST /api/employee/timesheets\nStatus = SUBMITTED]

    %% — View My Timesheets —
    E2[View My Timesheets] --> E2a[GET /api/employee/timesheets\nList weeks: SUBMITTED / MISSED]
    E2a --> E2b{View week detail?}
    E2b -->|V| E2c[Show project entries:\nhours + activity tags]
    E2b -->|B| EM

    %% — View My Allocations —
    E3[View My Allocations] --> E3a[GET /api/employee/allocations\nList allocations + total utilisation %]
```

---

## 6. Allocation Validation Flow

```mermaid
flowchart TD
    Start([Allocation Request Received]) --> V0{Employee on\nmanager's team?}
    V0 -->|No| Err0[403 Forbidden\nEmployee not on team]
    V0 -->|Yes| V1{From Date\nbefore To Date?}
    V1 -->|No| Err1[400 Bad Request\n'From date must be before To date']
    V1 -->|Yes| V2{Project status\nACTIVE or PLANNED?}
    V2 -->|No| Err2[400 Bad Request\n'Project is not open for allocation']
    V2 -->|Yes| V3[Query DB:\nSUM utilisation for employee\nin overlapping date range]
    V3 --> V4{Existing% + New% > 100?}
    V4 -->|Yes| Err3[400 Bad Request\n'Over-allocation: total would be X%\nMax allowed: 100%']
    V4 -->|No| V5[INSERT allocation\nSet is_active = true]
    V5 --> V6[UPDATE employee status = ALLOCATED]
    V6 --> End([201 Created - Allocation saved])

    Err0 & Err1 & Err2 & Err3 --> ErrEnd([Return error to client])
```

---

## 7. Timesheet Submission Validation Flow

```mermaid
flowchart TD
    Start([Timesheet Submit Request Received]) --> V1{Week start date\nin the past or today?}
    V1 -->|No - future week| Err1[400 Bad Request\n'Cannot submit for a future week']
    V1 -->|Yes| V2{Timesheet already exists\nfor this employee + week?}
    V2 -->|Yes| Err2[400 Bad Request\n'Timesheet already submitted for this week']
    V2 -->|No| V3[For each entry:\nVerify employee is allocated\nto that project this week]
    V3 --> V4{All projects valid?}
    V4 -->|No| Err3[400 Bad Request\n'Not allocated to project X this week']
    V4 -->|Yes| V5[For each entry:\nhours ≤ allocation% × maxWeeklyHours?]
    V5 --> V6{All entry hours valid?}
    V6 -->|No| Err4[400 Bad Request\n'Hours exceed allocation cap for project X']
    V6 -->|Yes| V7{Total hours across\nall entries ≤ maxWeeklyHours?}
    V7 -->|No| Err5[400 Bad Request\n'Total hours exceed maximum weekly hours']
    V7 -->|Yes| V8[INSERT timesheets\nINSERT timesheet_entries\nStatus = SUBMITTED]
    V8 --> End([201 Created - Timesheet submitted])

    Err1 & Err2 & Err3 & Err4 & Err5 --> ErrEnd([Return error to client])
```

---

## 8. AI Skill Match Flow

```mermaid
flowchart TD
    Start([Manager enters requirement]) --> S1[POST /api/manager/ai/skill-match\nprojectId + requirementText]
    S1 --> S2[Verify manager owns project]
    S2 --> S3[Load team employees only\nemployee.ManagerId = current manager]
    S3 --> S4[Pre-filter candidates]
    S4 --> S4a[Skip fully allocated\navailability ≤ 0%]
    S4a --> S4b[Filter by min availability %\nfrom requirement text]
    S4b --> S4c[Filter by required weekly hours\nAiRequirementParser]
    S4c --> S4d[Filter by skill keywords\nSkillRequirementMatcher]

    S4d --> S5{Any candidates\nremain after filter?}
    S5 -->|No| S6[Return 200 OK\nempty matches list]
    S5 -->|Yes| S7{LLM configured\nin system_config?}

    S7 -->|Yes| S8[Build structured prompt\nrequirement + candidate summaries]
    S8 --> S9[Call IAiProvider.CompleteAsync\nGemini or Groq]
    S9 --> S10{Parse + validate\nLLM response?}
    S10 -->|Yes| S11[Return ranked list\nUsedFallback = false]
    S10 -->|No| S12[AiFallbackMatcher\nkeyword-based ranking\nUsedFallback = true]

    S7 -->|No| S12

    S11 --> S13[Console shows results\nwith AI or KEYWORD-MATCHED note]
    S12 --> S13
    S13 --> S14{Manager selects\nan employee?}
    S14 -->|0 - search again| Start
    S14 -->|Select #N| S15[Proceed to Allocation\nValidation Flow]
```

---

## 9. AI Risk Summary Flow

```mermaid
flowchart TD
    Start([Manager requests risk summary\nfor a project]) --> R1[POST /api/manager/ai/risk-summary\nprojectId]
    R1 --> R2[Verify manager owns project]
    R2 --> R3[Load project milestones:\ntitle, due date, status, story points]
    R3 --> R4[Load active allocations:\nemployee name, utilisation%]
    R4 --> R5[Load timesheet entries\nfor last week on this project:\nhours per employee]
    R5 --> R6[Compute expected hours:\nalloc% × maxWeeklyHours]
    R6 --> R7[Build risk flags:\noverdue milestones, low effort,\nNOT_STARTED near due date]
    R7 --> R8{LLM configured?}

    R8 -->|Yes| R9[Call IAiProvider.CompleteAsync]
    R9 --> R10{API response OK\nand parseable?}
    R10 -->|Yes| R11[Return AI-generated summary]
    R10 -->|No| R12[AiFallbackMatcher.BuildRiskSummary\ntemplate prose from flags]

    R8 -->|No| R12
    R11 --> R13[Console displays summary]
    R12 --> R13
```

---

## 10. Background Scheduler Flow

```mermaid
flowchart TD
    AppStart([API App Starts]) --> Reg[AddHostedService SchedulerHostedService]
    Reg --> Host[SchedulerHostedService.ExecuteAsync\nauto-started by host]
    Host --> Sched[SchedulerService.RunScheduledTasksAsync]
    Sched --> ReadConfig[Read system_config:\nschedulerIntervalHours]

    subgraph Loop [Every N Hours]
        direction TB

        T1[Task A — Employee Status Recompute]
        T1 --> T1a[SELECT employee_ids with active allocations\nwhere today BETWEEN from_date AND to_date]
        T1a --> T1b[UPDATE employees SET status = ALLOCATED\nfor matched IDs]
        T1b --> T1c[UPDATE employees SET status = BENCH\nfor remaining active employees]

        T2[Task B — Project Health Flagging]
        T2 --> T2a[SELECT all ACTIVE projects with milestones]
        T2a --> T2b{Any milestone\nIN_PROGRESS\nand due_date < today?}
        T2b -->|Yes| T2c[SET health_status = AT_RISK]
        T2b -->|No| T2d{Any milestone\nNOT_STARTED\nand due_date ≤ today + 7 days?}
        T2d -->|Yes| T2e[SET health_status = ATTENTION]
        T2d -->|No| T2f[SET health_status = ON_TRACK]

        T3[Task C — Missed Timesheet Detection]
        T3 --> T3a[Calculate last_monday date]
        T3a --> T3b[SELECT employees who had\nactive allocations last week]
        T3b --> T3c[For each employee:\ncheck if timesheet exists\nfor week_start = last_monday]
        T3c --> T3d{Timesheet exists?}
        T3d -->|Yes| T3e[Skip — already submitted]
        T3d -->|No| T3f[INSERT timesheet\nstatus = MISSED\ntotal_hours = 0]

        T1 --> T2 --> T3
    end

    ReadConfig --> Loop
    Loop --> Wait[SchedulerHostedService\nawait Task.Delay N hours]
    Wait --> Loop
```

---

## 11. Employee Deactivation Flow

```mermaid
flowchart TD
    Start([Admin — Deactivate Employee]) --> D1[Enter Employee ID]
    D1 --> D2[Load employee record + active allocations]
    D2 --> D3{Employee found\nand is_active = true?}
    D3 -->|No| D4[Show: Employee not found or already inactive]
    D3 -->|Yes| D5{Has active\nallocations?}
    D5 -->|Yes| D6[Show warning:\nList of projects to be removed from]
    D6 --> D7{Admin confirms?}
    D7 -->|No - Cancel| D8([Return to Manage Employees])
    D7 -->|Yes| D9[End all active allocations:\nSET to_date = today\nSET is_active = false]
    D5 -->|No| D10[SET employee is_active = false]
    D9 --> D10
    D10 --> D11[SET user is_active = false\nBlock login for linked user account]
    D11 --> D12[All timesheet + allocation history preserved]
    D12 --> D13[Show: Employee deactivated]
    D13 --> D8
```

---

## 12. Password Policy & Reset Flow

```mermaid
flowchart TD
    Start([Password Input Received]) --> P1{Length ≥ 8 characters?}
    P1 -->|No| Err1[Validation Error:\n'Password must be at least 8 characters']
    P1 -->|Yes| P2{Contains at least\n1 uppercase letter?}
    P2 -->|No| Err2[Validation Error:\n'Password must contain at least one uppercase letter']
    P2 -->|Yes| P3{Contains at least\n1 number?}
    P3 -->|No| Err3[Validation Error:\n'Password must contain at least one number']
    P3 -->|Yes| P4[BCrypt hash password]
    P4 --> P5[Store password_hash in users table]
    P5 --> End([Password accepted])

    Err1 & Err2 & Err3 --> ErrEnd([Show error to user — re-prompt])

    subgraph Admin Reset
        AR1([Admin — Reset User Password]) --> AR2[Enter Username or User ID]
        AR2 --> AR3[Enter new temporary password]
        AR3 --> P1
        P5 --> AR4[SET force_password_change = true]
        AR4 --> AR5[User must change on next login]
    end
```

---

## 13. Exception Handling Flow

```mermaid
flowchart TD
    Req([HTTP Request]) --> MW[ExceptionHandlingMiddleware]
    MW --> Next[Controller → Service → Repository]
    Next --> OK{Success?}

    OK -->|Yes| Resp([200/201/204 JSON ApiResponse])
    OK -->|No - Exception thrown| Catch[Middleware catches exception]
    Catch --> Map[ExceptionResponseMapper.Map]
    Map --> Kind{Exception type?}

    Kind -->|AppException| AppMap[Map AppErrorKind to HTTP status\n404 / 400 / 401 / 403 / 500]
    Kind -->|KeyNotFoundException| N404[404 Not Found]
    Kind -->|UnauthorizedAccessException| N401[401 or 403 legacy check]
    Kind -->|ArgumentException etc.| N400[400 Bad Request]
    Kind -->|Other| N500[500 Unexpected Error]

    AppMap --> JSON[Return ApiResponse.Fail userMessage\nJSON camelCase]
    N404 & N401 & N400 & N500 --> JSON
    JSON --> Client([Client shows user-friendly error])
```

**Custom exceptions:** `NotFoundException`, `BusinessRuleException`, `ValidationException`, `UnauthorizedAppException`, `ForbiddenAppException` — all extend `AppException`.

---

## 14. Entity to DTO Mapping Flow (AutoMapper)

```mermaid
flowchart LR
    subgraph API["ProjectManagementSystem (Web API)"]
        Ctrl[Controllers]
    end

    subgraph App["ProjectManagementSystem.Application"]
        Svc[Service Layer]
    end

    subgraph Infra["ProjectManagementSystem.Infrastructure"]
        Repo[Repository]
        EF[AppDbContext]
        Profile[MappingProfile]
        Mapper[IMapper]
    end

    subgraph Core["ProjectManagementSystem.Core"]
        DTO[DTOs]
        Entity[EF Entity Models]
    end

    Ctrl --> Svc
    Svc -->|calls| Repo
    Repo -->|query| EF
    EF -->|returns entity| Repo
    Repo -->|mapper.Map| Mapper
    Mapper -->|uses| Profile
    Profile -->|Entity ↔ DTO rules| DTO
    Repo -->|returns DTO| Svc

    subgraph CreateFlow["Create / Update path"]
        CreateDto[Create*Dto / Update*Dto] -->|mapper.Map entity| Mapper
        Mapper -->|persist| EF
        EF -->|saved entity| Mapper
        Mapper -->|mapper.Map dto| DTO
    end
```

**Registered once at startup:** `builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);`

**Repositories using AutoMapper:** `UserRepository`, `EmployeeRepository`, `ProjectRepository`, `AllocationRepository`, `SkillRepository`, `TimesheetRepository`, `SystemConfigRepository`
