using FefuScheduleBot.Services;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents;

public interface IScheduleGeneratorState
{
    Task Process(TelegramBotService generator, ChatId message);
}