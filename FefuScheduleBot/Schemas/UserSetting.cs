using JetBrains.Annotations;

namespace FefuScheduleBot.Schemas;

[Serializable, Collection("UserSettings"), PublicAPI]
public class UserSetting : Schema
{
    public List<string> Subgroups = [];
    
    public UserSetting(string id) : base(id)
    {
    }
}