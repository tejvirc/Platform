namespace Aristocrat.Monaco.Kernel.MarketConfig;

using System;

/// <summary>
/// Helper classes for working with the Market Config system.
/// </summary>
public static class MarketConfigHelper
{
    /// <summary>
    /// Creates an instance of a type from a fully qualified type name, using the first matching type found in the loaded assemblies.
    /// </summary>
    /// <param name="typeName">The fully qualified name of the type to instantiate</param>
    /// <returns></returns>
    /// <exception cref="MarketConfigException">Thrown is the type cannot be found</exception>
    public static T CreateInstanceFromTypeName<T>(string typeName)
    {
        Type type = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null)
            {
                break;
            }
        }

        if (type == null)
        {
            throw new MarketConfigException($"Cannot create instance of type {typeName}, class not found in loaded assemblies.");
        }

        return (T) Activator.CreateInstance(type);
    }
}