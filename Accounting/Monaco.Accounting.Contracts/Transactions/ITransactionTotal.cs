namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    /// <summary>
    ///     Provides a mechanism to get the total amount for a transaction
    /// </summary>
    public interface ITransactionTotal
    {
        /// <summary>
        ///     Gets the finalized amount that was completed by this transaction
        /// </summary>
        long TransactionAmount { get; }
    }
}