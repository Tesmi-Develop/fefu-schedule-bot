using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FefuScheduleBot.Schemas;

[PublicAPI]
public abstract class Schema : ISchema
{
    [BsonId]
    public ObjectId DataId { get; private set; }
    public string Id { get; private set; }
    
    protected Schema(string id)
    {
        Id = id;
    }
}