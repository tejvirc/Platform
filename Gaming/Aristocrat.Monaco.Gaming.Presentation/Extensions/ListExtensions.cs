namespace Aristocrat.Monaco.Gaming.Presentation;

using System.Collections.Generic;

public static class ListExtensions
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
    {
        if (collection is null)
        {
            return;
        }

        foreach (var item in collection)
        {
            list.Add(item);
        }
    }
}
