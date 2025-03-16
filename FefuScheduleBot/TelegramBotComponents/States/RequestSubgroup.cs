using FefuScheduleBot.Services;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

public class RequestSubgroup : IStartState
{
    [Dependency] private readonly FefuService _fefuService = default!;
    [Dependency] private readonly TelegramBot _bot = default!;
    
    public async Task Process(ScheduleGenerator generator, ChatId id)
    {
        var inlineMarkup = new InlineKeyboardMarkup();
        var buttonsInRow = _fefuService.MaxSubgroups / 2;
        var rows = _fefuService.MaxSubgroups / buttonsInRow;

        var buttonId = 1;
        for (var i = 1; i <= rows; i++)
        {
            var buttons = new List<InlineKeyboardButton>();
            for (var j = 1; j <= buttonsInRow; j++)
            {
                if (buttonId > _fefuService.MaxSubgroups) break;
                
                buttons.Add(InlineKeyboardButton.WithCallbackData(
                    buttonId.ToString(), 
                    generator.GenerateTransferStateData<RequestWeekType>($"Subgroup={buttonId.ToString()}"))
                );
                buttonId++;
            }

            inlineMarkup.AddNewRow(buttons.ToArray());
        }
        
        await _bot.Client.SendMessage(id, "Выберите подгруппу", replyMarkup: inlineMarkup);
    }
}