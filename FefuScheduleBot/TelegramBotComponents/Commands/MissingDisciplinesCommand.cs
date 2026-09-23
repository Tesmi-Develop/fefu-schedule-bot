using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("missingdisciplines", "Получить новые дисциплины")]
public class MissingDisciplinesCommand : ICommand
{
    public void Init(DependenciesContainer container)
    {
        // Do nothing
    }

    public void Start(DependenciesContainer container)
    {
        // Do nothing
    }

    public async Task Execute(Message message, DependenciesContainer container)
    {
        var fefuService = container.Resolve<FefuService>();
        var telegramBotService = container.Resolve<TelegramBotService>();
        var disciplines = await fefuService.GetDisciplinesMissingFromConfig();

        if (!disciplines.Any())
        {
            await telegramBotService.Client.SendMessage(message.Chat.Id, 
                "Новых дисциплин нет"
            );
            return;
        }
        
        await telegramBotService.Client.SendMessage(message.Chat.Id, 
            fefuService.FormatDisciplines(disciplines)
        );
    }
}