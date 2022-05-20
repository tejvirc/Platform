namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to connect transactions
    /// </summary>
    public interface ITransactionConnector
    {
        /// <summary>
        ///     Gets or sets the associated transaction.  This includes items such as game play, vouchers, etc.
        ///     It is possible that the associated transactions are not available since the associated log may have rolled over
        /// </summary>
        IEnumerable<long> AssociatedTransactions { get; set; }
    }
}