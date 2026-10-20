namespace FefuScheduleBot.Models;

public enum CommandPriority
{
    Start = 1000,
    MySchedule = Start - 1,
    Schedule = MySchedule - 1,
    Default = 0,
    Low = -1000,
}
