using System.Collections;

namespace FefuScheduleBot.Models;

public class Week : IEnumerable<DateTime>
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public Week(DateTime start, DateTime end)
    {
        Start = start.Date; 
        End = end.Date;
    }

    public IEnumerator<DateTime> GetEnumerator()
    {
        var currentDay = Start;

        while (currentDay <= End)
        {
            yield return currentDay;
            currentDay = currentDay.AddDays(1);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public override string ToString()
    {
        return $"{Start:dd.MM.yyyy} - {End:dd.MM.yyyy}";
    }
}