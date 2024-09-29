using System.Globalization;
using System.Reflection;
using System.Text.Json;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Data;
using FefuScheduleBot.Environments;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using JetBrains.Annotations;
using Calendar = FefuScheduleBot.Classes.Calendar;

// ReSharper disable CoVariantArrayConversion

namespace FefuScheduleBot.Services;

[PublicAPI]
[Service]
public class FefuService : IInitializable
{
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    private readonly Logger _logger = default!;
    
    private const string Url = "https://univer.dvfu.ru/";
    private const string SheduleApi = "schedule/get";
    private readonly TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Vladivostok Standard Time");
    private readonly HttpClient _client = new();
    private readonly List<PropertyInfo> _allDateTimeProperties = []; 

    private Task<FefuEvent[]?> GetEvents(DateTime day)
    {
        return GetEvents(day, day);
    }
    
    private async Task<FefuEvent[]?> GetEvents(DateTime start, DateTime end)
    {
        end = end.AddDays(1);

        if (start >= end)
            throw new ArgumentException("start cannot be >= end");
        
        var startDate = start.Date.ToString(CultureInfo.CurrentCulture).Split(" ")[0];
        var endDate = end.Date.ToString(CultureInfo.CurrentCulture).Split(" ")[0];
        
        var request = new HttpRequestMessage();
        request.RequestUri = new Uri($"{Url}{SheduleApi}?type=agendaWeek&start={startDate}&end={endDate}&groups[]=6534&ppsId=&facilityId=0");
        request.Method = HttpMethod.Get;

        request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
        request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Headers.Add("Cookie", $"_univer_identity={_environmentData.UniverId}; _jwts={_environmentData.Jwts}; LtpaToken2={_environmentData.LtpaToken}");
        
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            return RepairFefuScheduleData(JsonSerializer.Deserialize<FefuScheduleData>(content)!).Events;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    private FefuScheduleData RepairFefuScheduleData(FefuScheduleData data)
    {
        foreach (var @event in data.Events)
        {
            foreach (var property in _allDateTimeProperties)
            {
                var value = property.GetValue(@event);
                if (value is null) continue;
                
                property.SetValue(@event, ToLocalTime((DateTime)value));
            }   
        }
        
        return data;
    }

    public DateTime GetLocalTime()
    {
        return ToLocalTime(DateTime.Now);
    }

    public DateTime ToLocalTime(DateTime time)
    {
        var utcTime = time.ToUniversalTime();
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _localTimeZone);
    }

    public async Task<Calendar> GetTomorrowSchedule()
    {
        var nextDay = GetLocalTime().AddDays(1);
        var events = await GetEvents(nextDay);

        if (events is not null) return new Calendar(events);
        events = [];
        
        _logger.Warning("Failed to retrieve the current schedule");

        return new Calendar(events);
    }

    public void Init()
    {
        var type = typeof(FefuEvent);
        var targetType = typeof(DateTime);
        
        foreach (var property in type.GetProperties())
        {
            if (property.PropertyType != targetType) continue;
            _allDateTimeProperties.Add(property);
        }
    }
}