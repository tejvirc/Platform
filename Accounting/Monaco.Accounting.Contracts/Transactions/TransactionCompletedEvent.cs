namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     This event is emitted whenever a the ITransactionCoordinator implementation
    ///     terminates a new transaction.
    /// </summary>
    [Serializable]
    public class TransactionCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionCompletedEvent" /> class, setting the 'TransactionId'
        ///     property to Guid.Empty.
        ///     A parameterless constructor is required for events that are sent from the Key to Event converter.
        /// </summary>
        public TransactionCompletedEvent()
        {
            TransactionId = Guid.Empty;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionCompletedEvent" /> class, setting
        ///     the 'TransactionId' property to the one passed-in.
        /// </summary>
        /// <param name="transactionId">The type of service.</param>
        public TransactionCompletedEvent(Guid transactionId)
        {
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Gets the transaction identifier.
        /// </summary>
        public Guid TransactionId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [TransactionId={1}]",
                GetType().Name,
                TransactionId.ToString());
        }
    }
}