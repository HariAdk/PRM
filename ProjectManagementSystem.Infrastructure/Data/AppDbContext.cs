using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ?? User ????????????????????????????????????????????????????????????
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.ForcePasswordChange).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ?? Employee ?????????????????????????????????????????????????????????
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReportingManager)
                  .WithMany()
                  .HasForeignKey(e => e.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ?? Skill ?????????????????????????????????????????????????????????????
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Category).HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(s => s.Name).IsUnique();
        });

        // ?? EmployeeSkill ?????????????????????????????????????????????????????
        modelBuilder.Entity<EmployeeSkill>(entity =>
        {
            entity.ToTable("EmployeeSkills");
            entity.HasKey(es => es.Id);

            entity.Property(es => es.ProficiencyLevel).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(es => es.Employee)
                  .WithMany(e => e.Skills)
                  .HasForeignKey(es => es.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(es => es.Skill)
                  .WithMany(s => s.EmployeeSkills)
                  .HasForeignKey(es => es.SkillId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(es => new { es.EmployeeId, es.SkillId }).IsUnique();
        });

        // ?? Project ???????????????????????????????????????????????????????????
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.HealthStatus).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(p => p.Manager)
                  .WithMany(u => u.ManagedProjects)
                  .HasForeignKey(p => p.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ?? Milestone ?????????????????????????????????????????????????????????
        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.ToTable("Milestones");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(m => m.Project)
                  .WithMany(p => p.Milestones)
                  .HasForeignKey(m => m.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ?? Allocation ????????????????????????????????????????????????????????
        modelBuilder.Entity<Allocation>(entity =>
        {
            entity.ToTable("Allocations");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.UtilisationPercent).IsRequired();
            entity.Property(a => a.IsActive).HasDefaultValue(true);

            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.Allocations)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Project)
                  .WithMany(p => p.Allocations)
                  .HasForeignKey(a => a.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ?? Timesheet ?????????????????????????????????????????????????????????
        modelBuilder.Entity<Timesheet>(entity =>
        {
            entity.ToTable("Timesheets");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TotalHours).HasColumnType("decimal(5,2)");
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(t => t.Employee)
                  .WithMany(e => e.Timesheets)
                  .HasForeignKey(t => t.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // One timesheet per employee per week — enforced at DB level
            entity.HasIndex(t => new { t.EmployeeId, t.WeekStartDate }).IsUnique();
        });

        // ?? TimesheetEntry ????????????????????????????????????????????????????
        modelBuilder.Entity<TimesheetEntry>(entity =>
        {
            entity.ToTable("TimesheetEntries");
            entity.HasKey(te => te.Id);

            entity.Property(te => te.Hours).HasColumnType("decimal(5,2)");
            entity.Property(te => te.ActivityTags).HasMaxLength(500);

            entity.HasOne(te => te.Timesheet)
                  .WithMany(t => t.Entries)
                  .HasForeignKey(te => te.TimesheetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(te => te.Project)
                  .WithMany(p => p.TimesheetEntries)
                  .HasForeignKey(te => te.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ?? SystemConfig ??????????????????????????????????????????????????????
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("SystemConfig");
            entity.HasKey(sc => sc.Id);

            entity.Property(sc => sc.LlmProvider).IsRequired().HasMaxLength(50);
            entity.Property(sc => sc.LlmApiKey).HasMaxLength(500);
            entity.Property(sc => sc.SchedulerIntervalHours).HasDefaultValue(4);
            entity.Property(sc => sc.MaxWeeklyHours).HasDefaultValue(40);
        });
    }
}
