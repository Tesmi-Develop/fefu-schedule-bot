using Discord;
using Discord.WebSocket;

namespace FefuScheduleBot.Modals;

public abstract class BaseModal
{
    public string Id => GetType().FullName ?? string.Empty;
    
    public Modal BuildForm()
    {
        var form = GenerateForm();
        return form.WithCustomId(Id).Build();
    }
    
    public abstract Task Process(SocketModal modal);
    protected abstract ModalBuilder GenerateForm();
}