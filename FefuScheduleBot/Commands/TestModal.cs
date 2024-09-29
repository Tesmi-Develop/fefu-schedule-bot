using Discord;
using Discord.Interactions;
using FefuScheduleBot.Modals;
using FefuScheduleBot.Services;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

[PublicAPI]
public class TestModal : InteractionModuleBase
{
    public MongoService MongoService { get; set; } = default!;
    
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("test_modal", "test modal")]
    public async Task TestModalHandle()
    {
        await Context.Interaction.RespondWithModalAsync(new RequestSchedule().BuildForm());
    }
}