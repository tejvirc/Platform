namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System;
    using Kernel;

    /// <summary>
    ///     Defines the base class for outcome events
    /// </summary>
    public abstract class BaseOutcomeEvent : BaseEvent
    {
        /// <summary>
        ///     Base class for bonus events
        /// </summary>
        /// <param name="transaction">The <see cref="CentralTransaction" /> associated with the event</param>
        protected BaseOutcomeEvent(CentralTransaction transaction)
        {
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        ///     Gets tha associated transaction
        /// </summary>
        public CentralTransaction Transaction { get; }
    }
}