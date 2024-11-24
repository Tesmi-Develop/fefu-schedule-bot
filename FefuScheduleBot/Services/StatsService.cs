using FefuScheduleBot.Data;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.ServiceRealisation;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;

namespace FefuScheduleBot.Services;

[Service]
public class StatsService : IInitializable
{
    [Dependency] private readonly FefuService _fefuService = default!;
    [Dependency] private readonly MongoService _mongoService = default!;
    private readonly Logger _logger = default!;
    
    private void MakeLog()
    {
        var id = $"{Guid.NewGuid().ToString()}-{DateTime.Now.Ticks}";
        var data = _mongoService.GetData<StatSchema>(id);
        
        data.Mutate((draft) =>
        {
            draft.Time = _fefuService.GetLocalTime().ToUniversalTime();
        });
        
        _logger.Debug("Added new log in stats");
    }

    public StatsInfo CollectInfo()
    {
        var currentTime = _fefuService.GetLocalTime();
        long totalUsage = 0;
        long weekUsage = 0;
        long todayUsage = 0;

        foreach (var log in _mongoService.GetData<StatSchema>())
        {
            var convertedTime = _fefuService.ToLocalTime(log.Data.Time);
            var studyWeek = _fefuService.GetStudyWeek(convertedTime);
            
            totalUsage++;
            weekUsage += studyWeek.End.AddDays(2).Date >= currentTime ? 1 : 0;
            todayUsage += convertedTime.Date == currentTime.Date ? 1 : 0;
        }

        return new StatsInfo(totalUsage, weekUsage, todayUsage);
    }
    
    public void Init()
    {
        _fefuService.CompletedRequest += MakeLog;
    }
}