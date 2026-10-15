using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace FefuScheduleBot.Services;

[Service]
public class ExceptionHandlerService : IInitializable
{
    [Dependency] private readonly EnvironmentData _environmentData = null!;
    [Dependency] private readonly TelegramBotService _telegramBotService = null!;
    [Dependency] private readonly EventBus _eventBus = null!;
    [Dependency] private readonly Logger _logger = null!;
    
    public void Init()
    {
        _eventBus.ScheduledSendException += ScheduleSendException;
    }
    
    public void ScheduleSendException(Exception exception)
    {
        if (!long.TryParse(_environmentData.DeveloperTelegramId, out var result))
        {
            _logger.Error($"Could not parse Developer Telegram Id: {_environmentData.DeveloperTelegramId}");
            return;
        }
        
        _ = _telegramBotService.SendSafeMessage(result, $"Произошла ошибка в боте!\n\n{exception}");
        _logger.Error(exception.ToString());
    }
}