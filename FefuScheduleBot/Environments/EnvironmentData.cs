using FefuScheduleBot.Environments.Attributes;
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
    
    [EnvironmentComment("Uri to connect to Redis")]
    public readonly string RedisUri = "localhost:6379";
    
    [EnvironmentSpacing]
    
    public readonly string UniverId = null!;
    public readonly string Jwts = null!;
    public readonly string LtpaToken = null!;

    [EnvironmentSpacing]
    
    public readonly string TelegramToken = null!;
    public readonly string DeveloperTelegramId = null!;
    
    [EnvironmentSpacing]
    
    public readonly string ExcludeList = "None";
}