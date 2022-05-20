namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.TransferOut;

    /// <summary>
    ///     Describes the cash out info for cash-outs that occur in a game round
    /// </summary>
    [Serializable]
    public class CashOutInfo
    {
        /// <summary>
        ///     Gets or sets the transfer amount
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the reason
        /// </summary>
        public TransferOutReason Reason { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not this is a handpay
        /// </summary>
        public bool Handpay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the transfer is complete
        /// </summary>
        public bool Complete { get; set; }

        /// <summary>
        ///     Gets or sets the trace Id that can be used to track the state of the transfer
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        ///     Gets or sets the associated transaction.  For example, the jackpot transaction Id
        /// </summary>
        public IEnumerable<long> AssociatedTransactions { get; set; }
    }
}