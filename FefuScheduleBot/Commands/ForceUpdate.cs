using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

public enum UpdateContext
{
    CurrentChat,
    Global
}

[PublicAPI]
public class ForceUpdate : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    public NotificationService NotificationService { get; set; } = default!;
    
    [SlashCommand("force_update", "Forcibly calls the current schedule")]
    public async Task ForceUpdateHandle(UpdateContext context)
    {
        await DeferAsync(ephemeral: true);
        
        var embed = new MessageWithEmbed(description: "Processing...");
        var message = (RestFollowupMessage)await FollowupAsync(ephemeral: true, embed: embed.Embed.Build());
        embed.BindMessage(message);

        var guildData = MongoService.GetData<GuildSchema>(Context.Guild.Id.ToString());

        switch (context)
        {
            case UpdateContext.CurrentChat:
            {
                var chatId = Context.Channel.Id.ToString();
                var isSuccess = guildData.Data.Chats.ContainsKey(chatId);

                if (!isSuccess)
                {
                    await embed.SetDescription("This chat is not registered.");
                    return;
                }

                NotificationService.ScheduleSending(guildData.Data, chatId);
                break;
            }
            case UpdateContext.Global:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, null);
        }

        await embed.SetDescription("Done!");
    }
}