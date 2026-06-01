namespace ProjectManagementSystem.Core.DTOs.Auth;

public class SignUpRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Manager or Employee only — Admin is blocked on self-registration.</summary>
    public string Role { get; set; } = string.Empty;
}
