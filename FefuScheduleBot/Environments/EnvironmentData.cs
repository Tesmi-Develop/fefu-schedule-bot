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
    
    public readonly string UniverId = null!;
    public readonly string Jwts = null!;
    public readonly string LtpaToken = null!;

    [EnvironmentSpacing]
    
    public readonly string TelegramToken = null!;
    
    [EnvironmentSpacing]
    
    public readonly string ExcludeList = "None";
}