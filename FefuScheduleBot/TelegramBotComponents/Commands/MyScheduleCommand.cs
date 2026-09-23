using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("my_schedule", "Сгенерировать моё расписание")]
public class MyScheduleCommand : ICommand
{
    public Task Execute(Message message, DependenciesContainer container)
    {
        var telegramBotService = container.Resolve<TelegramBotService>();
        var mongo = container.Resolve<MongoService>();

        if (message.From is null)
            return Task.CompletedTask;
        
        var userId = message.From.Id;
        var userSetting = mongo.GetData<UserSetting>(userId.ToString());

        if (userSetting.Data.Subgroups.Count == 0)
        {
            telegramBotService.Client.SendMessage(message.Chat, "Вы не выбрали подгруппы. Воспользуйтесь командой /settings");
            return Task.CompletedTask;
        }

        return telegramBotService.StartNewScheduleRequest(message.Chat, userSetting.Data.Subgroups.ToArray());
    }
}