using System.Globalization;
using System.Reflection;
using System.Text.Json;
using FefuScheduleBot.Classes;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Data;
using FefuScheduleBot.Environments;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using JetBrains.Annotations;

// ReSharper disable CoVariantArrayConversion

namespace FefuScheduleBot.Services;

public enum WeekType
{
    Current,
    Next
}

[Service, PublicAPI]
public class FefuService : IInitializable
{
    public event Action? CompletedRequest; 
    public readonly int MaxSubgroups = 10;
    
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    [Dependency] private readonly Config _config = default!;
    private readonly Logger _logger = default!;
    
    private const string Url = "https://univer.dvfu.ru/";
    private const string ScheduleApi = "schedule/get";
    private readonly TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Vladivostok Standard Time");
    private readonly HttpClient _client = new();
    private readonly List<PropertyInfo> _allDateTimeProperties = []; 

    public Task<FefuEvent[]?> GetEvents(DateTime day)
    {
        return GetEvents(day, day);
    }
    
    public async Task<FefuEvent[]?> GetEvents(DateTime start, DateTime end)
    {
        start = start.AddDays(-1);
        end = end.AddDays(1);

        if (start >= end)
            throw new ArgumentException("start cannot be >= end");
        
        var startDate = start.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var endDate = end.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        var request = new HttpRequestMessage();
        
        var client = new HttpClient();
        request.RequestUri = new Uri($"{Url}{ScheduleApi}?type=agendaWeek&start={startDate}&end={endDate}&groups%5B%5D=6534&ppsGuid=&facilityId=0");
        request.Method = HttpMethod.Get;

        request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Cookie", $"_univer_identity={_environmentData.UniverId};_jwts={_environmentData.Jwts};LtpaToken2={_environmentData.LtpaToken};");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        
        _logger.Debug("Sending fefu request");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            var returning = RepairFefuScheduleData(JsonSerializer.Deserialize<FefuScheduleData>(content)!).Events;
            _logger.Debug("The data was successfully deserialized");
            CompletedRequest?.Invoke();
            
            return returning;
        }
        catch (Exception e)
        {
            _logger.Error("An error occurred during data deserialization");
            Console.WriteLine(e);
            Console.WriteLine(content);
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

    public Week GetStudyWeek()
    {
        return GetStudyWeek(GetLocalTime());
    }
    
    public Week GetStudyWeek(DateTime date, bool useSunday = false)
    {
        var myCal = CultureInfo.InvariantCulture.Calendar;
        var dayOfWeek = myCal.GetDayOfWeek(date);

        if (dayOfWeek != DayOfWeek.Sunday)
            return new Week(date.AddDays(-((int)dayOfWeek - 1)).Date, date.AddDays(6 + (useSunday ? 1 : 0) - (int)dayOfWeek).Date);
        
        if (useSunday)
            return new Week(date.AddDays(-6).Date, date.Date);
            
        var first = date.AddDays(1).Date;
        var end = first.AddDays(5).Date;
        return new Week(first, end);
    }

    public DateTime GetLocalTime()
    {
        return ToLocalTime(DateTime.Now);
    }

    public DateTime ToLocalTime(DateTime time, bool isUtc = false)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(isUtc ? time : time.ToUniversalTime(), _localTimeZone);
    }

    public async Task<Schedule> GetSchedule(WeekType weekType)
    {
        var week = weekType == WeekType.Current
            ? GetStudyWeek() 
            : GetStudyWeek(GetLocalTime().AddDays(7));
        var events = await GetEvents(week.Start, week.End);

        if (events is not null) return new Schedule(week, events);
        _logger.Warning("Failed to retrieve the current schedule");
        
        return new Schedule(week, []);
    }

    public Schedule FilterBySubgroup(Schedule schedule, int subgroup)
    {
        return schedule.UseSubgroup(subgroup, _config.CommonDisciplines);
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