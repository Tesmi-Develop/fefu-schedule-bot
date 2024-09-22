using MongoDB.Driver;

namespace FefuScheduleBot.Schemas;

public class DataWrapper<T> where T : Schema
{
    public T Data { get; }

    private readonly IMongoCollection<T> _collection;
    private readonly string _id;
    
    public DataWrapper(IMongoCollection<T> collection, string id, Action<DataWrapper<T>>? onCreate = null)
    {
        _id = id;
        _collection = collection;
        Data = _collection.Find(item => item.Id == _id).FirstOrDefault();
        
        if (Data is not null) 
            return;
        
        Data = (T)Activator.CreateInstance(typeof(T), args: [_id])!;
        _collection.InsertOne(Data);
        onCreate?.Invoke(this);
    }

    public void Mutate(Action<T> mutator)
    {
        mutator(Data);
        Update();
    }

    private void Update()
    {
        _collection.ReplaceOne(item => item.Id == _id, Data);
    }
}