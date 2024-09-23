using System.Collections.Frozen;
using FefuScheduleBot.Environments;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using JetBrains.Annotations;
using MongoDB.Driver;

namespace FefuScheduleBot.Services;

[PublicAPI]
[Service]
public sealed class MongoService : IInitializable
{
    public event Action<object>? SchemaAdded;
    
    private MongoClient _client = default!;
    private IMongoDatabase _database = default!;
    private FrozenDictionary<Type, object> _collections = default!;
    
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    private readonly Logger _logger = default!;
    private readonly Dictionary<string, object> _dataWrappers = new();
    
    public void Init()
    {
        var mongoUri = _environmentData.MongoUri;
        var databaseName = _environmentData.MongoDatabaseName;
        
        _client = new MongoClient(mongoUri);
        _logger.Debug("Mongo client created");
        
        _database = _client.GetDatabase(databaseName);

        var dict = new Dictionary<Type, object>();

        foreach (var (type, attribute) in ReflectionHelper.GetAllTypes<CollectionAttribute>())
        {
            var name = attribute.Name ?? type.Name;

            _logger.Debug($"Register collection {name}");
            var method = _database.GetType().GetMethod(nameof(_database.GetCollection));
            if (method is null)
                continue;
            
            var generic = method.MakeGenericMethod([type]);
            dict[type] = generic.Invoke(_database, [name, null])!;
        }
        
        _collections = dict.ToFrozenDictionary();
        _logger.Debug("Collections registered");
    }

    public IEnumerable<DataWrapper<T>> GetData<T>() where T : Schema
    {
        return from item in ((IMongoCollection<T>)_collections[typeof(T)]).Find(_ => true).ToList() 
            select GetData<T>(item.Id);
    }

    public DataWrapper<T> GetData<T>(string id) where T : Schema
    {
        if (_dataWrappers.TryGetValue(id, out var wrapper))
            return (DataWrapper<T>)wrapper;

        var returned = new DataWrapper<T>((IMongoCollection<T>)_collections[typeof(T)], id, returned => SchemaAdded?.Invoke(returned));
        _dataWrappers.Add(id, returned);
        
        return returned;
    }

    public bool HaveData<T>(string id) where T : Schema
    {
       return ((IMongoCollection<T>)_collections[typeof(T)]).Find(schema => schema.Id == id).FirstOrDefault() is not null;
    }
}