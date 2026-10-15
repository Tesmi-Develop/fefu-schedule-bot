using System.Reflection;
using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.TelegramBotComponents;
using FefuScheduleBot.TelegramBotComponents.States;
using FefuScheduleBot.Utils;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;

namespace FefuScheduleBot.Services;

[Service]
public class TelegramBotService : IInitializable, IStartable
{
    public TelegramBotClient Client { get; private set; } = null!;
    
    [Dependency] private readonly EnvironmentData _environmentData = null!;
    [Dependency] private readonly DependenciesContainer _container = null!;
    [Dependency] private readonly StatsService _statsService = null!;
    [Dependency] private readonly EventBus _eventBus = null!;
    [Dependency] private readonly Logger _logger = null!;
    
    private Dictionary<string, (ICommand, CommandAttribute)> _commands = new();
    private CancellationTokenSource _cancellationToken = null!;
    private string[] _excludeList = [];
    private readonly Dictionary<string, Type> _allState = new();
    
    public void Init()
    {
        RegisterStates();
    }
    
    public async Task Start(CancellationToken token)
    {
        _commands = GetCommands(_container);
        
        if (_environmentData.TelegramToken == "None") 
            return;

        if (_environmentData.ExcludeList != "None")
            _excludeList = _environmentData.ExcludeList.Split(" ");
        
        Client = new TelegramBotClient(_environmentData.TelegramToken, cancellationToken: token);
        
        ConnectToEvents();

        await InitializeCommandsAsync();
        var me = await Client.GetMe();
        _logger.Info($"@{me.Username} is running...");
    }
    
    private static Dictionary<string, (ICommand Command, CommandAttribute Attribute)> GetCommands(
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
    
    private void ConnectToEvents()
    {
        Client.OnMessage += OnMessage;
        Client.OnError += OnError;
        Client.OnUpdate += OnUpdate;
    }

    private Task OnUpdate(Update update)
    {
        if (update.Type != UpdateType.CallbackQuery || update.CallbackQuery is null) return Task.CompletedTask;

        try
        {
            ProcessCallbackQuery(update.CallbackQuery);
        }
        catch (Exception e)
        {
            _eventBus.OnScheduledSendException(e);
        }
        
        return Task.CompletedTask;
    }

    private void RegisterStates()
    {
        foreach (var (type, _) in ReflectionHelper.GetAllTypes<StateAttribute>())
        {
            var targetType = typeof(IChainState);
            
            if (!type.IsAssignableTo(targetType))
            {
                _logger.Warning($"Found a {type.Name} that does not inherit a class {targetType.Name}");
                continue;
            }
            
            _allState[type.Name] = type;
        }
    }
    
    private void ProcessCallbackQuery(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data is null || callbackQuery.Message is null) 
            return;

        var data = Utility.ParseQueryParams(callbackQuery.Data);
        var nextState = data["State"];
        data.Remove("State");
        
        if (nextState is null) 
            return;
        var state = _allState[nextState];
        
        TransferToNextState(state, callbackQuery, Utility.ConvertQueryParams(data));
    }

    private void TransferToNextState(Type stateType, CallbackQuery callbackQuery, string data)
    {
        var state = (IChainState)stateType.GetConstructors()[0].Invoke([]);
        _container.Inject(state);

        _ = Callback();
        return;

        async Task Callback()
        {
            try
            {
                await state.Process(this, callbackQuery, data);
            }
            catch (Exception e)
            {
                _eventBus.OnScheduledSendException(e);
            }
        }
    }

    public string GenerateTransferStateData<T>(string additionalData = "")
    {
        return $"State={typeof(T).Name}&{additionalData}";
    }
    
    private Task OnError(Exception exception, HandleErrorSource source)
    {
        _logger.Error(exception.ToString());
        return Task.CompletedTask;
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

        _logger.Info($"Starting command: /{commandName}");
        
        try
        {
            await commandData.Item1.Execute(message, _container);
        }
        catch (Exception e)
        {
            if (message.From is not null)
                await SendSafeMessage(message.Chat,
                    $"Не удалось выполнить команду /{commandData.Item2.Name}. Ошибка была отправлена разработчику");
            _eventBus.OnScheduledSendException(e);
        }
    }

    public async Task SendSafeMessage(ChatId chatId, string messageText)
    {
        try
        {
            await Client.SendMessage(chatId, messageText);
        }
        catch (Exception e)
        {
            _logger.Error(e.ToString());
        }
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
    
    public async Task StartNewScheduleRequest(ChatId id, string[] subgroups)
    {
        var request = new RequestWeekType();
        _container.Inject(request);
        
        await request.Process(this, id, subgroups);
    }
    
    public async Task StartNewScheduleRequest(ChatId id)
    {
        var request = new RequestSubgroup();
        _container.Inject(request);
        
        await request.Process(this, id);
    }
    
    public async Task StartSettingsRequest(ChatId id, long userId)
    {
        var request = new SettingsState();
        _container.Inject(request);
        
        await request.Start(this, id, userId);
    }
}