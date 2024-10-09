using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using JetBrains.Annotations;

namespace FefuScheduleBot.Services;

[PublicAPI]
[Service]
public class NotificationService : IStartable
{
    [Dependency] private readonly DiscordBotService _discordBotService = default!;
    [Dependency] private readonly FefuService _fefuService = default!;
    [Dependency] private readonly MongoService _mongoService = default!;

    private readonly Logger _logger = default!;
    private readonly Dictionary<string, NotificationChatData> _scheduledChats = [];
    private bool _isStartingSending;
    
    public void ScheduleSending(GuildSchema guild, string chatId, SchedulingDay day = SchedulingDay.Next)
    {
        _scheduledChats.Add(chatId, guild.Chats[chatId]);
        _ = StartSending(day);
    }

    private async Task SendCalendar(string chatId, NotificationChatData chatData, Calendar calendar)
    {
        var table = calendar.UseSubgroup(chatData.Subgroup).ConvertInTable();
        await _discordBotService.SendMessage(chatId, table.ToString());
    }

    private async Task StartSending(SchedulingDay day)
    {
        if (_isStartingSending) return;
        _isStartingSending = true;

        var calendar = await _fefuService.GetSchedule(day);
        
        foreach (var (chatId, chatData) in _scheduledChats)
        {
            _ = SendCalendar(chatId, chatData, calendar);
        }
        
        _scheduledChats.Clear();
        _isStartingSending = false;
    }

    public void ScheduleGlobalSending(GuildSchema guildSchema, SchedulingDay day = SchedulingDay.Next)
    {
        foreach (var (chatId, _) in guildSchema.Chats)
        {
            ScheduleSending(guildSchema, chatId, day);
        }
    }
    
    public void UpdateGlobalUpdateTime()
    {
        var time = _fefuService.GetLocalTime();
        
        foreach (var guildWrapper in _mongoService.GetData<GuildSchema>())
        {
            if (time < guildWrapper.Data.NextUpdate) continue;
            
            ScheduleGlobalSending(guildWrapper.Data);

            var nextTime = Utility.GetNextUpdateDateTime();
            guildWrapper.Mutate(draft =>
            {
                draft.NextUpdate = nextTime;
            });
            
            _logger.Info($"Server {guildWrapper.Data.Id} has been update, next time: {nextTime.ToStringWithCulture()}");
        }
        
    }

    public async Task Start()
    {
        await _discordBotService.WaitForReady();
        
        while (true)
        {
            try
            {
                UpdateGlobalUpdateTime();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            await Task.Delay(new TimeSpan(0, 0, 1, 0));
        }
    }
}