namespace Aristocrat.Monaco.Common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A dictionary extensions.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     An <see cref="IDictionary{TKey, TValue}"/> extension method that query if 'dictionary' contains all the given keys.
        /// </summary>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="keys">A variable-length parameters list containing keys.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            return keys.All(dictionary.ContainsKey);
        }
    }
}