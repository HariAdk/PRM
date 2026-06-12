namespace ProjectManagementSystem.Infrastructure.Models;

public class User
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsForcePasswordChange { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;
    public Resource? Resource { get; set; }
    public ICollection<Project> ManagedProjects { get; set; } = [];
    public ICollection<Resource> DirectReports { get; set; } = [];
}
