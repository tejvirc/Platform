namespace Aristocrat.Extensions.Fluxor;

using System.Collections.Generic;
using System;
using System.Linq;

public class ActionInfo
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string type { get; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public object Payload { get; }

    public ActionInfo(object action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        type = $"{GetTypeDisplayName(action.GetType())}, {action.GetType().Namespace}";
        Payload = action;
    }

    public static string GetTypeDisplayName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        string name = type.GetGenericTypeDefinition().Name;
        name = name.Remove(name.IndexOf('`'));
        IEnumerable<string> genericTypes = type
            .GetGenericArguments()
            .Select(GetTypeDisplayName);
        return $"{name}<{string.Join(",", genericTypes)}>";
    }
}
