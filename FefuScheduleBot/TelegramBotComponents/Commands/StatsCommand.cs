using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("stats", "Посмотреть статистику")]
public class StatsCommand : ICommand
{
    public async Task Execute(Message message, DependenciesContainer container)
    {
        var telegramBotService = container.Resolve<TelegramBotService>();
        var statsService = container.Resolve<StatsService>();
        
        var statistics = statsService.CollectInfo();
        await telegramBotService.Client.SendMessage(message.Chat.Id, 
            "\ud83d\udcc8 Статистика запросов \ud83d\udcc8\n\n" +
            $"Всего: {statistics.TotalUsage}\n" +
            $"Неделя: {statistics.WeekUsage}\n" +
            $"Сегодня: {statistics.TodayUsage}"
        );
    }
}