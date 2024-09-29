using Discord;
using Discord.Interactions;
using Discord.Rest;
using FefuScheduleBot.Data;
using FefuScheduleBot.Embeds;
using FefuScheduleBot.Schemas;
using FefuScheduleBot.Services;
using JetBrains.Annotations;

namespace FefuScheduleBot.Commands;

[PublicAPI]
public class CreateScheduleForm : InteractionModuleBase
{
    
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("create_schedule_form", "creates an message with the form")]
    public async Task CreateScheduleFormHandle()
    {
        var embed = new EmbedBuilder().WithDescription(
            "Данная форма позволяет вам создать запрос на генерацию расписания для вашей подгруппы.\n" +
            "Нажмите на кнопку ниже и следуйте инструкции.").Build();

        var builder = new ComponentBuilder().WithButton("+", "requestSchedule").Build();
        await Context.Interaction.RespondAsync(embed: embed, components: builder);
    }
}