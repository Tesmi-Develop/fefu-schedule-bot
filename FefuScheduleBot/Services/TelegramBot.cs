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
public class TelegramBot : IInitializable
{
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    [Dependency] private readonly DependenciesContainer _container = default!;
    
    private readonly Logger _logger = default!;
    
    private CancellationTokenSource _cancellationToken = default!;
    private ScheduleGenerator _generator = default!;
    public TelegramBotClient Client { get; private set; } = default!;

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
        if (!message.Text.StartsWith("/start")) return;
        
        await OnStartCommand(message);
    }

    private async Task OnStartCommand(Message message)
    {
        await _generator.Start(message.Chat);
    }
    
    public void Init()
    {
        if (_environmentData.TelegramToken == "None") return;
        
        _cancellationToken = new CancellationTokenSource();
        Client = new TelegramBotClient(_environmentData.TelegramToken, cancellationToken: _cancellationToken.Token);

        _generator = new ScheduleGenerator();
        _container.Inject(_generator);
        
        ConnectToEvents();
        
        _logger.Info("Telegram bot started");
    }
}