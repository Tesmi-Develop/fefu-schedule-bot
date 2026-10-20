using FefuScheduleBot.Models;

namespace FefuScheduleBot.TelegramBotComponents;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public int Priority { get; }
    
    public CommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
        Priority = (int)CommandPriority.Default;
    }
    
    public CommandAttribute(string name, string description, CommandPriority priority)
    {
        Name = name;
        Description = description;
        Priority = (int)priority;
    }
}