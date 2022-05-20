using Aristocrat.Monaco.Accounting.Contracts;
using System.Collections.Generic;

namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a mechanism to interact with a bucket of money inserted between game rounds
    /// </summary>
    public interface ICurrencyInContainer
    {
        /// <summary>
        ///     Gets the current amount in the money in bucket
        /// </summary>
        long AmountIn { get; }

        /// <summary>
        ///     Gets the transactions associated to the game round
        /// </summary>
        IEnumerable<TransactionInfo> Transactions { get; }

        /// <summary>
        ///     Adds the provided value to the bucket
        /// </summary>
        /// <param name="value">The value to add to the bucket</param>
        /// <param name="paidAmount">The overwriting paid amount</param>
        /// <param name="transactionId"></param>
        void Credit(ITransaction value, long paidAmount, long transactionId);

        /// <summary>
        ///     Adds the provided value to the bucket
        /// </summary>
        /// <param name="value">The value to add to the bucket</param>
        void Credit(ITransaction value);

        /// <summary>
        ///     Resets the current bucket
        /// </summary>
        long Reset();
    }
}
