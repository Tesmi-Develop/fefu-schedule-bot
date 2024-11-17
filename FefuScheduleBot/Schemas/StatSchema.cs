using JetBrains.Annotations;

namespace FefuScheduleBot.Schemas;

[Serializable, Collection("Stats"), PublicAPI]
public sealed class StatSchema : Schema
{
    public DateTime Time = DateTime.Now;
    
    public StatSchema(string id) : base(id)
    {
    }
}