namespace Vgt.Client12.Testing.Tools
{
    using System;
    using System.Globalization;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Post this event to cause an escrowed debug credit transfer based on the credit value.
    /// </summary>
    [Serializable]
    public class DebugAnyCreditEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugAnyCreditEvent"/> class, setting the 'Denomination' property to 0.
        /// </summary>
        public DebugAnyCreditEvent()
        {
            Amount = 0;
            CreditType = AccountType.Cashable;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugAnyCreditEvent"/> class, setting the 'Denomination' property to the one passed-in.
        /// </summary>
        /// <param name="amount">The bill denomination that was escrowed.</param>
        /// <param name="creditType">The bill denomination that was escrowed.</param>
        public DebugAnyCreditEvent(int amount, AccountType creditType)
        {
            Amount = amount;
            CreditType = creditType;
        }

        /// <summary>
        ///     Gets the Amount.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        ///     Gets the CreditType.
        /// </summary>
        public AccountType CreditType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Amount={1}, CreditType={2}]",
                GetType().Name,
                Amount,
                CreditType);
        }
    }
}