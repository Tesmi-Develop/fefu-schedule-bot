using FefuScheduleBot.Models;
using FefuScheduleBot.Services;
using Hypercube.Utilities.Dependencies;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("settings", "Настроить подгруппы", CommandPriority.Default)]
public class SettingsCommand : ICommand
{
    public Task Execute(Message message, DependenciesContainer container)
    {
        if (message.From is null)
            return Task.CompletedTask;
        
        var telegramBotService = container.Resolve<TelegramBotService>();
        return telegramBotService.StartSettingsRequest(message.Chat, message.From.Id);
    }
}