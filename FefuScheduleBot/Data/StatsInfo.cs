namespace FefuScheduleBot.Data;

public class StatsInfo
{
    public readonly long TotalUsage;
    public readonly long WeekUsage;
    public readonly long TodayUsage;

    public StatsInfo(long totalUsage, long weekUsage, long todayUsage)
    {
        TotalUsage = totalUsage;
        WeekUsage = weekUsage;
        TodayUsage = todayUsage;
    }
}