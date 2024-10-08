using FefuScheduleBot.Data;
using FefuScheduleBot.Utils;
using JetBrains.Annotations;

namespace FefuScheduleBot.Schemas;

[Serializable, Collection("Guilds"), PublicAPI]
public sealed class GuildSchema : Schema
{
    public Dictionary<string, NotificationChatData> Chats { get; set; } = new();
    public DateTime NextUpdate = Utility.GetNextUpdateDateTime();

    public GuildSchema(string id) : base(id)
    {
    }
}