namespace ProjectManagementSystem.Core.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string username, string role);
}
