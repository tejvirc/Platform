namespace Aristocrat.Monaco.G2S.Data.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The hash helper.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        ///     Gets the collection hash.
        /// </summary>
        /// <typeparam name="T">Collection</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="hash">The hash.</param>
        /// <returns>Hash</returns>
        public static int GetCollectionHash<T>(IEnumerable<T> collection, int hash)
        {
            if (collection == null)
            {
                return hash;
            }

            return collection.OrderBy(v => v).Aggregate(hash, (i, item) => (i * 397) ^ item.GetHashCode());
        }
    }
}