using dotenv.net;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Shared.Utilities.Extensions;
using JetBrains.Annotations;

namespace FefuScheduleBot.Environments;

[PublicAPI]
public sealed class EnvironmentContainer
{
    public EnvironmentData Data { get; } = new();

    public bool TryLoad(string path)
    {
        var dictionary = DotEnv.Read(new DotEnvOptions(envFilePaths: [path]));
        var success = Validate(dictionary, out var message);
        ModifyEnvFile(path, message);
        
        if (!success)
            return false;
        
        var type = typeof(EnvironmentData);
        foreach (var field in type.GetAllFields())
        {
            var envNameField = field.Name.ToUnderscoreCase().ToUpper();
            if (dictionary.TryGetValue(envNameField, out var value))
                field.SetValue(Data, value);
        }
        
        return true;
    }

    private bool Validate(IDictionary<string, string> dictionary, out string message)
    {
        message = string.Empty;
        
        var type = typeof(EnvironmentData);
        var passedValidation = true;

        foreach (var field in type.GetAllFields())
        {
            var envNameField = field.Name.ToUnderscoreCase().ToUpper();
            var spacings = ReflectionHelper.GetAttribute<EnvironmentSpacingAttribute>(field)?.Count ?? 0;
            var description = ReflectionHelper.GetAttribute<EnvironmentCommentAttribute>(field)?.Description ?? string.Empty;
            var defaultValue = field.GetValue(Data);
            
            dictionary.TryGetValue(envNameField, out var value);
            value ??= string.Empty;
            
            if (value == string.Empty)
            {
                value = defaultValue?.ToString() ?? string.Empty;
                dictionary[envNameField] = value;
            }

            for (var i = 0; i < spacings; i++) 
            {
                message += "\r\n";
            }

            if (description != string.Empty)
            {
                description = $"# {description}\r\n";
            }
            
            message += $"{description}{envNameField}={value}\r\n";
            if (value != string.Empty) 
                continue;
            
            if (passedValidation)
                passedValidation = false;
        }

        return passedValidation;
    }

    private static void ModifyEnvFile(string path, string content)
    {
        using var file = File.CreateText(path);
        file.Write(content);
    }
}