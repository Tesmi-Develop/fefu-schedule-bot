using Discord.Interactions;

namespace FefuScheduleBot.Commands;

public class EchoCommand : InteractionModuleBase
{
    
    [SlashCommand("echo", "echoes whatever you say")]
    public async Task Echo(string input)
    {
        await RespondAsync(input, ephemeral: true);
    }
}