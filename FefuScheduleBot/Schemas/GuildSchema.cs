using FefuScheduleBot.Data;
using JetBrains.Annotations;

namespace FefuScheduleBot.Schemas;

[Serializable, Collection("Guilds"), PublicAPI]
public sealed class GuildSchema : Schema
{
    public Dictionary<string, NotificationChatData> Chats { get; set; } = new();

    public GuildSchema(string id) : base(id)
    {
    }
}