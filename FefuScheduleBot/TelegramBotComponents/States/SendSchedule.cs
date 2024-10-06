using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using Hypercube.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace FefuScheduleBot.TelegramBotComponents.States;

[State]
public class SendSchedule : IChainState
{
    [Dependency] private readonly TelegramBot _bot = default!;
    [Dependency] private readonly ExcelService _excelService = default!;

    private async Task StartSending(WeekType weekType, int subgroup, CallbackQuery callbackQuery)
    {
        var table = await _excelService.GenerateSchedule(weekType, subgroup);

        await using Stream stream = table.File.OpenRead();
        await _bot.Client.SendDocumentAsync(callbackQuery.Message!.Chat, InputFile.FromStream(stream, table.File.Name));
        File.Delete(table.File.FullName);
    }
    
    public async Task Process(ScheduleGenerator generator, CallbackQuery callbackQuery, string data)
    {
        var parsedData = Utility.ParseQueryParams(data);
        var weekType = (WeekType)Enum.Parse(typeof(WeekType), parsedData["WeekType"]!);
        var subgroup = int.Parse(parsedData["Subgroup"]!);
        
        await _bot.Client.EditMessageTextAsync(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "Расписание будет отправлено в ближайшее время"
            );
        await StartSending(weekType, subgroup, callbackQuery);
    }
}