using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using ProjectManagementSystem.Application;
using ProjectManagementSystem.Core.Settings;

namespace ProjectManagementSystem.Tests.Application;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var settings = new JwtSettings
        {
            Key = "ThisIsATestJwtSigningKeyWith32Chars!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryHours = 1
        };
        _sut = new JwtTokenService(Options.Create(settings));
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyJwtString()
    {
        var token = _sut.GenerateToken(1, "testuser", "Employee");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var token = _sut.GenerateToken(42, "testuser", "Manager");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("42", jwt.Subject);
    }
}
