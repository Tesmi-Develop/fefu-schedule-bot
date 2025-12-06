using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
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
    [Dependency] private readonly FefuService _fefuService = default!;
    [Dependency] private readonly ImageService _imageService = default!;

    private async Task StartSending(WeekType weekType, int subgroup, ScheduleFormat format, CallbackQuery callbackQuery)
    {
        var fileName = $"Расписание {_fefuService.GetLocalTime().ToStringWithCulture("d")}.xlsx";
        var schedule = await _fefuService.GetSchedule(weekType);
        schedule = _fefuService.FilterBySubgroup(schedule, subgroup);

        using var streamTable = _excelService.GenerateStreamTable(schedule);
        using var resultStream = new MemoryStream();
        streamTable.Position = 0;

        switch (format)
        {
            case ScheduleFormat.Xlsx:
                await streamTable.CopyToAsync(resultStream);
                break;
            case ScheduleFormat.Jpeg:
                var tableImage = _imageService.GenerateStreamFromTable(streamTable);
                tableImage = _imageService.ApplyBackground(tableImage);
                await tableImage.CopyToAsync(resultStream);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        resultStream.Position = 0;
        _ = format switch
        {
            ScheduleFormat.Jpeg => await _bot.Client.SendPhoto(callbackQuery.Message!.Chat,
                InputFile.FromStream(resultStream, fileName)),
            _ => await _bot.Client.SendDocument(callbackQuery.Message!.Chat, InputFile.FromStream(resultStream, fileName)),
        };
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