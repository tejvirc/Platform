namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event used to indicate the bank balance has changed.
    /// </summary>
    /// <remarks>
    ///     Any components which are sensitive to credit change should always listen to this event.
    /// </remarks>
    public class BankBalanceChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BankBalanceChangedEvent" /> class.
        /// </summary>
        /// <param name="oldBalance"> The old balance. </param>
        /// <param name="newBalance"> The new balance. </param>
        /// <param name="transactionId"> The new balance. </param>
        public BankBalanceChangedEvent(long oldBalance, long newBalance, Guid transactionId)
        {
            OldBalance = oldBalance;
            NewBalance = newBalance;
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Gets the old balance.
        /// </summary>
        public long OldBalance { get; }

        /// <summary>
        ///     Gets the new balance.
        /// </summary>
        public long NewBalance { get; }

        /// <summary>
        ///     Gets the TransactionId
        /// </summary>
        public Guid TransactionId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} [Timestamp={Timestamp}, OldBalance={OldBalance}, NewBalance={NewBalance}, TransactionId={TransactionId}]");
        }
    }
}