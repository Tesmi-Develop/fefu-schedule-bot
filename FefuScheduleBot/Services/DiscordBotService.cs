using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FefuScheduleBot.Environments;
using FefuScheduleBot.Modals;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace FefuScheduleBot.Services;

[PublicAPI]
[Service]
public sealed class DiscordBotService : IStartable, IInitializable
{
    public event Action? Connected;
    public bool IsConnected => _client.ConnectionState == ConnectionState.Connected;
    private readonly DiscordSocketClient _client;

    [Dependency] private readonly EnvironmentData _environmentData = default!;
    private readonly Logger _logger = default!;
    private InteractionService _commands = default!;
    private DependencyContainerWrapper _dependencyWrapper = default!;
    private DependenciesContainer _dependenciesContainer = default!;
    private Dictionary<string, Type> _modals = new();

    public DiscordBotService()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions |
                             GatewayIntents.GuildBans |
                             GatewayIntents.GuildEmojis,
            AlwaysDownloadUsers = true
        };
        
        _client = new DiscordSocketClient(config);
    }

    public async Task WaitForReady()
    {
        if (IsConnected) return;

        var task = new TaskCompletionSource();
        var connection = () =>
        {
            task.TrySetResult();
        };

        Connected += connection;
        await task.Task;
        Connected -= connection;
    }
    private SocketGuildUser GetUserInGuild(string userId, string guildId)
    {
        var guild = _client.GetGuild(ulong.Parse(guildId));
        if (guild is null)
            throw new ArgumentException("User not found");

        var guildUser = guild.GetUser(ulong.Parse(userId));
        if (guildUser is null)
            throw new ArgumentException("User not found");

        return guildUser;
    }

    public async Task<IUserMessage> SendMessage(string chatId, string message)
    {
        if (_client.GetChannel(ulong.Parse(chatId)) is not IMessageChannel channel)
        {
            throw new ArgumentException($"Not found channel by id {chatId}");
        }
        
        return await channel.SendMessageAsync(text: message);
    }
    
    public async Task<IUserMessage> SendFile(string chatId, string message)
    {
        if (_client.GetChannel(ulong.Parse(chatId)) is not IMessageChannel channel)
        {
            throw new ArgumentException($"Not found channel by id {chatId}");
        }
        
        return await channel.SendMessageAsync(text: message);
    }
    
    public async Task AwardRole(string userId, string guildId, string roleId)
    {
        var guildUser = GetUserInGuild(userId, guildId);
        await guildUser.AddRoleAsync(ulong.Parse(roleId));
    }
    
    public bool HaveRole(string userId, string guildId, string roleId)
    {
        var guildUser = GetUserInGuild(userId, guildId);
        var roleIdUlong = ulong.Parse(roleId);
        
        return guildUser.Roles.FirstOrDefault(role => role.Id == roleIdUlong) is not null;
    }
    
    private void InitDependency()
    {
        _dependenciesContainer = DependencyManager.Create();
        _dependencyWrapper = new DependencyContainerWrapper(_dependenciesContainer);
        
        DependencyManager.Register<IServiceScopeFactory>(_ => new CustomServiceScopeFactory(_dependencyWrapper));
        DependencyManager.Register(_client);
        DependencyManager.Register(x => new InteractionService(x.Resolve<DiscordSocketClient>()));
    }

    private void ConnectToEvents()
    {
        _client.Log += message =>
        {
            _logger.Debug(message.ToString());
            return Task.CompletedTask;
        };
        
        _client.Ready += async () =>
        {
            _commands = new InteractionService(_client);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _dependencyWrapper);
            await RegisterCommands();
            
            _client.InteractionCreated += HandleInteraction;
            Connected?.Invoke();
        };

        _client.ModalSubmitted += async modal =>
        {
            var id = modal.Data.CustomId;
            var haveModal = _modals.TryGetValue(id, out var type);
            if (!haveModal)
            {
                _logger.Warning($"Sent modal with unknown id {id}");
                return;
            }

            var constructors = type!.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var constructorInfo = constructors.Length == 1 ? constructors[0] : throw new InvalidOperationException();
            var instance = (BaseModal)constructorInfo.Invoke([]);
            _dependenciesContainer.Inject(instance);

            await instance.Process(modal);
        };

        _client.ButtonExecuted += async component =>
        {
            // Temporary solution, there will be an abstraction in the future
            
            if (component.Data.CustomId == "requestSchedule")
            {
                await component.RespondWithModalAsync(new RequestSchedule().BuildForm());
            }
        };
    }
    
    private async Task RegisterCommands()
    {
        if (Program.IsDebug)
        {
            var guildId = _environmentData.DiscordDevelopmentGuildId;
                
            await _commands.RegisterCommandsToGuildAsync(ulong.Parse(guildId));
            _logger.Debug($"Commands registered to {guildId}");
            return;
        }
        
        await _commands.RegisterCommandsGloballyAsync();
    }
    
    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _dependencyWrapper);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
           
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
    
    public async Task Start()
    {
        InitDependency();
        ConnectToEvents();
        
        await _client.LoginAsync(TokenType.Bot, _environmentData.DiscordToken);
        await _client.StartAsync();
        
        _logger.Debug("Bot created");
    }

    public void Init()
    {
        foreach (var (type, _) in ReflectionHelper.GetAllTypes<ModalAttribute>())
        {
            var targetType = typeof(BaseModal);
            
            if (!type.IsAssignableTo(targetType))
            {
                _logger.Warning($"Found a {type.Name} that does not inherit a class {targetType.Name}");
                continue;
            }
            
            _modals[type.FullName ?? string.Empty] = type;
        }
    }
}