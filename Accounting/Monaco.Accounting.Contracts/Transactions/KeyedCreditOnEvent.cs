namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     This event is emitted when we need to handle KeyedCreditOn logic, example ConfirmKeyOnCreditsPressed from KeyedCreditsPageViewModel
    ///     this signals the KeyedCreditOnConsumer 
    /// </summary>
    /// <summary>
    ///     This event is emitted whenever ConfirmKeyOnCreditsPressed in the KeyedCreditsPageViewModel
    ///     to signal the consumer 
    /// </summary>
    [Serializable]
    public class KeyedCreditOnEvent : BaseEvent
    {
        /// <summary>
        /// Gets the underlying Transaction
        /// </summary>
        public KeyedCreditsTransaction Transaction { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyedCreditOnEvent" /> class, setting the underlying 'Transaction'
        /// </summary>
        /// <param name="transaction">the underlying transaction</param>
        public KeyedCreditOnEvent(KeyedCreditsTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// overriding the class toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Transaction={1}]",
                GetType().Name,
                Transaction);
        }
    }
}
