using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Infrastructure.Models;

namespace ProjectManagementSystem.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ResourceSkill> ResourceSkills => Set<ResourceSkill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<TimesheetReminderState> TimesheetReminderStates => Set<TimesheetReminderState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(200);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Designation).HasMaxLength(100);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.IsForcePasswordChange).HasDefaultValue(false);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resources");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(r => r.UserId).IsUnique();

            entity.HasOne(r => r.User)
                .WithOne(u => u.Resource)
                .HasForeignKey<Resource>(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReportingManager)
                .WithMany(u => u.DirectReports)
                .HasForeignKey(r => r.ReportingManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Category).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(s => s.Name).IsUnique();
        });

        modelBuilder.Entity<ResourceSkill>(entity =>
        {
            entity.ToTable("ResourceSkills");
            entity.HasKey(rs => rs.Id);
            entity.Property(rs => rs.ProficiencyLevel).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(rs => rs.Resource)
                .WithMany(r => r.Skills)
                .HasForeignKey(rs => rs.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rs => rs.Skill)
                .WithMany(s => s.ResourceSkills)
                .HasForeignKey(rs => rs.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(rs => new { rs.ResourceId, rs.SkillId }).IsUnique();
        });

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

        modelBuilder.Entity<Allocation>(entity =>
        {
            entity.ToTable("Allocations");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.IsActive).HasDefaultValue(true);

            entity.HasOne(a => a.Resource)
                .WithMany(r => r.Allocations)
                .HasForeignKey(a => a.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Project)
                .WithMany(p => p.Allocations)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Timesheet>(entity =>
        {
            entity.ToTable("Timesheets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TotalHours).HasColumnType("decimal(5,2)");
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(t => t.Resource)
                .WithMany(r => r.Timesheets)
                .HasForeignKey(t => t.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => new { t.ResourceId, t.WeekStartDate }).IsUnique();
        });

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

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("SystemConfig");
            entity.HasKey(sc => sc.Id);
            entity.Property(sc => sc.LlmProvider).IsRequired().HasMaxLength(50);
            entity.Property(sc => sc.LlmApiKey).HasMaxLength(500);
            entity.Property(sc => sc.SchedulerIntervalHours).HasDefaultValue(4);
            entity.Property(sc => sc.MaxWeeklyHours).HasDefaultValue(40);
            entity.Property(sc => sc.SmtpHost).HasMaxLength(200);
            entity.Property(sc => sc.SmtpUsername).HasMaxLength(150);
            entity.Property(sc => sc.SmtpPassword).HasMaxLength(500);
            entity.Property(sc => sc.EmailFromAddress).HasMaxLength(150);
            entity.Property(sc => sc.SmtpPort).HasDefaultValue(587);
        });

        modelBuilder.Entity<TimesheetReminderState>(entity =>
        {
            entity.ToTable("TimesheetReminderStates");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.ResourceId, s.WeekStartDate }).IsUnique();

            entity.HasOne(s => s.Resource)
                .WithMany()
                .HasForeignKey(s => s.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
