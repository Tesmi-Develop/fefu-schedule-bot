using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Data;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

[PublicAPI]
public class RemoveChat : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    
    [SlashCommand("remove_chat", "Deletes chat data")]
    public async Task RemoveChatHandle()
    {
        await DeferAsync(ephemeral: true);
        
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage)await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        embed.BindMessage(message);
        
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        
        guildData.Mutate((draft) =>
        {
            draft.Chats.Remove(Context.Channel.Id.ToString());
        });
        
        await embed.SetDescription("Done!");
    }
}