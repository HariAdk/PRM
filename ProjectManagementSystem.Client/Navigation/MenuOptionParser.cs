namespace ProjectManagementSystem.Client.Navigation;

public static class MenuOptionParser
{
    public static bool TryParse<TEnum>(string input, out TEnum action) where TEnum : struct, Enum
    {
        action = default;
        if (!int.TryParse(input, out var value))
            return false;

        if (!Enum.IsDefined(typeof(TEnum), value))
            return false;

        action = (TEnum)Enum.ToObject(typeof(TEnum), value);
        return true;
    }
}
