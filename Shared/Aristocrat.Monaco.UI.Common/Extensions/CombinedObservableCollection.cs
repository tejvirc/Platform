namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///     The combined observable collection consists of ordered observable collections which are considered as one whole.
    /// </summary>
    /// <typeparam name="T">The type param.</typeparam>
    public class CombinedObservableCollection<T> : IReadOnlyList<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CombinedObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="collections">The observable collections.</param>
        public CombinedObservableCollection(params ObservableCollection<T>[] collections)
        {
            ObservableCollections = new List<ObservableCollection<T>>();

            foreach (var collection in collections)
            {
                ObservableCollections.Add(collection);
            }
        }

        private List<ObservableCollection<T>> ObservableCollections { get; }

        /// <inheritdoc />
        public int Count => ObservableCollections.Select(x => x.Count).Sum();

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                var currentCount = 0;
                foreach (var collection in ObservableCollections)
                {
                    if (index < currentCount + collection.Count)
                    {
                        return collection[index - currentCount];
                    }

                    currentCount += collection.Count;
                }

                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    @"Index was out of range. Must be non-negative and less than the size of the collection.");
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return ObservableCollections.SelectMany(collection => collection).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Determines whether item with given index exists.
        /// </summary>
        /// <param name="index">The index of item.</param>
        /// <returns><c>true</c> if exists otherwise <c>false</c></returns>
        public bool ContainsWith(int index)
        {
            return index >= 0 && index < Count;
        }

        /// <summary>
        ///     Gets the index of item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The index.</returns>
        public int IndexOf(T item)
        {
            var absoluteIndex = 0;
            foreach (var collection in ObservableCollections)
            {
                var relativeIndex = collection.IndexOf(item);
                if (relativeIndex != -1)
                {
                    absoluteIndex += relativeIndex;
                    return absoluteIndex;
                }

                absoluteIndex += collection.Count;
            }

            return -1;
        }
    }
}