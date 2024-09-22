using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace FefuScheduleBot.Services;

[Service]
public sealed class BotService : IStartable
{
    public event Action? Connected;
    public bool IsConnected => Client.ConnectionState == ConnectionState.Connected;
    public readonly DiscordSocketClient Client;

    [Dependency] private readonly EnvironmentData _environmentData = default!;
    private readonly Logger _logger = default!;
    private InteractionService _commands = default!;
    private DependencyContainerWrapper _dependencyWrapper = default!;

    public BotService()
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
        
        Client = new DiscordSocketClient(config);
    }
    
    public async Task Start()
    {
        InitDependency();
        ConnectToEvents();
        
        await Client.LoginAsync(TokenType.Bot, _environmentData.DiscordToken);
        await Client.StartAsync();
        
        _logger.Debug("Bot created");
    }

    private SocketGuildUser GetUserInGuild(string userId, string guildId)
    {
        var guild = Client.GetGuild(ulong.Parse(guildId));
        if (guild is null)
            throw new ArgumentException("User not found");

        var guildUser = guild.GetUser(ulong.Parse(userId));
        if (guildUser is null)
            throw new ArgumentException("User not found");

        return guildUser;
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
        _dependencyWrapper = new DependencyContainerWrapper(DependencyManager.Create());
        
        DependencyManager.Register<IServiceScopeFactory>(_ => new CustomServiceScopeFactory(_dependencyWrapper));
        DependencyManager.Register(Client);
        DependencyManager.Register(x => new InteractionService(x.Resolve<DiscordSocketClient>()));
    }

    private void ConnectToEvents()
    {
        Client.Log += message =>
        {
            _logger.Debug(message.ToString());
            return Task.CompletedTask;
        };
        
        Client.Ready += async () =>
        {
            _commands = new InteractionService(Client);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _dependencyWrapper);
            await RegisterCommands();
            
            Client.InteractionCreated += HandleInteraction;
            Connected?.Invoke();
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
            var ctx = new SocketInteractionContext(Client, arg);
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
}