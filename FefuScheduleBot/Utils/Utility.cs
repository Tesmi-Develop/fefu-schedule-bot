﻿using System.Collections.Specialized;
using FefuScheduleBot.Services;
using MongoDB.Driver.Linq;

namespace FefuScheduleBot.Utils;

public static class Utility
{
    public static NameValueCollection ParseQueryParams(string data)
    {
        var nameValueCollection = new NameValueCollection();
        var querySegments = data.Split('&');
        
        foreach(var segment in querySegments)
        { 
            var parts = segment.Split('=');
            if (parts.Length <= 0) continue;
            
            var key = parts[0].Trim(['?', ' ']);
            var val = parts[1].Trim();
            nameValueCollection.Add(key, val);
        }

        return nameValueCollection;
    }

    public static string ConvertQueryParams(NameValueCollection queryParams)
    {
        return string.Join("&", queryParams.AllKeys.Select(key => $"{key}={queryParams[key]}"));
    }

    public static DateTime GetNextUpdateDateTime()
    {
        var fefuService = Program.DependenciesContainer.Resolve<FefuService>();
        return fefuService.GetLocalTime().AddDays(1).Date.AddHours(20);
    }
}