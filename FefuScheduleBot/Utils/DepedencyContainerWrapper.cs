using Hypercube.Dependencies;

namespace FefuScheduleBot.Utils;

public class DependencyContainerWrapper(DependenciesContainer container) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return container.Resolve(serviceType);
    }
}