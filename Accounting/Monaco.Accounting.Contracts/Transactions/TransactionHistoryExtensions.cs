namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;

    /// <summary>
    ///     A set of subscription extensions
    /// </summary>
    public static class TransactionHistoryExtensions
    {
        /// <summary>
        ///     Used to recall all transactions of a specified transaction type from the TransactionHistory
        /// </summary>
        /// <param name="this">The transaction history instance</param>
        /// <param name="type">The transaction type</param>
        public static ICollection<ITransaction> RecallTransactions(this ITransactionHistory @this, Type type)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var method = @this.GetType().GetMethodEx(nameof(ITransactionHistory.RecallTransactions), new Type[0]);

            var genericMethod = method.MakeGenericMethod(type);
            return genericMethod.Invoke(@this, null) as ICollection<ITransaction>;
        }

        /// <summary>
        ///     Gets the last (most recent transaction) of type T
        /// </summary>
        /// <typeparam name="T">A <see cref="ITransaction" /> type</typeparam>
        /// <param name="this">The transaction history instance</param>
        /// <returns>The last transaction or null</returns>
        public static T GetLast<T>(this ITransactionHistory @this) where T : ITransaction
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.RecallTransactions<T>().OrderByDescending(t => t.TransactionId).FirstOrDefault();
        }

        /// <summary>
        ///     An ITransactionHistory extension method that recall transaction.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="transactionId">Identifier for the transaction.</param>
        /// <returns>A T.</returns>
        public static T RecallTransaction<T>(this ITransactionHistory @this, long transactionId)
            where T : class, ITransaction
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.RecallTransactions<T>()?.SingleOrDefault(t => t.TransactionId == transactionId);
        }
    }
}