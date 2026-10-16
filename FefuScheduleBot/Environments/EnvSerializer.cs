using FefuScheduleBot.Environments.Attributes;
using FefuScheduleBot.Environments.Parsing;
using FefuScheduleBot.Utils;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Utilities.Extensions;
using Hypercube.Utilities.Helpers;
using JetBrains.Annotations;

namespace FefuScheduleBot.Environments;

[PublicAPI]
public static class EnvSerializer
{
    public static object Deserialize(Type type, string content)
    {
        var result = Activator.CreateInstance(type) 
                     ?? throw new InvalidOperationException($"Could not create instance of type '{type.FullName}'.");

        PopulateObject(result, content);
        return result;
    }

    public static bool TryDeserialize(Type type, string content, out object? result)
    {
        result = null;

        var instance = Activator.CreateInstance(type);
        if (instance == null)
            return false;

        var dictionary = ParseToDictionary(content);
        PopulateObject(instance, dictionary);

        if (!ValidateObject(instance, dictionary))
            return false;

        result = instance;
        return true;
    }
    
    public static T Deserialize<T>(string content) where T : new()
    {
        var result = new T();
        PopulateObject(result, content);
        return result;
    }

    public static bool TryDeserialize<T>(string content, out T result) where T : new()
    {
        result = new T();
        var dictionary = ParseToDictionary(content);
        
        PopulateObject(result, dictionary);
        return ValidateObject(result, dictionary);
    }

    public static string Serialize<T>(T instance)
    {
        var dictionary = new Dictionary<string, string>();
        
        FormatContent(instance, dictionary, out var content);
        return content;
    }

    public static bool Sync<T>(T instance, string content, out string updatedContent)
    {
        var dictionary = ParseToDictionary(content);
        var isValid = FormatContent(instance, dictionary, out updatedContent);
        
        PopulateObject(instance, dictionary);
        return isValid;
    }

    private static Dictionary<string, string> ParseToDictionary(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new Dictionary<string, string>();

        var lexer = new EnvLexer(content);
        var tokens = lexer.Tokenize();

        var parser = new EnvParser(tokens);
        var document = parser.Parse();

        return document.ToDictionary();
    }

    private static void PopulateObject<T>(T instance, string content)
    {
        var dictionary = ParseToDictionary(content);
        PopulateObject(instance, dictionary);
    }

    private static void PopulateObject<T>(T instance, IDictionary<string, string> dictionary)
    {
        var type = typeof(T);

        foreach (var field in type.GetAllFields())
        {
            var envNameField = field.Name.ToUnderscoreCase().ToUpper();
            if (dictionary.TryGetValue(envNameField, out var value))
                field.SetValue(instance, value);
        }
    }

    private static bool ValidateObject<T>(T instance, IDictionary<string, string> dictionary)
    {
        var type = typeof(T);

        foreach (var field in type.GetAllFields())
        {
            var envNameField = field.Name.ToUnderscoreCase().ToUpper();
            if (!dictionary.TryGetValue(envNameField, out var value) || string.IsNullOrEmpty(value))
                return false;
        }

        return true;
    }

    private static bool FormatContent<T>(T instance, IDictionary<string, string> dictionary, out string formattedContent)
    {
        formattedContent = string.Empty;
        var type = typeof(T);
        var isValid = true;

        foreach (var field in type.GetAllFields())
        {
            var envNameField = field.Name.ToUnderscoreCase().ToUpper();
            var spacings = ReflectionHelper.GetAttribute<EnvironmentSpacingAttribute>(field)?.Count ?? 0;
            var description = ReflectionHelper.GetAttribute<EnvironmentCommentAttribute>(field)?.Description ?? string.Empty;
            var defaultValue = field.GetValue(instance);
            
            dictionary.TryGetValue(envNameField, out var value);
            value ??= string.Empty;

            if (value == string.Empty)
            {
                value = defaultValue?.ToString() ?? string.Empty;
                dictionary[envNameField] = value;
            }

            for (var i = 0; i < spacings; i++)
                formattedContent += "\r\n";

            if (description != string.Empty)
                formattedContent += $"# {description}\r\n";

            formattedContent += $"{envNameField}={value}\r\n";

            if (value == string.Empty && isValid)
                isValid = false;
        }

        return isValid;
    }
}