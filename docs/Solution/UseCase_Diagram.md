# PRM Tool — Use Case Diagram

## 1. System Overview & Actors

The **Project & Resource Management (PRM) System** is a client-server application designed to streamline resource planning, project milestone tracking, timesheet management, and AI-driven skill matching/risk analysis.

The system defines four primary actors:
1. **Admin (System Operator)**: Manages core system data, users, employees, projects, and global configuration. Creates all user accounts.
2. **Manager (Delivery Manager)**: Manages their **team** (employees assigned via `manager_id`), allocates resources, tracks project health, and reviews team timesheets.
3. **Employee (Individual Contributor)**: Views active allocations, submits weekly timesheets with activity tags, and views historical timesheets.
4. **Background Scheduler (System Actor)**: Automatically executes periodic updates to recompute resource utilization, project health, and flag missed timesheets.

---

## 2. Main Use Case Diagram

The diagram below maps all actor-to-system interactions and use-case boundaries.

```mermaid
flowchart TB
    %% Actors
    Admin((👤 Admin))
    Manager((👤 Manager))
    Employee((👤 Employee))
    Scheduler((⚙️ Background Scheduler))

    subgraph PRM["Project & Resource Management (PRM) System"]
        
        %% Core Auth Use Cases
        subgraph Auth["Authentication & Session"]
            UC_Login(["Login"])
            UC_Logout(["Logout"])
            UC_ChangePassword(["Change Password"])
        end

        %% Admin Use Cases
        subgraph AdminUCs["Admin Features"]
            UC_ManageUsers(["Manage Users"])
            UC_CreateUser(["Create User Account"])
            UC_AutoCreateEmployee(["Auto-Create Employee Profile"])
            UC_ViewUsers(["View All Users"])
            UC_ResetPassword(["Reset User Password"])
            UC_DeactivateUser(["Deactivate User"])
            UC_ReactivateUser(["Reactivate User"])

            UC_ManageEmployees(["Manage Employees"])
            UC_ViewEmployees(["View All Employees"])
            UC_UpdateEmployee(["Update Employee Details"])
            UC_DeactivateEmployee(["Deactivate Employee"])
            UC_AssignManager(["Assign Manager"])
            UC_ManageSkills(["Manage Employee Skills"])

            UC_ManageProjects(["Manage Projects"])
            UC_CreateProject(["Create Project with Story Points"])
            UC_ViewProjects(["View All Projects"])
            UC_UpdateProject(["Update Project Details"])
            UC_ManageMilestones(["Manage Milestones with Story Points"])

            UC_ViewAllAllocations(["View All Allocations"])
            UC_SystemConfig(["System Configuration"])
        end

        %% Manager Use Cases
        subgraph ManagerUCs["Manager Features"]
            UC_ResourceDashboard(["View Team Resource Dashboard"])
            UC_DrillEmployee(["Drill into Team Employee Details"])

            UC_AllocateResource(["Allocate Resource to Project"])
            UC_FindResourceAI(["Find Resource using AI"])
            UC_AllocateDirectly(["Allocate Directly"])
            UC_EndAllocation(["End Allocation"])

            UC_MyProjects(["View My Projects"])
            UC_GetAIRiskSummary(["Get AI Risk Summary"])

            UC_ViewTeamTimesheets(["View Team Timesheets"])
            UC_ViewTimesheetDetail(["View Employee Timesheet Detail"])

            UC_AIAssistant(["Use AI Assistant"])
            UC_AISkillMatch(["Get AI Skill Match Recommendations"])
            UC_AIRiskSummary(["Get AI Risk Summary Analysis"])
        end

        %% Employee Use Cases
        subgraph EmployeeUCs["Employee Features"]
            UC_SubmitTimesheet(["Submit Timesheet"])
            UC_LogHours(["Log Hours Worked"])
            UC_TagWork(["Select/Add Activity Tags"])

            UC_ViewMyTimesheets(["View My Timesheets"])
            UC_ViewMyTimesheetDetail(["View Week Details"])

            UC_ViewMyAllocations(["View My Allocations"])
        end

        %% Scheduler Use Cases
        subgraph SchedulerUCs["Background Jobs"]
            UC_RecomputeBenchStatus(["Recompute Bench Status"])
            UC_UpdateProjHealth(["Update Project Health"])
            UC_FlagMissedTimesheets(["Flag Missed Timesheets"])
        end
    end

    %% Admin Associations
    Admin --> UC_Login
    Admin --> UC_Logout
    Admin --> UC_ChangePassword
    Admin --> UC_ManageUsers
    Admin --> UC_ManageEmployees
    Admin --> UC_ManageProjects
    Admin --> UC_ViewAllAllocations
    Admin --> UC_SystemConfig

    %% Admin Sub-use case relations
    UC_ManageUsers --> UC_CreateUser
    UC_ManageUsers --> UC_ViewUsers
    UC_ManageUsers --> UC_ResetPassword
    UC_ManageUsers --> UC_DeactivateUser
    UC_ManageUsers --> UC_ReactivateUser
    UC_CreateUser -.->|"<<include>> when role = Employee"| UC_AutoCreateEmployee

    UC_ManageEmployees --> UC_ViewEmployees
    UC_ManageEmployees --> UC_UpdateEmployee
    UC_ManageEmployees --> UC_DeactivateEmployee
    UC_ManageEmployees --> UC_AssignManager
    UC_ManageEmployees --> UC_ManageSkills

    UC_ManageProjects --> UC_CreateProject
    UC_ManageProjects --> UC_ViewProjects
    UC_ManageProjects --> UC_UpdateProject
    UC_ManageProjects --> UC_ManageMilestones

    %% Deactivate Employee includes ending allocations & blocking user login
    UC_DeactivateEmployee -.->|"<<include>>"| UC_EndAllocation
    UC_DeactivateEmployee -.->|"<<include>>"| UC_DeactivateUser

    %% Manager Associations
    Manager --> UC_Login
    Manager --> UC_Logout
    Manager --> UC_ChangePassword
    Manager --> UC_ResourceDashboard
    Manager --> UC_AllocateResource
    Manager --> UC_MyProjects
    Manager --> UC_ViewTeamTimesheets
    Manager --> UC_AIAssistant

    %% Manager Sub-use case relations
    UC_ResourceDashboard --> UC_DrillEmployee
    UC_AllocateResource --> UC_FindResourceAI
    UC_AllocateResource --> UC_AllocateDirectly
    UC_AllocateResource --> UC_EndAllocation

    UC_MyProjects --> UC_GetAIRiskSummary
    UC_ViewTeamTimesheets --> UC_ViewTimesheetDetail

    UC_AIAssistant --> UC_AISkillMatch
    UC_AIAssistant --> UC_AIRiskSummary

    %% AI Integrations (Includes/Extends)
    UC_FindResourceAI -.->|"<<include>>"| UC_AISkillMatch
    UC_GetAIRiskSummary -.->|"<<include>>"| UC_AIRiskSummary

    %% Employee Associations
    Employee --> UC_Login
    Employee --> UC_Logout
    Employee --> UC_ChangePassword
    Employee --> UC_SubmitTimesheet
    Employee --> UC_ViewMyTimesheets
    Employee --> UC_ViewMyAllocations

    %% Employee Sub-use case relations
    UC_SubmitTimesheet --> UC_LogHours
    UC_SubmitTimesheet --> UC_TagWork
    UC_ViewMyTimesheets --> UC_ViewMyTimesheetDetail

    %% Scheduler Associations
    Scheduler --> UC_RecomputeBenchStatus
    Scheduler --> UC_UpdateProjHealth
    Scheduler --> UC_FlagMissedTimesheets

    %% General relationships
    UC_CreateUser -.->|"<<include>> forces change on login"| UC_ChangePassword
    UC_ResetPassword -.->|"<<include>> forces change on login"| UC_ChangePassword
```

---

## 3. Actor & Use Case Specifications

### 3.1 Common Authentication & Security
The following use cases apply to all user accounts and govern access control.

| Use Case | Description | Trigger / Source | Preconditions & Validation Rules |
|---|---|---|---|
| **Login** | User enters username and password to receive a JWT session token. | Client startup (Option 1) | Credentials must match active record in DB. If `force_password_change` is `true`, must redirect to **Change Password**. |
| **Logout** | Invalidates the local session context. | Any Main Menu | User must be logged in. |
| **Change Password** | Sets a new password and resets the `force_password_change` flag to `false`. | Redirect on Login, or manual update | Must enter matching password and confirmation. Must meet password strength criteria. |

---

### 3.2 Admin Use Cases (System Operator)
Admin handles the administrative and configuration data, leaving operational activities to Managers and Employees.

```mermaid
flowchart LR
    Admin((👤 Admin))
    
    subgraph Users["User & Config Management"]
        UC_CreateUser(["Create User"])
        UC_ResetPassword(["Reset Password"])
        UC_DeactivateUser(["Deactivate User"])
        UC_SystemConfig(["System Config"])
    end
    
    subgraph Master["Master Data Management"]
        UC_AssignManager(["Assign Manager"])
        UC_ManageSkills(["Manage Skills"])
        UC_CreateProject(["Create Project"])
        UC_ManageMilestones(["Manage Milestones"])
        UC_ViewAllAllocations(["View All Allocations"])
    end
    
    Admin --> UC_CreateUser
    Admin --> UC_ResetPassword
    Admin --> UC_DeactivateUser
    Admin --> UC_SystemConfig
    Admin --> UC_AssignManager
    Admin --> UC_ManageSkills
    Admin --> UC_CreateProject
    Admin --> UC_ManageMilestones
    Admin --> UC_ViewAllAllocations
```

#### 3.2.1 Manage Users & Employees
- **Create User Account**: Admins can create accounts for any role (Admin, Manager, Employee). The user is flagged with `force_password_change = true`.
- **Auto-Create Employee Profile ()**: When Admin creates a user with role `Employee`, the system automatically creates an employee profile (`status = BENCH`, department/designation = Unassigned). No separate "Add Employee" step required.
- **Assign Manager**: Admin links an employee to a reporting manager by setting `employee.manager_id` to the manager's `user.id`. Validates employee role = Employee and manager role = Manager.
- **Deactivate Employee**:
  > Triggering deactivation ends all active allocations as of today (`to_date` set to today), sets `employee.is_active` to `false`, and deactivates the linked User account (blocking login access). History is preserved.
- **Manage Skills**: Allows adding skills (grouped by `Backend`, `Frontend`, `DevOps`, `QA`, `Other` categories) and setting proficiency level (`Beginner`, `Intermediate`, `Advanced`).

#### 3.2.2 Manage Projects & Global Configuration
- **Create Project**: Creates a new project in `PLANNED`, `ACTIVE`, or `ON_HOLD` status, assigns a Manager, and sets `total_story_points`.
- **Manage Milestones**: Defines due dates, statuses (`NOT_STARTED`, `IN_PROGRESS`, `DONE`), and `story_points` per deliverable.
- **System Configuration**: Updates configuration variables stored in `SYSTEM_CONFIG`:
  - LLM Provider choice (`Gemini` / `Groq`)
  - Encrypted LLM API Key
  - Scheduler Interval Hours
  - Max Weekly Hours (Default: 40)

---

### 3.3 Manager Use Cases (Project & Delivery Owner)
Managers run project deliveries for their **team only** (employees where `manager_id` = manager's user id), perform resource mapping, check team hours, and utilize AI analytics.

```mermaid
flowchart LR
    Manager((👤 Manager))
    
    subgraph Dashboard["Team Resource Dashboard"]
        UC_ResourceDashboard(["Team Dashboard"])
        UC_DrillEmployee(["Drill into Team Employee"])
    end
    
    subgraph Allocations["Team Allocations"]
        UC_FindResourceAI(["Find Resource using AI"])
        UC_AllocateDirectly(["Allocate Directly"])
        UC_EndAllocation(["End Allocation"])
    end

    subgraph Projects["Project Delivery"]
        UC_MyProjects(["View My Projects"])
        UC_GetAIRiskSummary(["Get AI Risk Summary"])
        UC_ViewTeamTimesheets(["View Team Timesheets"])
    end

    Manager --> UC_ResourceDashboard
    Manager --> UC_FindResourceAI
    Manager --> UC_AllocateDirectly
    Manager --> UC_EndAllocation
    Manager --> UC_MyProjects
    Manager --> UC_ViewTeamTimesheets
    
    UC_ResourceDashboard --> UC_DrillEmployee
    UC_MyProjects --> UC_GetAIRiskSummary
    UC_FindResourceAI -.->|"<<include>>"| UC_GetAIRiskSummary
```

#### 3.3.1 Resource Dashboards & Drill Downs
- **View Team Resource Dashboard**: Lists **team members** currently on `BENCH` and those with active allocations (including free percentages). Scoped by `employee.manager_id`.
- **Drill into Employee Details**: Displays the employee's active allocations, skills, and **Recent Activity Tags** from their last 4 weeks of timesheets. Only accessible for direct reports.

#### 3.3.2 Allocate Resource (Operational Constraints)
- **Find Resource using AI**: Uses natural language requirements to pre-filter team candidates (availability, skill keywords), then queries the LLM or falls back to rule-based keyword matching (`UsedFallback` flag).
- **Allocate Directly**: Manager selects an owned project and a **team employee**, then defines utilization % and date range.
  > **Allocation Limit Constraints**: Sum of utilization percentages across overlapping allocations must not exceed 100%. Employee must be on manager's team (`manager_id` check).
- **End Allocation**: Ends an active allocation on a managed project by setting its `to_date` to today's date.

#### 3.3.3 Project Health & Team Timesheets
- **View My Projects**: Lists projects managed by the logged-in manager, displaying calculated health status and story point progress.
- **Get AI Risk Summary**: Compiles milestones, allocations, and logged vs. expected hours. Sends to LLM or uses template fallback.
- **View Team Timesheets**: Displays **direct reports'** hours and status for a selected week on manager's projects.

---

### 3.4 Employee Use Cases (Individual Contributor)
Employees track tasks and log hours worked against their allocations.

```mermaid
flowchart LR
    Employee((👤 Employee))

    subgraph Actions["Employee Actions"]
        UC_SubmitTimesheet(["Submit Timesheet"])
        UC_LogHours(["Log Hours"])
        UC_TagWork(["Select/Add Activity Tags"])
        UC_ViewMyTimesheets(["View My Timesheets"])
        UC_ViewMyAllocations(["View My Allocations"])
    end

    Employee --> UC_SubmitTimesheet
    Employee --> UC_ViewMyTimesheets
    Employee --> UC_ViewMyAllocations
    
    UC_SubmitTimesheet --> UC_LogHours
    UC_SubmitTimesheet --> UC_TagWork
```

- **Submit Timesheet**: Logs hours worked for allocated projects. Employees select a start week (must be a Monday, cannot be a future week) and enter hours worked.
  > **Timesheet Validation Constraints**:
  > - Hours logged per project cannot exceed `(allocation % × max weekly hours)`.
  > - Total hours across all entries cannot exceed the configured maximum weekly hours (default: 40).
  > - Employee can only log hours against projects they have an active allocation for in that week.
  > - Duplicate timesheets for the same week start date are blocked.
- **Select/Add Activity Tags**: Selects standard tags (e.g., "Backend API", "Testing & QA") or inputs custom ones. Indexed by AI Skill Matcher for real skill application evidence.
- **View My Timesheets**: Lists past timesheets with status (`SUBMITTED` or `MISSED`) and allows drilling down into specific entries.

---

### 3.5 Background Scheduler Use Cases (System Actor)
A background hosted service (`SchedulerHostedService` via `AddHostedService`) runs periodically according to system configuration.

```mermaid
flowchart LR
    Scheduler((⚙️ Background Scheduler))

    subgraph Jobs["Scheduled Jobs"]
        UC_RecomputeBenchStatus(["Recompute Bench Status"])
        UC_UpdateProjHealth(["Update Project Health"])
        UC_FlagMissedTimesheets(["Flag Missed Timesheets"])
    end

    Scheduler --> UC_RecomputeBenchStatus
    Scheduler --> UC_UpdateProjHealth
    Scheduler --> UC_FlagMissedTimesheets
```

- **Recompute Bench Status**: Loops through all employees. If they have active allocations today, status is set to `ALLOCATED`. Otherwise, status is updated to `BENCH`.
- **Update Project Health**: Scans active projects using milestone due dates and statuses.
- **Flag Missed Timesheets**: Scans the previous completed week. For any employee with active allocations who has not submitted a timesheet, creates a `MISSED` record.

---

## 4. Key Business Constraints & Relationship Summary

### 4.1 Use Case Relationships (Include / Extend)

| Source Use Case | Target Use Case | Relationship | Rationale |
|---|---|---|---|
| **Create User Account** | **Auto-Create Employee Profile** | `<<include>>` | When role = Employee, profile is created automatically. |
| **Deactivate Employee** | **End Allocation** | `<<include>>` | Deactivating an employee ends all active allocations today. |
| **Deactivate Employee** | **Deactivate User** | `<<include>>` | Deactivating an employee blocks their login. |
| **Create User Account** | **Change Password** | `<<include>>` | Forces a password change on first login. |
| **Reset User Password** | **Change Password** | `<<include>>` | Forces a password change on next login. |
| **Find Resource using AI** | **Get AI Skill Match Recommendations** | `<<include>>` | Calls AI provider or rule-based fallback. |
| **Get AI Risk Summary** | **Get AI Risk Summary Analysis** | `<<include>>` | Sends project details to LLM or template fallback. |
| **Submit Timesheet** | **Log Hours** | `<<include>>` | A timesheet must specify hours worked per project. |
| **Submit Timesheet** | **Select/Add Activity Tags** | `<<include>>` | Each entry must have activity tags. |

### 4.2 System Bounds & Integrity Constraints
- **Separation of Roles**: Each user is mapped to exactly one role (`Admin`, `Manager`, or `Employee`). Menus, screen routing, and REST controller endpoints are partitioned and validated via JWT role-based claims.
- **Manager Team Scoping**: Managers can only view, allocate, and review timesheets for employees where `employee.manager_id` equals the manager's `user.id`.
- **Resource Protection**: Only the manager assigned to a project can allocate resources to it, end allocations on it, or view timesheets for it. Employees can only view their own allocations and submit/view their own timesheets.
- **Capacity Safe-Guards**: A manager cannot over-allocate a resource past 100% total utilization in any overlapping timeframe.
- **Data Retention**: Deactivating users or ending allocations does not delete historical records.
- **No Self-Registration**: All accounts created by Admin only.
