namespace Aristocrat.Monaco.Sas
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     TransactionHistory extension methods
    /// </summary>
    public static class TransactionHistoryExtensions
    {
        /// <summary>
        ///     Gets whether or not there are any pending transaction needing host acknowledgement
        /// </summary>
        /// <param name="this">The ITransactionHistory to use</param>
        /// <returns>Whether or not there are any pending transaction needing host acknowledgement</returns>
        public static bool AnyPendingHostAcknowledged(this ITransactionHistory @this)
        {
            return @this.RecallTransactions(true).Any(NeedsHostAcknowledgement);
        }

        /// <summary>
        ///     Gets the total count of pending transactions needing host acknowledgement
        /// </summary>
        /// <param name="this">The ITransactionHistory to use</param>
        /// <returns>The total count of pending transactions needing host acknowledgement</returns>
        public static int GetPendingHostAcknowledgedCount(this ITransactionHistory @this)
        {
            return @this.RecallTransactions(true).Count(NeedsHostAcknowledgement);
        }

        /// <summary>
        ///     Gets the next host needing acknowledged transaction
        /// </summary>
        /// <param name="this">The ITransactionHistory to use</param>
        /// <returns>The next host needing acknowledged transaction or null if no transaction is found</returns>
        public static ITransaction GetNextNeedingHostAcknowledgedTransaction(this ITransactionHistory @this)
        {
            return @this.RecallTransactions(false).FirstOrDefault(NeedsHostAcknowledgement);
        }

        /// <summary>
        ///     Gets the transaction with the set host sequence ID
        /// </summary>
        /// <param name="this">The ITransactionHistory to use</param>
        /// <param name="hostSequence">The host sequence ID to get</param>
        /// <param name="maxQueueSize">The max queue size to use</param>
        /// <returns>The transaction or null if no transaction is found</returns>
        public static ITransaction GetTransaction(this ITransactionHistory @this, long hostSequence, long maxQueueSize)
        {
            return @this.RecallTransactions(true)?.FirstOrDefault(
                x => x is IAcknowledgeableTransaction transaction &&
                    transaction.HostSequence != 0 &&
                    ((transaction.HostSequence - 1) % maxQueueSize) == (hostSequence - 1));
        }

        /// <summary>
        ///     Gets the host acknowledge queue size.  This will be the minimum size for either of the three values:
        ///     - Voucher Out Max Transaction Size
        ///     - Handpay Max Transaction Size
        ///     - SasConstants.MaxHostSequence (31)
        /// </summary>
        /// <param name="this">The ITransactionHistory to use</param>
        /// <returns>The host acknowledge queue size</returns>
        public static int GetHostAcknowledgedQueueSize(this ITransactionHistory @this)
        {
            var voucherQueueSize = @this.GetMaxTransactions<VoucherOutTransaction>();
            var handPayQueueSize = @this.GetMaxTransactions<HandpayTransaction>();

            // We need to get the transaction sizes and take the min so we don't have any dropped transactions
            // We can have all voucher outs or handpays so check both as they can have different sizes
            return Math.Min(SasConstants.MaxHostSequence, Math.Min(voucherQueueSize, handPayQueueSize));
        }

        private static bool NeedsHostAcknowledgement(ITransaction transaction)
        {
            return (transaction is VoucherOutTransaction voucherOutTransaction && !voucherOutTransaction.HostAcknowledged) ||
                     (transaction is HandpayTransaction handpayTransaction
                     && !handpayTransaction.IsCreditType()
                     && handpayTransaction.State == HandpayState.Committed
                     && handpayTransaction.Validated);
        }
    }
}