namespace FefuScheduleBot.Environments;

[AttributeUsage(AttributeTargets.Field)]
public sealed class EnvironmentCommentAttribute : Attribute
{
    public readonly string Description;

    public EnvironmentCommentAttribute(string description = "")
    {
        Description = description;
    }
}