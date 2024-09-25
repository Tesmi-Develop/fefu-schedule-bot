using Discord.Rest;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;

namespace FefuScheduleBot.Services;

[Service]
public class NotificationService : IStartable
{
    [Dependency] private readonly BotService _botService = default!;
    [Dependency] private readonly FefuService _fefuService = default!;
    [Dependency] private readonly MongoService _mongoService = default!;

    private readonly Logger _logger = default!;
    private readonly Dictionary<string, NotificationChatData> _scheduledChats = [];
    private bool _isStartingSending;
    
    public void ScheduleSending(GuildSchema guild, string chatId)
    {
        _scheduledChats.Add(chatId, guild.Chats[chatId]);
        _ = StartSending();
    }

    private async Task SendCalendar(string chatId, NotificationChatData chatData, Calendar calendar)
    {
        var table = calendar.UseSubgroup(chatData.Subgroup).ConvertInTable();
        await _botService.SendMessage(chatId, table.ToString());
    }

    private async Task StartSending()
    {
        if (_isStartingSending) return;
        _isStartingSending = true;

        var calendar = await _fefuService.GetTomorrowSchedule();
        
        foreach (var (chatId, chatData) in _scheduledChats)
        {
            _ = SendCalendar(chatId, chatData, calendar);
        }
        
        _scheduledChats.Clear();
        _isStartingSending = false;
    }

    public void ScheduleGlobalSending(GuildSchema guildSchema)
    {
        foreach (var (chatId, _) in guildSchema.Chats)
        {
            ScheduleSending(guildSchema, chatId);
        }
    }
    
    public void ScheduleGlobalSending()
    {
        foreach (var guildWrapper in _mongoService.GetData<GuildSchema>())
        {
            ScheduleGlobalSending(guildWrapper.Data);
        }
    }

    public async Task Start()
    {
        await _botService.WaitForReady();
        var target = _fefuService.GetLocalTime().AddDays(1).Date.AddHours(20);
        _logger.Info($"Scheduled for a schedule update on {target.ToStringWithCulture("t")}");
        
        while (true)
        {
            if (target <= _fefuService.GetLocalTime())
            {
                ScheduleGlobalSending();
                target = _fefuService.GetLocalTime().AddDays(1).Date.AddHours(20);
                _logger.Info($"There has been a schedule update, next time: {target.ToStringWithCulture("t")}");
            }
            
            await Task.Delay(10000);
        }
    }
}