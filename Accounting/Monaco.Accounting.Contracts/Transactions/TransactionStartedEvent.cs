namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     This event is emitted whenever a ITransactionCoordinator implementation starts a new transaction.
    /// </summary>
    [Serializable]
    public class TransactionStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionStartedEvent" /> class.
        /// </summary>
        /// <remarks>
        ///     Default constructor necessary for serialization. Transaction type
        ///     defaults to Write.
        /// </remarks>
        public TransactionStartedEvent()
            : this(TransactionType.Write)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionStartedEvent" /> class.
        /// </summary>
        /// <param name="transactionType">The transaction type</param>
        public TransactionStartedEvent(TransactionType transactionType)
        {
            TransactionType = transactionType;
        }

        /// <summary>
        ///     Gets or sets the type of the transaction
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [transactionType={1}]",
                GetType().Name,
                TransactionType);
        }
    }
}