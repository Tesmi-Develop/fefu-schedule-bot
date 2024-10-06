using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents;

public interface IStartState
{
    Task Process(ScheduleGenerator generator, ChatId message);
}