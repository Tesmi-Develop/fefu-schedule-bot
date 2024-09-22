using System.Collections;
using FefuScheduleBot.Data;

namespace FefuScheduleBot.Classes;

public class Calendar : IEnumerable<Tuple<List<FefuEvent>, string, string>>
{
    private const int ExcludeDiscipline = 13933; // PE
    private Dictionary<string, Dictionary<string, List<FefuEvent>>> Days = new();
    
    public Calendar(FefuEvent[] events)
    {
        foreach (var @event in events)
        {
            AddEvent(@event);
        }
    }

    private string GenerateTimePeriod(DateTime start, DateTime end)
    {
        return $"{start.TimeOfDay}-{end.TimeOfDay}";
    }

    public List<List<string>> ToColumns(Func<List<FefuEvent>, string> resolveEvent)
    {
        var сolumns = new List<List<string>>();
        var firstColumn = new List<string>();
        
        firstColumn.Add("");
        сolumns.Add(firstColumn);
        var allTimes = new SortedSet<string>();

        foreach (var (day, times) in Days)
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
            var nextColumn = new List<string>();
            
            nextColumn.Add(day);
            сolumns.Add(nextColumn);
            
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
        
        return сolumns;
    }
    
    public void AddEvent(FefuEvent @event) 
    {
        var period = GenerateTimePeriod(@event.Start, @event.End);
        var day = @event.Start.Date.ToString().Split(" ")[0];

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

        foreach (var (events, day, times) in this)
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

    public IEnumerator<Tuple<List<FefuEvent>, string, string>> GetEnumerator()
    {
        foreach (var (day, times) in Days)
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