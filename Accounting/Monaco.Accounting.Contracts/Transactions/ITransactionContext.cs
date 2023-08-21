namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System;

    /// <summary>
    ///     Provides a mechanism to track a transaction with a provided unique identifier
    /// </summary>
    public interface ITransactionContext : ITransactionTotal
    {
        /// <summary>
        ///     Gets or sets the associated bank trace Id
        /// </summary>
        Guid TraceId { get; set; }
    }
}