namespace FefuScheduleBot.ServiceRealisation;

public interface IStartable
{ 
    Task Start(CancellationToken token);
}