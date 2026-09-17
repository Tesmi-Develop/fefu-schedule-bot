using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("start", "Запустить бота")]
public class StartCommand : ICommand
{
    public async Task Execute(Message message, DependenciesContainer container)
    {
        var generator = new ScheduleGenerator();
        container.Inject(generator);
        await generator.Start(message.Chat);
    }
}