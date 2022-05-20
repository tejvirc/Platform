namespace Aristocrat.Monaco.Accounting.Contracts.Handpay
{
    using Kernel;

    /// <summary>
    ///     Defines a base handpay event
    /// </summary>
    public abstract class BaseHandpayEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseHandpayEvent" /> class.
        /// </summary>
        /// <param name="transaction">The associated transaction</param>
        protected BaseHandpayEvent(HandpayTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the associated transaction
        /// </summary>
        public HandpayTransaction Transaction { get; }
    }
}