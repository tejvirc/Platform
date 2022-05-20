namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     ITransactionHistory provides the interface to recall/save transactions from TransactionHistoryProviders.
    /// </summary>
    /// <remarks>
    ///     Each type of transaction has its own implementation of ITransaction and a supporting TransactionHistoryProvider. It
    ///     is expected that the implementation of ITransactionHistory will use mono-addins to "discover" specifications for
    ///     TransactionHistoryProvider objects, including the type of transactions they support. Mechanisms for storing and
    ///     retrieving transaction information will then be established. Access to the transaction history information will be
    ///     available to the rest of the system through this interface.
    /// </remarks>
    /// <seealso cref="ITransaction" />
    public interface ITransactionHistory
    {
        /// <summary>
        ///     Gets All transaction types known to transaction history
        /// </summary>
        IReadOnlyCollection<Type> TransactionTypes { get; }

        /// <summary>
        ///     Method to recall all transactions of a specified transaction type from the TransactionHistory.
        /// </summary>
        /// <returns>List of the transactions stored for the specified type.</returns>
        IReadOnlyCollection<T> RecallTransactions<T>() where T : ITransaction;

        /// <summary>
        ///     Method to recall all transactions from the TransactionHistory.
        /// </summary>
        /// <returns>List of all transactions stored.</returns>
        IOrderedEnumerable<ITransaction> RecallTransactions();

        /// <summary>
        ///     Method to recall all transactions from the TransactionHistory.
        /// </summary>
        /// <param name="newestFirst">Whether or not to sort with the newest items first</param>
        /// <returns>List of all transactions stored.</returns>
        IOrderedEnumerable<ITransaction> RecallTransactions(bool newestFirst);

        /// <summary>
        ///     Gets the total number of entries before queue-cycling.
        /// </summary>
        /// <returns>The total number of entries before queue-cycling.</returns>
        int GetMaxTransactions<T>() where T : ITransaction;

        /// <summary>
        ///     Method to save a transaction to the appropriate TransactionHistoryProviders.
        /// </summary>
        /// <param name="transaction">Transaction to be saved.</param>
        void AddTransaction(ITransaction transaction);

        /// <summary>
        ///     Method to update a transaction to the appropriate TransactionHistoryProviders.
        /// </summary>
        /// <param name="transaction">Transaction to be updated.</param>
        void UpdateTransaction(ITransaction transaction);

        /// <summary>
        ///     Method to update a transaction to the appropriate TransactionHistoryProviders.
        /// </summary>
        /// <param name="transactionId">The unique transactionId of the transaction to overwrite.</param>
        /// <param name="transaction">Transaction to be updated.</param>
        void OverwriteTransaction(long transactionId, ITransaction transaction);

        /// <summary>
        ///     Returns the print enable/disable status.
        /// </summary>
        /// <returns>true/false.</returns>
        bool IsPrintable<T>() where T : ITransaction;
    }
}