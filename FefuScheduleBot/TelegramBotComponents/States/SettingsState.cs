using FefuScheduleBot.Data;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FefuScheduleBot.TelegramBotComponents.States;

[State]
public class SettingsState : IChainState
{
    [Dependency] private readonly TelegramBotService _botService = null!;
    [Dependency] private readonly MongoService _mongoService = null!;
    [Dependency] private readonly Config _config = null!;

    private InlineKeyboardMarkup GenerateButtons(TelegramBotService generator, Dictionary<string, bool> statuses)
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
                    var status = statuses.GetValueOrDefault(buttonName, false);
                    var callbackData = generator.GenerateTransferStateData<SettingsState>($"Id={buttonName}&Status={status}");
                    
                    return InlineKeyboardButton.WithCallbackData($"{(status ? "✅" : "❌")} {buttonName}", callbackData);
                })
                .ToArray();

            inlineMarkup.AddNewRow(rowButtons);
        }
        
        return inlineMarkup;
    }

    private Dictionary<string, bool> GenerateStatuses(string[] subgroups)
    {
        var result = new Dictionary<string, bool>();
        
        foreach (var subgroupName in _config.Subgroups)
            result.Add(subgroupName, subgroups.Contains(subgroupName));

        return result;
    }
    
    public async Task Process(TelegramBotService generator, CallbackQuery callbackQuery, string data)
    {
        var userId = callbackQuery.From.Id;
        var parsedData = Utility.ParseQueryParams(data);
    
        var subgroupName = parsedData["Id"] ?? string.Empty;
        var rawStatus = parsedData["Status"];

        if (!bool.TryParse(rawStatus, out var currentStatus))
            currentStatus = true;

        var status = !currentStatus;
        var userData = _mongoService.GetData<UserSetting>(userId.ToString());
        userData.Mutate((currentData) =>
        {
            switch (status)
            {
                case true when !currentData.Subgroups.Contains(subgroupName):
                    currentData.Subgroups.Add(subgroupName);
                    break;
                case false when currentData.Subgroups.Contains(subgroupName):
                    currentData.Subgroups.Remove(subgroupName);
                    break;
            }
        });

        var buttons = GenerateButtons(generator, GenerateStatuses(userData.Data.Subgroups.ToArray()));
        var chatId = callbackQuery.Message?.Chat.Id;
        var messageId = callbackQuery.Message?.MessageId;

        if (chatId is null || messageId is null)
            return;

        await _botService.Client.EditMessageText(
            chatId: chatId,
            messageId: messageId.Value,
            text: "Выберите подгруппу",
            replyMarkup: buttons
        );
    }

    public Task Start(TelegramBotService generator, ChatId chatId, long userId)
    {
        var data = _mongoService.GetData<UserSetting>(userId.ToString()).Data;
        var buttons = GenerateButtons(generator, GenerateStatuses(data.Subgroups.ToArray()));
        
        return _botService.Client.SendMessage(chatId, "Выберите подгруппы", replyMarkup: buttons);
    }
}