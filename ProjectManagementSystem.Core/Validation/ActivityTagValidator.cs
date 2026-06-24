using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Exceptions;

namespace ProjectManagementSystem.Core.Validation;

public static class ActivityTagValidator
{
    public static void Validate(string tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            throw new BusinessRuleException(ErrorMessages.ActivityTagRequired);

        foreach (var tag in tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!ActivityTags.All.Contains(tag, StringComparer.OrdinalIgnoreCase))
                throw new ValidationException(ErrorMessages.InvalidActivityTag(tag));
        }
    }

    public static void ValidateEntryTags(IEnumerable<(decimal Hours, string ActivityTags)> entries)
    {
        foreach (var (hours, tags) in entries)
        {
            if (hours <= 0)
                continue;

            Validate(tags);
        }
    }
}
