namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Some useful extensions utilities.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Determines whether this string is convertible to integer value (in g2s spec this means int64).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the specified value is int; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInt(this string value)
        {
            return long.TryParse(value, out _);
        }

        /// <summary>
        ///     Determines whether this string is convertible to decimal value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the specified value is decimal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDecimal(this string value)
        {
            return decimal.TryParse(value, out _);
        }

        /// <summary>
        ///     Determines whether this string is convertible to boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the specified value is boolean; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBoolean(this string value)
        {
            return bool.FalseString.Equals(value) || bool.TrueString.Equals(value);
        }

        /// <summary>
        ///     Adds the specified values to dictionary and returns the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="values">The values.</param>
        /// <returns>The original dictionary with new values</returns>
        public static Dictionary<TKey, TValue> AddValues<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            IEnumerable<Tuple<TKey, TValue>> values)
        {
            foreach (var val in values)
            {
                dictionary.Add(val.Item1, val.Item2);
            }

            return dictionary;
        }
    }
}