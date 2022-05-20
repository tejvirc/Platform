namespace Aristocrat.Monaco.Asp.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Check if the dictionary contains all entries, matching the keys passed in.
        /// </summary>
        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] list) => list.All(l => dictionary.ContainsKey(l));

        /// <summary>
        ///     Get the value by the key, if no entry is found for the key, return default value of the TValue type.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) => dictionary.TryGetValue(key, out var value) ? value : default;
    }
}