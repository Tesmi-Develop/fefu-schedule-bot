using System.Reflection;
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
public class TelegramBotService : IStartable
{
    public TelegramBotClient Client { get; private set; } = null!;
    
    [Dependency] private readonly EnvironmentData _environmentData = null!;
    [Dependency] private readonly DependenciesContainer _container = null!;
    [Dependency] private readonly StatsService _statsService = null!;
    private readonly Logger _logger = null!;
    private Dictionary<string, (ICommand, CommandAttribute)> _commands = new();
    
    private CancellationTokenSource _cancellationToken = null!;
    private ScheduleGenerator _generator = null!;
    private string[] _excludeList = [];
    private void ConnectToEvents()
    {
        Client.OnMessage += OnMessage;
        Client.OnError += OnError;
    }
    
    public static Dictionary<string, (ICommand Command, CommandAttribute Attribute)> GetCommands(
        DependenciesContainer container, 
        Assembly? assembly = null)
    {
        var targetAssembly = assembly ?? Assembly.GetExecutingAssembly();
    
        var commandTypes = targetAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } 
                           && typeof(ICommand).IsAssignableFrom(type) 
                           && type.IsDefined(typeof(CommandAttribute), inherit: false));

        var result = new Dictionary<string, (ICommand Command, CommandAttribute Attribute)>();

        foreach (var type in commandTypes)
        {
            var attribute = type.GetCustomAttribute<CommandAttribute>();
            if (attribute is null)
                continue;

            if (Activator.CreateInstance(type) is not ICommand command)
                continue;

            result[attribute.Name] = (command, attribute);
        }

        return result;
    }

    private async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
        await Task.Delay(2000, _cancellationToken.Token);
    }
    
    private async Task OnMessage(Message message, UpdateType updateType)
    {
        if (message.Text == null || !message.Text.StartsWith('/')) 
            return;

        if (_excludeList.Length > 0 && message.Chat.Username is not null && _excludeList.Contains(message.Chat.Username))
            return;
        
        var commandName = message.Text.Substring(1,  message.Text.Length - 1);
        if (!_commands.TryGetValue(commandName, out var commandData))
            return;

        await commandData.Item1.Execute(message, _container);
    }
    
    public async Task InitializeCommandsAsync(CancellationToken cancellationToken = default)
    {
        if (_commands.Count == 0) 
            return;

        var botCommands = _commands.Values
            .Select(c => new BotCommand 
            { 
                Command = c.Item2.Name.TrimStart('/'), 
                Description = c.Item2.Description 
            });

        await Client.SetMyCommands(botCommands, cancellationToken: cancellationToken);
    }
    
    public async Task Start()
    {
        _commands = GetCommands(_container);
        
        if (_environmentData.TelegramToken == "None") 
            return;

        if (_environmentData.ExcludeList != "None")
            _excludeList = _environmentData.ExcludeList.Split(" ");
        
        _cancellationToken = new CancellationTokenSource();
        Client = new TelegramBotClient(_environmentData.TelegramToken, cancellationToken: _cancellationToken.Token);

        _generator = new ScheduleGenerator();
        _container.Inject(_generator);
        
        ConnectToEvents();

        await InitializeCommandsAsync();
        var me = await Client.GetMe();
        _logger.Info($"@{me.Username} is running...");
    }
}