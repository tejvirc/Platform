namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public static class EnumerableExtensions
{
    /// <summary>
    ///     In contrast to ToDictionary, this only uses the first occurrence of each key and places it in the result Dictionary
    /// </summary>
    public static Dictionary<TKey, TElement> ToSafeDictionary<TElement, TKey>(
        this IEnumerable<TElement> elements, Func<TElement, TKey> keySelector)
        where TKey : notnull
    {
        return elements.ToSafeDictionary(keySelector, each => each);
    }

    /// <summary>
    ///     In contrast to ToDictionary, this only uses the first occurrence of each key and places it in the result Dictionary
    /// </summary>
    public static Dictionary<TKey, TValue> ToSafeDictionary<TElement, TKey, TValue>(
        this IEnumerable<TElement> elements, Func<TElement, TKey> keySelector, Func<TElement, TValue> valueSelector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();
        foreach (var each in elements)
        {
            var key = keySelector(each);
            if (result.ContainsKey(key))
                continue;

            result.Add(key, valueSelector(each));
        }
        return result;
    }

    public static void UpdateObservable<T, TKey>(this IEnumerable<T> list, ObservableCollection<T> observable, Func<T, TKey> keySelector)
        where T : IEquatable<T>
        where TKey : notnull
    {
        list.UpdateObservable(observable, keySelector, (left, right) => left.Equals(right));
    }

    public static void UpdateObservable<T, TKey>(this IEnumerable<T> list, ObservableCollection<T> observable, Func<T, TKey> keySelector, Func<T, T, bool> equals)
        where TKey : notnull
    {
        var oldKeyToIndex = observable
            .Select((x, index) => (Value: x, Index: index))
            .ToSafeDictionary(x => keySelector(x.Value), x => x.Index);
        var newKeys = list.Select(keySelector).ToHashSet();

        var indicesToRemove = oldKeyToIndex
            .Where(tuple => !newKeys.Contains(tuple.Key))
            .Select(tuple => tuple.Value)
            .OrderByDescending(x => x);

        // Remove elements in backwards order to not invalidate the lower indices
        foreach (var index in indicesToRemove)
        {
            observable.RemoveAt(index);
        }

        // Now insert new elements
        var target = 0;
        foreach (var each in list.OrderBy(keySelector))
        {
            // Invariant: all indices below index are synchronized
            if (target >= observable.Count)
            {
                observable.Add(each);
            }
            else if (!keySelector(each).Equals(keySelector(observable[target])))
            {
                observable.Insert(target, each);
            }
            else if (!equals(each, observable[target]))
            {
                observable[target] = each;
            }

            ++target;
        }
    }
}
