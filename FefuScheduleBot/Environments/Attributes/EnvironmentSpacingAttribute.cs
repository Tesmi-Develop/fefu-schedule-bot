namespace FefuScheduleBot.Environments.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class EnvironmentSpacingAttribute : Attribute
{
    public readonly int Count;

    public EnvironmentSpacingAttribute(int count = 1)
    {
        Count = count;
    }
}