using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.TelegramBotComponents;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;

namespace FefuScheduleBot.Services;

[Service]
public class TelegramBot : IStartable
{
    public TelegramBotClient Client { get; private set; } = default!;
    
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    [Dependency] private readonly DependenciesContainer _container = default!;
    [Dependency] private readonly StatsService _statsService = default!;
    private readonly Logger _logger = default!;
    
    private CancellationTokenSource _cancellationToken = default!;
    private ScheduleGenerator _generator = default!;
    private string[] _excludeList = [];
    private void ConnectToEvents()
    {
        Client.OnMessage += OnMessage;
        Client.OnError += OnError;
    }

    private async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
        await Task.Delay(2000, _cancellationToken.Token);
    }
    
    private async Task OnMessage(Message message, UpdateType updateType)
    {
        if (message.Text == null) return;

        if (_excludeList.Length > 0 && message.Chat.Username is not null && _excludeList.Contains(message.Chat.Username))
            return;
        
        if (message.Text.StartsWith("/start"))
        {
            await OnStartCommand(message);
            return;
        }

        if (message.Text.StartsWith("/stats"))
        {
            await OnStatisticsCommand(message);
            return;
        }
    }

    private async Task OnStatisticsCommand(Message message)
    {
        var statistics = _statsService.CollectInfo();
        await Client.SendMessage(message.Chat.Id, 
            "\ud83d\udcc8 Статистика запросов \ud83d\udcc8\n\n" +
            $"Всего: {statistics.TotalUsage}\n" +
            $"Неделя: {statistics.WeekUsage}\n" +
            $"Сегодня: {statistics.TodayUsage}"
            );
    }

    private async Task OnStartCommand(Message message)
    {
        await _generator.Start(message.Chat);
    }
    
    public async Task Start()
    {
        if (_environmentData.TelegramToken == "None") return;

        if (_environmentData.ExcludeList != "None")
            _excludeList = _environmentData.ExcludeList.Split(" ");
        
        _cancellationToken = new CancellationTokenSource();
        Client = new TelegramBotClient(_environmentData.TelegramToken, cancellationToken: _cancellationToken.Token);

        _generator = new ScheduleGenerator();
        _container.Inject(_generator);
        
        ConnectToEvents();
        
        var me = await Client.GetMe();
        _logger.Info($"@{me.Username} is running...");
    }
}