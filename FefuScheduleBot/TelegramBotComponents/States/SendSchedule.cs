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

    private async Task StartSending(WeekType weekType, int subgroup, ScheduleFormat format, CallbackQuery callbackQuery)
    {
        var table = format switch
        {
            ScheduleFormat.Xlsx => await _excelService.GenerateSchedule(weekType, subgroup),
            ScheduleFormat.Jpeg => await _excelService.GenerateScheduleImage(weekType, subgroup),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        await using Stream stream = table.OpenRead();
        
        _ = format switch
        {
            ScheduleFormat.Jpeg => await _bot.Client.SendPhoto(callbackQuery.Message!.Chat,
                InputFile.FromStream(stream, table.Name)),
            _ => await _bot.Client.SendDocument(callbackQuery.Message!.Chat, InputFile.FromStream(stream, table.Name)),
        };
        
        File.Delete(table.FullName);
    }
    
    public async Task Process(ScheduleGenerator generator, CallbackQuery callbackQuery, string data)
    {
        var parsedData = Utility.ParseQueryParams(data);
        var format = (ScheduleFormat)Enum.Parse(typeof(ScheduleFormat), parsedData["Format"]!);
        var weekType = (WeekType)Enum.Parse(typeof(WeekType), parsedData["WeekType"]!);
        var subgroup = int.Parse(parsedData["Subgroup"]!);
        
        await _bot.Client.EditMessageText(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "Расписание будет отправлено в ближайшее время"
            );
        await StartSending(weekType, subgroup, format, callbackQuery);
    }
}