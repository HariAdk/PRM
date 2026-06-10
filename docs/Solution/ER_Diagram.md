# PRM Tool — Entity Relationship (ER) Diagram


```mermaid
erDiagram

    %% ────────────────────────────────────────
    %%  USERS
    %% ────────────────────────────────────────
    USERS {
        int     id                      PK
        string  full_name
        string  email                   "UNIQUE"
        string  username                "UNIQUE"
        string  password_hash
        string  role                    "ADMIN | MANAGER | EMPLOYEE"
        bool    is_active               "default true"
        bool    force_password_change   "default false"
        datetime created_at
    }

    %% ────────────────────────────────────────
    %%  EMPLOYEES
    %% ────────────────────────────────────────
    EMPLOYEES {
        int     id              PK
        int     user_id         FK "→ users.id"
        int     manager_id      FK "nullable → users.id (reporting manager)"
        string  full_name
        string  email
        string  department
        string  designation
        string  status          "BENCH | ALLOCATED"
        bool    is_active
    }

    %% ────────────────────────────────────────
    %%  SKILLS
    %% ────────────────────────────────────────
    SKILLS {
        int     id          PK
        string  name
        string  category    "BACKEND | FRONTEND | DEVOPS | QA | OTHER"
    }

    %% ────────────────────────────────────────
    %%  EMPLOYEE_SKILLS  (junction)
    %% ────────────────────────────────────────
    EMPLOYEE_SKILLS {
        int     id                  PK
        int     employee_id         FK
        int     skill_id            FK
        string  proficiency_level   "BEGINNER | INTERMEDIATE | ADVANCED"
    }

    %% ────────────────────────────────────────
    %%  PROJECTS
    %% ────────────────────────────────────────
    PROJECTS {
        int     id                  PK
        int     manager_id          FK "→ users.id (delivery manager)"
        string  name
        string  description
        date    start_date
        date    end_date
        string  status              "PLANNED | ACTIVE | ON_HOLD | COMPLETED"
        string  health_status       "ON_TRACK | ATTENTION | AT_RISK"
        int     total_story_points  "admin-set project estimate"
    }

    %% ────────────────────────────────────────
    %%  MILESTONES
    %% ────────────────────────────────────────
    MILESTONES {
        int     id              PK
        int     project_id      FK
        string  title
        date    due_date
        string  status          "NOT_STARTED | IN_PROGRESS | DONE"
        int     story_points    "per-milestone estimate"
    }

    %% ────────────────────────────────────────
    %%  ALLOCATIONS
    %% ────────────────────────────────────────
    ALLOCATIONS {
        int     id                  PK
        int     employee_id         FK
        int     project_id          FK
        int     utilisation_percent "1 – 100"
        date    from_date
        date    to_date
        bool    is_active
    }

    %% ────────────────────────────────────────
    %%  TIMESHEETS
    %% ────────────────────────────────────────
    TIMESHEETS {
        int      id              PK
        int      employee_id     FK
        date     week_start_date "always Monday"
        decimal  total_hours
        string   status          "SUBMITTED | MISSED"
        datetime submitted_at
    }

    %% ────────────────────────────────────────
    %%  TIMESHEET_ENTRIES
    %% ────────────────────────────────────────
    TIMESHEET_ENTRIES {
        int      id              PK
        int      timesheet_id    FK
        int      project_id      FK
        decimal  hours
        string   activity_tags   "comma-separated"
    }

    %% ────────────────────────────────────────
    %%  SYSTEM_CONFIG  (single-row)
    %% ────────────────────────────────────────
    SYSTEM_CONFIG {
        int     id                          PK
        string  llm_provider                "Gemini | Groq"
        string  llm_api_key                 "encrypted"
        int     scheduler_interval_hours    "default 4"
        int     max_weekly_hours            "default 40"
    }

    %% ────────────────────────────────────────
    %%  RELATIONSHIPS
    %% ────────────────────────────────────────

    USERS          ||--o|  EMPLOYEES        : "has profile (user_id)"
    USERS          ||--o{  PROJECTS         : "manages (manager_id)"
    USERS          ||--o{  EMPLOYEES        : "reports to (manager_id)"

    EMPLOYEES      ||--o{  EMPLOYEE_SKILLS  : "has skills"
    SKILLS         ||--o{  EMPLOYEE_SKILLS  : "assigned via"

    PROJECTS       ||--o{  MILESTONES       : "has milestones"
    PROJECTS       ||--o{  ALLOCATIONS      : "has allocations"
    EMPLOYEES      ||--o{  ALLOCATIONS      : "allocated to"

    EMPLOYEES      ||--o{  TIMESHEETS       : "submits"
    TIMESHEETS     ||--o{  TIMESHEET_ENTRIES: "contains entries"
    PROJECTS       ||--o{  TIMESHEET_ENTRIES: "logged against"
```

---

## Relationship Summary

| Relationship | Cardinality | Description |
|---|---|---|
| `USERS` → `EMPLOYEES` | 1 : 0..1 | One user may have one employee profile (Admin has none) |
| `USERS` → `EMPLOYEES` (manager) | 1 : 0..N | A Manager user is the reporting manager for zero or many employees (`manager_id`) |
| `USERS` → `PROJECTS` | 1 : 0..N | A Manager user owns zero or many projects |
| `EMPLOYEES` → `EMPLOYEE_SKILLS` | 1 : 0..N | An employee can hold multiple skills |
| `SKILLS` → `EMPLOYEE_SKILLS` | 1 : 0..N | A skill can be assigned to many employees |
| `PROJECTS` → `MILESTONES` | 1 : 0..N | A project has zero or many milestones (each with story points) |
| `PROJECTS` → `ALLOCATIONS` | 1 : 0..N | A project has zero or many allocations |
| `EMPLOYEES` → `ALLOCATIONS` | 1 : 0..N | An employee can be on multiple projects (sum ≤ 100%) |
| `EMPLOYEES` → `TIMESHEETS` | 1 : 0..N | An employee submits one timesheet per week |
| `TIMESHEETS` → `TIMESHEET_ENTRIES` | 1 : 1..N | Each timesheet has one entry per allocated project |
| `PROJECTS` → `TIMESHEET_ENTRIES` | 1 : 0..N | Hours are logged against a project |

---

## Key Business Constraints

```
ALLOCATIONS:
  SUM(utilisation_percent) for an employee
  across all overlapping date ranges  <=  100%
  Manager can only allocate employees where employee.manager_id = manager's user id

TIMESHEETS:
  hours per entry  <=  (allocation% x max_weekly_hours)
  SUM(hours) across all entries  <=  max_weekly_hours
  One timesheet per employee per week_start_date (unique constraint)
  Cannot submit for a future week

EMPLOYEES:
  status = BENCH      if no active allocation exists for today
  status = ALLOCATED  if at least one active allocation exists for today
  (recomputed by SchedulerHostedService every N hours)
  manager_id          nullable FK to users.id — set by Admin via Assign Manager
  Auto-created        when Admin creates a user with role EMPLOYEE

PROJECTS:
  total_story_points  admin-set at project creation
  completed_story_points (DTO) = SUM(milestone.story_points WHERE status = DONE)
  health_status = AT_RISK   if any milestone is IN_PROGRESS and overdue
  health_status = ATTENTION if any milestone is NOT_STARTED and due within 7 days
  health_status = ON_TRACK  otherwise
  (persisted by scheduler; ManagerService also computes live display health)

  Accounts created only by Admin
```

---

## Application Layer Mapping (AutoMapper)

Entity models in `Infrastructure/Models/` are mapped to DTOs in `Core/DTOs/` via `Infrastructure/Mapping/MappingProfile.cs`. Repositories inject `IMapper` and return DTOs to the Application service layer — the database schema above is unchanged; mapping is an application-layer concern only.

