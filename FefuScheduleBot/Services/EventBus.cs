using FefuScheduleBot.ServiceRealisation;

namespace FefuScheduleBot.Services;

[Service]
public class EventBus
{
    public event Action<Exception> ScheduledSendException = null!;

    public void OnScheduledSendException(Exception exception)
    {
        ScheduledSendException?.Invoke(exception);
    }
}