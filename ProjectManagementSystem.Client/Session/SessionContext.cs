namespace ProjectManagementSystem.Client.Session;

/// <summary>
/// Holds the currently logged-in user's data for the duration of the session.
/// Cleared on logout.
/// </summary>
public class SessionContext
{
    public int    UserId              { get; private set; }
    public string FullName            { get; private set; } = string.Empty;
    public string Role                { get; private set; } = string.Empty;
    public string Token               { get; private set; } = string.Empty;
    public bool   ForcePasswordChange { get; private set; }
    public bool   IsLoggedIn          => UserId > 0;

    public void Set(int userId, string fullName, string role, string token, bool forcePasswordChange)
    {
        UserId              = userId;
        FullName            = fullName;
        Role                = role;
        Token               = token;
        ForcePasswordChange = forcePasswordChange;
    }

    public void ClearForceChange() => ForcePasswordChange = false;

    public void Clear()
    {
        UserId              = 0;
        FullName            = string.Empty;
        Role                = string.Empty;
        Token               = string.Empty;
        ForcePasswordChange = false;
    }
}
