namespace Aristocrat.Monaco.G2S.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;

    /// <summary>
    ///     A set of extension methods for transaction logs
    /// </summary>
    public static class TransactionExtensions
    {
        /// <summary>
        ///     Filters and sorts a sequence based on the provided parameters
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable to filter</param>
        /// <param name="lastSequence">
        ///     The sequence number of the transaction that should be the first entry in the list; if set to
        ///     0 (zero) then default to the last transaction.
        /// </param>
        /// <param name="totalEntries">
        ///     The total number of transactions that should be included in the list; if set to 0 (zero)
        ///     then default to all transactions.
        /// </param>
        /// <returns>An System.Linq.IOrderedEnumerable whose elements are sorted according to a key</returns>
        public static IOrderedEnumerable<TSource> TakeTransactions<TSource>(
            this IEnumerable<TSource> source,
            long lastSequence,
            int totalEntries)
            where TSource : ILogSequence
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var transactions = source as IList<TSource> ?? source.ToList();

            if (lastSequence != 0 && transactions.All(t => t.LogSequence != lastSequence))
            {
                return Enumerable.Empty<TSource>().OrderBy(x => 1);
            }

            if (totalEntries == 0)
            {
                return transactions
                    .Where(x => lastSequence == 0 || x.LogSequence <= lastSequence)
                    .OrderByDescending(x => x.LogSequence);
            }

            return transactions
                .Where(x => lastSequence == 0 || x.LogSequence <= lastSequence)
                .OrderByDescending(x => x.LogSequence)
                .Take(totalEntries)
                .OrderByDescending(x => x.LogSequence);
        }

        /// <summary>
        ///     Filters and sorts a sequence based on the parameters
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable to filter</param>
        /// <param name="lastSequence">
        ///     The sequence number of the transaction that should be the first entry in the list; if set to
        ///     0 (zero) then default to the last transaction.
        /// </param>
        /// <param name="totalEntries">
        ///     The total number of transactions that should be included in the list; if set to 0 (zero)
        ///     then default to all transactions.
        /// </param>
        /// <returns>An System.Linq.IOrderedEnumerable whose elements are sorted according to a key</returns>
        public static IOrderedEnumerable<TSource> TakeLogs<TSource>(
            this IEnumerable<TSource> source,
            long lastSequence,
            int totalEntries)
            where TSource : Monaco.Common.Storage.ILogSequence
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var transactions = source as IList<TSource> ?? source.ToList();

            if (lastSequence != 0 && transactions.All(t => t.Id != lastSequence))
            {
                return Enumerable.Empty<TSource>().OrderBy(x => 1);
            }

            if (totalEntries == 0)
            {
                return transactions
                    .Where(x => lastSequence == 0 || x.Id <= lastSequence)
                    .OrderByDescending(x => x.Id);
            }

            return transactions
                .Where(x => lastSequence == 0 || x.Id <= lastSequence)
                .OrderByDescending(x => x.Id)
                .Take(totalEntries)
                .OrderByDescending(x => x.Id);
        }
    }
}