using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.Commands;

[Command("start", "Запустить бота")]
public class StartCommand : ICommand
{
    public async Task Execute(Message message, DependenciesContainer container)
    {
        var telegramBotService = container.Resolve<TelegramBotService>();
        await telegramBotService.Client.SendMessage(
            message.Chat.Id, 
            "Привет! 👋 Я бот для удобного просмотра расписания.\n\n" +
            "📌 Как пользоваться:\n" +
            "• /my_schedule — получить расписание для вашей подгруппы (из настроек профиля)\n" +
            "• /schedule_subgroup — получить расписание для конкретной подгруппы\n" +
            "• /settings — настроить свои подгруппы по умолчанию\n\n" +
            "Начните с настройки через /settings, чтобы получать расписание в один клик!"
        );
    }
}