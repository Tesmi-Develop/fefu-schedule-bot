using FefuScheduleBot.Services;
using FefuScheduleBot.TelegramBotComponents.States;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FefuScheduleBot.TelegramBotComponents;

public class ScheduleGenerator : IPostInject
{
    [Dependency] private readonly TelegramBot _bot = null!;
    [Dependency] private readonly DependenciesContainer _container = null!;
    private readonly Logger _logger = null!;
    
    private readonly Dictionary<string, Type> _allState = new();
    
    public void PostInject()
    {
        _bot.Client.OnUpdate += update =>
        {
            if (update.Type != UpdateType.CallbackQuery || update.CallbackQuery is null) return Task.CompletedTask;
            
            ProcessCallbackQuery(update.CallbackQuery);
            return Task.CompletedTask;
        };
        
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
        if (callbackQuery.Data is null || callbackQuery.Message is null) return;

        var data = Utility.ParseQueryParams(callbackQuery.Data);
        var nextState = data["State"];
        data.Remove("State");
        
        if (nextState is null) return;
        var state = _allState[nextState];
        
        TransferToNextState(state, callbackQuery, Utility.ConvertQueryParams(data));
    }

    private void TransferToNextState(Type stateType, CallbackQuery callbackQuery, string data)
    {
        var state = (IChainState)stateType.GetConstructors()[0].Invoke([]);
        _container.Inject(state);

        async Task Callback()
        {
            try
            {
                await state.Process(this, callbackQuery, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        _ = Callback();
    }

    public string GenerateTransferStateData<T>(string additionalData = "")
    {
        return $"State={typeof(T).Name}&{additionalData}";
    }
    
    public async Task Start(ChatId id)
    {
        var request = new RequestSubgroup();
        _container.Inject(request);
        
        await request.Process(this, id);
    }
}