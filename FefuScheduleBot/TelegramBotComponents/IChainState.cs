using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents;

public interface IChainState
{
    Task Process(ScheduleGenerator generator, CallbackQuery callbackQuery, string data);
}