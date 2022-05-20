namespace Aristocrat.Monaco.Accounting.Contracts.Wat
{
    using Kernel;

    /// <summary>
    ///     Base class for WAT events
    /// </summary>
    public abstract class BaseWatEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseWatEvent" /> class.
        /// </summary>
        /// <param name="transaction">The Wat transaction associated with the complete event</param>
        protected BaseWatEvent(WatTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     The associated transaction
        /// </summary>
        public WatTransaction Transaction { get; }
    }
}
