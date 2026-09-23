using FefuScheduleBot.Services;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents;

public interface IChainState
{
    Task Process(TelegramBotService generator, CallbackQuery callbackQuery, string data);
}