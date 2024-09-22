using System.Reflection;

namespace FefuScheduleBot.Utils;

public static class ReflectionHelper
{
    public static IEnumerable<KeyValuePair<Type, T>> GetAllTypes<T>() where T : Attribute
    {
        return from type in GetAllTypes() 
            from customAttribute in type.GetCustomAttributes(typeof(T), false)
            select new KeyValuePair<Type, T>(type, (T) customAttribute);
    }

    private static IEnumerable<Type> GetAllTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
    }
    
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