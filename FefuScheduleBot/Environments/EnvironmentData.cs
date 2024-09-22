using JetBrains.Annotations;

namespace FefuScheduleBot.Environments;

[PublicAPI]
public sealed class EnvironmentData
{
    [EnvironmentComment("Uri to connect to MongoDB")]
    public readonly string MongoUri = "mongodb://localhost:27017/";
    [EnvironmentComment("Name of the MongoDB database")]
    public readonly string MongoDatabaseName = "main";
    
    [EnvironmentSpacing]
    
    [EnvironmentComment("Bot token")]
    public readonly string DiscordToken = default!;
    [EnvironmentComment("Guild id for bot testing")]
    public readonly string DiscordDevelopmentGuildId = default!;

    [EnvironmentSpacing]
    
    public readonly string UniverId = default!;
    public readonly string Jwts = default!;
    public readonly string LtpaToken = default!;
}