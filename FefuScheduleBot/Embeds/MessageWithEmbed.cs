using Discord;
using Discord.Rest;

namespace FefuScheduleBot.Embeds;

public class MessageWithEmbed
{
    public readonly EmbedBuilder Embed;

    private RestFollowupMessage? _message;

    public MessageWithEmbed(string? title = null, string description = "", RestFollowupMessage? message = null)
    {
        _message = message;
        Embed = new EmbedBuilder();
        Embed.WithColor(Color.Purple)
            .WithDescription(description);

        if (title is not null)
            Embed.WithTitle(title);
    }
    
    public void BindMessage(RestFollowupMessage message)
    {
        _message = message;
    }
    
    public Task SetDescription(string description)
    {
        Embed.WithDescription(description);
        return UpdateMessage();
    }

    private async Task UpdateMessage()
    {
        if (_message is null) return;

        await _message.ModifyAsync(props =>
        {
            props.Embed = Embed.Build();
        });
    }
}