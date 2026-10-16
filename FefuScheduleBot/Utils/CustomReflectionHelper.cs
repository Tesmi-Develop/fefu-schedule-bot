using System.Reflection;
using Hypercube.Utilities.Helpers;

namespace FefuScheduleBot.Utils;

public static class CustomReflectionHelper
{
    extension(ReflectionHelper)
    {
        public static T? GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            foreach (var customAttribute in field.GetCustomAttributes())
            {
                if (customAttribute is T attribute)
                    return attribute;
            }
            return null;
        }
    }
    
    public static IEnumerable<KeyValuePair<Type, T>> GetAllTypes<T>() where T : Attribute
    {
        return from type in GetAllTypes() 
            from customAttribute in type.GetCustomAttributes(typeof(T), false)
            select new KeyValuePair<Type, T>(type, (T) customAttribute);
    }

    private static IEnumerable<Type> GetAllTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetTypesSafely);
    }

    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
        catch
        {
            return Enumerable.Empty<Type>();
        }
    }
}