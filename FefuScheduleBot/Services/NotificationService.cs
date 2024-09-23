using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.ServiceRealisation;
using Hypercube.Dependencies;

namespace FefuScheduleBot.Services;

[Service]
public class NotificationService
{
    [Dependency] private readonly BotService _botService = default!;
    [Dependency] private readonly FefuService _fefuService = default!;
    
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
}