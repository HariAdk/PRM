using ProjectManagementSystem.Core.Constants;
using ProjectManagementSystem.Core.Enums;

namespace ProjectManagementSystem.Core.Validation;

public static class ActivityTagValidator
{
    private static readonly HashSet<string> AllowedTags =
        ActivityTags.All.ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static void Validate(string activityTags)
    {
        if (string.IsNullOrWhiteSpace(activityTags))
            throw new InvalidOperationException("At least one activity tag is required for entries with hours.");

        var tags = activityTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tags.Length == 0)
            throw new InvalidOperationException("At least one activity tag is required for entries with hours.");

        foreach (var tag in tags)
        {
            if (!AllowedTags.Contains(tag))
            {
                throw new InvalidOperationException(
                    $"Invalid activity tag '{tag}'. Choose from the predefined list.");
            }
        }
    }

    public static void ValidateEntryTags(IEnumerable<(decimal Hours, string ActivityTags)> entries)
    {
        foreach (var (hours, tags) in entries)
        {
            if (hours > 0)
                Validate(tags);
        }
    }
}
