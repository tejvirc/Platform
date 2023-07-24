namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.TransferOut;

    /// <summary>
    ///     Describes the cash out info for cash-outs that occur in a game round
    /// </summary>
    [Serializable]
    public class CashOutInfo : IEquatable<CashOutInfo>
    {
        /// <summary>
        ///     Gets or sets the transfer amount
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the wager amount for large win
        /// </summary>
        public long Wager { get; set; }

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

        /// <inheritdoc />
        public bool Equals(CashOutInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Amount == other.Amount &&
                   Wager == other.Wager &&
                   Reason == other.Reason &&
                   Handpay == other.Handpay &&
                   Complete == other.Complete &&
                   TraceId.Equals(other.TraceId) &&
                   Equals(AssociatedTransactions, other.AssociatedTransactions);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CashOutInfo)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ Wager.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Reason;
                hashCode = (hashCode * 397) ^ Handpay.GetHashCode();
                hashCode = (hashCode * 397) ^ Complete.GetHashCode();
                hashCode = (hashCode * 397) ^ TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (AssociatedTransactions != null ? AssociatedTransactions.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}