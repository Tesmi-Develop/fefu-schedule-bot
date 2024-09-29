using System.Globalization;

namespace FefuScheduleBot.Utils.Extensions;

public static class TimeOnlyExtension
{
    public static string ToStringWithCulture(this TimeOnly date)
    {
        return date.ToString(new CultureInfo("de-DE"));
    }
}