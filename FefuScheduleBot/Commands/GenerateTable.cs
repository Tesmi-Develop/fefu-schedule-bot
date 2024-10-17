using Discord;
using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Services;
// ReSharper disable MemberCanBePrivate.Global

namespace FefuScheduleBot.Commands;

public class GenerateTable : InteractionModuleBase
{
    public ExcelService ExcelService { get; set; } = default!;
    
    [SlashCommand("generate_table", "generates a schedule in the form of an excel spreadsheet")]
    public async Task GenerateTableHandle(WeekType weekType, int subgroup)
    {
        await DeferAsync(ephemeral: true);
        
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage)await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        embed.BindMessage(message);
        
        try
        {
            var user = Context.User;
            var table = await ExcelService.GenerateSchedule(weekType, subgroup);

            await user.SendFileAsync(filePath: table.FullName);
            File.Delete(table.FullName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await embed.SetDescription("Fail");
            return;
        }
        
        await embed.SetDescription("Done!");
    }
}