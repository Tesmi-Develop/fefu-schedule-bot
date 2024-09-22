namespace FefuScheduleBot.Schemas;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CollectionAttribute : Attribute
{
    public readonly string? Name;
    
    public CollectionAttribute(string? name = null)
    {
        Name = name;
    }
}