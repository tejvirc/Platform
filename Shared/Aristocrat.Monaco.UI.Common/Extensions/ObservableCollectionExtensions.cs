namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     Extensions for <c>ObservableCollection</c>.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        ///     Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
        /// </summary>
        /// <param name="observableCollection">Current collection.</param>
        /// <param name="collection">Collection to add.</param>
        /// <typeparam name="T">Type of elements in collection.</typeparam>
        public static void AddRange<T>(this ObservableCollection<T> observableCollection, IEnumerable<T> collection)
            where T : class
        {
            if (observableCollection == null)
            {
                throw new ArgumentNullException(nameof(observableCollection));
            }

            if (collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                observableCollection.Add(item);
            }
        }

        /// <summary>
        ///     Split collection into list of collection of specified size.
        /// </summary>
        /// <typeparam name="T">Current type of collection</typeparam>
        /// <param name="source">Source list.</param>
        /// <param name="chunkSize">Chunk size.</param>
        /// <returns>Returns list of list where count of less then chunk size.</returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this ObservableCollection<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        ///     Remove item from collection matching condition
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Remove<T>(this ObservableCollection<T> source, Predicate<T> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];

                if (predicate(item))
                {
                    source.Remove(item);
                    return true;
                }
            }

            return false;
        }
    }
}