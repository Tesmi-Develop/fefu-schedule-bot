namespace FefuScheduleBot.Data;

public class Week
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public Week(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }
}