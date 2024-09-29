using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using Hypercube.Shared.Utilities.Extensions;

namespace FefuScheduleBot.ServiceRealisation;

public static class ServiceManager
{
    private static bool _isCreated;
    private static readonly List<object> Services = [];
    private static readonly Logger Logger = LoggingManager.GetLogger(nameof(ServiceManager));

    public static void InitAll()
    {
        foreach (var service in Services)
        {
            var type = service.GetType();
            if (!type.IsAssignableTo(typeof(IInitializable))) 
                continue;
            
            ((IInitializable) service).Init();
        }
    }

    public static void StartAll()
    {
        foreach (var service in Services)
        {
            var type = service.GetType();
            if (!type.IsAssignableTo(typeof(IStartable))) 
                continue;

            async Task Start()
            {
                try
                {
                    await ((IStartable)service).Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            _ = Start();
        }
    }

    private static void InjectLogger(object @object)
    {
        var type = @object.GetType();

        foreach (var field in  type.GetAllFields())
        {
            var fieldType = field.FieldType;
            if (fieldType.IsAssignableTo(typeof(ILogger)))
            {
                field.SetValue(@object, LoggingManager.GetLogger(type.Name));
            }
        }
    }
    
    public static void CreateAll()
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

                DependencyManager.Register(type, type);
                types.Add(type);
            }
        }
        
        DependencyManager.InstantiateAll();

        foreach (var ctor in types)
        {
            var service = DependencyManager.Resolve(ctor);
            Services.Add(service);

            InjectLogger(service);
            Logger.Debug($"Instantiated {ctor.Name}");
        }
    }
}