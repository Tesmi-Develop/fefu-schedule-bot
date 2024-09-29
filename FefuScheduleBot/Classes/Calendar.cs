using System.Collections;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;
using FefuScheduleBot.Data;
using FefuScheduleBot.Utils.Extensions;
using JetBrains.Annotations;
// ReSharper disable SuspiciousTypeConversion.Global

namespace FefuScheduleBot.Classes;

public class CalendarPairList {
    private readonly List<FefuEvent> _events;
    private readonly string _day;
    private readonly string _time;
        
    public CalendarPairList(List<FefuEvent> events, string day, string time)
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
public class Calendar : IEnumerable<CalendarPairList>
{
    public static int CountLessons = 7;
    public static int CountWorkingDays = 6;
    public static readonly TimeOnly LessonStartTime = new(8, 30);
    public static readonly TimeSpan LessonDuractionTime = new(0, 1, 30, 0);
    public static readonly TimeSpan BreakTime = new(0, 0, 10, 0);
    public static readonly List<string> HashedLessonTimes = [];

    static Calendar()
    {
        var startTime = LessonStartTime;
        for (var i = 1; i <= CountLessons; i++)
        {
            var endTime = startTime.Add(LessonDuractionTime);
            HashedLessonTimes.Add($"{startTime.ToStringWithCulture()}-{endTime.ToStringWithCulture()}");

            startTime = endTime.Add(BreakTime);
        }
    }
    
    public Dictionary<string, Dictionary<string, List<FefuEvent>>> Days { get; } = new();

    private const int ExcludeDiscipline = 13933; // PE

    private static string GenerateTimePeriod(DateTime start, DateTime end)
    {
        return $"{start.ToStringWithCulture("t")}-{end.ToStringWithCulture("t")}";
    }
    
    public Calendar(FefuEvent[] events)
    {
        foreach (var @event in events)
        {
            AddEvent(@event);
        }
    }

    public List<List<string>> ToColumns(Func<List<FefuEvent>, string> resolveEvent)
    {
        var columns = new List<List<string>>();
        var firstColumn = new List<string> { "" };

        columns.Add(firstColumn);
        var allTimes = new SortedSet<string>();

        foreach (var (_, times) in Days)
        {
            foreach (var (time, _) in times)
            {
                allTimes.Add(time);
            }
        }

        var offsets = new Dictionary<string, int>();

        var offset = 0;
        foreach (var time in allTimes)
        {
            firstColumn.Add(time);
            offsets.Add(time, offset);
            offset++;
        }

        foreach (var (day, times) in Days)
        {
            var nextColumn = new List<string> { day };

            columns.Add(nextColumn);
            
            for (var i = 0; i < offset; i++)
            {
                nextColumn.Add("");
            }
            
            foreach (var (time, @event) in times)
            {
                var index = offsets[time] + 1;
                nextColumn[index] = resolveEvent(@event);
            }
        }
        
        return columns;
    }
    
    private void AddEvent(FefuEvent @event)
    {
        var period = GenerateTimePeriod(@event.Start, @event.End);
        var day = @event.Start.Date.ToStringWithCulture("d");
        
        if (!Days.TryGetValue(day, out var times))
        {
            times = new();
            Days[day] = times;
        }
        
        if (!times.TryGetValue(period, out var events))
        {
            events = [];
            times[period] = events;
        }
        
        events.Add(@event);
    }

    public Calendar UseSubgroup(int subgroup)
    {
        var newCalendar = new Calendar([]);

        foreach (var (events, _, _) in this)
        {
            foreach (var @event in events)
            {
                if (@event.Subgroup != string.Empty && @event.Subgroup != subgroup.ToString() && @event.DisciplineId != ExcludeDiscipline)
                    continue;
                    
                newCalendar.AddEvent(@event);
            }
        }

        return newCalendar;
    }

    public Table ConvertInTable()
    {
        var columns = ToColumns((@event) => @event[0].Title);
        var maxHeight = columns[0].Count;
        var header = new List<Column>();

        foreach (var column in columns)
        {
            header.Add(new Column(column[0]));
        }
        
        var table = new Table(header.ToArray())
        {
            Config = TableConfig.Markdown()
        };

        for (var i = 1; i < maxHeight; i++)
        {
            var row = new List<Cell>();
            
            foreach (var column in columns)
            {
                row.Add(new Cell(column[i]));
            }

            table.AddRow(row.ToArray());
        }

        return table;
    }

    public IEnumerator<CalendarPairList> GetEnumerator()
    {
        foreach (var (day, times) in Days)
        {
            foreach (var (time, events) in times)
            {
                yield return new CalendarPairList(events, day, time);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}