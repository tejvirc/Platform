namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A set of <see cref="IEnumerable{T}"/> extensions that aren't provided by Linq
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Returns distinct elements from a sequence by using a specified key selector
        /// </summary>
        /// <typeparam name="TSource">The source collection type</typeparam>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <param name="this">The source</param>
        /// <param name="keySelector">The key</param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var seen = new HashSet<TKey>();
            foreach (var element in @this)
            {
                if (seen.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        ///     Returns the contiguous elements starting from the matching element.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns></returns>
        public static IEnumerable<T> TakeFrom<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var found = false;

            foreach (var item in source)
            {
                if (!found && !predicate(item))
                {
                    continue;
                }

                found = true;

                yield return item;
            }
        }

        /// <summary>
        ///     Gets the max value or returns the default value
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">The source to get the max for</param>
        /// <param name="selector">The selector for the max value</param>
        /// <param name="defaultValue">The default value to use if the list is empty</param>
        /// <returns>The max value or default if the list is empty</returns>
        public static long MaxOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, long defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Select(selector.Invoke).DefaultIfEmpty(defaultValue).Max();
        }

        /// <summary>
        ///     Gets the max value or returns the default value
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">The source to get the max for</param>
        /// <param name="selector">The selector for the max value</param>
        /// <param name="defaultValue">The default value to use if the list is empty</param>
        /// <returns>The max value or default if the list is empty</returns>
        public static int MaxOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, int defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Select(selector.Invoke).DefaultIfEmpty(defaultValue).Max();
        }

        /// <summary>
        ///     Gets the min value or returns the default value
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">The source to get the min for</param>
        /// <param name="selector">The selector for the min value</param>
        /// <param name="defaultValue">The default value to use if the list is empty</param>
        /// <returns>The min value or default if the list is empty</returns>
        public static long MinOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector, long defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Select(selector.Invoke).DefaultIfEmpty(defaultValue).Min();
        }

        /// <summary>
        ///     Gets the min value or returns the default value
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">The source to get the min for</param>
        /// <param name="selector">The selector for the min value</param>
        /// <param name="defaultValue">The default value to use if the list is empty</param>
        /// <returns>The min value or default if the list is empty</returns>
        public static int MinOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector, int defaultValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Select(selector.Invoke).DefaultIfEmpty(defaultValue).Min();
        }
    }
}