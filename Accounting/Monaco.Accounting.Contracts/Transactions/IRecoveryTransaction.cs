namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System;

    /// <summary>
    ///     The interface to a recovery transaction
    /// </summary>
    public interface IRecoveryTransaction
    {
        /// <summary>
        ///     Get or sets the id for the transaction
        /// </summary>
        Guid TransactionId { get; set; }
        
        /// <summary>
        ///     Get or sets the trace id for the transaction
        /// </summary>
        Guid TraceId { get; set; }
    }
}