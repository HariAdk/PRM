using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Enums;
using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Core.Validation;

public static class EnumParseHelper
{
    public static UserRole ParseUserRole(string role)
    {
        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed))
        {
            throw new ValidationException(
                $"Invalid role '{role}'. Allowed values: {RoleNames.Admin}, {RoleNames.Manager}, {RoleNames.Employee}.");
        }

        return parsed;
    }
}
