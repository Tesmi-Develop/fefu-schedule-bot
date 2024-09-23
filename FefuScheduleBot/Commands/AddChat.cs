using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Data;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

[PublicAPI]
public class AddChat : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    
    [SlashCommand("add_chat", "Sets up a chat to follow up on the current schedule")]
    public async Task AddChatHandle(int subgroup)
    {
        await DeferAsync(ephemeral: true);
        
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage)await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        embed.BindMessage(message);
        
        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());
        
        guildData.Mutate((draft) =>
        {
            draft.Chats[Context.Channel.Id.ToString()] = new NotificationChatData()
            {
                Subgroup = subgroup
            };
        });
        
        await embed.SetDescription("Done!");
    }
}