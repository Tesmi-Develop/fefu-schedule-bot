using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

[PublicAPI]
public class GetNextUpdateTime : InteractionModuleBase
{
    // ReSharper disable once MemberCanBePrivate.Global
    public MongoService MongoService { get; set; } = default!;
    public FefuService FefuService { get; set; } = default!;
    
    [SlashCommand("get_next_time", "returning next update time")]
    public async Task GetNextUpdateTimeHandle()
    {
        await DeferAsync(ephemeral: true);
        
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage)await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        embed.BindMessage(message);
        
        await embed.SetDescription($"Time {FefuService.ToLocalTime(guildData.Data.NextUpdate)}");
    }
}