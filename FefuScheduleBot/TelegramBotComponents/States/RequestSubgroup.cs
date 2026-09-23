using FefuScheduleBot.Data;
using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

public class RequestSubgroup : IScheduleGeneratorState
{
    [Dependency] private readonly TelegramBotService _botService = null!;
    [Dependency] private readonly Config _config = null!;
    
    public async Task Process(TelegramBotService generator, ChatId id)
    {
        var inlineMarkup = new InlineKeyboardMarkup();

        var buttonsInRow = Math.Max(1, _config.MaxButtonsInRow);
        var subgroups = _config.Subgroups;

        for (var i = 0; i < subgroups.Length; i += buttonsInRow)
        {
            var rowButtons = subgroups
                .Skip(i)
                .Take(buttonsInRow)
                .Select((subgroup, index) =>
                {
                    var buttonName = _config.Subgroups[i + index];
                    var callbackData = generator.GenerateTransferStateData<RequestWeekType>(
                        $"Subgroups={Utility.EncodeSubgroups([buttonName], _config.Subgroups)}"
                        );

                    return InlineKeyboardButton.WithCallbackData(buttonName, callbackData);
                })
                .ToArray();

            inlineMarkup.AddNewRow(rowButtons);
        }
        
        await _botService.Client.SendMessage(id, "Выберите подгруппу", replyMarkup: inlineMarkup);
    }
}