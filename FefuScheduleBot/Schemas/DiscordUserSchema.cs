using JetBrains.Annotations;

namespace FefuScheduleBot.Schemas;

[Serializable, Collection("Users"), PublicAPI]
public sealed class DiscordUserSchema : Schema
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime TokenExpiration { get; set; } = DateTime.Now;

    public DiscordUserSchema(string id) : base(id)
    {
    }
}