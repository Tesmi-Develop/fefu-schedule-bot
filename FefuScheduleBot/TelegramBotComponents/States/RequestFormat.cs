using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

public enum ScheduleFormat
{
    Xlsx,
    Jpeg,
}

[State]
public class RequestFormat : IChainState
{
    [Dependency] private readonly TelegramBot _bot = null!;

    private InlineKeyboardMarkup GenerateButtons(ScheduleGenerator generator, string data)
    {
        var markup = new InlineKeyboardMarkup();
        var nextStateData = generator.GenerateTransferStateData<SendSchedule>(data);
        
        markup.AddButton("xlsx",$"{nextStateData}&Format={ScheduleFormat.Xlsx}");
        markup.AddButton("jpeg",$"{nextStateData}&Format={ScheduleFormat.Jpeg}");
        
        return markup;
    }
    public async Task Process(ScheduleGenerator generator, CallbackQuery callbackQuery, string data)
    {
        await _bot.Client.EditMessageText(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "В каком формате вы хотите расписание?",
            replyMarkup: GenerateButtons(generator, data));
    }
}