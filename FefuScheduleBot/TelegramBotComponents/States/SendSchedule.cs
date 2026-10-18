using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FefuScheduleBot.TelegramBotComponents.States;

[State]
public class SendSchedule : IChainState
{
    [Dependency] private readonly TelegramBotService _botService = null!;
    [Dependency] private readonly ExcelService _excelService = null!;
    [Dependency] private readonly FefuService _fefuService = null!;
    [Dependency] private readonly ImageService _imageService = null!;
    [Dependency] private readonly CacheService _cacheService = null!;
    [Dependency] private readonly Config _config = null!;
    [Dependency] private readonly Logger _logger = null!;

    private async Task<bool> TrySendScheduleFromCache(Week week, string[] subgroups, Schedule schedule, CallbackQuery callbackQuery)
    {
        var scheduleCache = await _cacheService.GetScheduleCache(week, subgroups, schedule);
        if (scheduleCache is null)
            return false;
        
        _logger.Debug($"Sending schedule from cache for week: {week}");
        await _botService.Client.SendPhoto(callbackQuery.Message!.Chat, new InputFileId(scheduleCache.FileId));
        
        return true;
    }

    private async Task SendNewSchedule(Week week, Schedule schedule, string[] subgroups, CallbackQuery callbackQuery)
    {
        var fileName = $"Расписание {_fefuService.LocalTime.ToStringWithCulture("d")}.xlsx";
        
        using var streamTable = _excelService.GenerateStreamTable(schedule);
        using var resultStream = new MemoryStream();
        streamTable.Position = 0;

        var tableImage = _imageService.GenerateStreamFromTable(streamTable);
        tableImage = _imageService.ApplyBackground(tableImage, _fefuService.LocalTime);
        await tableImage.CopyToAsync(resultStream);

        resultStream.Position = 0;
        var message = await _botService.Client.SendPhoto(callbackQuery.Message!.Chat, InputFile.FromStream(resultStream, fileName));
        var fileId = message.Photo?.LastOrDefault()?.FileId;

        if (fileId is not null)
            await _cacheService.SafeSaveSchedule(week, subgroups, schedule, fileId);
        
        _logger.Info("Sent new schedule");
    }
    
    private async Task StartSending(WeekType weekType, string[] subgroups, CallbackQuery callbackQuery)
    {
        var week = _fefuService.GetStudyWeek(weekType);
        var schedule = await _fefuService.GetSchedule(weekType);
        schedule = _fefuService.FilterBySubgroups(schedule, subgroups);
        
        if (await TrySendScheduleFromCache(week, subgroups, schedule, callbackQuery))
            return;

        _logger.Debug($"Cache miss for week {week}. Generating and sending a new schedule");
        await SendNewSchedule(week, schedule, subgroups, callbackQuery);
    }
    
    public async Task Process(TelegramBotService generator, CallbackQuery callbackQuery, string data)
    {
        var parsedData = Utility.ParseQueryParams(data);
        var weekType = (WeekType)Enum.Parse(typeof(WeekType), parsedData["WeekType"]!);
        var subgroups = Utility.DecodeSubgroups(parsedData["Subgroups"] ?? string.Empty, _config.Subgroups);
        
        await _botService.Client.EditMessageText(
            callbackQuery.Message!.Chat,
            callbackQuery.Message.MessageId,
            "Расписание будет отправлено в ближайшее время"
            );

        try
        {
            await StartSending(weekType, subgroups, callbackQuery);
        }
        catch (Exception e)
        {
            _logger.Error(e.ToString());
            await _botService.SendSafeMessage(callbackQuery.Message!.Chat,
                "Не удалось получить расписание. Попробуйте позже");
        }
    }
}