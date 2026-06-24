using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetCurrentUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(value) || !int.TryParse(value, out var userId) || userId <= 0)
            throw new UnauthorizedAppException(ErrorMessages.InvalidUserIdentity);

        return userId;
    }

    public static void EnsureUserIdMatches(this ClaimsPrincipal user, int requestedUserId)
    {
        if (user.GetCurrentUserId() != requestedUserId)
            throw new ForbiddenAppException(ErrorMessages.CannotChangeOtherUserPassword);
    }
}
