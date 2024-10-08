﻿using System.Globalization;

namespace FefuScheduleBot.Utils.Extensions;

public static class DateTimeExtension
{
    public static string ToStringWithCulture(this DateTime date, string format)
    {
        return date.ToString(format, new CultureInfo("de-DE"));
    }
    
    public static string ToStringWithCulture(this DateTime date)
    {
        return date.ToString(new CultureInfo("de-DE"));
    }
}