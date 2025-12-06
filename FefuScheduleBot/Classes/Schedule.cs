using System.Collections;
using FefuScheduleBot.Data;
using FefuScheduleBot.Utils.Extensions;
using JetBrains.Annotations;
// ReSharper disable SuspiciousTypeConversion.Global

namespace FefuScheduleBot.Classes;

public class SchedulePairList {
    private readonly List<FefuEvent> _events;
    private readonly string _day;
    private readonly string _time;
        
    public SchedulePairList(List<FefuEvent> events, string day, string time)
    {
        _events = events;
        _day = day;
        _time = time;
    }

    public void Deconstruct(out List<FefuEvent> events, out string day, out string time)
    {
        events = _events;
        day = _day;
        time = _time;
    }
}

[PublicAPI]
public class Schedule : IEnumerable<SchedulePairList>
{
    public const int CountWorkingDays = 6;
    public const int CountLessons = 7;
    private static readonly TimeOnly LessonStartTime = new(8, 30);
    private static readonly TimeSpan LessonDurationTime = new(0, 1, 30, 0);
    private static readonly TimeSpan BreakTime = new(0, 0, 10, 0);
    public static readonly List<string> HashedLessonTimes = [];

    static Schedule()
    {
        var startTime = LessonStartTime;
        for (var i = 1; i <= CountLessons; i++)
        {
            var endTime = startTime.Add(LessonDurationTime);
            HashedLessonTimes.Add($"{startTime.ToStringWithCulture()}-{endTime.ToStringWithCulture()}");

            startTime = endTime.Add(BreakTime);
        }
    }
    
    public SortedDictionary<DateTime, Dictionary<string, List<FefuEvent>>> Days { get; } = new();
    public Week Week { get; }

    private static string GenerateTimePeriod(DateTime start, DateTime end)
    {
        return $"{start.ToStringWithCulture("t")}-{end.ToStringWithCulture("t")}";
    }
    
    public Schedule(Week week, FefuEvent[] events)
    {
        Week = week;
        
        foreach (var day in week)
        {
            Days[day] = [];

            for (var i = 1; i <= CountLessons; i++)
            {
                var hashedTime = Schedule.HashedLessonTimes[i - 1];
                Days[day][hashedTime] = [];
            }
        }

        foreach (var @event in events)
            AddEvent(@event);
    }

    public Schedule UseSubgroup(int subgroup, IReadOnlyList<int> excludeDiscipline)
    {
        var newEvents = new List<FefuEvent>();

        foreach (var (events, _, _) in this)
        {
            foreach (var @event in events)
            {
                if (@event.Subgroup != string.Empty && @event.Subgroup != subgroup.ToString() &&  !excludeDiscipline.Contains(@event.DisciplineId))
                    continue;
                    
                newEvents.Add(@event);
            }
        }

        return new Schedule(Week, newEvents.ToArray());
    }

    public IEnumerator<SchedulePairList> GetEnumerator()
    {
        foreach (var (day, times) in Days)
        {
            foreach (var (time, events) in times)
            {
                yield return new SchedulePairList(events, day.ToStringWithCulture("d"), time);
            }
        }
    }
    
    private void AddEvent(FefuEvent @event)
    {
        var period = GenerateTimePeriod(@event.Start, @event.End);
        var day = @event.Start.Date;
        
        if (!Days.TryGetValue(day, out var times))
        {
            times = new Dictionary<string, List<FefuEvent>>();
            Days[day] = times;
        }
        
        if (!times.TryGetValue(period, out var events))
        {
            events = [];
            times[period] = events;
        }
        
        events.Add(@event);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}