using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

[State]
public class RequestWeekType : IChainState
{
    [Dependency] private readonly TelegramBot _bot = null!;

    private InlineKeyboardMarkup GenerateButtons(ScheduleGenerator generator, string data)
    {
        var markup = new InlineKeyboardMarkup();
        var nextStateData = generator.GenerateTransferStateData<RequestFormat>(data);
        
        markup.AddButton("Текущая неделя",$"{nextStateData}&WeekType={WeekType.Current}");
        markup.AddButton("Cледующая неделя",$"{nextStateData}&WeekType={WeekType.Next}");
        
        return markup;
    }
    public async Task Process(ScheduleGenerator generator, CallbackQuery callbackQuery, string data)
    {
        await _bot.Client.EditMessageText(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "На какую неделю вы хотите расписание?",
            replyMarkup: GenerateButtons(generator, data));
    }
}