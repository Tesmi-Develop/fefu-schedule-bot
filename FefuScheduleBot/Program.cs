using System.Text.Json;
using FefuScheduleBot.Data;
using FefuScheduleBot.Environments;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace FefuScheduleBot;

public static class Program
{
    private static readonly DependenciesContainer Container = new();
    private const string ConfigName = "Config.json";
    private static string _loggerTimeZoneId = "UTC";

    private static Config LoadConfig()
    {
        using var reader = File.OpenText(ConfigName);
        var text = reader.ReadToEnd();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonColorConverter());

        var config = JsonSerializer.Deserialize<Config>(text, options);
        if (config is null)
            throw new Exception("Not found config");

        return config;
    }
    
    private static void PrepareLogger()
    {
        Container.Register<ILogger>((_, target) => CreateLogger(target), DependencyLifetime.Transient);
        Container.Register<Logger>((_, target) => (Logger)CreateLogger(target), DependencyLifetime.Transient);
    }

    private static ILogger CreateLogger(object? target = null)
    {
        return target is null ? new MyLogger(_loggerTimeZoneId) : new MyLogger(target, _loggerTimeZoneId);
    }
    
    public static Task Main()
    {
        var solutionPath = Path.GetFullPath(Path.Combine("..", "..", ".."));
        var filePath = Path.Combine(solutionPath, ".env");

        if (!File.Exists(filePath))
            filePath = ".env";

        var environmentData = TryLoadEnv(filePath);
        if (environmentData is null)
            return Task.CompletedTask;
        
        var config = LoadConfig();
        _loggerTimeZoneId = config.LoggerTimeZoneId;
        PrepareLogger();
        
        Container.Register<ServiceManager>();
        Container.RegisterSingleton<Config>(config);
        Container.RegisterSingleton<DependenciesContainer>(Container);
        Container.RegisterSingleton<EnvironmentData>(environmentData);
        
        var serviceManager = Container.Resolve<ServiceManager>();
        serviceManager.CreateAll(Container);
        serviceManager.InitAll();
        serviceManager.StartAll(CancellationToken.None);

        while (true)
            Thread.Sleep(10);
    }
    
    private static EnvironmentData? TryLoadEnv(string path = ".env")
    {
        var logger = CreateLogger();
        var container = new EnvironmentContainer();
        
        if (container.TryLoad(path))
            return container.Data;
        
        logger.Error($"Failed to load environment file. Created template file: {path}");
        return null;
    }
}
