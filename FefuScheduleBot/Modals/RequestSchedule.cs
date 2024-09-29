using Discord;
using Discord.WebSocket;
using FefuScheduleBot.Services;
using Hypercube.Dependencies;

namespace FefuScheduleBot.Modals;

[Modal]
public class RequestSchedule : BaseModal
{
    [Dependency] private readonly ExcelService _excelService = default!;
    protected override ModalBuilder GenerateForm()
    {
        return new ModalBuilder()
            .WithTitle("Генератор рассписаний")
            .WithCustomId(nameof(RequestSchedule))
            .AddTextInput("Ваша подгруппа", "subgroup", placeholder: "Цифра от 1 до 10")
            .AddTextInput("На какую неделю вы хотите расписание?", "weekType", 
                placeholder: "'Т' - Текущая неделя, 'С' - Следующая");
    }

    public override async Task Process(SocketModal modal)
    {
        var components = modal.Data.Components.ToList();
        var subgroupStr = components.First(x => x.CustomId == "subgroup").Value;
        var weekTypeStr = components.First(x => x.CustomId == "weekType").Value;
        
        try
        {
            var subgroup = int.Parse(subgroupStr);
            var weekType = weekTypeStr switch
            {
                "Т" => WeekType.Current,
                "Н" => WeekType.Next,
                _ => throw new ArgumentException()
            };

            async Task StartSending()
            {
                var user = modal.User;
                var table = await _excelService.GenerateSchedule(weekType, subgroup);
                
                await user.SendFileAsync(filePath: table.File.FullName);
                File.Delete(table.File.FullName);
            }

            _ = StartSending();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await modal.RespondAsync("Произошла ошибка, убедитесь, что данные были введены верно", ephemeral: true);
            return;
        }
        
        await modal.RespondAsync("Запрос создан, таблица будет отправлена вам в DM", ephemeral: true);
    }
}