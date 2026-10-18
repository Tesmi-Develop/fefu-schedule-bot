using System.Text.Json;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using StackExchange.Redis;
using Config = FefuScheduleBot.Data.Config;

namespace FefuScheduleBot.Services;

[Service]
public class CacheService : IInitializable
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(7);
    
    [Dependency] private readonly Config _config = null!;
    [Dependency] private readonly Logger _logger = null!;
    [Dependency] private readonly EnvironmentData _environmentData = null!;
    private ConnectionMultiplexer _redis = null!;
    private IDatabase _database = null!;
    
    public void Init()
    {
        _logger.Debug($"Try connection to redis server: {_environmentData.RedisDatabaseUri}");
        _redis = ConnectionMultiplexer.Connect(_environmentData.RedisDatabaseUri);
        _database = _redis.GetDatabase();
    }

    private string GenerateScheduleKey(Week week, string[] subgroups)
    {
        var date = week.Start.ToStringWithCulture("d");
        var subgroupEncoded = Utility.EncodeSubgroups(subgroups, _config.Subgroups);
        
        return $"{date}_{subgroupEncoded}";
    }
    
    public async Task<ScheduleCache?> GetScheduleCache(Week week, string[] selectedGroups, Schedule schedule)
    {
        var key = GenerateScheduleKey(week, selectedGroups);
        RedisValue data;
        
        try
        {
            data = await _database.StringGetAsync(key);
            if (data.IsNullOrEmpty)
                return null;

            var scheduleCache = JsonSerializer.Deserialize<ScheduleCache>(data.ToString());
            if (scheduleCache is null)
                throw new NullReferenceException($"Schedule cache for key {key} found, but deserialization resulted in null");
            
            var hash = schedule.ComputeHash();
            if (hash == scheduleCache.Hash) 
                return scheduleCache;
            
            // schedule outdate
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception e)
            {
                _logger.Error($"An unexpected error occurred while removing schedule cache {e}");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }

        return null;
    }

    private string GenerateScheduleCache(string fileId, string hash)
    {
        var cache = new ScheduleCache()
        {
            FileId = fileId,
            Hash = hash
        };

        return JsonSerializer.Serialize(cache);
    }
    
    public async Task SaveSchedule(string key, Schedule schedule, string fileId)
    {
        var hash = schedule.ComputeHash();
        var data = GenerateScheduleCache(fileId, hash);
        
        await _database.StringSetAsync(key, data, Ttl);
    }

    public async Task SafeSaveSchedule(Week week, string[] selectedGroups, Schedule schedule, string fileId)
    {
        var key = GenerateScheduleKey(week, selectedGroups);
        
        try
        {
            await SaveSchedule(key, schedule, fileId);
        }
        catch (Exception e)
        {
            _logger.Error($"An unexpected error occurred while saving schedule cache for key {key}. Details: {e.Message}");
        }
    }
}