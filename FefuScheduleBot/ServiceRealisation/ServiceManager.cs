using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace FefuScheduleBot.ServiceRealisation;

public class ServiceManager
{
    [Dependency] private readonly ILogger _logger = null!;
    private bool _isCreated;
    private readonly List<object> _services = [];
    
    public void InitAll()
    {
        foreach (var service in _services)
        {
            if (service is not IInitializable initializable)
                continue;

            try
            {
                initializable.Init();
            }
            catch (Exception e)
            {
                _logger.Error($"Service initialization failed because service [{service.GetType().Name}] threw an exception: \n{e}");
                break;
            }
        }
    }

    public void StartAll(CancellationToken token)
    {
        foreach (var service in _services)
        {
            if (service is not IStartable startable)
                continue;

            var serviceName = service.GetType().Name;

            try
            {
                startable.Start(token).ContinueWith(task =>
                {
                    if (!task.IsFaulted)
                        return;

                    _logger.Error($"Failed to start service [{serviceName}] due to an unhandled exception in task: \n{task.Exception}");
                }, token);
            }
            catch (Exception e)
            {
                _logger.Error($"Service startup failed because service [{serviceName}] threw an exception: \n{e}");
            }
        }
    }
    
    public void CreateAll(DependenciesContainer container)
    {
        if (_isCreated) 
            return;
        
        _isCreated = true;

        var types = new List<Type>();
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass) 
                    continue;
                
                if (!Attribute.IsDefined(type, typeof(ServiceAttribute))) 
                    continue;

                container.Register(type);
                types.Add(type);
            }
        }
        
        container.ResolveAll();

        foreach (var ctor in types)
        {
            var service = container.Resolve(ctor);
            _services.Add(service);
            _logger.Debug($"Service [{service.GetType().Name}] has been created.");
        }
    }
}