using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Infrastructure.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Employee? Employee { get; set; }
    public ICollection<Project> ManagedProjects { get; set; } = [];
}
