namespace ProjectManagementSystem.Core.DTOs.Auth;

public class ChangePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
