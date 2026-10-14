using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;

namespace FefuScheduleBot.Services;

[Service]
public class ExceptionHandlerService
{
    [Dependency] private readonly EnvironmentData _environmentData = null!;
    [Dependency] private readonly TelegramBotService _telegramBotService = null!;
    private readonly Logger _logger = null!;
    
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