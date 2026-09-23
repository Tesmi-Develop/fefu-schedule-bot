using FefuScheduleBot.Data;
using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

[State]
public class RequestWeekType : IChainState
{
    [Dependency] private readonly TelegramBotService _botService = null!;
    [Dependency] private readonly Config _config = null!;

    private InlineKeyboardMarkup GenerateButtons(TelegramBotService generator, string data)
    {
        var markup = new InlineKeyboardMarkup();
        var nextStateData = generator.GenerateTransferStateData<SendSchedule>(data);
        
        markup.AddButton("Текущая неделя",$"{nextStateData}&WeekType={WeekType.Current}");
        markup.AddButton("Cледующая неделя",$"{nextStateData}&WeekType={WeekType.Next}");
        
        return markup;
    }
    public async Task Process(TelegramBotService generator, CallbackQuery callbackQuery, string data)
    {
        await _botService.Client.EditMessageText(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "На какую неделю вы хотите расписание?",
            replyMarkup: GenerateButtons(generator, data));
    }
    
    public async Task Process(TelegramBotService generator, ChatId id, string[] subgroups)
    {
        var data = $"Subgroups={Utility.EncodeSubgroups(subgroups, _config.Subgroups)}";
        var inlineMarkup = GenerateButtons(generator, data);
        await _botService.Client.SendMessage(id, "На какую неделю вы хотите расписание?", replyMarkup: inlineMarkup);
    }
}