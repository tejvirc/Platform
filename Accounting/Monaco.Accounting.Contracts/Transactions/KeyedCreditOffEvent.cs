namespace Aristocrat.Monaco.Accounting.Contracts.Transactions
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     This event is emitted when we need to handle KeyedCreditOff logic, example ConfirmKeyOffCreditsPressed from KeyedCreditsPageViewModel
    ///     this signals the KeyedCreditOffConsumer 
    /// </summary>
    [Serializable]
    public class KeyedCreditOffEvent : BaseEvent
    {
        /// <summary>
        /// Gets the underlying Transaction
        /// </summary>
        public KeyedOffCreditsTransaction Transaction { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyedCreditOffEvent" /> class, setting the underlying 'Transaction'
        /// </summary>
        /// <param name="transaction">the underlying transaction</param>
        public KeyedCreditOffEvent(KeyedOffCreditsTransaction transaction)
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