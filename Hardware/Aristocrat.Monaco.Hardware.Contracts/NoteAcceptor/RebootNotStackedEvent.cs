namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Definition of the RebootNotStackedEvent class.</summary>
    [Serializable]
    public class RebootNotStackedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RebootNotStackedEvent" /> class.
        /// </summary>
        /// <param name="isVoucher">Indicates whether this is a voucher or not.</param>
        /// <param name="amount">The amount of the escrowed document.</param>
        public RebootNotStackedEvent(bool isVoucher, long amount)
        {
            IsVoucher = isVoucher;
            Amount = amount;
        }

        /// <summary>Gets a value indicating whether or not this is a voucher.</summary>
        public bool IsVoucher { get; }

        /// <summary>Gets a amount.</summary>
        public long Amount { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [{1}, Amount={2}]",
                GetType().Name,
                IsVoucher ? "Voucher" : "Currency",
                Amount);
        }
    }
}