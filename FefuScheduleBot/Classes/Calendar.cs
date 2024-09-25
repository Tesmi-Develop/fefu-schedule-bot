using System.Collections;
using System.Globalization;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;
using FefuScheduleBot.Data;
using JetBrains.Annotations;

namespace FefuScheduleBot.Classes;

[PublicAPI]
public class Calendar : IEnumerable<Tuple<List<FefuEvent>, string, string>>
{
    private const int ExcludeDiscipline = 13933; // PE
    private readonly Dictionary<string, Dictionary<string, List<FefuEvent>>> _days = new();

    private static string GenerateTimePeriod(DateTime start, DateTime end)
    {
        return $"{start.ToShortTimeString()}-{end.ToShortTimeString()}";
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

        foreach (var (_, times) in _days)
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

        foreach (var (day, times) in _days)
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
        var day = @event.Start.Date.ToString("d", new CultureInfo("de-DE"));

        if (!_days.TryGetValue(day, out var times))
        {
            times = new();
            _days[day] = times;
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

    public IEnumerator<Tuple<List<FefuEvent>, string, string>> GetEnumerator()
    {
        foreach (var (day, times) in _days)
        {
            foreach (var (time, events) in times)
            {
                yield return new (events, day, time);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}