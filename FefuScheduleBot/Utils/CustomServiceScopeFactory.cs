using Microsoft.Extensions.DependencyInjection;

namespace FefuScheduleBot.Utils;

/// <summary>
/// Средство для наёба Microsoft хуйни
/// В кратце: Microsoft хуйня параша, Hypercube победа наша!
/// </summary>
public class CustomServiceScopeFactory : IServiceScopeFactory
{
    private DependencyContainerWrapper _wrapper;
    
    public CustomServiceScopeFactory(DependencyContainerWrapper wrapper)
    {
        _wrapper = wrapper;
    }
    
    public IServiceScope CreateScope()
    {
        return new CustomServiceScope(_wrapper);
    }
}

public class CustomServiceScope : IServiceScope
{
    public IServiceProvider ServiceProvider { get; }

    public CustomServiceScope(DependencyContainerWrapper wrapper)
    {
        ServiceProvider = wrapper;
    }
    
    public void Dispose()
    {
      
    }
}