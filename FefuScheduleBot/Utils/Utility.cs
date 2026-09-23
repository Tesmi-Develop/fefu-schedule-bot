using System.Collections.Specialized;
using FefuScheduleBot.Services;

namespace FefuScheduleBot.Utils;

public static class Utility
{
    public static NameValueCollection ParseQueryParams(string data)
    {
        var nameValueCollection = new NameValueCollection();
        if (string.IsNullOrWhiteSpace(data))
        {
            return nameValueCollection;
        }

        var querySegments = data.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in querySegments)
        {
            var parts = segment.Split('=', 2);
            if (parts.Length == 0)
            {
                continue;
            }

            var key = parts[0].Trim('?', ' ');
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            var val = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            nameValueCollection.Add(key, val);
        }

        return nameValueCollection;
    }

    public static string ConvertQueryParams(NameValueCollection? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
            return string.Empty;

        var segments = queryParams.AllKeys
            .Where(key => !string.IsNullOrEmpty(key))
            .SelectMany(key => queryParams.GetValues(key) ?? [], 
                (key, val) => $"{key}={val}");

        return string.Join("&", segments);
    }
    
    public static string EncodeSubgroups(IEnumerable<string> selectedSubgroups, string[] availableSubgroups)
    {
        ushort mask = 0;
        var selectedSet = selectedSubgroups.ToHashSet();

        for (var i = 0; i < availableSubgroups.Length; i++)
        {
            if (selectedSet.Contains(availableSubgroups[i]))
            {
                mask |= (ushort)(1 << i);
            }
        }

        return mask.ToString("X");
    }

    public static string[] DecodeSubgroups(string hexMask, string[] availableSubgroups)
    {
        if (!ushort.TryParse(hexMask, System.Globalization.NumberStyles.HexNumber, null, out var mask))
        {
            return [];
        }

        var selected = new List<string>();

        for (var i = 0; i < availableSubgroups.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                selected.Add(availableSubgroups[i]);
            }
        }

        return selected.ToArray();
    }

    public static DateTime GetNextUpdateDateTime()
    {
        var fefuService = Program.DependenciesContainer.Resolve<FefuService>();
        return fefuService.GetLocalTime().AddDays(1).Date.AddHours(20);
    }
}