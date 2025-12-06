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
    
    public readonly string UniverId = default!;
    public readonly string Jwts = default!;
    public readonly string LtpaToken = default!;

    [EnvironmentSpacing]
    
    public readonly string TelegramToken = default!;
    
    [EnvironmentSpacing]
    
    public readonly string ExcludeList = "None";
}