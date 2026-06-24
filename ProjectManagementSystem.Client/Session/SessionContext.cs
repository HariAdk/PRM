using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Client.Session;

/// <summary>
/// Holds the currently logged-in user's data for the duration of the session.
/// Cleared on logout.
/// </summary>
public class SessionContext
{
    public int      UserId              { get; private set; }
    public string   FullName            { get; private set; } = string.Empty;
    public UserRole Role                { get; private set; }
    public string   Token               { get; private set; } = string.Empty;
    public bool     ForcePasswordChange { get; private set; }
    public bool     IsLoggedIn          => UserId > 0;

    public void Set(int userId, string fullName, string role, string token, bool forcePasswordChange)
    {
        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
            throw new InvalidOperationException($"Unknown role '{role}' returned from the API.");

        UserId              = userId;
        FullName            = fullName;
        Role                = parsedRole;
        Token               = token;
        ForcePasswordChange = forcePasswordChange;
    }

    public void ClearForceChange() => ForcePasswordChange = false;

    public void Clear()
    {
        UserId              = 0;
        FullName            = string.Empty;
        Role                = default;
        Token               = string.Empty;
        ForcePasswordChange = false;
    }
}
