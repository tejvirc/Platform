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
    }
}