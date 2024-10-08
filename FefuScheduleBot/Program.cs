using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;

namespace FefuScheduleBot;

public static class Program
{
    public static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    public static DependenciesContainer DependenciesContainer = default!;
    
    private static bool _running;
    
    public static Task Main()
    {
        var solutionPath = Path.GetFullPath(@"..\..\..\");
        var filePath = Path.Combine(solutionPath, ".env");

        if (!File.Exists(filePath))
            filePath = ".env";

        var container = TryLoadEnv(filePath);
        if (container is null)
            return Task.CompletedTask;
        
        DependencyManager.InitThread();
        DependenciesContainer = DependencyManager.GetContainer();
        
        DependencyManager.Register(DependencyManager.GetContainer());
        DependencyManager.Register(container);
        
        ServiceManager.CreateAll();
        ServiceManager.InitAll();
        ServiceManager.StartAll();

        _running = true;
        while (_running)
        {
            Thread.Sleep(10);
        }

        return Task.CompletedTask;
    }
    
    private static EnvironmentData? TryLoadEnv(string path = ".env")
    {
        var logger = LoggingManager.GetLogger("environment");
        var container = new EnvironmentContainer();
        
        if (container.TryLoad(path))
            return container.Data;
        
        logger.Error($"Failed to load environment file. Created template file: {path}");
        return null;
    }
    
    public static void Shutdown()
    {
        _running = false;
    }
}
