namespace Aristocrat.Monaco.Accounting.Contracts
{
    using Kernel;

    /// <summary>
    ///     Post this event for currency-in in demonstration mode only.
    /// </summary>
    public class DemonstrationCurrencyEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DemonstrationCurrencyEvent" /> class.
        /// </summary>
        /// <param name="amount">The bill amount that need to be inserted.</param>
        public DemonstrationCurrencyEvent(int amount)
        {
            Amount = amount;
        }

        /// <summary>
        ///     Gets the Amount of the bill that need to be inserted.
        /// </summary>
        public int Amount { get; }
    }
}