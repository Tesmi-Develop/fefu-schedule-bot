using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("schedule_subgroup", "Сгенерировать расписание")]
public class ScheduleSubgroupCommand : ICommand
{
    public async Task Execute(Message message, DependenciesContainer container)
    {
        var telegramBotService = container.Resolve<TelegramBotService>();
        await telegramBotService.StartNewScheduleRequest(message.Chat);
    }
}