namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Globalization;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;

    /// <summary>
    ///     Event emitted when a currency-in transaction has been completed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is always posted no matter the currency is stacked or rejected.
    ///         And it is always posted after <c>CurrencyInStartedEvent</c>
    ///     </para>
    ///     <para>
    ///         The amounts included in this class are basic XSpin currency units, The magnitude
    ///         of the units are determined dynamically based on the currency code.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class CurrencyInCompletedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInCompletedEvent" /> class.
        /// </summary>
        public CurrencyInCompletedEvent(long amount, INote note = null, BillTransaction transaction = null)
        {
            Amount = amount;
            Transaction = transaction;
            Note = note;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInCompletedEvent" /> class.
        /// </summary>
        private CurrencyInCompletedEvent()
        {
        }

        /// <summary>
        ///     Gets the currency amount that were added by the note acceptor.
        /// </summary>
        public long Amount { get; }

        /// <summary>
        ///     Gets the information on the Note that was accepted.
        /// </summary>
        public INote Note { get; }

        /// <summary>
        ///     Gets the transaction that was accepted.
        /// </summary>
        public BillTransaction Transaction { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} [Timestamp={Timestamp}, Amount={Amount}]");
        }
    }
}